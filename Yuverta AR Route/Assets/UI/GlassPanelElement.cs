using UnityEngine;
using UnityEngine.UIElements;

public class GlassPanelElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<GlassPanelElement, UxmlTraits> { }

    private VisualElement imageElement;

    public GlassPanelElement()
    {
        // Create a VisualElement to hold the image
        imageElement = new VisualElement();
        imageElement.style.flexGrow = 1;
        this.Add(imageElement);
    }

    public void SetRenderTexture(RenderTexture renderTexture)
    {
        // Convert RenderTexture to Texture2D
        Texture2D texture2D = ToTexture2D(renderTexture);

        // Set the background image of the imageElement
        imageElement.style.backgroundImage = new StyleBackground(texture2D);
    }

    private Texture2D ToTexture2D(RenderTexture rTex)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = rTex;

        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentActiveRT;
        return tex;
    }
}