using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649

public class TexInstancer_3D : MonoBehaviour
{
    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    [SerializeField, Range(.0f, 100.0f)]
    float spacing = .1f;

    [SerializeField, Range(.0f, .5f)]
    float size = .4f;

    enum ColorType 
    {
        flat = 0,
        direction = 1,
        speed = 2
    }

    [SerializeField]
    ColorType color = ColorType.flat;

    [SerializeField]
    Color flatColor = Color.white;

    [HideInInspector]
    public float maxSpeed = 1.0f;
    [SerializeField]
    Color noSpeedColor = Color.black;
    [SerializeField]
    Color fullSpeedColor = Color.white;

    ComputeBuffer argumentBuffer;
    private uint[] arguments = new uint[5] { 0, 0, 0, 0, 0 };

    readonly Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 25.0f);

    [HideInInspector]
    public ComputeBuffer agentsBuffer = null;

    public void Render(int resolution, int agentsAmount)
    {
        ReleaseResources();

        if (agentsBuffer != null)
        {
            material.SetBuffer("agents", agentsBuffer);

            argumentBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            arguments[0] = mesh.GetIndexCount(0);
            arguments[1] = (uint)agentsAmount;
            arguments[2] = mesh.GetIndexStart(0);
            arguments[3] = mesh.GetBaseVertex(0);
            argumentBuffer.SetData(arguments);

            material.SetMatrix("transform", transform.localToWorldMatrix);
            material.SetVector("position", transform.position);
            material.SetInt("resolution", resolution);
            material.SetFloat("size", size);
            material.SetFloat("spacing", spacing);

            material.SetInt("colorType", (int)color);

            material.SetColor("flatColor", flatColor);

            material.SetFloat("maxSpeed", maxSpeed * resolution);
            material.SetColor("noSpeedColor", noSpeedColor);
            material.SetColor("fullSpeedColor", fullSpeedColor);

            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argumentBuffer);
        }

    }

    private void OnDestroy() => ReleaseResources();
    private void OnDisable() => ReleaseResources();
    private void OnEnable() => ReleaseResources();

    void ReleaseResources() 
    {
        if (argumentBuffer != null)
        {
            argumentBuffer.Release();
        }
    }
}

