using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ComputeKernel
{
    public int value;
    public string name;
    public readonly static int invalidValue = -1;

    public void Find(ComputeShader shader) 
    {
        value = shader.FindKernel(name);
    }

    public ComputeKernel(string name)
    {
        this.name = name;
        value = invalidValue;
    }
}
