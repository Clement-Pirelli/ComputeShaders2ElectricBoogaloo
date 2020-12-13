using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WhiteNoiseColorPicker", menuName = "ColorPickers/White Noise")]
class WhiteNoiseColorPicker : ColorPicker
{
    public override Color[] pickAmount(int amount)
    {
        var colors = new Color[amount];

        for (var i = 0; i < amount; i++)
        {
            colors[i] = RandomColor;
        }

        return colors;
    }
}