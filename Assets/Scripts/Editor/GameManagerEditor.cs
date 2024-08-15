using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameplayManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GameplayManager colliderCreator = (GameplayManager)target;
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
