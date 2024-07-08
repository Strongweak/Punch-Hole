using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GridVisualizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GameManager colliderCreator = (GameManager)target;
        if (GUILayout.Button("Grid test"))
        {
            colliderCreator.DebugGrid();
        }
        if (GUILayout.Button("combo text test"))
        {
            colliderCreator.TestCombo();
        }
        // if (GUILayout.Button("Center camera to the grid"))
        // {
        //     colliderCreator.CalculateCameraCenter();
        // }
        
    }
}
