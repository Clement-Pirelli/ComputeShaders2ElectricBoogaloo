using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings
#pragma warning disable CS0414 //disable Variable set but never used warnings due to ComputeVariable attribute

public class PrimordialParticlesScript : ComputeShaderScript
{
    [ComputeVariable(resetAgentsKernelName, UpdateFrequency.OnStart, "nextAgents")]
    [ComputeVariable(moveAgentsKernelName, variableName: "agents")]
    [ComputeVariable(renderKernelName, variableName: "agents")]
    ComputeBuffer firstParticleBuffer;
    
    [ComputeVariable(moveAgentsKernelName, variableName: "nextAgents")]
    ComputeBuffer secondParticleBuffer;

    const string resetAgentsKernelName = "ResetAgentsKernel";
    const string moveAgentsKernelName = "MoveAgentsKernel";
    const string renderKernelName = "RenderKernel";
    int resetAgentsKernel;
    int moveAgentsKernel;
    int renderKernel;


    [SerializeField, Range(.0f, 6.28318f), ComputeVariable]
    float constantAngle;
    [SerializeField, Range(.0f, 6.28318f), ComputeVariable]
    float neighborsAngle;
    [SerializeField, Range(.01f, 50.0f), ComputeVariable(variableName: "neighborhoodRadius")]
    float radius;

    [SerializeField, ComputeVariable]
    float agentSpeed;

    [SerializeField, ComputeVariable(frequency: UpdateFrequency.OnStart | UpdateFrequency.OnStep)]
    int domainSize;

    [SerializeField, Range(NUMTHREAD_AGENTS, 10000), ComputeVariable(frequency:UpdateFrequency.OnStart)]
    int agentsCount;

    [SerializeField, ComputeVariable]
    float agentRadius = 1.0f;
    [SerializeField, ComputeVariable]
    float neighborMax = 10.0f;

    [SerializeField, ComputeVariable]
    float smoothing = .1f;

    [SerializeField, ComputeVariable(variableName:"dim")]
    Color dimColor;
    [SerializeField, ComputeVariable(variableName:"bright")]
    Color brightColor;

    [SerializeField, ComputeVariable]
    int resolution = 1024;
    [ComputeVariable(renderKernelName)]
    RenderTexture renderTexture;

    const int NUMTHREAD_AGENTS = 32;
    const int NUMTHREAD_RESOLUTION = 32;
    public int toDispatchAgents { get { return agentsCount / NUMTHREAD_AGENTS; } }
    public int toDispatchTexture { get { return resolution / NUMTHREAD_RESOLUTION; } }

    protected override void SetupResources()
    {
        const int agentStructSize = sizeof(float) * 3 + sizeof(int) * 1;
        firstParticleBuffer = new ComputeBuffer(agentsCount, agentStructSize);
        secondParticleBuffer = new ComputeBuffer(agentsCount, agentStructSize);
        renderTexture = CSUtilities.CreateRenderTexture(resolution, FilterMode.Trilinear, RenderTextureFormat.ARGB32);

        resetAgentsKernel = computeShader.FindKernel(resetAgentsKernelName);
        moveAgentsKernel = computeShader.FindKernel(moveAgentsKernelName);
        renderKernel = computeShader.FindKernel(renderKernelName);
    }

    protected override void ResetState()
    {
        computeShader.SetInt("randomSeed", Random.Range(0, 10000));
        computeShader.Dispatch(resetAgentsKernel, toDispatchAgents, 1, 1);
    }

    protected override void Step()
    {
        {
            computeShader.Dispatch(moveAgentsKernel, toDispatchAgents, 1, 1);
        }

        SwapBuffers();

        computeShader.Dispatch(renderKernel, toDispatchTexture, toDispatchTexture, 1);

        outMaterial.SetTexture("_MainTex", renderTexture);
    }

    void SwapBuffers() 
    {
        (firstParticleBuffer, secondParticleBuffer) = (secondParticleBuffer, firstParticleBuffer);
    }

    protected override void ReleaseResources()
    {
        firstParticleBuffer?.Release();
        secondParticleBuffer?.Release();
    }
}
