using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class PaintManager : Singleton<PaintManager>
{
    public Shader texturePaint;
    public Shader extendIslands;
    public Shader zoomToBounds;

    public int coveredTreshold;
    
    [Tooltip("Event that fires when a threshold is reached for the amount of objects covered in the 2nd mask color")]
    public UnityEvent OnTresholdReached;
    public UnityEvent<float> OnTresholdStep = new();

    private Dictionary<Paintable, int> paintables = new();

    private static readonly int PrepareUVID = Shader.PropertyToID("_PrepareUV");
    private static readonly int PositionID = Shader.PropertyToID("_PainterPosition");
    private static readonly int HardnessID = Shader.PropertyToID("_Hardness");
    private static readonly int StrengthID = Shader.PropertyToID("_Strength");
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int BlendOpID = Shader.PropertyToID("_BlendOp");
    private static readonly int ColorID = Shader.PropertyToID("_PainterColor");
    private static readonly int TextureID = Shader.PropertyToID("_MainTex");
    private static readonly int UVOffsetID = Shader.PropertyToID("_OffsetUV");
    private static readonly int UVIslandsID = Shader.PropertyToID("_UVIslands");

    private Material paintMaterial;
    private Material extendMaterial;
    private Material zoomMaterial;

    private CommandBuffer command;

    private int coveredCount = 0;

    public bool HasReachedTreshold => coveredCount >= coveredTreshold;

    public override void Awake()
    {
        base.Awake();

        paintMaterial = new Material(texturePaint);
        extendMaterial = new Material(extendIslands);
        zoomMaterial = new Material(zoomToBounds);
        command = new CommandBuffer { name = "CommandBuffer - " + gameObject.name };
    }

    public void InitTextures(Paintable paintable)
    {
        var mask = paintable.getMask();
        var uvIslands = paintable.getUVIslands();
        var extend = paintable.getExtend();
        var support = paintable.getSupport();
        var rend = paintable.getRenderer();

        command.SetRenderTarget(mask);
        command.SetRenderTarget(extend);
        command.SetRenderTarget(support);
        paintMaterial.SetFloat(PrepareUVID, 1);
        command.DrawRenderer(rend, paintMaterial, 0);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
    }

    public void Paint(Paintable paintable, Vector3 pos, PaintColors colors, float radius = 1f, float hardness = .5f,
        float strength = .5f, int colorIndex = -1)
    {
        // Early exit if colorIndex is lower than previously filled color
        if (paintable.previouslyFilledColorIndex != -1 && paintable.previouslyFilledColorIndex > colorIndex)
        {
            return;
        }

        PerformPainting(paintable, pos, radius, hardness, strength, colorIndex > -1 ? colors.GetColor(colorIndex) : colors.GetColor(0));
    }

    public void Paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f,
        Color? color = null)
    {
        PerformPainting(paintable, pos, radius, hardness, strength, color ?? Color.white);
    }

    private void PerformPainting(Paintable paintable, Vector3 pos, float radius, float hardness, float strength, Color color)
    {
        var mask = paintable.getMask();
        var uvIslands = paintable.getUVIslands();
        var extend = paintable.getExtend();
        var support = paintable.getSupport();
        var rend = paintable.getRenderer();

        paintMaterial.SetFloat(PrepareUVID, 0);
        paintMaterial.SetVector(PositionID, pos);
        paintMaterial.SetFloat(HardnessID, hardness);
        paintMaterial.SetFloat(StrengthID, strength);
        paintMaterial.SetFloat(RadiusID, radius);
        paintMaterial.SetTexture(TextureID, support);
        paintMaterial.SetColor(ColorID, color);
        extendMaterial.SetFloat(UVOffsetID, paintable.extendsIslandOffset);
        extendMaterial.SetTexture(UVIslandsID, uvIslands);

        command.SetRenderTarget(mask);
        command.DrawRenderer(rend, paintMaterial, 0);

        command.SetRenderTarget(support);
        command.Blit(mask, support);

        command.SetRenderTarget(extend);
        command.Blit(mask, extend, extendMaterial);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();
    }

    public void CalculateCoverage(Paintable paintable, Vector2 uvBoundsMin, Vector2 uvBoundsMax, System.Action<Vector4> onCoverageCalculated)
    {
        var mask = paintable.getMask();
        var islands = paintable.getIslands();
        var rend = paintable.getRenderer();

        // Set the bounds on the material
        zoomMaterial.SetVector("_MinBound", new Vector4(uvBoundsMin.x, uvBoundsMin.y, 0, 0));
        zoomMaterial.SetVector("_MaxBound", new Vector4(uvBoundsMax.x, uvBoundsMax.y, 0, 0));

        // Perform the rendering
        command.SetRenderTarget(islands);
        command.DrawRenderer(rend, zoomMaterial, 0);
        command.Blit(mask, islands, zoomMaterial);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();

        // Only activate the render texture if we need to read from it
        var previousActiveRT = RenderTexture.active;
        RenderTexture.active = islands;

        // Initiate the asynchronous GPU readback
        AsyncGPUReadback.Request(islands, islands.mipmapCount - 1, request =>
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error");
            }
            else
            {
                // Extract average color at a lower mip level if possible
                var average = request.GetData<Color32>()[0];

                // Calculate how close the color is to red
                var coveredPixels = new Vector4(average.r / 255f, average.g / 255f, average.b / 255f, average.a / 255f);

                // Call the callback with the result
                onCoverageCalculated?.Invoke(coveredPixels);
            }

            // Restore the previous render texture
            RenderTexture.active = previousActiveRT;

            CheckIfStepThresholdIsReached();
        });
    }


    public void SetMaskToColor(Paintable paintable, Color color)
    {
        var mask = paintable.getMask();
        var support = paintable.getSupport();

        command.SetRenderTarget(mask);
        command.SetRenderTarget(support);
        command.ClearRenderTarget(true, true, color);

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();

        // Reset the coverage
        paintable.coverage = Vector4.zero;
    }

    public void AddToPaintablesList(Paintable paintable, int coverageID)
    {
        if (paintables.TryGetValue(paintable, out int currentValue))
        {
            if (currentValue < coverageID)
            {
                paintables[paintable] = coverageID;

                if (coverageID == 1 && currentValue != 1)
                {
                    coveredCount++;
                }
            }
        }
        else
        {
            paintables.Add(paintable, coverageID);

            if (coverageID == 1)
            {
                coveredCount++;
            }
        }

        if (HasReachedTreshold)
        {
            OnTresholdReached.Invoke();
        }
    }

    public void CheckIfStepThresholdIsReached()
    {
        if (paintables.Count == 0)
            return;

        var stepSize = 1f / paintables.Count;
        var step = Mathf.Clamp01(stepSize * coveredCount);

        OnTresholdStep.Invoke(step);
    }
}
