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
    }

    private void OnEnable()
    {
        StartCoroutine(SetMaskColor());
    }

    private void OnDisable()
    {
        StopCoroutine(SetMaskColor());
    }

    private IEnumerator SetMaskColor()
    {
        yield return new WaitForSeconds(minutes * 60);
        foreach (var p in paintables)
        {
            yield return new WaitForEndOfFrame();
            Paintable.SetMaskToColor(p, paintColors.colors[paintColorIndexToSet], paintColorIndexToSet);
            p.OnCovered.Invoke(paintColorIndexToSet);
        }
    }
}