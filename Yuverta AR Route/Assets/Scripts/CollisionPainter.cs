using UnityEngine;

// this script requires a collider to work
[RequireComponent(typeof(Collider))]

public class CollisionPainter : MonoBehaviour{
    public Color paintColor;
    
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;


    private void OnCollisionStay(Collision other) {
        Paintable p = other.collider.GetComponent<Paintable>();
        if (p != null) {
            Vector3 pos = other.contacts[0].point;
            PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColor);
            
            var coverage = GetCoverage(p);
            Debug.Log($"Coverage: {GetCoverage(p) * 100} %");
            
            p.coverage = coverage;

            if (p.CheckCoverage())
            {
                p.SetMaskToColor(p, paintColor);
            }
        }
    }
    
    public float GetCoverage(Paintable p)
    {
        return PaintManager.instance.CalculateCoverage(p, p.uvMin, p.uvMax);
    }
}
