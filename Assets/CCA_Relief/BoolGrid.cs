using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "BoolGrid", menuName = "Bool Grid")]
public class BoolGrid : ScriptableObject
{
    public Vector4[] packToVector4s()
    {
        Vector4[] vector4s = new Vector4[Mathf.CeilToInt(gridData.Length / 4.0f)];
        
        for (int i = 0; i < gridData.Length; i++)
        {
            vector4s[i / 4][i % 4] = gridData[i] ? 1.0f : .0f;
        }

        return vector4s;
    }

    public int Dimension { get { return range * 2 + 1; } }

    public void ResizeToRange(int range)
    {
        this.range = range;
        gridData = new bool[Dimension * Dimension];
    }

    [SerializeField]

    public int range = 10;
    public bool[] gridData = new bool[10*10];


    private void ForEach(System.Func<bool, bool> applyToAll)
    {
        for (var i = 0; i < gridData.Length; i++)
        {
            gridData[i] = applyToAll(gridData[i]);
        }
    }

    private void SetAt(int x, int y, bool newValue)
    {
        gridData[x + y * Dimension] = newValue;
    }

    public void FillAll()
    {
        ForEach((bool inBool) => true);
    }

    public void EmptyAll()
    {
        ForEach((bool inBool) => false);
    }

    public void InvertAll()
    {
        ForEach((bool inBool) => !inBool);
    }

    public void RandomizeAll()
    {
        ForEach((bool inBool) => Random.Range(0, 2) == 0);
    }

    public void RandomizeSymmetrical()
    {
        for(var y = 0; y >= Dimension / 2; y++)
        for(var x = 0; x >= Dimension / 2; x++)
        {
                bool randomValue = Random.Range(0, 2) == 0;
                SetAt(x, y, randomValue);
                SetAt(y, x, randomValue);
                SetAt(Dimension - x, y, randomValue);
                SetAt(x, Dimension - y, randomValue);
        }
    }
}
