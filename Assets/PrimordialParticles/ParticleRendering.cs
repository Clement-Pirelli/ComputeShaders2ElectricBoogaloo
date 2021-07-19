using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRendering : MonoBehaviour
{
    [SerializeField]
    float scale = 1.0f;

    [SerializeField]
    Mesh mesh;
    private uint[] arguments = new uint[5] { 0, 0, 0, 0, 0 };
    Bounds bounds;

    private void Start()
    {
        //CreateMesh();
    }

    void CreateMesh() 
    {
        mesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 60.0f;
            vertices[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), .0f);
        }
        mesh.vertices = vertices;
    }

    ComputeBuffer CreateBuffer(int count) 
    {
        ComputeBuffer buffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        arguments[0] = mesh.GetIndexCount(0);
        arguments[1] = (uint)count;
        arguments[2] = mesh.GetIndexStart(0);
        arguments[3] = mesh.GetBaseVertex(0);
        buffer.SetData(arguments);
        return buffer;
    }

    public void Render(Material material, float domain, int count) 
    {
        using (ComputeBuffer buffer = CreateBuffer(count))
        {
            bounds = new Bounds(Vector3.zero, new Vector3(domain, domain, domain) * scale);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, buffer);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
