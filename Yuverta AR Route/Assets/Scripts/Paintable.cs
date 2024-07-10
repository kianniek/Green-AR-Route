using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

public class Paintable : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;

    public float extendsIslandOffset = 1;

    private float _coverage;
    public float coverage
    {
        get => _coverage;
        set
        {
            _coverage = value;
            CheckCoverage();
        }
    }

    public float coverageThreshold = 0.5f;
    RenderTexture extendIslandsRenderTexture;
    RenderTexture uvIslandsRenderTexture;
    RenderTexture maskRenderTexture;
    RenderTexture supportTexture;

    Renderer rend;

    int maskTextureID = Shader.PropertyToID("_MaskTexture");

    public RenderTexture getMask() => maskRenderTexture;
    public RenderTexture getUVIslands() => uvIslandsRenderTexture;
    public RenderTexture getExtend() => extendIslandsRenderTexture;
    public RenderTexture getSupport() => supportTexture;
    public Renderer getRenderer() => rend;

    void Start()
    {
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

        uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        uvIslandsRenderTexture.name = "UVIslandsTexture";
        uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        supportTexture.name = "SupportTexture";
        supportTexture.filterMode = FilterMode.Bilinear;

        rend = GetComponent<Renderer>();

        if (rend.materials.Length > 1)
        {
            foreach (var material in rend.materials)
            {
                material.SetTexture(maskTextureID, maskRenderTexture);
            }
        }
        else
        {
            rend.material.SetTexture(maskTextureID, extendIslandsRenderTexture);
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

    void OnDisable()
    {
        maskRenderTexture.Release();
        uvIslandsRenderTexture.Release();
        extendIslandsRenderTexture.Release();
        supportTexture.Release();
    }
}