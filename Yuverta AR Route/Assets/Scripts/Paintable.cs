using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

public class Paintable : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;

    public float extendsIslandOffset = 1;

    public bool useHitTreshold = false;
    public int hitTreshold = 1;
    private int hitCount = 0;

    [Range(0, 1)] public float coverageThreshold = 0.5f;
    [SerializeField] private Vector4 _coverage;
    public int previouslyFilledColorIndex { get; set; } = -1;

    public Vector4 coverage
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

    [Space(10f)] public UnityEvent<int> OnCovered = new();

    RenderTexture extendIslandsRenderTexture;
    RenderTexture uvIslandsRenderTexture;
    RenderTexture islandsRenderTexture;
    RenderTexture maskRenderTexture;
    RenderTexture supportTexture;

    Renderer rend;

    int MaskTextureID = Shader.PropertyToID("_MaskTexture");
    int UVIslandID = Shader.PropertyToID("_UVTexture");
    int Extend = Shader.PropertyToID("_ExtendTexture");
    int Support = Shader.PropertyToID("_SupportTexture");

    public RenderTexture getMask() => maskRenderTexture;
    public RenderTexture getUVIslands() => uvIslandsRenderTexture;
    public RenderTexture getIslands() => islandsRenderTexture;
    public RenderTexture getExtend() => extendIslandsRenderTexture;
    public RenderTexture getSupport() => supportTexture;
    public Renderer getRenderer() => rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        CalculateUVBounds();

        maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0, RenderTextureFormat.ARGB32, 10)
        {
            name = "MaskTexture",
            filterMode = FilterMode.Bilinear,
            useMipMap = true,
            autoGenerateMips = true
        };

        extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0)
        {
            name = "ExtendIslandsTexture",
            filterMode = FilterMode.Bilinear
        };

        uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0)
        {
            name = "UVIslandsTexture",
            filterMode = FilterMode.Bilinear,
            useMipMap = true,
            autoGenerateMips = true
        };

        islandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0)
        {
            name = "islandsTexture",
            filterMode = FilterMode.Bilinear,
            useMipMap = true,
            autoGenerateMips = true
        };

        supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0)
        {
            name = "SupportTexture",
            filterMode = FilterMode.Bilinear
        };

        if (rend.materials.Length > 1)
        {
            foreach (var material in rend.materials)
            {
                material.SetTexture(MaskTextureID, extendIslandsRenderTexture);
                material.SetTexture(UVIslandID, islandsRenderTexture);
                material.SetTexture(Extend, extendIslandsRenderTexture);
                material.SetTexture(Support, supportTexture);
            }
        }
        else
        {
            rend.material.SetTexture(MaskTextureID, maskRenderTexture);
            rend.material.SetTexture(UVIslandID, islandsRenderTexture);
            rend.material.SetTexture(Extend, extendIslandsRenderTexture);
            rend.material.SetTexture(Support, supportTexture);
        }


        PaintManager.instance.InitTextures(this);
    }

    public int CheckCoverage()
    {
        if (coverage.z > coverageThreshold && previouslyFilledColorIndex != 2)
        {
            PaintManager.instance.AddToPaintablesList(this, 2);
            OnCovered.Invoke(2);
            return 2; // Color index for z
        }

        if (coverage.y > coverageThreshold && previouslyFilledColorIndex != 1)
        {
            PaintManager.instance.AddToPaintablesList(this, 1);
            OnCovered.Invoke(1);
            return 1; // Color index for y
        }

        if (coverage.x > coverageThreshold && previouslyFilledColorIndex != 0)
        {
            PaintManager.instance.AddToPaintablesList(this, 0);
            OnCovered.Invoke(0);
            return 0; // Color index for x
        }

        PaintManager.instance.AddToPaintablesList(this, -1);

        return -1; // No color meets the threshold
    }

    public void SetPreviouslyFilledColorIndex(int index)
    {
        previouslyFilledColorIndex = index;
    }


    public static void SetMaskToColor(Paintable p, Color color)
    {
        PaintManager.instance.SetMaskToColor(p, color);
    }

    private void CalculateUVBounds()
    {
        // Get the mesh filter component
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found!", gameObject);
            return;
        }

        // Get the mesh from the mesh filter
        var mesh = meshFilter.mesh;

        // Ensure the mesh has UVs
        if (mesh.uv.Length == 0)
        {
            Debug.LogError("Mesh does not have UV coordinates!", gameObject);
            return;
        }

        // Extract UV coordinates
        var uvs = mesh.uv;

        // Initialize min and max bounds
        var uvMin = new Vector2(float.MaxValue, float.MaxValue);
        var uvMax = new Vector2(float.MinValue, float.MinValue);

        // Iterate through UVs to find the bounds
        foreach (var uv in uvs)
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

    public void OnHit(Color color, int index)
    {
        if (!useHitTreshold || previouslyFilledColorIndex >= index)
            return;

        hitCount++;
        if (hitCount >= hitTreshold)
        {
            SetMaskToColor(this, color);
            OnCovered.Invoke(index);
        }
    }

    private void OnDisable()
    {
        maskRenderTexture?.Release();
        islandsRenderTexture?.Release();
        uvIslandsRenderTexture?.Release();
        extendIslandsRenderTexture?.Release();
        supportTexture?.Release();
    }
}