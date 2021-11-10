using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649

public class DifferentialLineScript : ComputeShaderScript
{
    [ComputeKernel]
    int updateKernel;
    [ComputeKernel]
    int renderKernel;
    [ComputeKernel]
    int resetKernel;
    [ComputeKernel]
    int resetTextureKernel;
    [ComputeKernel]
    int appendKernel;

    const int NUMTHREADS_POINTS = 32;
    const int NUMTHREADS_RESOLUTION = 32;

    int ToDispatchResolution { get { return resolution / NUMTHREADS_RESOLUTION; } }
    int ToDispatchPoints { get { return nodeCount / NUMTHREADS_POINTS + 1; } }

    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    int maxCount;
    [SerializeField, ComputeVariable]
    float initialRadius;
    [SerializeField, ComputeVariable]
    int initialCount;

    [SerializeField, ComputeVariable]
    float repulsionRadius;
    [SerializeField, ComputeVariable]
    float minNeighborDist;

    [SerializeField, ComputeVariable]
    float movementSpeed;

    [ComputeVariable]
    float deltaTime;

    [SerializeField]
    float newNodesPerSecond;
    float newNodesCounter;

    [ComputeVariable(nameof(updateKernel)),
     ComputeVariable(nameof(resetKernel)),
     ComputeVariable(nameof(appendKernel))]
    ComputeBuffer writeBuffer;
    
    [ComputeVariable(nameof(updateKernel)), 
     ComputeVariable(nameof(renderKernel)), 
     ComputeVariable(nameof(appendKernel), variableName: "writeBuffer2"), 
     ComputeVariable(nameof(resetKernel))]
    ComputeBuffer readBuffer;

    [ComputeVariable(nameof(updateKernel), frequency: UpdateFrequency.OnStart),
     ComputeVariable(nameof(renderKernel), frequency: UpdateFrequency.OnStart),
     ComputeVariable(nameof(resetTextureKernel), frequency: UpdateFrequency.OnStart)]
    RenderTexture outTexture;

    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    int resolution;

    [SerializeField, Range(1, 10), ComputeVariable]
    int nodeSize = 3;

    [SerializeField]
    bool clearTexture;
    [SerializeField, ComputeVariable]
    float fadeCoefficient;

    [ComputeVariable(frequency: UpdateFrequency.OnStart)]
    readonly float aspectRatio = 16f / 9f;
    
    [ComputeVariable]
    int nodeCount = 0;

    protected override void ResetState()
    {
        nodeCount = initialCount;

        computeShader.Dispatch(resetKernel, ToDispatchPoints, 1, 1);

        SwapBuffers();
        
        computeShader.SetBuffer(GetKernel(nameof(resetKernel)), nameof(readBuffer), readBuffer);
        computeShader.SetBuffer(GetKernel(nameof(resetKernel)), nameof(writeBuffer), writeBuffer);
        computeShader.Dispatch(resetKernel, ToDispatchPoints, 1, 1);
    }

    protected override void SetupResources()
    {
        const int elementSize = sizeof(float) * 2 + sizeof(uint) * 2;
        writeBuffer = new ComputeBuffer(maxCount, elementSize);
        readBuffer = new ComputeBuffer(maxCount, elementSize);
        outTexture = CSUtilities.CreateRenderTexture((int)(resolution*aspectRatio), resolution, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
    }

    protected override void Step()
    {
        deltaTime = Time.deltaTime * simulationSpeed;
        newNodesCounter += deltaTime;
        if (clearTexture)
        {
            computeShader.Dispatch(resetTextureKernel, (int)(ToDispatchResolution*aspectRatio), ToDispatchResolution, 1);
        }
        int toDispatchPoints = ToDispatchPoints;
        computeShader.SetInt("step", steps);
        computeShader.Dispatch(updateKernel, toDispatchPoints, 1, 1);
        computeShader.Dispatch(renderKernel, toDispatchPoints, 1, 1);
        outMaterial.SetTexture("_MainTex", outTexture);

        if (newNodesCounter >= 1.0f && nodeCount != maxCount)
        {
            int flooredCounter = (int)newNodesCounter;
            int nodesToAdd = Mathf.Min(flooredCounter, maxCount - nodeCount);
            if (nodesToAdd > 0)
            {
                computeShader.SetInt("nodesToAppend", nodesToAdd);
                computeShader.Dispatch(appendKernel, 1, 1, 1);
                nodeCount += nodesToAdd;
            }
            newNodesCounter -= flooredCounter;
        }

        SwapBuffers();
    }

    void SwapBuffers() 
    {
        (readBuffer, writeBuffer) = (writeBuffer, readBuffer);
    }

    protected override void ReleaseResources()
    {
        readBuffer?.Dispose();
        writeBuffer?.Dispose();
    }
}
