using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance;
    
    [SerializeField] private EnemyVisual _enemyVisualPrefab;
    public Transform enemyContainer;
    [SerializeField] private Transform _enemyVisualContainer;
    #region UI

    public RectTransform _mainCanvas;
    [Header("Visual and block")]
    [SerializeField] private Transform shapeContainer;
    [SerializeField] private Transform positionTemplate;
    [SerializeField] private List<Transform> displayTransforms;
    public List<Transform> worldSpaceTransforms;

    [Header("Score and life counter")] 
    [SerializeField] private TextMeshProUGUI _txtLifeCounter;
    [SerializeField] private TextMeshProUGUI _txtScoreCounter;
    [Space(10f)] 
    [Header("UI")] 
    public static int screenWidth = Screen.width;
    public static int screenHeight = Screen.height;
    [SerializeField] private RectTransform _battleWonUI;
    [SerializeField] private RectTransform _winUI;
    [SerializeField] private RectTransform _loseUI;
    [SerializeField] private RectTransform _confirmUI;

    [Space(5f)] [SerializeField] private EnemyInfoUI _infoUI;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Observer.Instance.AddObserver(ObserverConstant.OnDisplayEnemyInfo, o => ShowInfo(o));
    }

    public void SetupScreenSize()
    {
        _mainCanvas.sizeDelta = new Vector2(screenWidth,screenHeight);

    }
    public void SpawnEnemyVisualForEnemy(Enemy parent)
    {
        EnemyVisual newVisual = Instantiate(_enemyVisualPrefab, _enemyVisualContainer, false);
        parent.SetChildVisual(newVisual);
        newVisual.SetupParent(parent);
    }
    public async Task SetupWorldPosition()
    {
        displayTransforms = new List<Transform>();
        worldSpaceTransforms = new List<Transform>();
        for (int i = 0; i < GameplayManager.Instance.maxVisibleShape; i++)
        {
            Transform newUI = Instantiate(positionTemplate, shapeContainer, false);
            newUI.gameObject.SetActive(true);
            displayTransforms.Add(newUI);
            GameObject newWorldSpace = new GameObject();
            newWorldSpace.name = "slot";
            worldSpaceTransforms.Add(newWorldSpace.transform);
        }
        await Task.Yield();
        //yield return new WaitForEndOfFrame();
        for (int i = 0; i < GameplayManager.Instance.maxVisibleShape; i++)
        {
            Vector3 worldPos;
            Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(GameplayManager.Instance.cam, displayTransforms[i].position);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_mainCanvas, screenPosition, GameplayManager.Instance.cam, out worldPos);
            worldPos.z = 0;
            worldSpaceTransforms[i].transform.position = worldPos;
        }
    }
    
    /// <summary>
    /// Setup first time playing UI
    /// </summary>
    public void SetupUI()
    {
        //
        _battleWonUI.anchoredPosition = new Vector2(0, -_mainCanvas.rect.height);
        _winUI.anchoredPosition = new Vector2(0, -_mainCanvas.rect.height);
        //
        _loseUI.anchoredPosition = new Vector2(_mainCanvas.rect.width, 0);
        _confirmUI.anchoredPosition = new Vector2(_mainCanvas.rect.height, 0);
        //
        //battleWonUI.gameObject.SetActive(false);
        //winUI.gameObject.SetActive(false);
        //loseUI.gameObject.SetActive(false);
        //confirmUI.gameObject.SetActive(false);
    }
    
    public void DisplayBattleWinUI(bool isOn = true)
    {
        _battleWonUI.gameObject.SetActive(isOn);
        if (isOn)
        {
            _battleWonUI.anchoredPosition = new Vector2(0, -_mainCanvas.rect.height);
        }
        else
        {
            _battleWonUI.anchoredPosition = new Vector2(0, 0);
        }
        Tween.UIAnchoredPosition(_battleWonUI, Vector3.zero, 0.3f).OnComplete(() => { GameplayManager.Instance.ShakeCamera(1, 0.1f, 2f); });
    }

    public void DisplayLoseUI()
    {
        _loseUI.gameObject.SetActive(true);
        _loseUI.anchoredPosition = new Vector2(_mainCanvas.rect.height, 0);
        Tween.UIAnchoredPosition(_loseUI, Vector3.zero, 0.3f);
    }

    private void ShowInfo(object o)
    {
        EnemySO data = (EnemySO)o;
        if (data == null)
        {
            _infoUI.Hide();
            return;
        }
        _infoUI.gameObject.SetActive(true);
        _infoUI.FeedData(data);
    }

    public void UpdateScore(int value)
    {
        _txtScoreCounter.SetText(value.ToString("D6"));
    }

    public void UpdateLife(int value)
    {
        _txtLifeCounter.SetText($"X {value.ToString()}");
    }
}
