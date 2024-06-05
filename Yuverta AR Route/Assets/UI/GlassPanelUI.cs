using UnityEngine;
using UnityEngine.UIElements;

public class GlassPanelUI : MonoBehaviour
{
    public RenderTexture glassRenderTexture;
public VisualTreeAsset visualTree;
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;
        visualTree.CloneTree(rootVisualElement);

        // Find the GlassPanelElement and set its render texture
        var glassPanel = rootVisualElement.Q<GlassPanelElement>();
        if (glassPanel != null)
        {
            glassPanel.SetRenderTexture(glassRenderTexture);
        }
    }
}