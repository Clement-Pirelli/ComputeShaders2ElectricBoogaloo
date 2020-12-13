using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#pragma warning disable CS0649 //disable Serializefield private warnings



public class SmoothLifeScript : ComputeShaderScript
{
    [Header("SmoothLife variables")]
    [SerializeField, OnValueChanged(nameof(UpdateAllSmoothLifeVariables)), Range(1.0f, 30.0f)]
    float innerRadius;

    [SerializeField, OnValueChanged(nameof(UpdateAllSmoothLifeVariables)), MinMaxSlider(.0f, 1.0f)]
    Vector2 deathThreshold;

    [SerializeField, OnValueChanged(nameof(UpdateAllSmoothLifeVariables)), MinMaxSlider(.0f, 1.0f)]
    Vector2 birthThreshold;

    [SerializeField, OnValueChanged(nameof(UpdateAllSmoothLifeVariables))]
    float alphaOuter;
    [SerializeField, OnValueChanged(nameof(UpdateAllSmoothLifeVariables))]
    float alphaInner;

    [SerializeField, Range(2, 100)]
    int colorAmount = 2;

    [SerializeField, Expandable]
    ColorPicker colorPicker;

    [SerializeField, OnValueChanged(nameof(UpdateAllSmoothLifeVariables))]
    Color[] colors = { Color.black, Color.white };

    [Header("Misc.")]

    [SerializeField, Range(8, 2048)]
    int resolution = 8;
    [SerializeField]
    bool resetOnUpdate = false;


    RenderTexture outTexture;

    RenderTexture readTexture;
    RenderTexture writeTexture;



    const int NUMTHREADS_STEP_KERNEL = 32;
    int stepKernel;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Second);
        Reset();
    }

    [Header("Randomization")]
    [SerializeField, EnumFlags]
    RandomizationFlags randomizationFlags;
    public enum RandomizationFlags
    {
        InnerRadius = 1,
        Color = 1 << 1
    };

    [Button]
    void PickColors() 
    {
        colors = colorPicker.pickAmount(colorAmount);
        UpdateAllSmoothLifeVariables();
    }

    [SerializeField]
    bool useImage;
    [SerializeField]
    Texture2D image;

    [Button]
    override protected void Reset()
    {
        readTexture = CreateRenderTexture(RenderTextureFormat.RFloat);
        writeTexture = CreateRenderTexture(RenderTextureFormat.RFloat);
        outTexture = CreateRenderTexture(RenderTextureFormat.ARGBFloat);

        stepKernel = computeShader.FindKernel("StepKernel");

        DispatchResetKernel();
    }

    [Button]
    void UpdateAllSmoothLifeVariables()
    {
        computeShader.SetFloat("innerRadius", innerRadius);
        computeShader.SetVector("deathThreshold", new Vector4(deathThreshold.x, deathThreshold.y, .0f, .0f));
        computeShader.SetVector("birthThreshold", new Vector4(birthThreshold.x, birthThreshold.y, .0f, .0f));
        computeShader.SetFloat("alphaInner", alphaInner);
        computeShader.SetFloat("alphaOuter", alphaOuter);
        computeShader.SetInt("colorAmount", colorAmount);
        computeShader.SetVectorArray("colors", colors.asVector4s());

        if (resetOnUpdate) Reset();
    }
    
    override protected void Step()
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
        if(image != null) computeShader.SetTexture(resetKernel, "resetTexture", image);
        computeShader.Dispatch(resetKernel, resolution, resolution, 1);

        if(!resetOnUpdate) UpdateAllSmoothLifeVariables();

        SwapTextures();
    }

    void SwapTextures()
    {
        (readTexture, writeTexture) = (writeTexture, readTexture);
    }
}
