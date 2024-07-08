using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject nomralUI;
    [SerializeField] private GameObject difficultyUI;

    // Start is called before the first frame update
    void Start()
    {
        nomralUI.SetActive(true);
        difficultyUI.SetActive(false);
    }
    
    public void ChooseDificulty()
    {
        difficultyUI.SetActive(true);
        nomralUI.SetActive(false);
    }

    public void Play(int diff)
    {
        MainMenuDataTransfer.Instance.difficulty = diff;
        SceneManager.LoadScene(1);
    }
}
