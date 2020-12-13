using UnityEngine;
using NaughtyAttributes;

public abstract class ColorPicker : ScriptableObject
{
    [SerializeField, MinMaxSlider(.01f, 1.0f)]
    protected Vector2 hueRange = new Vector2(.01f, 1.0f);

    [SerializeField, MinMaxSlider(.01f, 1.0f)]
    protected Vector2 saturationRange = new Vector2(.01f, 1.0f);

    [SerializeField, MinMaxSlider(.01f, 1.0f)]
    protected Vector2 valueRange = new Vector2(.01f, 1.0f);

    protected Color RandomColor
    {
        get
        {
            return Random.ColorHSV(
                hueRange.x,
                hueRange.y,
                saturationRange.x,
                saturationRange.y,
                valueRange.x,
                valueRange.y);
        }
    }

    public abstract Color[] pickAmount(int amount);
}
