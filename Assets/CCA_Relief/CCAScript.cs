using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#pragma warning disable CS0649 //disable Serializefield private warnings


[ExecuteAlways]
public class CCAScript : ComputeShaderScript
{
    [Header("CCA variables")]

    const int MAX_RANGE = 10;
    [SerializeField, Range(1, MAX_RANGE), EnableIf(nameof(NeighborhoodIsNotByHand)), OnValueChanged(nameof(UpdateAllCCAVariables))]
    int range;

    const int MAX_THRESHOLD = 10;
    [SerializeField, Range(1, MAX_THRESHOLD), OnValueChanged(nameof(UpdateAllCCAVariables))]
    int threshold = 3;

    const int MAX_STATES = 100;
    [SerializeField, Range(1, MAX_STATES), OnValueChanged(nameof(UpdateAllCCAVariables))]
    int statesAmount = 10;

    enum NeighborhoodType
    {
        Moore = 0,
        VonNeumann = 1,
        ByHand = 2,
        COUNT
    }
    [SerializeField, OnValueChanged(nameof(UpdateAllCCAVariables))]
    NeighborhoodType neighborhoodType;


    [SerializeField, Expandable]
    ColorPicker colorPicker;

    [SerializeField, OnValueChanged(nameof(UpdateAllCCAVariables))]
    Color[] colors;

    public bool NeighborhoodIsByHand { get { return neighborhoodType == NeighborhoodType.ByHand; } }
    public bool NeighborhoodIsNotByHand { get { return !NeighborhoodIsByHand; } }
    [SerializeField, EnableIf("NeighborhoodIsByHand"), Expandable, OnValueChanged(nameof(UpdateAllCCAVariables))]
    BoolGrid byHandNeighborhoodGrid;


    [Header("Setup")]

    [SerializeField, Range(8, 2048)]
    int resolution = 8;

    RenderTexture outTexture;

    RenderTexture readTexture;
    RenderTexture writeTexture;



    const int NUMTHREADS_STEP_KERNEL = 16;
    int stepKernel;

    [Header("Randomization")]
    [SerializeField, EnumFlags]
    RandomizationFlags randomizationFlags;
    public enum RandomizationFlags
    {
        Color = 1,
        Range = 1 << 1,
        Threshold = 1 << 2,
        States = 1 << 3,
        Neighborhood = 1 << 4, 
    };

    [SerializeField]
    bool useImage;
    [SerializeField]
    Texture2D image;

    [Button]
    void FullReset()
    {
        Randomize();
        PickColors();
        ResetState();
    }

    [Button]
    protected override void ResetState()
    {
        readTexture = CreateRenderTexture(RenderTextureFormat.RFloat);
        writeTexture = CreateRenderTexture(RenderTextureFormat.RFloat);
        outTexture = CreateRenderTexture(RenderTextureFormat.ARGBFloat);

        stepKernel = computeShader.FindKernel("StepKernel");

        DispatchResetKernel();
    }

    [Button]
    void Randomize()
    {
        if(randomizationFlags.HasFlag(RandomizationFlags.Range))
            range = Random.Range(2, MAX_RANGE+1);

        if (randomizationFlags.HasFlag(RandomizationFlags.Threshold))
            threshold = Random.Range(1, MAX_THRESHOLD+1);

        if (randomizationFlags.HasFlag(RandomizationFlags.States))
            statesAmount = Random.Range(1, MAX_STATES+1);

        if (randomizationFlags.HasFlag(RandomizationFlags.Neighborhood))
            neighborhoodType = (NeighborhoodType)Random.Range(0, (int)NeighborhoodType.COUNT);

        if(neighborhoodType == NeighborhoodType.ByHand)
        {
            range = byHandNeighborhoodGrid.range;
        }

        UpdateAllCCAVariables();
    }

    [Button]
    void PickColors(int startingPoint = 0)
    {
        if (startingPoint != 0 && randomizationFlags.HasFlag(RandomizationFlags.Color)) return;

        var newColors = colorPicker.pickAmount(statesAmount);
        for (int i = 0; i < startingPoint; i++)
        {
            newColors[i] = colors[i];
        }
        colors = newColors;
        computeShader.SetVectorArray("colors", colors.asVector4s());
    }

    [Button]
    void RandomizeAndReset()
    {
        Randomize();
        ResetState();
    }

    [Button]
    void UpdateAllCCAVariables()
    {
        computeShader.SetInt("statesAmount", statesAmount);
        computeShader.SetInt("range", range);
        computeShader.SetInt("threshold", threshold);

        if(statesAmount > colors.Length)
        {
            PickColors(colors.Length);
        }

        computeShader.SetVectorArray("colors", colors.asVector4s());
        computeShader.SetInt("neighborhoodType", (int)neighborhoodType);

        if(byHandNeighborhoodGrid != null) 
        {
            Vector4[] packedNeighborhood = byHandNeighborhoodGrid.packToVector4s();

            computeShader.SetVectorArray("byHandNeighborhood", packedNeighborhood);
        }
    }
    
    protected override void Step()
    {
        computeShader.SetTexture(stepKernel, "outTexture", outTexture);
        computeShader.SetTexture(stepKernel, "readTexture", readTexture);
        computeShader.SetTexture(stepKernel, "writeTexture", writeTexture);

        computeShader.Dispatch(stepKernel, resolution/NUMTHREADS_STEP_KERNEL, resolution/NUMTHREADS_STEP_KERNEL, 1);

        SwapTextures();

        outMaterial.SetTexture("_UnlitColorMap", outTexture);
    }

    RenderTexture CreateRenderTexture(RenderTextureFormat format)
    {
        RenderTexture texture = new RenderTexture(resolution, resolution, 1, format)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Repeat,
            useMipMap = false
        };

        texture.Create();

        return texture;
    }

    void DispatchResetKernel()
    {
        int resetKernel = computeShader.FindKernel("ResetKernel");
        computeShader.SetTexture(resetKernel, "writeTexture", writeTexture);
        computeShader.SetInt("resolution", resolution);
        computeShader.SetFloat("time", Time.time);

        computeShader.SetBool("randomReset", !useImage);
        if (image != null)
        {
            computeShader.SetTexture(resetKernel, "resetTexture", image);
        }
        computeShader.Dispatch(resetKernel, resolution, resolution, 1);

        UpdateAllCCAVariables();

        SwapTextures();
    }

    void SwapTextures()
    {
        (readTexture, writeTexture) = (writeTexture, readTexture);
    }
}
