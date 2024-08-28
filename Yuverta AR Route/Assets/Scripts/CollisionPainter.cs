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

    private Paintable p;
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
        p = other.collider.GetComponent<Paintable>();
        if (p != null)
        {
            p.OnHit(paintColors.GetColor(paintColorIndex), paintColorIndex);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (p == null)
            return;

        var pos = other.contacts[0].point;
        PaintManager.instance.Paint(p, pos, paintColors, radius, hardness, strength, paintColorIndex);
    }

    private void OnCollisionExit(Collision other)
    {
        if (p == null)
            return;
        
        var coverage = GetCoverage(p);

        p.coverage = coverage;

        var coverageIndex = p.CheckCoverage();

        // Set the previously filled color index to allow overlay
        if (coverageIndex == -1)
            return;

        Debug.Log("Coverage Index: " + coverageIndex);

        p.SetPreviouslyFilledColorIndex(coverageIndex);
        Paintable.SetMaskToColor(p, paintColors.GetColor(coverageIndex));

        p = null;
    }


    private static Vector4 GetCoverage(Paintable p)
    {
        var returnValue = Vector4.zero;
        PaintManager.instance.CalculateCoverage(p, p.uvMin, p.uvMax, coveredPixels =>
        {
            // Handle the result here
            returnValue = coveredPixels;
            Debug.Log($"Coverage calculated: {coveredPixels}");
        });
        return returnValue;
    }

    private void OnValidate()
    {
        if (paintColors == null)
            return;

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