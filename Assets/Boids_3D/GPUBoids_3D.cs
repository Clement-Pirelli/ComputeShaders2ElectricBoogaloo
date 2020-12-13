using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#pragma warning disable CS0649 //disable Serializefield private warnings

public class GPUBoids_3D : MonoBehaviour
{
    [Header("Agents")]

    [Range(NUMTHREADS_AGENTS, 1000000), OnValueChanged(nameof(Reset))]
    public int agentCount = NUMTHREADS_AGENTS;
    private ComputeBuffer agentsBuffer;
    private ComputeBuffer nextAgentsBuffer;

    [SerializeField, Range(.01f, .2f)]
    float maxSpeed = .1f;


    enum EdgeBehaviour 
    {
        wrap = 0,
        clamp = 1,
        none = 2
    }
    [SerializeField]
    EdgeBehaviour edgeBehaviour = EdgeBehaviour.wrap;

    [Header("Appearance")]

    [SerializeField, Range(.01f, 1.0f)]
    float simulationSpeed = .1f;

    [Header("Forces")]

    [SerializeField, Range(.0f, 1.0f)]
    float lineOfSight = 1.0f;

    [Space()]

    [SerializeField, Range(.001f, .2f)]
    float separationRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float separationWeight = .5f;

    [Space()]

    [SerializeField, Range(.001f, .2f)]
    float cohesionRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float cohesionWeight = .5f;

    [Space()]

    [SerializeField, Range(.001f, .2f)]
    float alignmentRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float alignmentWeight = .5f;
    
    [Space()]
    [SerializeField, Range(.0f, 1.0f)]
    float wanderingWeight = .5f;

    [Header("Setup")]

    [SerializeField, Range(8, 2048)]
    int boundsSize = 8;

    [SerializeField]
    ComputeShader computeShader;
    
    List<ComputeBuffer> buffers = new List<ComputeBuffer>();
    
    int velocityKernel;
    
    int steps;

    const float TIME_OFFSET = 2.2312f;
    const int NUMTHREADS_AGENTS = 64;

    [SerializeField]
    TexInstancer_3D instancer;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Second);
        Reset();
    }

    float accTime = .0f;
    void Update()
    {
        accTime += Time.deltaTime;
        if (accTime > 1.0f / 30.0f)
        {
            Step();
            accTime -= 1.0f / 30.0f;
        }

        Render();
    }

    [Button]
    void Reset()
    {
        steps = 0;

        velocityKernel = computeShader.FindKernel("VelocityKernel");

        agentsBuffer = new ComputeBuffer(agentCount, sizeof(float) * 6);
        buffers.Add(agentsBuffer);


        nextAgentsBuffer = new ComputeBuffer(agentCount, sizeof(float) * 6);
        buffers.Add(nextAgentsBuffer);

        DispatchResetKernel();
    }

    void Step()
    {
        SwapBuffers();
        steps++;

        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);

        DispatchDirectionKernel();
    }

    void Render()
    {
        instancer.agentsBuffer = nextAgentsBuffer;
        instancer.maxSpeed = maxSpeed;
        instancer.Render(boundsSize, agentCount);
    }

    void DispatchDirectionKernel()
    {
        computeShader.SetBuffer(velocityKernel, "agents", agentsBuffer);
        computeShader.SetBuffer(velocityKernel, "nextAgents", nextAgentsBuffer);

        computeShader.SetInt("agentCount", agentCount);
        computeShader.SetFloat("speed", simulationSpeed);
        computeShader.SetFloat("maxSpeed", maxSpeed);

        computeShader.SetFloat("lineOfSight", lineOfSight);

        computeShader.SetFloat("separationRadius", separationRadius * boundsSize / 2.0f);
        computeShader.SetFloat("separationWeight", separationWeight);

        computeShader.SetFloat("cohesionRadius", cohesionRadius * boundsSize / 2.0f);
        computeShader.SetFloat("cohesionWeight", cohesionWeight);

        computeShader.SetFloat("alignmentRadius", alignmentRadius * boundsSize / 2.0f);
        computeShader.SetFloat("alignmentWeight", alignmentWeight);

        computeShader.SetFloat("wanderingWeight", wanderingWeight);

        computeShader.SetInt("edgeType", (int)edgeBehaviour);

        computeShader.Dispatch(velocityKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void DispatchResetKernel()
    {
        computeShader.SetInt("resolution", boundsSize);
        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);

        int resetAgentsKernel = computeShader.FindKernel("ResetAgentsKernel");

        computeShader.SetBuffer(resetAgentsKernel, "agents", agentsBuffer);
        computeShader.SetBuffer(resetAgentsKernel, "nextAgents", nextAgentsBuffer);
        computeShader.Dispatch(resetAgentsKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    

    void SwapBuffers() 
    {
        (agentsBuffer, nextAgentsBuffer) = (nextAgentsBuffer, agentsBuffer);
    }
    public void ReleaseResources()
    {
        instancer.agentsBuffer = null;
        foreach (ComputeBuffer buffer in buffers)
            buffer.Release();

        buffers = new List<ComputeBuffer>();
    }
    private void OnDestroy() => ReleaseResources();
    private void OnEnable() => ReleaseResources();
    private void OnDisable() => ReleaseResources();
}
