using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class PaintManager : Singleton<PaintManager>
{
    public Shader texturePaint;
    public Shader extendIslands;
    public Shader zoomToBounds;

    int prepareUVID = Shader.PropertyToID("_PrepareUV");
    int positionID = Shader.PropertyToID("_PainterPosition");
    int hardnessID = Shader.PropertyToID("_Hardness");
    int strengthID = Shader.PropertyToID("_Strength");
    int radiusID = Shader.PropertyToID("_Radius");
    int blendOpID = Shader.PropertyToID("_BlendOp");
    int colorID = Shader.PropertyToID("_PainterColor");
    int textureID = Shader.PropertyToID("_MainTex");
    int uvOffsetID = Shader.PropertyToID("_OffsetUV");
    int uvIslandsID = Shader.PropertyToID("_UVIslands");

    Material paintMaterial;
    Material extendMaterial;
    Material zoomMaterial;

    CommandBuffer command;

    public override void Awake()
    {
        base.Awake();

        paintMaterial = new Material(texturePaint);
        extendMaterial = new Material(extendIslands);
        zoomMaterial = new Material(zoomToBounds);
        command = new CommandBuffer();
        command.name = "CommmandBuffer - " + gameObject.name;
    }

    public void initTextures(Paintable paintable)
    {
        var mask = paintable.getMask();
        var uvIslands = paintable.getUVIslands();
        var extend = paintable.getExtend();
        var support = paintable.getSupport();
        var rend = paintable.getRenderer();

        command.SetRenderTarget(mask);
        command.SetRenderTarget(extend);
        command.SetRenderTarget(support);

        paintMaterial.SetFloat(prepareUVID, 1);
        command.SetRenderTarget(uvIslands);
        command.DrawRenderer(rend, paintMaterial, 0);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
    }


    public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f,
        Color[]? colors = null, int colorIndex = -1)
    {
        //if paintable previously filled, check if the color is the same or of an lower index
        if (paintable.previouslyFilledColorIndex != -1)
        {
            if (colors.Length != 0)
            {
                if (paintable.previouslyFilledColorIndex > colorIndex)
                {
                    return;
                }
            }
        }
        
        
        var mask = paintable.getMask();
        var uvIslands = paintable.getUVIslands();
        var extend = paintable.getExtend();
        var support = paintable.getSupport();
        var rend = paintable.getRenderer();
    
        paintMaterial.SetFloat(prepareUVID, 0);
        paintMaterial.SetVector(positionID, pos);
        paintMaterial.SetFloat(hardnessID, hardness);
        paintMaterial.SetFloat(strengthID, strength);
        paintMaterial.SetFloat(radiusID, radius);
        paintMaterial.SetTexture(textureID, support);
        paintMaterial.SetColor(colorID, colorIndex > -1 ? colors[colorIndex] : colors[0]);
        extendMaterial.SetFloat(uvOffsetID, paintable.extendsIslandOffset);
        extendMaterial.SetTexture(uvIslandsID, uvIslands);

        command.SetRenderTarget(mask);
        command.DrawRenderer(rend, paintMaterial, 0);

        command.SetRenderTarget(support);
        command.Blit(mask, support);

        command.SetRenderTarget(extend);
        command.Blit(mask, extend, extendMaterial);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
    }
    
    public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f,
        Color? color = null)
    {
        var mask = paintable.getMask();
        var uvIslands = paintable.getUVIslands();
        var extend = paintable.getExtend();
        var support = paintable.getSupport();
        var rend = paintable.getRenderer();
    
        paintMaterial.SetFloat(prepareUVID, 0);
        paintMaterial.SetVector(positionID, pos);
        paintMaterial.SetFloat(hardnessID, hardness);
        paintMaterial.SetFloat(strengthID, strength);
        paintMaterial.SetFloat(radiusID, radius);
        paintMaterial.SetTexture(textureID, support);
        paintMaterial.SetColor(colorID, color ?? Color.white);
        extendMaterial.SetFloat(uvOffsetID, paintable.extendsIslandOffset);
        extendMaterial.SetTexture(uvIslandsID, uvIslands);

        command.SetRenderTarget(mask);
        command.DrawRenderer(rend, paintMaterial, 0);

        command.SetRenderTarget(support);
        command.Blit(mask, support);

        command.SetRenderTarget(extend);
        command.Blit(mask, extend, extendMaterial);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
    }


    public Vector4 CalculateCoverage(Paintable paintable, Vector2 uvBoundsMin, Vector2 uvBoundsMax)
    {
        var mask = paintable.getMask();
        var islands = paintable.getIslands();
        var rend = paintable.getRenderer();
        
        // Set the bounds on the material
        zoomMaterial.SetVector("_MinBound", new Vector4(uvBoundsMin.x, uvBoundsMin.y, 0, 0));
        zoomMaterial.SetVector("_MaxBound", new Vector4(uvBoundsMax.x, uvBoundsMax.y, 0, 0));
        
        command.SetRenderTarget(islands);
        command.DrawRenderer(rend, zoomMaterial, 0);
        command.Blit(mask, islands, zoomMaterial);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
        
        RenderTexture.active = islands;

        var asyncAction = AsyncGPUReadback.Request(islands, islands.mipmapCount - 1);
        asyncAction.WaitForCompletion();

        // Extract average color
        var Average = asyncAction.GetData<Color32>()[0];

        RenderTexture.active = null;

        //calculate how close the color is to red
        var coveredPixels = new Vector4(Average.r / 255f, Average.g / 255f, Average.b / 255f, Average.a / 255f); 

        return coveredPixels;
    }

    public void SetMaskToColor(Paintable paintable, Color color)
    {
        var mask = paintable.getMask();
        var support = paintable.getSupport();

        command.SetRenderTarget(mask);
        command.ClearRenderTarget(true, true, color);
        command.SetRenderTarget(support);
        command.ClearRenderTarget(true, true, color);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();

        // Reset the coverage
        paintable.coverage = Vector4.zero;
    }

}