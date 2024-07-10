using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuDataTransfer : MonoBehaviour
{
    public static MainMenuDataTransfer Instance;
    public int difficulty;
    public bool mute;
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(Instance);
        }

        Application.targetFrameRate = 60;
        Instance = this;
    }
}
