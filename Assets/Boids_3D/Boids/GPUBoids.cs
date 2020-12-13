using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#pragma warning disable CS0649 //disable Serializefield private warnings



public class GPUBoids : MonoBehaviour
{
    [Header("Agents")]

    [Range(NUMTHREADS_AGENTS, 1000000), OnValueChanged(nameof(Reset))]
    public int agentCount = 1;
    private ComputeBuffer agentsBuffer;
    private ComputeBuffer nextVelocitiesBuffer;

    [SerializeField, Range(.01f, .2f)]
    float maxSpeed = .1f;

    [Header("Appearance")]
    
    [SerializeField]
    Color brightColor = Color.white;

    [SerializeField, Range(.01f, 1.0f)]
    float trailDecayRate;

    [SerializeField, Range(.01f, 1.0f)]
    float simulationSpeed = .1f;

    [SerializeField, Range(1, 5)]
    int boidSize = 3;

    [Header("Separation")]

    [SerializeField, Range(.001f, .2f)]
    float separationRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float separationWeight = .5f;

    [Header("Cohesion")]

    [SerializeField, Range(.001f, .2f)]
    float cohesionRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float cohesionWeight = .5f;

    [Header("Alignment")]

    [SerializeField, Range(.001f, .2f)]
    float alignmentRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float alignmentWeight = .5f;

    [EnableIf("useImageInReset")]
    public Texture2D resetImage = null;
    public bool useImageInReset = false;

    [Header("Setup")]

    [SerializeField, Range(8, 2048)]
    int resolution = 8;

    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Material outMaterial;

    RenderTexture outTexture;

    List<ComputeBuffer> buffers = new List<ComputeBuffer>();
    List<RenderTexture> textures = new List<RenderTexture>();
    
    int moveKernel;
    int renderKernel;
    int velocityKernel;
    int clearKernel;

    int steps;

    const float TIME_OFFSET = 2.2312f;
    const int NUMTHREADS_AGENTS = 64;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Second);
        Reset();
    }

    float accTime = .0f;
    void Update()
    {
        accTime += Time.deltaTime;
        if (accTime > 1.0f/60.0f)
        {
            Step();
            accTime -= 1.0f / 60.0f;
        }
    }

    [Button]
    void Reset()
    {
        steps = 0;

        moveKernel = computeShader.FindKernel("MoveAgentsKernel");
        velocityKernel = computeShader.FindKernel("VelocityKernel");
        renderKernel = computeShader.FindKernel("RenderKernel");
        clearKernel = computeShader.FindKernel("ClearKernel");

        outTexture = CSUtilities.CreateRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        textures.Add(outTexture);

        agentsBuffer = new ComputeBuffer(agentCount, sizeof(float) * 4);
        buffers.Add(agentsBuffer);

        nextVelocitiesBuffer = new ComputeBuffer(agentCount, sizeof(float) * 2);
        buffers.Add(nextVelocitiesBuffer);

        DispatchResetKernel();
        Render();
    }

    void Step()
    {
        steps++;

        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);

        DispatchDirectionKernel();
        DispatchMoveKernel();

        
        Render();
    }

    void Render()
    {
        DispatchRenderKernel();
        outMaterial.SetTexture("_UnlitColorMap", outTexture);
        DispatchClearKernel();
    }

    void DispatchRenderKernel()
    {
        computeShader.SetBuffer(renderKernel, "agents", agentsBuffer);
        computeShader.SetTexture(renderKernel, "outTexture", outTexture);
        computeShader.SetInt("boidSize", boidSize);
        computeShader.SetVector("brightColor", brightColor);

        computeShader.Dispatch(renderKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }   

    void DispatchClearKernel()
    {
        computeShader.SetTexture(clearKernel, "outTexture", outTexture);
        computeShader.SetFloat("trailDecayRate", trailDecayRate);

        computeShader.Dispatch(clearKernel, resolution, resolution, 1);
    }

    void DispatchDirectionKernel()
    {
        computeShader.SetBuffer(velocityKernel, "agents", agentsBuffer);
        computeShader.SetBuffer(velocityKernel, "nextVelocities", nextVelocitiesBuffer);

        computeShader.SetInt("agentCount", agentCount);
        computeShader.SetFloat("speed", simulationSpeed);
        computeShader.SetFloat("maxSpeed", maxSpeed);

        computeShader.SetFloat("separationRadius", separationRadius * resolution/2.0f);
        computeShader.SetFloat("separationWeight", separationWeight);

        computeShader.SetFloat("cohesionRadius", cohesionRadius * resolution / 2.0f);
        computeShader.SetFloat("cohesionWeight", cohesionWeight);

        computeShader.SetFloat("alignmentRadius", alignmentRadius * resolution / 2.0f);
        computeShader.SetFloat("alignmentWeight", alignmentWeight);

        computeShader.Dispatch(velocityKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void DispatchMoveKernel()
    {
        computeShader.SetBuffer(moveKernel, "agents", agentsBuffer);
        computeShader.SetBuffer(moveKernel, "nextVelocities", nextVelocitiesBuffer);


        computeShader.Dispatch(moveKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void DispatchResetKernel()
    {
        computeShader.SetInt("resolution", resolution);
        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);

        int resetAgentsKernel = computeShader.FindKernel("ResetAgentsKernel");
        computeShader.SetBool("randomReset", resetImage != null ? !useImageInReset : true);
        if (resetImage != null)
            computeShader.SetTexture(resetAgentsKernel, "resetTexture", resetImage);

        computeShader.SetBuffer(resetAgentsKernel, "agents", agentsBuffer);
        computeShader.SetBuffer(resetAgentsKernel, "nextVelocities", nextVelocitiesBuffer);
        computeShader.Dispatch(resetAgentsKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    public void ReleaseResources()
    {
        foreach (ComputeBuffer buffer in buffers)
            buffer.Release();

        foreach (RenderTexture texture in textures)
            texture.Release();

        buffers = new List<ComputeBuffer>();
        textures = new List<RenderTexture>();
    }

    private void OnDestroy() => ReleaseResources();
    private void OnEnable() => ReleaseResources();
    private void OnDisable() => ReleaseResources();
}
