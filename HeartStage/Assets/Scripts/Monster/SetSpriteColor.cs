using UnityEngine;

public class SpriteColorGroup : MonoBehaviour
{
    public Color color = Color.gray;

    public void ApplyColor()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sp in sprites)
        {
            sp.color = color;
        }
    }

    private void Start()
    {
        ApplyColor();
    }
}