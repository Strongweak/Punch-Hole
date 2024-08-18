using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayUI))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GameplayUI colliderCreator = (GameplayUI)target;
        if (GUILayout.Button("Win"))
        {
            colliderCreator.DisplayBattleWinUI();
        }
        if (GUILayout.Button("Lose"))
        {
            colliderCreator.DisplayLoseUI();
        }

    }
}
