using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings


public class ReactionDiffusion3DScript : ComputeShaderScript
{
    int stepKernel;
    int renderKernel;
    int resetKernel;

    RenderTexture readTexture;
    RenderTexture writeTexture;
    RenderTexture renderTexture;

    [Header("Reaction-Diffusion variables")]
    [SerializeField]
    float firstDiffusionRate = .8f;
    [SerializeField]
    float growthRate = .04f;
    [SerializeField]
    float growthVariation = .010f;

    [SerializeField]
    float secondDiffusionRate = .4f;
    [SerializeField]
    float deathRate = .06f;
    [SerializeField]
    float deathVariation = .001f;
    [SerializeField]
    float maxAmount = 1.0f;
    [SerializeField]
    float speed = 10.0f;
    [SerializeField]
    float startRadius = .5f;

    [SerializeField]
    CullingPlane plane;
    [SerializeField]
    ColorPalette palette;
    public enum LaplacianType 
    {
        TwentySevenPoint,
        SevenPoint
    }

    [SerializeField]
    LaplacianType laplacianType;

    [SerializeField]
    int resolution = 256;

    int frameCount = 0;

    const int NUMTHREADS_RESOLUTION = 8;
    public int toDispatch { get { return resolution / NUMTHREADS_RESOLUTION; } }


    protected override void SetupResources()
    {
        stepKernel = computeShader.FindKernel("StepKernel");
        renderKernel = computeShader.FindKernel("RenderKernel");
        resetKernel = computeShader.FindKernel("ResetKernel");

        readTexture = CSUtilities.Create3DRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        writeTexture = CSUtilities.Create3DRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        renderTexture = CSUtilities.Create3DRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.ARGBFloat);
    }

    [NaughtyAttributes.Button("Reset")]
    protected override void ResetState()
    {
        computeShader.SetFloat("startRadius", startRadius);
        computeShader.SetInt("resolution", resolution);
        DispatchResetKernels();
    }

    void DispatchResetKernels() 
    {
        computeShader.SetTexture(resetKernel, "writeTexture", writeTexture); 
        DispatchKernel(resetKernel);

        computeShader.SetTexture(resetKernel, "writeTexture", readTexture); //this is normal, we're resetting
        DispatchKernel(resetKernel);
    }

    protected override void Step()
    {
        frameCount++;

        computeShader.SetFloat("firstDiffusionRate", firstDiffusionRate);
        computeShader.SetFloat("growthRate", growthRate);
        computeShader.SetFloat("growthVariation", growthVariation);
        computeShader.SetFloat("secondDiffusionRate", secondDiffusionRate);
        computeShader.SetFloat("deathRate", deathRate);
        computeShader.SetFloat("deathVariation", deathVariation);
        computeShader.SetFloat("maxAmount", maxAmount);
        computeShader.SetFloat("speed", speed);
        computeShader.SetInt("laplacianType", laplacianType == LaplacianType.TwentySevenPoint ? 0 : 1);
        {
            computeShader.SetTexture(stepKernel, "readTexture", readTexture);
            computeShader.SetTexture(stepKernel, "writeTexture", writeTexture);
            computeShader.SetFloat("deltaTime", Time.deltaTime);
            DispatchKernel(stepKernel);
        }

        {
            computeShader.SetTexture(renderKernel, "renderTexture", renderTexture);
            computeShader.SetTexture(renderKernel, "readTexture", frameCount % 2 == 0 ? readTexture : writeTexture);
            plane.UploadToComputeShader(computeShader);
            palette.UploadToComputeShader(computeShader);
            DispatchKernel(renderKernel);
        }

        SwapTextures();

        outMaterial.SetTexture("_MainTex", renderTexture);
    }
    
    void DispatchKernel(int k) 
    {
        computeShader.Dispatch(k, toDispatch, toDispatch, toDispatch);
    }

    void SwapTextures()
    {
        (readTexture, writeTexture) = (writeTexture, readTexture);
    }

    [NaughtyAttributes.Button("Randomize")]
    void RandomizeSettings() 
    {
        firstDiffusionRate = Random.value;
        secondDiffusionRate = Random.value;
        growthRate = Random.value;
        deathRate = Random.value;
    }

    [NaughtyAttributes.Button("Randomize and Reset")]
    void RandomizeAndReset()
    {
        RandomizeSettings();
        DispatchResetKernels();
    }
}
