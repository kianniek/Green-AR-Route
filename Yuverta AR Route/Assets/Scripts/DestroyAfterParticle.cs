using UnityEngine;

[ExecuteAlways]
public class DestroyAfterParticle : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Check if the particle system is done playing
        if (particleSystem != null && !particleSystem.IsAlive())
        {
#if UNITY_EDITOR
            gameObject.SetActive(false);
#else
            Destroy(gameObject);
#endif
        }
    }
}