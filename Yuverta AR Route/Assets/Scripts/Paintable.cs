using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

public class Paintable : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;

    public float extendsIslandOffset = 1;

    public float _coverage;
    public float coverage
    {
        get => _coverage;
        set
        {
            _coverage = value;
            CheckCoverage();
        }
    }
    
    public Vector2 uvMin;
    public Vector2 uvMax;

    [Range(0,1)]
    public float coverageThreshold = 0.5f;
    RenderTexture extendIslandsRenderTexture;
    RenderTexture uvIslandsRenderTexture;
    RenderTexture islandsRenderTexture;
    RenderTexture maskRenderTexture;
    RenderTexture supportTexture;

    Renderer rend;

    int maskTextureID = Shader.PropertyToID("_MaskTexture");
    int UVIslandID = Shader.PropertyToID("_UVTexture");
    int Extend = Shader.PropertyToID("_ExtendTexture");
    int Support = Shader.PropertyToID("_SupportTexture");

    public RenderTexture getMask() => maskRenderTexture;
    public RenderTexture getUVIslands() => uvIslandsRenderTexture;
    public RenderTexture getIslands() => islandsRenderTexture;
    public RenderTexture getExtend() => extendIslandsRenderTexture;
    public RenderTexture getSupport() => supportTexture;
    public Renderer getRenderer() => rend;

    void Start()
    {
        CalculateUVBounds();
        
        maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0, RenderTextureFormat.ARGB32, 10)
        {
            name = "MaskTexture",
            filterMode = FilterMode.Bilinear,
            useMipMap = true,
            autoGenerateMips = true
        };
        maskRenderTexture.name = "MaskTexture";
        maskRenderTexture.filterMode = FilterMode.Bilinear;

        extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        extendIslandsRenderTexture.name = "ExtendIslandsTexture";
        extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0){
            name = "UVIslandsTexture",
            filterMode = FilterMode.Bilinear,
            useMipMap = true,
            autoGenerateMips = true
        };
        
        islandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0){
            name = "islandsTexture",
            filterMode = FilterMode.Bilinear,
            useMipMap = true,
            autoGenerateMips = true
        };

        supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        supportTexture.name = "SupportTexture";
        supportTexture.filterMode = FilterMode.Bilinear;

        rend = GetComponent<Renderer>();

        if (rend.materials.Length > 1)
        {
            foreach (var material in rend.materials)
            {
                material.SetTexture(maskTextureID, extendIslandsRenderTexture);
                material.SetTexture(UVIslandID, islandsRenderTexture);
                material.SetTexture(Extend, extendIslandsRenderTexture);
                material.SetTexture(Support, supportTexture);
            }
        }
        else
        {
            rend.material.SetTexture(maskTextureID, extendIslandsRenderTexture);
            rend.material.SetTexture(maskTextureID, maskRenderTexture);
            rend.material.SetTexture(UVIslandID, islandsRenderTexture);
            rend.material.SetTexture(Extend, extendIslandsRenderTexture);
            rend.material.SetTexture(Support, supportTexture);
        }


        PaintManager.instance.initTextures(this);
    }

    public bool CheckCoverage()
    {
        if (_coverage > coverageThreshold)
        {
            Debug.Log("Painted");
            return true;
        }
        return false;
    }
    
    public void SetMaskToColor(Paintable p, Color color)
    {
        PaintManager.instance.SetMaskToColor(p, color);
    }
    
    void CalculateUVBounds()
    {
        // Get the mesh filter component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found!");
            return;
        }

        // Get the mesh from the mesh filter
        Mesh mesh = meshFilter.mesh;

        // Ensure the mesh has UVs
        if (mesh.uv.Length == 0)
        {
            Debug.LogError("Mesh does not have UV coordinates!");
            return;
        }

        // Extract UV coordinates
        Vector2[] uvs = mesh.uv;

        // Initialize min and max bounds
        Vector2 uvMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 uvMax = new Vector2(float.MinValue, float.MinValue);

        // Iterate through UVs to find the bounds
        foreach (Vector2 uv in uvs)
        {
            if (uv.x < uvMin.x) uvMin.x = uv.x;
            if (uv.y < uvMin.y) uvMin.y = uv.y;
            if (uv.x > uvMax.x) uvMax.x = uv.x;
            if (uv.y > uvMax.y) uvMax.y = uv.y;
        }

        // Store the bounds
        this.uvMin = uvMin;
        this.uvMax = uvMax;
    }

    void OnDisable()
    {
        maskRenderTexture.Release();
        uvIslandsRenderTexture.Release();
        extendIslandsRenderTexture.Release();
        supportTexture.Release();
    }
}