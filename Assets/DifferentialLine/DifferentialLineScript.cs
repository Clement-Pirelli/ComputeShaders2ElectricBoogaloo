﻿using System.Collections;
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

    const int NUMTHREADS_POINTS = 32;
    const int NUMTHREADS_RESOLUTION = 32;

    ComputeBuffer countBuffer;
    int lastStepQueried = -1;
    int cachedCounter = 0;
    int ToDispatchPoints { get 
        {
            //cache the data, it will most likely change every frame so ask again then
            if(lastStepQueried != steps)
            {
                lastStepQueried = steps;
                ComputeBuffer.CopyCount(readBuffer, countBuffer, 0);
                int[] counter = new int[] { 0 };
                countBuffer.GetData(counter);
                cachedCounter = counter[0];
            }
            return cachedCounter / NUMTHREADS_POINTS + 1;
        } 
    }

    int ToDispatchResolution { get { return resolution / NUMTHREADS_RESOLUTION; } }

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

    [ComputeVariable(nameof(updateKernel), variableName: "writeBuffer"), 
     ComputeVariable(nameof(resetKernel), variableName: "appendBuffer")]
    ComputeBuffer writeBuffer;
    
    [ComputeVariable(nameof(updateKernel)), 
     ComputeVariable(nameof(renderKernel)), 
     ComputeVariable(nameof(resetKernel))]
    ComputeBuffer readBuffer;

    [ComputeVariable(nameof(updateKernel), frequency: UpdateFrequency.OnStart),
     ComputeVariable(nameof(renderKernel), frequency: UpdateFrequency.OnStart),
     ComputeVariable(nameof(resetTextureKernel), frequency: UpdateFrequency.OnStart)]
    RenderTexture outTexture;

    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    int resolution;

    [SerializeField]
    bool clearTexture;

    protected override void ResetState()
    {
        computeShader.Dispatch(resetKernel, initialCount / NUMTHREADS_POINTS, 1, 1);

        SwapBuffers();
        
        computeShader.SetBuffer(GetKernel(nameof(resetKernel)), nameof(readBuffer), readBuffer);
        computeShader.SetBuffer(GetKernel(nameof(resetKernel)), nameof(writeBuffer), writeBuffer);
        computeShader.Dispatch(resetKernel, initialCount / NUMTHREADS_POINTS, 1, 1);
    }

    protected override void SetupResources()
    {
        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

        const int elementSize = sizeof(float) * 2 + sizeof(uint) * 2;
        writeBuffer = new ComputeBuffer(maxCount, elementSize, ComputeBufferType.Append);
        writeBuffer.SetCounterValue(0);
        readBuffer = new ComputeBuffer(maxCount, elementSize, ComputeBufferType.Append);
        writeBuffer.SetCounterValue(0);
        outTexture = CSUtilities.CreateRenderTexture(resolution, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
    }

    protected override void Step()
    {
        //computeShader.Dispatch(updateKernel, ToDispatchPoints, 1, 1);
        if (clearTexture)
        {
            computeShader.Dispatch(resetTextureKernel, ToDispatchResolution, ToDispatchResolution, 1);
        }
        computeShader.Dispatch(renderKernel, ToDispatchPoints, 1, 1);
        outMaterial.SetTexture("_MainTex", outTexture);
        //SwapBuffers();
    }

    void SwapBuffers() 
    {
        (readBuffer, writeBuffer) = (writeBuffer, readBuffer);
    }

    protected override void ReleaseResources()
    {
        readBuffer?.Dispose();
        writeBuffer?.Dispose();
        countBuffer?.Dispose();
    }
}