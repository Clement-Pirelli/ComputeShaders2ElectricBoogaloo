using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComputeShaderScript : MonoBehaviour
{

    [SerializeField]
    protected ComputeShader computeShader;

    [SerializeField]
    protected Material outMaterial;

    [SerializeField, Range(.0f, 2.0f)]
    float simulationSpeed = 1.0f;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Second);
        ResetState();
    }

    float accTime = .0f;
    void Update()
    {
        accTime += Time.deltaTime * simulationSpeed;
        if (accTime > 1.0f / 60.0f)
        {
            Step();
            accTime = .0f;
        }
        Render();
    }

    protected abstract void ResetState();
    protected abstract void Step();
    protected virtual void Render() { }
    protected virtual void ReleaseResources() { }

    private void OnDestroy() => ReleaseResources();
    private void OnEnable() => ReleaseResources();
    private void OnDisable() => ReleaseResources();
}
