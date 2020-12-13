using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#pragma warning disable CS0649 //disable Serializefield private warnings



public class PhysariumScript : MonoBehaviour
{
    [Header("Physarium variables")]

    [SerializeField]
    Color dimColor = Color.black;
    [SerializeField]
    Color brightColor = Color.white;

    [Range(NUMTHREADS_AGENTS, 1000000), OnValueChanged(nameof(Reset))]
    public int agentCount = 1;
    private ComputeBuffer agentBuffer;
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

    [EnableIf("useImageInReset")]
    public Texture2D resetImage = null;
    public bool useImageInReset = false;

    [Header("Setup")]

    [SerializeField, Range(8, 2048)]
    int resolution = 8;

    [SerializeField, Range(0, 50)]
    int stepsPerFrame = 1;

    [SerializeField, Range(1, 50)]
    int stepsPerFrameModulo = 1;

    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Material outMaterial;

    RenderTexture outTexture;

    RenderTexture readTexture;
    RenderTexture writeTexture;

    List<ComputeBuffer> buffers = new List<ComputeBuffer>();
    List<RenderTexture> textures = new List<RenderTexture>();

    int debugKernel;
    int moveKernel;
    int renderKernel;
    int trailKernel;
    int trailDecayKernel;

    int steps;

    const float TIME_OFFSET = 2.2312f;
    const int MAX_RANGE = 10;
    const int NUMTHREADS_AGENTS = 64;
    const int NUMTHREADS_RESOLUTION = 16;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Second);
        Reset();
    }

    void Update()
    {
        if (Time.frameCount % stepsPerFrameModulo == 0)
        {
            for (int i = 0; i < stepsPerFrame; i++)
            {
                Step();
            }
        }
    }

    [Button]
    void Reset()
    {
        steps = 0;

        moveKernel = computeShader.FindKernel("MoveAgentsKernel");
        trailKernel = computeShader.FindKernel("TrailsKernel");
        renderKernel = computeShader.FindKernel("RenderKernel");
        trailDecayKernel = computeShader.FindKernel("DecayKernel");

        readTexture = CreateRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        writeTexture = CreateRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        outTexture = CreateRenderTexture(resolution, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        textures.Add(readTexture);
        textures.Add(writeTexture);
        textures.Add(outTexture);

        agentBuffer = new ComputeBuffer(agentCount, sizeof(float) * 4);
        buffers.Add(agentBuffer);


        DispatchResetKernels();
        Render();
    }

    void Step()
    {
        steps++;
   
        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);

        DispatchMoveKernel();

        DispatchDecayKernel();
        DispatchTrailsKernel();

        SwapTextures();
        Render();
    }

    void Render()
    {
        DispatchRenderKernel();
        outMaterial.SetTexture("_UnlitColorMap", outTexture);
    }

    void DispatchDecayKernel()
    {
        computeShader.SetTexture(trailDecayKernel, "readTexture", readTexture);
        computeShader.SetTexture(trailDecayKernel, "writeTexture", writeTexture);
        computeShader.SetFloat("trailDecayFactor", trailDecayFactor);

        computeShader.Dispatch(trailDecayKernel, resolution / NUMTHREADS_RESOLUTION, resolution / NUMTHREADS_RESOLUTION, 1);
    }

    void DispatchRenderKernel()
    {
        computeShader.SetTexture(renderKernel, "readTexture", readTexture);
        computeShader.SetTexture(renderKernel, "outTexture", outTexture);
        computeShader.SetVector("brightColor", brightColor);
        computeShader.SetVector("dimColor", dimColor);

        computeShader.Dispatch(renderKernel, resolution / NUMTHREADS_RESOLUTION, resolution / NUMTHREADS_RESOLUTION, 1);
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
        computeShader.SetTexture(moveKernel, "readTexture", readTexture);
        computeShader.SetInt("range", range);
        computeShader.SetFloat("lineOfSight", lineOfSight);
        computeShader.SetFloat("trailDetectionThreshold", trailDetectionThreshold);
        computeShader.SetFloat("trailApproximationBias", trailApproximationBias);
        computeShader.SetFloat("trailStrengthPerAgent", trailStrengthPerAgent);

        computeShader.Dispatch(moveKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void DispatchResetKernels()
    {
        int resetTextureKernel = computeShader.FindKernel("ResetTextureKernel");
        computeShader.SetInt("resolution", resolution);
        computeShader.SetFloat("time", Time.time + TIME_OFFSET);
        computeShader.SetInt("stepNumber", steps);

        computeShader.SetTexture(resetTextureKernel, "writeTexture", writeTexture);
        computeShader.Dispatch(resetTextureKernel, resolution / NUMTHREADS_RESOLUTION, resolution / NUMTHREADS_RESOLUTION, 1);

        computeShader.SetTexture(resetTextureKernel, "writeTexture", readTexture); //this is normal, we're resetting
        computeShader.Dispatch(resetTextureKernel, resolution / NUMTHREADS_RESOLUTION, resolution / NUMTHREADS_RESOLUTION, 1);

        int resetAgentsKernel = computeShader.FindKernel("ResetAgentsKernel");
        computeShader.SetBool("randomReset", resetImage != null ? !useImageInReset : true);
        if (resetImage != null)
            computeShader.SetTexture(resetAgentsKernel, "resetTexture", resetImage);
        computeShader.SetBuffer(resetAgentsKernel, "agents", agentBuffer);
        computeShader.Dispatch(resetAgentsKernel, agentCount / NUMTHREADS_AGENTS, 1, 1);
    }

    void SwapTextures()
    {
        (readTexture, writeTexture) = (writeTexture, readTexture);
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

    RenderTexture CreateRenderTexture(int resolution, FilterMode filterMode, RenderTextureFormat format)
    {
        RenderTexture texture = new RenderTexture(resolution, resolution, 1, format)
        {
            enableRandomWrite = true,
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Repeat,
            useMipMap = false,
            name = "out",
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            wrapModeU = TextureWrapMode.Repeat,
            wrapModeV = TextureWrapMode.Repeat,
            volumeDepth = 1,
            autoGenerateMips = false
        };

        texture.Create();

        return texture;
    }
    
    private void OnDestroy() => ReleaseResources();
    private void OnEnable() => ReleaseResources();
    private void OnDisable() => ReleaseResources();
}
