using System;
using UnityEngine;

// this script requires a collider to work
[RequireComponent(typeof(Collider))]
public class CollisionPainter : MonoBehaviour
{
    public PaintColors paintColors;
    public int paintColorIndex;
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    private void Awake()
    {
        if (paintColors == null)
        {
            // Add Default Colors
            Debug.LogWarning("Please assign a PaintColors scriptable object to the CollisionPainter script.");
        }
    }
    

    private void OnCollisionEnter(Collision other)
    {
        Paintable p = other.collider.GetComponent<Paintable>();
        if (p != null)
        {
            p.OnHit(paintColors.GetColor(paintColorIndex), paintColorIndex);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        var p = other.collider.GetComponent<Paintable>();

        if (p == null) return;
        var pos = other.contacts[0].point;
        PaintManager.instance.paint(p, pos,paintColors, radius, hardness, strength,  paintColorIndex);

        var coverage = GetCoverage(p);

        p.coverage = coverage;

        var coverageIndex = p.CheckCoverage();

        // Set the previously filled color index to allow overlay
        if (coverageIndex != -1)
        {
            p.SetPreviouslyFilledColorIndex(coverageIndex);
            Paintable.SetMaskToColor(p, paintColors.GetColor(coverageIndex));
        }
    }


    private static Vector4 GetCoverage(Paintable p)
    {
        return PaintManager.instance.CalculateCoverage(p, p.uvMin, p.uvMax);
    }

    private void OnValidate()
    {
        if (paintColorIndex < 0)
        {
            paintColorIndex = 0;
        }
        else if (paintColorIndex >= paintColors.GetColorCount())
        {
            paintColorIndex = paintColors.GetColorCount() - 1;
        }
    }
}