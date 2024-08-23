using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInAnimation : MonoBehaviour
{
    public List<SkinnedMeshRenderer> MR = new List<SkinnedMeshRenderer>();
    public List<Material> materials = new List<Material>();
    public string valueName = "_DissolveValue";

    // Start is called before the first frame update
    void Awake()
    {
        MR = new List<SkinnedMeshRenderer>();

        foreach (Transform child in transform)
        {
            MR.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }

        foreach (var smr in MR)
        {
            materials.AddRange(smr.materials);
        }

        StartCoroutine(SpawnIn());
    }

    public void StartRemoval()
    {
        //stop all coroutines
        StopAllCoroutines();

        StartCoroutine(spawnOut());
    }

    private IEnumerator SpawnIn()
    {
        float value = 1;
        while (value > 0)
        {
            value -= Time.deltaTime;
            SetFloat(value);
            yield return null;
        }
    }

    private IEnumerator spawnOut()
    {
        float value = 0;
        while (value < 1)
        {
            value += Time.deltaTime;
            SetFloat(value);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void SetFloat(float value)
    {
        foreach (var material in materials)
        {
            if (material)
            {
                material.SetFloat(valueName, value);
            }
        }
    }
}