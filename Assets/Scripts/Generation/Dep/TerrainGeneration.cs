using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using System.Drawing;


public class TerrainGeneration : MonoBehaviour
{
 
    public bool perlinNoise = true;
    
    // Variables
    [Header("Dimensions")]
    public int width;
    public int height;

    public int depth;

    public float max;
    public float min;
    
    public float scale = 20f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    // Map Variable
    // private float[,] heightMap;

    void Start()
    {
        offsetX = UnityEngine.Random.Range(0f, 9999f);
        offsetY = UnityEngine.Random.Range(0f, 9999f);
        
        Terrain terrain =  GetComponent<Terrain>();

        terrain.transform.position = new Vector3(-width/2, 0, -height/2);
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    void Update() {
        
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        
        terrainData.size = new Vector3 (width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    // Perlin noise generator
     float CalculateHeight (int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
