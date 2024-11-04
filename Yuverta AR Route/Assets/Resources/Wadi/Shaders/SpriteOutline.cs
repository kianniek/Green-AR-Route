using Kamgam.UGUIWorldImage;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour {
    public Color color = Color.white;

    [Range(0, 16)]
    public int outlineSize = 1;

    private WorldImage spriteRenderer;

    void OnEnable() {
        spriteRenderer = GetComponent<WorldImage>();

        UpdateOutline(true);
    }

    void OnDisable() {
        UpdateOutline(false);
    }

    void Update() {
        UpdateOutline(true);
    }

    void UpdateOutline(bool outline) {
        Material material = spriteRenderer.material;
        material.SetFloat("_Outline", outline ? 1f : 0);
        material.SetColor("_OutlineColor", color);
        material.SetFloat("_OutlineSize", outlineSize);
    }
}