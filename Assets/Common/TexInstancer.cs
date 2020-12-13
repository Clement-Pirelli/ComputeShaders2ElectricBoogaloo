using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649

[ExecuteAlways]
public class TexInstancer : MonoBehaviour
{
    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;
    [SerializeField]
    Material inputMaterial;

    [SerializeField, Range(.0f, 10.0f)]
    float sizeY;

    [SerializeField, Range(.0f, .5f)]
    float spacing = .1f;

    [SerializeField, Range(.0f, .5f)]
    float size = .4f;

    ComputeBuffer argumentBuffer;
    private uint[] arguments = new uint[5] { 0,0,0,0,0 };

    readonly Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 25.0f);


    private void Update()
    {
        if (argumentBuffer != null) 
        {
            argumentBuffer.Release();
        }

        Texture tex = inputMaterial.GetTexture("_UnlitColorMap");
        if (tex) 
        {
            int resolution = tex.width;
            material.SetTexture("inputTexture", tex);

            argumentBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            arguments[0] = mesh.GetIndexCount(0);
            arguments[1] = (uint)(resolution * resolution);
            arguments[2] = mesh.GetIndexStart(0);
            arguments[3] = mesh.GetBaseVertex(0);
            argumentBuffer.SetData(arguments);

            material.SetMatrix("transform", transform.localToWorldMatrix);
            material.SetVector("position", transform.position);
            material.SetInt("resolution", resolution);
            material.SetFloat("size", size);
            material.SetFloat("spacing", spacing);
            material.SetFloat("sizeY", sizeY);


            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argumentBuffer);

        }

    }
}
