using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings

public class PrimordialParticlesScript : ComputeShaderScript
{
    readonly ComputeBuffer[] particleBuffers = new ComputeBuffer[2];
    ComputeKernel resetAgentsKernel = new ComputeKernel("ResetAgentsKernel");
    ComputeKernel moveAgentsKernel = new ComputeKernel("MoveAgentsKernel");

    [SerializeField, Range(.0f, 6.28318f)]
    float constantAngle;
    [SerializeField, Range(.0f, 6.28318f)]
    float neighborsAngle;
    [SerializeField, Range(.01f, 50.0f)]
    float radius;

    [SerializeField]
    float agentSpeed;

    [SerializeField]
    int domainSize;

    [SerializeField, Range(NUMTHREAD_AGENTS, 10000)]
    int agentsCount;

    [SerializeField]
    ParticleRendering rendering;

    const int NUMTHREAD_AGENTS = 32;

    public int toDispatchAgents { get { return agentsCount / NUMTHREAD_AGENTS; } }

    [NaughtyAttributes.Button]
    protected override void ResetState()
    {
        for(int i = 0; i < 2; i++)
        {
            particleBuffers[i] = new ComputeBuffer(agentsCount, sizeof(float) * 3 + sizeof(int) * 1);
        }
        resetAgentsKernel.Find(computeShader);
        moveAgentsKernel.Find(computeShader);

        computeShader.SetInt("randomSeed", Random.Range(0, 10000));
        computeShader.SetBuffer(resetAgentsKernel.value, "nextAgents", particleBuffers[0]);
        computeShader.SetInt("domainSize", domainSize);
        computeShader.Dispatch(resetAgentsKernel.value, toDispatchAgents, 1, 1);
    }

    protected override void Step()
    {
        {
            computeShader.SetInt("domainSize", domainSize);
            computeShader.SetInt("agentsCount", agentsCount);
            computeShader.SetFloat("agentSpeed", agentSpeed);
            computeShader.SetFloat("neighborhoodRadius", radius);
            computeShader.SetFloat("neighborsAngle", neighborsAngle);
            computeShader.SetFloat("constantAngle", constantAngle);
            computeShader.SetBuffer(moveAgentsKernel.value, "agents", particleBuffers[0]);
            computeShader.SetBuffer(moveAgentsKernel.value, "nextAgents", particleBuffers[1]);
            computeShader.Dispatch(moveAgentsKernel.value, toDispatchAgents, 1, 1);
        }

        SwapBuffers();
    }

    protected override void Render()
    {
        outMaterial.SetBuffer("agentsBuffer", particleBuffers[0]);
        rendering.Render(outMaterial, domainSize, agentsCount);
    }

    void SwapBuffers() 
    {
        (particleBuffers[0], particleBuffers[1]) = (particleBuffers[1], particleBuffers[0]);
    }

    protected override void ReleaseResources()
    {
        foreach(var buffer in particleBuffers) 
        {
            buffer?.Release();
        }
    }
}
