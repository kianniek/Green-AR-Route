using System;
using UnityEngine;

// this script requires a collider to work
[RequireComponent(typeof(Collider))]
public class CollisionPainter : MonoBehaviour
{
    public Color[] paintColors;
    public int paintColorIndex;
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    private void Awake()
    {
        if (paintColors.Length == 0)
        {
            // Add Default Colors
            paintColors = new Color[4];
            paintColors[0] = Color.red;
            paintColors[1] = Color.green;
            paintColors[2] = Color.blue;
            paintColors[3] = Color.white;
        }
    }
    

    private void OnCollisionEnter(Collision other)
    {
        Paintable p = other.collider.GetComponent<Paintable>();
        if (p != null)
        {
            p.OnHit(paintColors[paintColorIndex]);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        var p = other.collider.GetComponent<Paintable>();

        if (p == null) return;
        var pos = other.contacts[0].point;
        PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColors, paintColorIndex);

        var coverage = GetCoverage(p);
        Debug.Log($"Coverage: {coverage * 100} %");

        p.coverage = coverage;

        var coverageIndex = p.CheckCoverage();

        // Set the previously filled color index to allow overlay
        if (coverageIndex != -1)
        {
            p.SetPreviouslyFilledColorIndex(coverageIndex);
            Paintable.SetMaskToColor(p, paintColors[coverageIndex]);
            p.OnCovered.Invoke();
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
        else if (paintColorIndex >= paintColors.Length)
        {
            paintColorIndex = paintColors.Length - 1;
        }
    }
}