using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaskColorAfterMinutes : MonoBehaviour
{
    public float minutes = 3f;
    public int paintColorIndexToSet;
    public PaintColors paintColors;

    private Paintable[] paintables;

    private void Start()
    {
        paintables = FindObjectsOfType<Paintable>();
        StartCoroutine(SetMaskColor());
    }

    private IEnumerator SetMaskColor()
    {
        yield return new WaitForSeconds(minutes * 60);
        foreach (var p in paintables)
        {
            PaintManager.instance.SetMaskToColor(p, paintColors.colors[paintColorIndexToSet]);
            p.OnCovered.Invoke(paintColorIndexToSet);
        }
    }
}