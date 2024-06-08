using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class WeatherHandler : MonoBehaviour
{
    [Header("Logic")]
    //This is the layer that the rain and snow should collide with
    [SerializeField] private GameObject collisionLayer;

    public bool isSnowActive;
    public bool isRainActive;
    
    [Header("Snow")]
    public VisualEffect snow;
    //The snow can overlap with other materials so for that reason this is an array
    [SerializeField] private  Material[] snowMaterials;
    //This is the time it takes the snow to fall and fully update all shaders
    [SerializeField] private float timeUntilSnowUpdate;
    
    [Header("Rain")]
    public VisualEffect rain;

    [Header("Water")] 
    [SerializeField] private Material waterMaterial;
    
    //The water speeds are set here to make sure it is updated consistently
    [SerializeField] private Vector2 originalWaterSpeed;
    [SerializeField] private Vector2 originalWaveSpeed;
    
    [Header("Ice")]
    [SerializeField] private Material iceMaterial;
    
    //Below are all ID's for properties inside of shader
    private static readonly int SnowAmount = Shader.PropertyToID("_SnowAmount");
    private static readonly int WaterSpeed = Shader.PropertyToID("_DistortionSpeed");
    private static readonly int WaveSpeed = Shader.PropertyToID("_WaveSpeed");
    private static readonly int IceAmount = Shader.PropertyToID("_IceAmount");

    // Start is called before the first frame update
    private void Start()
    {
        Stop();

        //originalWaterSpeed = waterMaterial.GetVector(WaterSpeed);
        //originalWaveSpeed = waterMaterial.GetVector(WaveSpeed);
        
        foreach (var layer in snowMaterials)
        {
            layer.SetFloat(SnowAmount, 0.0f);
        }
        iceMaterial.SetFloat(IceAmount, 0.0f);
        
        waterMaterial.SetVector(WaterSpeed, originalWaterSpeed);
        waterMaterial.SetVector(WaveSpeed, originalWaveSpeed);
    }
    
    public void Stop()
    {
        snow.Stop();
        rain.Stop();
    }

    //All the variables that the snow shader and effect use are updated here
    private void UpdateSnowVariables()
    {
        collisionLayer.TryGetComponent<BoxCollider>(out var collider);

        if (collider != null)
        {
            snow.SetVector3("PlaneColliderSize", collider.size);
        }
        else snow.SetVector3("PlaneColliderSize", new Vector3(0, 1, 0));

        var distance = collisionLayer.transform.position.y - snow.gameObject.transform.position.y;
        
        snow.SetVector3("DissolvePosition", new Vector3(0, distance, 0));
    }

    //This coroutine handles the logic for how much snow is still on the ground or has to be dropped
    private IEnumerator IncreaseSnowHeight(float timeSpanInSeconds, bool reverse = false)
    {
        //If there is no snow start dropping snowflakes and reset the current snow amount
        if (snow.aliveParticleCount <= 0 && !reverse)
        {
            snow.Play();
            StartCoroutine(UpdateSnowShader(0));
            yield return new WaitForSeconds(timeUntilSnowUpdate);
        }
        //Else make sure the water is running again and stop snowflakes from falling
        else
        {
            waterMaterial.SetVector(WaterSpeed, originalWaterSpeed);
            waterMaterial.SetVector(WaveSpeed, originalWaveSpeed);
            snow.Stop();
            yield return new WaitForSeconds(timeUntilSnowUpdate);
        }
        
        var direction = reverse ? 1 : 0;
        var currentSnowAmount = snowMaterials[0].GetFloat(SnowAmount);
        
        //Since this function can also be used during a previous animation this line handles the possibility of clearing/adding snow halfway
        var duration = reverse ? timeSpanInSeconds - timeSpanInSeconds * currentSnowAmount : 0f;
        
        while (duration < timeSpanInSeconds)
        {
            duration += Time.deltaTime;
            var snowAmount = Mathf.Abs(duration / timeSpanInSeconds - direction);
            StartCoroutine(UpdateSnowShader(snowAmount));
            yield return null;
        }

        //This function is only called after all the ice has grown over the water.
        //Since the water is then invisible the movement is turned off here.
        if (!reverse)
        {
            waterMaterial.SetVector(WaterSpeed, Vector2.zero);
            waterMaterial.SetVector(WaveSpeed, Vector2.zero);
        }
    }

    //The snow and ice shader are updated here
    private IEnumerator UpdateSnowShader(float newSnowAmount)
    {
        foreach (var layer in snowMaterials) 
        { 
            layer.SetFloat(SnowAmount, newSnowAmount);
        }
        iceMaterial.SetFloat(IceAmount, newSnowAmount);
        
        yield return null;
    }

    //The rain is updated to collide with the given layer
    private void UpdateRainVariables()
    {
        collisionLayer.TryGetComponent<BoxCollider>(out var colliderBox);
        if (colliderBox != null)
        {
            rain.SetVector3("SizeRainCollisionLayer", colliderBox.size);
            rain.SetVector3("CollisionPlaneNormal", new Vector3(0, 1, 0));
            
            var distance = rain.gameObject.transform.position.y - collisionLayer.transform.position.y;
            rain.SetVector3("CollisionPlanePosition", new Vector3(0, distance, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isRainActive)
        {
            //If the particles are above 0 the rain has already been activated and should be stopped
            if (rain.aliveParticleCount > 0)
            {
                rain.Stop();
                isRainActive = false;
                return;
            }
            rain.Play();
            UpdateRainVariables();
            isRainActive = false;
        }

        if (isSnowActive)
        {
            //If the particles are above 0 the snow has already been activated and should be stopped
            if (snow.aliveParticleCount > 0)
            {
                StopAllCoroutines();
                StartCoroutine(IncreaseSnowHeight(12f, true));
                isSnowActive = false;
                return;
            }
            StopAllCoroutines();
            StartCoroutine(IncreaseSnowHeight(12f));
            UpdateSnowVariables();
            isSnowActive = false;
        }
            
    }
}
