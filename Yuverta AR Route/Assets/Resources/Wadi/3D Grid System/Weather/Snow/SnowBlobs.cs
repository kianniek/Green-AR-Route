using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowSpawner : MonoBehaviour
{
    public GameObject snowBlobParent;
    public Shader shader;
    public ObjectPool snowBlobPool;
    public int textureResolution = 512;
    public float worldSize = 10.0f;

    public RenderTexture renderTexture;
    public Texture2D snowMask;
    private List<GameObject> activeSnowBlobs = new List<GameObject>();

    public GameObject targetObject;  // The object on which snow blobs will be spread
    private Renderer targetRenderer;
    private Bounds targetBounds;
    private bool shouldUpdateSnowMask = false;

    void Start()
    {
        targetRenderer = targetObject.GetComponent<Renderer>();
        targetBounds = targetRenderer.bounds;

        snowMask = ConvertRenderTextureToTexture2D(renderTexture); 
        textureResolution = snowMask.width;

        shouldUpdateSnowMask = true;
    }

    void Update()
    {
        if (ShouldUpdateSnow())
        {
            ClearSnowBlobs();
            shouldUpdateSnowMask = true;
        }
    }

    void OnRenderObject()
    {
        if (shouldUpdateSnowMask)
        {
            RenderSnowMask();
            StartCoroutine(SpawnSnowBlobsCoroutine());
            shouldUpdateSnowMask = false;
        }
    }

    void RenderSnowMask()
    {
        RenderTexture.active = renderTexture;
        Graphics.Blit(null, renderTexture, new Material(shader));
        snowMask = ConvertRenderTextureToTexture2D(renderTexture); 
        snowMask.ReadPixels(new Rect(0, 0, textureResolution, textureResolution), 0, 0);
        snowMask.Apply();
        RenderTexture.active = null;
    }

    private IEnumerator SpawnSnowBlobsCoroutine()
    {
        Color[] pixels = snowMask.GetPixels();
        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                if (pixels.Length > y * textureResolution + x && pixels[y * textureResolution + x].r > 0.9f)
                {
                    Vector3 spawnPosition = GetWorldPositionFromTextureCoordinate(x, y);
                    var blob = snowBlobPool.GetObject(spawnPosition);
                    activeSnowBlobs.Add(blob);

                    yield return null;
                }
            }
        }
    }

    void ClearSnowBlobs()
    {
        foreach (var blob in activeSnowBlobs)
        {
            snowBlobPool.ReturnObject(blob);
        }
        activeSnowBlobs.Clear();
    }

    bool ShouldUpdateSnow()
    {
        // Add logic to determine when to update snow spots
        return true;
    }
    
    Texture2D ConvertRenderTextureToTexture2D(RenderTexture rt)
    {
        // Set the active RenderTexture
        RenderTexture.active = rt;

        // Create a new Texture2D with the same dimensions as the RenderTexture
        Texture2D texture2D = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);

        // Read the pixels from the RenderTexture into the Texture2D
        texture2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture2D.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = null;

        return texture2D;
    }

    Vector3 GetWorldPositionFromTextureCoordinate(int x, int y)
    {
        // Convert texture coordinates to UV coordinates
        float u = x / (float)textureResolution;
        float v = y / (float)textureResolution;

        // Calculate local position in the object's bounds
        Vector3 localPosition = new Vector3(u * targetBounds.size.x, 0, v * targetBounds.size.z);

        // Transform local position to world position
        Vector3 worldPosition = targetObject.transform.TransformPoint(localPosition - targetBounds.extents);

        return worldPosition;
    }
}