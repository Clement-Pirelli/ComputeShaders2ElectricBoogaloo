using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings



public class Physarium3DScript : ComputeShaderScript
{
    [Header("Physarium variables")]

    [Range(NUMTHREADS_AGENTS, 100000)]
    public int agentCount = 1;
    private ComputeBuffer agentBuffer;
    private ComputeBuffer randStateBuffer;
    [Range(.0f, 1.0f)]
    public float trailDecayFactor = .9f;
    [Range(1, MAX_RANGE)]
    public int range = 1;
    [Range(.0f, 1.0f)]
    public float trailDetectionThreshold = .1f;
    [Range(.0f, .2f)]
    public float trailApproximationBias = .001f;
    [Range(.0f, 1.0f)]
    public float trailStrengthPerAgent = .3f;

    [Range(0.0f, 1.0f)]
    public float lineOfSight;
    [Range(.1f, 2.0f)]
    public float agentSpeed = 1.0f;

    public bool decayTrails = true;

    public bool usingSphere = false;
    [Range(.01f, 1.0f)]
    public float sphereRadius = .5f;
    
    [Header("Graphical")]

    [SerializeField]
    Color dim;
    [SerializeField]
    Color bright;
    [SerializeField, Range(0.0f, .99f)]
    float cutoffThreshold;

    [SerializeField]
    FilterMode renderTextureFilterMode;

    [SerializeField]
    bool useCullingPlane;
    [SerializeField]
    Vector3 cullingPlaneOrigin;
    [SerializeField]
    Vector3 cullingPlaneNormal;

    [Header("Setup")]

    [SerializeField, Range(8, 2048)]
    int resolution = 8;

    RenderTexture readTexture;
    RenderTexture writeTexture;
    RenderTexture renderTexture;

    List<ComputeBuffer> buffers = new List<ComputeBuffer>();
    List<RenderTexture> textures = new List<RenderTexture>();

    int renderKernel;
    int moveKernel;
    int trailKernel;
    int trailDecayKernel;

    int steps;

    const float TIME_OFFSET = 2.2312f;
    const int MAX_RANGE = 10;
    const int NUMTHREADS_AGENTS = 64;
    const int NUMTHREADS_RESOLUTION = 8;

    void DispatchDecayKernel()
    {
        if (!decayTrails) return;

        computeShader.SetTexture(trailDecayKernel, "readTexture", readTexture);
        computeShader.SetTexture(trailDecayKernel, "writeTexture", writeTexture);
        computeShader.SetFloat("trailDecayFactor", trailDecayFactor);

        int toDispatch = resolution / NUMTHREADS_RESOLUTION;
        computeShader.Dispatch(trailDecayKernel, toDispatch, toDispatch, toDispatch);
    }

    void DispatchTrailsKernel()
    {
        computeShader.SetBuffer(trailKernel, "agents", agentBuffer);
        computeShader.SetTexture(trailKernel, "writeTexture", writeTexture);

        computeShader.Dispatch(trailKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void DispatchMoveKernel()
    {
        computeShader.SetBuffer(moveKernel, "agents", agentBuffer);
        computeShader.SetBuffer(moveKernel, "randStates", randStateBuffer);
        computeShader.SetTexture(moveKernel, "readTexture", readTexture);
        computeShader.SetInt("range", range);
        computeShader.SetFloat("lineOfSight", lineOfSight);
        computeShader.SetFloat("trailDetectionThreshold", trailDetectionThreshold);
        computeShader.SetFloat("trailApproximationBias", trailApproximationBias);
        computeShader.SetFloat("trailStrengthPerAgent", trailStrengthPerAgent);
        computeShader.SetFloat("agentSpeed", agentSpeed);

        computeShader.Dispatch(moveKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void DispatchResetKernels()
    {
        int resetTextureKernel = computeShader.FindKernel("ResetTextureKernel");
        computeShader.SetInt("resolution", resolution);
        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);
        computeShader.SetFloat("sphereRadius", sphereRadius);
        computeShader.SetInt("usingSphere", usingSphere ? 1 : 0);

        computeShader.SetTexture(resetTextureKernel, "writeTexture", writeTexture);
        int toDispatch = resolution / NUMTHREADS_RESOLUTION;
        computeShader.Dispatch(resetTextureKernel, toDispatch, toDispatch, toDispatch);

        computeShader.SetTexture(resetTextureKernel, "writeTexture", readTexture); //this is normal, we're resetting
        computeShader.Dispatch(resetTextureKernel, toDispatch, toDispatch, toDispatch);

        int resetAgentsKernel = computeShader.FindKernel("ResetAgentsKernel");
        computeShader.SetBuffer(resetAgentsKernel, "agents", agentBuffer);
        computeShader.SetBuffer(resetAgentsKernel, "randStates", randStateBuffer);
        computeShader.Dispatch(resetAgentsKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void DispatchRenderKernel()
    {
        computeShader.SetTexture(renderKernel, "readTexture", steps % 2 == 0 ? writeTexture : readTexture);
        computeShader.SetTexture(renderKernel, "renderTexture", renderTexture);
        computeShader.SetVector("dim", new Vector4(dim.r, dim.g, dim.b, 1.0f));
        computeShader.SetVector("bright", new Vector4(bright.r, bright.g, bright.b, 1.0f));
        computeShader.SetVector("planeOrigin", new Vector4(cullingPlaneOrigin.x, cullingPlaneOrigin.y, cullingPlaneOrigin.z, 1.0f));
        cullingPlaneNormal.Normalize();
        computeShader.SetVector("planeNormal", new Vector4(cullingPlaneNormal.x, cullingPlaneNormal.y, cullingPlaneNormal.z, .0f));
        computeShader.SetInt("usingCullingPlane", useCullingPlane ? 1 : 0);
        computeShader.SetFloat("cutoffThreshold", cutoffThreshold);
        int toDispatch = resolution / NUMTHREADS_RESOLUTION;
        computeShader.Dispatch(renderKernel, toDispatch, toDispatch, toDispatch);
    }
    void SwapTextures()
    {
        (readTexture, writeTexture) = (writeTexture, readTexture);
    }

    override protected void ReleaseResources() 
    {
        foreach (ComputeBuffer buffer in buffers)
            buffer.Release();

        foreach (RenderTexture texture in textures)
            texture.Release();

        buffers = new List<ComputeBuffer>();
        textures = new List<RenderTexture>();
    }

    [NaughtyAttributes.Button]
    protected override void ResetState()
    {
        steps = 0; 

        moveKernel = computeShader.FindKernel("MoveAgentsKernel");
        trailKernel = computeShader.FindKernel("TrailsKernel");
        trailDecayKernel = computeShader.FindKernel("DecayKernel");
        renderKernel = computeShader.FindKernel("RenderKernel");

        readTexture = CSUtilities.Create3DRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.RFloat);
        writeTexture = CSUtilities.Create3DRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.RFloat);
        renderTexture = CSUtilities.Create3DRenderTexture(resolution, renderTextureFilterMode, RenderTextureFormat.ARGBFloat);

        textures.Add(readTexture);
        textures.Add(writeTexture);
        textures.Add(renderTexture);

        agentBuffer = new ComputeBuffer(agentCount, sizeof(float) * 6);
        buffers.Add(agentBuffer);
        randStateBuffer = new ComputeBuffer(agentCount, sizeof(uint));
        buffers.Add(randStateBuffer);

        DispatchResetKernels();
    }

    protected override void Step()
    {
        steps++;

        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);

        DispatchMoveKernel();

        DispatchDecayKernel(); 
        DispatchTrailsKernel();
        DispatchRenderKernel();

        SwapTextures();

        outMaterial.SetTexture("_MainTex", renderTexture);
    }
}
