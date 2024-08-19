using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PaintColors", menuName = "ScriptableObjects/PaintColors", order = 1)]
public class PaintColors : ScriptableObject
{
    public Color[] colors = new[] { Color.red, Color.blue, Color.green, Color.white };
    
    public Color GetColor(int index)
    {
        return colors[index];
    }
    
    public bool IsColorInPalette(Color color)
    {
        return colors.Any(c => c == color);
    }
    
    public int GetColorIndex(Color color)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == color)
            {
                return i;
            }
        }

        return -1;
    }
    
    public Color[] ReadColors()
    {
        return colors;
    }
    
    public int GetColorCount()
    {
        return colors.Length;
    }
}