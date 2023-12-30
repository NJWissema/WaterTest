using System.Collections;
using System.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]

public class CameraUnderwaterEffect : MonoBehaviour
{
    public LayerMask waterLayers;
    public Shader shader;

    [Header("Dpeth Effect")]
    public Color depthColour = new Color(0, 0.42f, 0.87f);
    public float depthStart = -12, depthEnd = 98;
    public LayerMask depthLayers = ~0; //All layers selected by default

    Camera cam, depthCam;
    RenderTexture depthTexture, colourTexture;
    Material material;
    bool inWater;


    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();

        // Make camera send depth information to shader
        // cam.depthTextureMode = DepthTextureMode.Depth;

        // Create a material using assigned shader
        if (shader) material = new Material(shader);

        // Create render textures for the camera to save the coour and depth information
        // prevent the camera from rendering onto the game scene
        depthTexture = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 16, RenderTextureFormat.Depth);
        colourTexture = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.Default);

        // Create depth camera and paretn it to main camera
        GameObject go = new GameObject("Depth Camera");
        depthCam = go.AddComponent<Camera>();
        go.transform.SetParent(transform);
        go.transform.position = transform.position;

        // Copy over main camera settings, but with a different culling mask and depthtexturemode.depth
        depthCam.CopyFrom(cam);
        depthCam.cullingMask = depthLayers;
        depthCam.depthTextureMode = DepthTextureMode.Depth;

        // Make depthcam use colorTexture and depthTexture and also disable depthcam
        depthCam.SetTargetBuffers(colourTexture.colorBuffer, depthTexture.depthBuffer);
        depthCam.enabled = false;

        // Send depth teexture to shader
        material.SetTexture("_DepthMap", depthTexture);
    }

    private void OnApplicationQuit()
    {
        RenderTexture.ReleaseTemporary(depthTexture);
        RenderTexture.ReleaseTemporary(colourTexture);
    }

    private void FixedUpdate()
    {
        // Collider[] c  = Physics.OverlapSphere(transform.position, 0.01f, waterLayers);
        // inWater = c.Length > 0;

        // Need to knwo exact height of water.
        // Get the camera frustrum of the near plane
        Vector3[] corners = new Vector3[4];

        cam.CalculateFrustumCorners(new Rect(0,0,1,1), cam.nearClipPlane, cam.stereoActiveEye, corners);

        // check where water layer is, without facotirng in rolling
        // check how far submerged we are into the water, using corner[0] and corner[1]
        // which are both the bottom left and top left corners respectively
        RaycastHit hit;
        Vector3 start = transform.position + transform.TransformVector(corners[1]), end = transform.position + transform.TransformVector(corners[0]);


        Collider[] c = Physics.OverlapSphere(end, 0.01f, waterLayers);
        if(c.Length > 0){
            inWater = true;

            c = Physics.OverlapSphere(start, 0.01f, waterLayers);
            // If c.length > 0, then the player is completely underwater.
            if(c.Length > 0){
                material.SetVector("_WaterLevel", new Vector2(0,1));
            }
            else{
                if(Physics.Linecast(start, end, out hit, waterLayers)){
                    // GGet inerpolation  value (delta) of the point the linecast hit
                    // the reverse of the lerp function gices us delta
                    float delta = hit.distance / (end-start).magnitude;

                    // Set the water level
                    // Use 1 - delta to get the reverse of the number
                    //  e.g if delta is 0.25, the water level will be 0.75
                    material.SetVector("_WaterLevel", new Vector2(0, 1-delta));
                }
            }
        }
        else{
            inWater = false;
        }
    }

    // Automaticallly finds and assignerd inspector variables so the script can be immediatly used when attatche d to game object
    private void Reset()
    {   

        Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
        foreach(Shader s in shaders){
            if (s.name.Contains(GetType().Name)) {
                shader = s;
                return;
            }
        }
    }

    // This is where the image effect is applied
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material && inWater){

            // Update the depth render texture
            depthCam.Render();

            // We pass the information to our material
            material.SetColor("_DepthColour", depthColour);
            material.SetFloat("_DepthStart", depthStart);
            material.SetFloat("_DepthEnd", depthEnd);

            // Apply to the image using blit;
            Graphics.Blit(src, dest, material);
        }
        else{
            Graphics.Blit(src, dest);
        }
    }
}
