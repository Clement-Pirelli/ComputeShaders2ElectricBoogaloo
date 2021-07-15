using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Color Palette")]
public class ColorPalette : ScriptableObject
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
    public Vector3 d;

    public void UploadToComputeShader(ComputeShader computeShader)
    {
        computeShader.SetVector("paletteA", a.asPosition());
        computeShader.SetVector("paletteB", b.asPosition());
        computeShader.SetVector("paletteC", c.asPosition());
        computeShader.SetVector("paletteD", d.asPosition());
    }
}
