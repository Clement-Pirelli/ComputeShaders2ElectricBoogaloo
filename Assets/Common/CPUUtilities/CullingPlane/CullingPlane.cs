using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings

[CreateAssetMenu(fileName = "NewCullingPlane", menuName = "Culling Plane")]
public class CullingPlane : ScriptableObject
{
    [SerializeField]
    bool useCullingPlane;
    public Vector3 cullingPlaneOrigin;
    [SerializeField]
    Vector3 cullingPlaneNormal;

    public void UploadToComputeShader(ComputeShader computeShader) 
    {
        computeShader.SetVector("planeOrigin", cullingPlaneOrigin.asDirection());
        computeShader.SetVector("planeNormal", cullingPlaneNormal.normalized.asDirection());
        computeShader.SetInt("usingCullingPlane", useCullingPlane ? 1 : 0);
    }
}
