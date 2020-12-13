using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MyExtensions
{
    public static Vector4[] asVector4s(this Color[] colors)
    {
        return colors.Select(c => new Vector4(c.r, c.g, c.b, c.a)).ToArray();
    }
}

public static class CSUtilities
{
    public static RenderTexture CreateRenderTexture(int resolution, FilterMode filterMode, RenderTextureFormat format)
    {
        RenderTexture texture = new RenderTexture(resolution, resolution, 1, format)
        {
            enableRandomWrite = true,
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Repeat,
            useMipMap = false,
            name = "out",
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            wrapModeU = TextureWrapMode.Repeat,
            wrapModeV = TextureWrapMode.Repeat,
            volumeDepth = 1,
            autoGenerateMips = false
        };

        texture.Create();

        return texture;
    }

    public static RenderTexture Create3DRenderTexture(int resolution, FilterMode filterMode, RenderTextureFormat format)
    {
        RenderTexture texture = new RenderTexture(resolution, resolution, 0, format)
        {
            enableRandomWrite = true,
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Repeat,
            useMipMap = false,
            name = "out",
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            autoGenerateMips = false,
            volumeDepth = resolution,
        };

        texture.Create();

        return texture;
    }
}