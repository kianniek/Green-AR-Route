using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class SplatMakerExample : MonoBehaviour
{
    Vector4 channelMask = new Vector4(1, 0, 0, 0);

    int splatsX = 1;
    int splatsY = 1;

    public float splatScale = 1.0f;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Get how many splats are in the splat atlas
        splatsX = SplatManagerSystem.instance.splatsX;
        splatsY = SplatManagerSystem.instance.splatsY;

        // Cast a ray from the camera to the mouse pointer and draw a splat there.
        // This just picks a random scale and bias for a 4x4 splat atlas
        // You could use a larger atlas of splat textures and pick a scale and offset for the specific splat you want to use

        if (Input.GetKeyDown(KeyCode.P))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 10000))
            {
                Debug.Log(hit.collider.gameObject.name);
                OnHit(hit);
            }
        }
    }

    public void OnHit(RaycastHit hit)
    {
        OnHit(hit.normal, hit.point);
    }
    
    public void OnHit(Vector3 normal, Vector3 point)
    {
        var leftVec = Vector3.Cross(normal, Vector3.up);
        var randScale = Random.Range(0.5f, 1.5f);

        var newSplatObject = new GameObject
        {
            transform =
            {
                position = point
            }
        };

        if (leftVec.magnitude > 0.001f)
        {
            newSplatObject.transform.rotation = Quaternion.LookRotation(leftVec, normal);
        }

        newSplatObject.transform.RotateAround(point, normal, Random.Range(-180, 180));
        newSplatObject.transform.localScale = new Vector3(randScale, randScale * 0.5f, randScale) * splatScale;

        Splat newSplat;
        newSplat.splatMatrix = newSplatObject.transform.worldToLocalMatrix;
        newSplat.channelMask = channelMask;

        var splatscaleX = 1.0f / splatsX;
        var splatscaleY = 1.0f / splatsY;
        var splatsBiasX = Mathf.Floor(Random.Range(0, splatsX * 0.99f)) / splatsX;
        var splatsBiasY = Mathf.Floor(Random.Range(0, splatsY * 0.99f)) / splatsY;

        newSplat.scaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY);

        SplatManagerSystem.instance.AddSplat(newSplat);

        GameObject.Destroy(newSplatObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        OnHit(other.contacts[0].normal, other.contacts[0].point);
    }
}