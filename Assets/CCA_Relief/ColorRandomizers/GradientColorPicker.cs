using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GradientColorPicker", menuName = "ColorPickers/Gradient")]
class GradientColorPicker : ColorPicker
{

    public override Color[] pickAmount(int amount)
    {
        Color first = RandomColor;
        Color last = RandomColor;
        var colors = new Color[amount];

        for (var i = 0; i < amount; i++)
        {
            colors[i] = Color.Lerp(first, last, i /(float)amount);
        }

        return colors;
    }
}