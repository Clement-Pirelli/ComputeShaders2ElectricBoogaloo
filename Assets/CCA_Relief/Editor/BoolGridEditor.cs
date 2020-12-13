using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoolGrid))]
public class BoolGridEditor : Editor
{
    int newRange = 5;

    public override void OnInspectorGUI()
    {

        BoolGrid grid = target as BoolGrid;
        int dimensions = (int)Mathf.Sqrt(grid.gridData.Length);

        for (var x = 0; x < dimensions; x++)
        {
            _ = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            for (var y = 0; y < dimensions; y++)
            {
                var index = x + y * dimensions;
                bool middle = x == (dimensions-1) / 2 && y == (dimensions-1) / 2;

                if (middle) EditorGUI.BeginDisabledGroup(true);
                

                grid.gridData[index] = GUILayout.Toggle(grid.gridData[index], "");

                if (middle) EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
        

        if (GUILayout.Button("Fill All"))
        {
            grid.FillAll();
        }

        if (GUILayout.Button("Empty All"))
        {
            grid.EmptyAll();
        }

        if (GUILayout.Button("Invert All"))
        {
            grid.InvertAll();
        }

        if (GUILayout.Button("Randomize All"))
        {
            grid.RandomizeAll();
        }

        if (GUILayout.Button("Randomize Symmetrical"))
        {
            grid.RandomizeSymmetrical();
        }
        
        

        var secondRect = EditorGUILayout.GetControlRect();
        newRange = EditorGUI.IntSlider(secondRect, "New range",newRange, 1, 10);
        if (GUILayout.Button("Resize to new range"))
        {
            grid.ResizeToRange(newRange);
        }
    }
}
