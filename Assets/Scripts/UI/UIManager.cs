using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    //Main menu
    public MainMenuUI _mainMenuUI;
    //Gameplay

    public GameplayUI _gameplayUI;
    //Pause

    //Transition
    public Image _curtain;
    private Sequence _introSequence;
    [SerializeField] float _transitionTime;
    private void Awake()
    {

    }

    private void Start()
    {
        StartCoroutine(BootupTransition());
    }
    public IEnumerator BootupTransition()
    {
        _gameplayUI.gameObject.SetActive(true);

        _introSequence = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        // Animate 'floatField' from 0 to 1 
        _introSequence.Group(Tween.Custom(0, 1, duration: 1, onValueChange: newVal => _curtain.material.SetFloat("_Dissolve", newVal))).OnComplete(() =>{
            _curtain.raycastTarget = false;
        });
        yield return _introSequence.ToYieldInstruction();
        yield return new WaitForEndOfFrame();
        GameplayManager.Instance.StartGame();
    }

    public void SkipIntro()
    {
        if (_introSequence.isAlive)
        {
            _introSequence.Complete();
        }
    }
}