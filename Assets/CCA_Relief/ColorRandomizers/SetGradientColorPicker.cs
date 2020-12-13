using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings

[CreateAssetMenu(fileName = "SetGradientColorPicker", menuName = "ColorPickers/Set Gradient")]
class SetGradientColorPicker : ColorPicker
{
    [SerializeField]
    Color first;
    [SerializeField]
    Color last;

    public override Color[] pickAmount(int amount)
    {
        var colors = new Color[amount];

        for (var i = 0; i < amount; i++)
        {
            colors[i] = Color.Lerp(first, last, i / (float)amount);
        }

        return colors;
    }
}