using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
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

    enum StartShape 
    {
        Line = 0,
        Circle = 1
    }

    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    StartShape startShape;

    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    float initialRadius;
    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    Vector4 lineStartAndEnd;
    [SerializeField, ComputeVariable]
    bool pinEnds;

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

    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    int maxCount;
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

    [ComputeVariable(frequency: UpdateFrequency.OnStart)]
    readonly float aspectRatio = 16f / 9f;

    public Vector2Int outputDimensions => new Vector2Int(150, 100) * 10;
    
    [ComputeVariable]
    int nodeCount = 0;

    [SerializeField]
    bool clearTexture;
    [SerializeField, ComputeVariable]
    float fadeCoefficient;
    [SerializeField, Range(1, 20), ComputeVariable]
    int pointSize = 3;
    [SerializeField, ComputeVariable]
    Color pointColor;
    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart)]
    int resolution;

    public Vector2Int textureSize => new Vector2Int(outTexture.width, outTexture.height);

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

    [StructLayout(LayoutKind.Sequential)]
    public struct DifferentialNode
    {
        public Vector2 position;
        public int previous;
        public int next;
    };

    public DifferentialNode[] DownloadNodes() 
    {
        var result = new DifferentialNode[nodeCount];

        readBuffer.GetData(result);

        for (int i = 0; i < result.Length; i++) 
        {
            result[i].position = result[i].position * 10.0f;
        }

        return result;
    }
}
