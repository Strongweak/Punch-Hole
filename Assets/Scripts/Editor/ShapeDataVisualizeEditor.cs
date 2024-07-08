using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShapeData), false)]
[CanEditMultipleObjects]
[System.Serializable]
public class ShapeDataVisualizeEditor : Editor
{
    private ShapeData instance => target as ShapeData;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("COL IS X, ROW IS Y");   
        ClearBoard();
        EditorGUILayout.Space();
        
        DrawColumnInput();
        EditorGUILayout.Space();

        if (instance.board != null && instance.row > 0 && instance.col > 0)
        {
            DrawTable();
        }
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(instance);
        }
    }

    private void ClearBoard()
    {
        if (GUILayout.Button("Clear board"))
        {
            instance.Clear();
        }
    } 
    private void DrawColumnInput()
    {
        var columnsTemp = instance.row;
        var rowsTemp = instance.col;

        instance.col = EditorGUILayout.IntField("Rows", instance.col);
        instance.row = EditorGUILayout.IntField("Cols", instance.row);

        if ((instance.row != columnsTemp || instance.col != rowsTemp) && (instance.row > 0 && instance.col > 0)) 
        {
            instance.CreateNewBoard();
        }
    } 
    private void DrawTable()
    {
        var tableStyle = new GUIStyle("box");
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        var headerColumnStyle = new GUIStyle();
        headerColumnStyle.fixedWidth = 65;
        headerColumnStyle.alignment = TextAnchor.MiddleCenter;

        var rowStyle = new GUIStyle();
        rowStyle.fixedHeight = 25;
        rowStyle.alignment = TextAnchor.MiddleCenter;

        var dataFieldStyle = new GUIStyle(EditorStyles.miniButtonMid);
        dataFieldStyle.normal.background = Texture2D.grayTexture;
        dataFieldStyle.onNormal.background = Texture2D.whiteTexture;


        for (var row = 0; row < instance.row;row++)
        {
            EditorGUILayout.BeginHorizontal(headerColumnStyle);
            for (var col = 0; col < instance.col;col++)
            {
                EditorGUILayout.BeginHorizontal(headerColumnStyle);
                var data = EditorGUILayout.Toggle(instance.board[row].col[col], dataFieldStyle);
                instance.board[row].col[col] = data;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }
    } 
    
}
// public class ShapeDataVisualizeEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         ShapeData arrayData = (ShapeData)target;
//
//         arrayData.width = EditorGUILayout.IntField("Rows", arrayData.width);
//         arrayData.height = EditorGUILayout.IntField("Columns", arrayData.height);
//
//         if (arrayData.dataArr == null || arrayData.dataArr.GetLength(0) != arrayData.width || arrayData.dataArr.GetLength(1) != arrayData.height)
//         {
//             arrayData.dataArr = new int[arrayData.width, arrayData.height];
//         }
//
//         for (int i = 0; i < arrayData.width; i++)
//         {
//             EditorGUILayout.BeginHorizontal();
//             for (int j = 0; j < arrayData.height; j++)
//             {
//                 EditorGUILayout.BeginHorizontal();
//                 arrayData.dataArr[i, j] = EditorGUILayout.IntField(arrayData.dataArr[i, j]);
//                 EditorGUILayout.EndHorizontal();
//             }
//             EditorGUILayout.EndHorizontal();
//         }
//
//         if (GUI.changed)
//         {
//             EditorUtility.SetDirty(arrayData);
//         }
//     }
// }
