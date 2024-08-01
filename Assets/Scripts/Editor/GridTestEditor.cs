using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSystem))]
public class GridTestEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GridSystem colliderCreator = (GridSystem)target;
        if (GUILayout.Button("Grid test"))
        {
            colliderCreator.GenerateGrid();
        }
    }
}
