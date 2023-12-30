using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class EndlessTerrain : MonoBehaviour{

    const float viewMoveThreshHoldForChunkUpdate = 25f;
    const float sqrMoveThreshHoldForChunkUpdate = viewMoveThreshHoldForChunkUpdate * viewMoveThreshHoldForChunkUpdate;

    public LOODInfo[] detailLevels;
    public static float maxViewDist;
    
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDist;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshHold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist/chunkSize);

        updateVisibleChunks();
    }
    void  Update(){
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrMoveThreshHoldForChunkUpdate){
            viewerPositionOld = viewerPosition;
            updateVisibleChunks();
        }
    }

    void updateVisibleChunks(){

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++){
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();


        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x/chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y/chunkSize);


        for (int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++){
            for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++){
                Vector2 viewChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if(terrainChunkDictionary.ContainsKey(viewChunkCoord)){
                    //Exists/generated. Update
                    terrainChunkDictionary[viewChunkCoord].UpdateTerrainChunk();
                } else{
                    //make new terrrain chunk
                    terrainChunkDictionary.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }


    public class TerrainChunk {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollidor;

        LOODInfo[] detailLevels;
        LODMesh[] lODMeshes;

        MapData mapData;
        bool mapDataRecieved;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LOODInfo[] detailLevels, Transform parent, Material material){
            this.detailLevels = detailLevels;

            
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollidor = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            //meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);


            lODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataRecieved);
        }

        void OnMapDataRecieved( MapData mapData){
            this.mapData = mapData;
            this.mapDataRecieved = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk(){
            if (mapDataRecieved){
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDist;

                if(visible){
                    int lodIndex = 0;
                    for (int i = 0; i < lODMeshes.Length - 1; i++){
                        if(viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshHold){
                            lodIndex = i + 1;
                        }
                        else{break;}
                    }

                    if(lodIndex != previousLODIndex){
                        LODMesh lodMesh = lODMeshes[lodIndex];
                        if(lodMesh.hasMesh){
                            meshFilter.mesh = lodMesh.mesh;
                            meshCollidor.sharedMesh = lodMesh.mesh;
                            previousLODIndex = lodIndex;
                        }
                        else if (!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }
                
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible){
            meshObject.SetActive (visible);
        }

        public bool isVisible(){
            return meshObject.activeSelf;
        }

    }

    class LODMesh {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData){
            mesh = meshData.CreateMesh();
            hasMesh = true;
            
            updateCallback();
        }

        public void RequestMesh(MapData mapData){
            hasRequestedMesh=true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }


    [System.Serializable]
    public struct LOODInfo {
        public int lod;
        public float visibleDistanceThreshHold;
    }
}
