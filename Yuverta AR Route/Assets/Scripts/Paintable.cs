using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using UnityEngine.TestTools;

public class Paintable : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;
    
    public bool isBuilding = false;

    public float extendsIslandOffset = 1;

    public bool useHitTreshold = false;
    public int hitTreshold = 1;
    private int hitCount = 0;

    [Range(0, 1)] public float coverageThreshold = 0.5f;
    [SerializeField] private Vector4 _coverage = new(0, 0, 0, -1);

    private int coverageIndex = -1;
    private int previousCoverageIndex = -1;

    public int CoverageIndex
    {
        get => coverageIndex;
        private set => coverageIndex = value;
    }

    public int PreviousCoverageIndex {
        get => previousCoverageIndex;
        private set => previousCoverageIndex = value;
    }


    public Vector4 coverage
    {
        get => _coverage;
        set => _coverage = value;
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

        _coverage = new Vector4(0, 0, 0, -1);

        CoverageIndex = -1;
        PreviousCoverageIndex = -1;
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
        PaintManager.instance.AddToPaintablesList(this, -1);
    }

    public int CheckCoverage()
    {
        const float PrevColorCoverageFactor = 1.3f;
        const float PrevColorComboCoverageFactor = 2f;

        //// Check the highest index (z-axis)
        //if ((coverage.z > coverageThreshold && CoverageIndex < 2) || 
        //    (coverage.y > coverageThreshold / PrevColorCoverageFactor && coverage.z > coverageThreshold / PrevColorComboCoverageFactor && CoverageIndex < 1))
        //{
        //    // Only allow upward progression, prevent downgrading
        //    if (CoverageIndex < 2)
        //    {
        //        PaintManager.instance.AddToPaintablesList(this, 2);
        //        OnCovered.Invoke(2);
        //        PreviousCoverageIndex = 1;
        //        CoverageIndex = 2;
        //        SetMaskToColor(this, Color.blue, 2);
        //        return 2; // Color index for z
        //    }
        //}

        // Check the middle index (y-axis)
        if ((coverage.y > coverageThreshold && CoverageIndex < 1) || 
            (coverage.x > coverageThreshold / PrevColorCoverageFactor && coverage.y > coverageThreshold / PrevColorComboCoverageFactor && CoverageIndex < 0))
        {
            // Only allow upward progression, prevent downgrading
            if (CoverageIndex < 1)
            {
                PaintManager.instance.AddToPaintablesList(this, 1);
                OnCovered.Invoke(1);
                PreviousCoverageIndex = 0;
                CoverageIndex = 1;
                SetMaskToColor(this, Color.green, 1);
                return 1; // Color index for y
            }
        }

        // Check the lowest index (x-axis)
        if (coverage.x > coverageThreshold && CoverageIndex < 0)
        {
            // Only allow upward progression, prevent downgrading
            if (CoverageIndex < 0)
            {
                PaintManager.instance.AddToPaintablesList(this, 0);
                OnCovered.Invoke(0);
                PreviousCoverageIndex = -1;
                CoverageIndex = 0;
                coverage = new Vector4(coverage.x, coverage.y, coverage.z, CoverageIndex);
                SetMaskToColor(this, Color.red, 0);
                return 0; // Color index for x
            }
        }

        // No new color meets the threshold, return the current index if not already -1
        if (CoverageIndex != -1)
        {
            Debug.Log("No new color meets the threshold");
            return CoverageIndex;
        }

        // Default case: reset if nothing meets the threshold
        PaintManager.instance.AddToPaintablesList(this, CoverageIndex);
        Debug.Log("No color meets the threshold");
        return CoverageIndex; // No color meets the threshold
    }


    public static void SetMaskToColor(Paintable p, Color color, int coverageID = -1)
    {
        PaintManager.instance.SetMaskToColor(p, color, coverageID);
    }

    private void CalculateUVBounds()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh.uv.Length == 0)
        {
            Debug.LogError("MeshFilter or UV data is missing", gameObject);
            return;
        }

        var uvs = meshFilter.mesh.uv;
        uvMin = new Vector2(float.MaxValue, float.MaxValue);
        uvMax = new Vector2(float.MinValue, float.MinValue);

        foreach (var uv in uvs)
        {
            uvMin = new Vector2(Mathf.Min(uvMin.x, uv.x), Mathf.Min(uvMin.y, uv.y));
            uvMax = new Vector2(Mathf.Max(uvMax.x, uv.x), Mathf.Max(uvMax.y, uv.y));
        }
    }


    public void OnHit(Color color, int index)
    {
        if (!useHitTreshold || _coverage.w >= index)
            return;

        hitCount++;
        
        if (hitCount < hitTreshold) 
            return;
        // Store the current coverage index before evaluation
        int currentCoverageIndex = CoverageIndex;

        // Only allow upward progression by one, prevent downgrading

        if (currentCoverageIndex + 1 != index)
            return;

        PaintManager.instance.AddToPaintablesList(this, index);
        PreviousCoverageIndex = index - 1;
        CoverageIndex = index;
        OnCovered.Invoke(index);
        SetMaskToColor(this, color, index);
    }

    private void OnDisable()
    {
        maskRenderTexture?.Release();
        islandsRenderTexture?.Release();
        uvIslandsRenderTexture?.Release();
        extendIslandsRenderTexture?.Release();
        supportTexture?.Release();

        maskRenderTexture = null;
        islandsRenderTexture = null;
        uvIslandsRenderTexture = null;
        extendIslandsRenderTexture = null;
        supportTexture = null;
    }

}