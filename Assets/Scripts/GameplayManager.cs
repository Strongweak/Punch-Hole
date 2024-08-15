using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    public float gameplaySpeed = 0.2f;

    #region Player stat

    [Header("Player stat")]
    [SerializeField]
    private int maxLife;

    [SerializeField] private ShapeListSO spawnableBlock;
    [SerializeField] private int maxVisibleShape;
    public List<Block> currentBlock;
    private int currentlife;

    #endregion

    #region Enemy stat

    [Header("Enemy")][SerializeField] private int maxNumberOfEnemies = 3;
    public List<Enemy> currentEnemies;
    [SerializeField] private EnemyVisual _enemyVisualPrefab;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform _enemyVisualContainer;
    [SerializeField] private List<Enemy> enemyprefab;

    #endregion

    #region Scoring

    [Header("Score")][SerializeField] private ScoringSO comboScore;
    private int currentScore;
    [SerializeField] private int beforeStartStreak = 3;
    [SerializeField] private int currentChain = 0;
    [HideInInspector] public int currentStreak;
    public float plusScore = 1f;

    #endregion

    #region Camera

    [SerializeField] private Camera cam;

    #endregion

    #region UI

    [SerializeField] private RectTransform mainCanvas;
    [Header("Visual and block")]
    [SerializeField] private Transform shapeContainer;
    [SerializeField] private Transform positionTemplate;
    [SerializeField] private List<Transform> displayTransforms;
    [SerializeField] private List<Transform> worldSpaceTransforms;

    [Space(10f)] [Header("UI")] 
    private int screenWidth = Screen.width;
    private int screenHeight = Screen.height;
    [SerializeField] private RectTransform battleWonUI;
    [SerializeField] private RectTransform winUI;
    [SerializeField] private RectTransform loseUI;
    [SerializeField] private RectTransform confirmUI;
    #endregion

    #region Game state

    public GameState _state;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentBlock = new List<Block>();
        currentlife = maxLife;
        StartCoroutine(SetupGameplay());
        Observer.Instance.AddObserver(ObserverConstant.OnPlayerMove, o =>
        {
            StartCoroutine(CheckEnemyTurn());
            //StartCoroutine(SpawnNewBlocks());
        });
        Observer.Instance.AddObserver(ObserverConstant.OnStateChange, o => ChangeState(o));
        ChangeState(GameState.PlayerTurn);
        SetupUI();
        StartCoroutine(SpawnEnemy(maxNumberOfEnemies));
    }

    public void ShakeCamera(float strength, float duration, float frequency)
    {
        Tween.ShakeCamera(cam, strength, duration, frequency);
    }

    /// <summary>
    /// Spawn new block, and make player have to wait until it spawn
    /// </summary>
    /// <returns></returns>
    public IEnumerator SpawnNewBlocks()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.Holdup);
        yield return null;
        for (int i = 0; i < maxVisibleShape; i++)
        {
            if (worldSpaceTransforms[i].transform.childCount == 0)
            {
                Block newBlock =
                    Instantiate(spawnableBlock.spawnableBlock[Random.Range(0, spawnableBlock.spawnableBlock.Count)]);
                newBlock.transform.parent = worldSpaceTransforms[i];
                newBlock.transform.localPosition = Vector3.zero;
                currentBlock.Add(newBlock);
            }
        }

        yield return new WaitForSeconds(gameplaySpeed);
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.PlayerTurn);
        GridSystem.Instance.CheckOutOfMove();
    }
    /// <summary>
    /// Spawnning enemy
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private IEnumerator SpawnEnemy(int count)
    {
        currentEnemies = new List<Enemy>();
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = Instantiate(enemyprefab[Random.Range(0, enemyprefab.Count)], enemyContainer);
            EnemyVisual newVisual = Instantiate(_enemyVisualPrefab,_enemyVisualContainer,false);
            enemy.SetChildVisual(newVisual);
            newVisual.SetupParent(enemy);
            currentEnemies.Add(enemy);
            yield return new WaitForSeconds(gameplaySpeed);
        }
    }

    /// <summary>
    /// Manage the movement of enemies, enemies will taking turn one after another
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckCurrentEnemy()
    {
        List<Enemy> enemiesToRemove = new List<Enemy>();
        foreach (var enemy in currentEnemies)
        {
            if (enemy._currentHealth <= 0)
            {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            yield return new WaitForSeconds(gameplaySpeed);
            currentEnemies.Remove(enemy);
            Destroy(enemy.gameObject);
        }

        if (currentEnemies.Count == 0)
        {
            //
            Debug.Log("Wave cleared");
            DisplayBattleWinUI();
        }

        yield return new WaitForSeconds(gameplaySpeed);
    }

    private IEnumerator CheckEnemyTurn()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.EnemyTurn);
        foreach (var enemy in currentEnemies)
        {
            enemy._currentMoveCount--;
            enemy.UpdateText();
            //not dead, and ready to strike
            if (enemy._currentMoveCount <= 0 && !enemy._isDead)
            {
                yield return Tween.Delay(gameplaySpeed).ToYieldInstruction();
                yield return StartCoroutine(enemy.EffectEvent());
                Debug.Log(enemy.name + " is done move");
                enemy._currentMoveCount = enemy._delayAfterMove;
                enemy.UpdateText();
            }
        }

        yield return StartCoroutine(CheckCurrentEnemy());
        yield return StartCoroutine(GridSystem.Instance.CheckLineAndRowAfterUpdate());
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.PlayerTurn);
        StartCoroutine(SpawnNewBlocks());
    }

    //<SUMMARY>
    // Unfinished
    //<SUMMARY>
    public void ModifySlot(int count)
    {
        if (count > 0)
        {
            for (int i = 0; i < maxVisibleShape; i++)
            {
                //worldSpaceTransforms.Remove();
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Transform newUI = Instantiate(positionTemplate, shapeContainer, false);
                newUI.gameObject.SetActive(true);
                newUI.gameObject.SetActive(true);
                displayTransforms.Add(newUI);
                GameObject newWorldSpace = new GameObject();
                worldSpaceTransforms.Add(newWorldSpace.transform);
            }
        }
    }

    /// <summary>
    /// Setup the gameplay space for the rest of the play section
    /// include, camera, grid, block position...
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetupGameplay()
    {
        yield return StartCoroutine(GridSystem.Instance.GenerateGrid());
        displayTransforms = new List<Transform>();
        worldSpaceTransforms = new List<Transform>();
        for (int i = 0; i < maxVisibleShape; i++)
        {
            Transform newUI = Instantiate(positionTemplate, shapeContainer, false);
            newUI.gameObject.SetActive(true);
            displayTransforms.Add(newUI);
            GameObject newWorldSpace = new GameObject();
            newWorldSpace.name = "slot";
            worldSpaceTransforms.Add(newWorldSpace.transform);
        }

        yield return new WaitForEndOfFrame();
        for (int i = 0; i < maxVisibleShape; i++)
        {
            Vector3 worldPos;
            Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(cam, displayTransforms[i].position);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(mainCanvas, screenPosition, cam, out worldPos);
            worldPos.z = 0;
            worldSpaceTransforms[i].transform.position = worldPos;
        }

        StartCoroutine(SpawnNewBlocks());
    }

    private void ChangeState(object o)
    {
        _state = (GameState)o;
        switch (_state)
        {
            case GameState.Ready:
                break;
            case GameState.Holdup:
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.EnemyTurn:
                break;
        }
    }

    /// <summary>
    /// Setup first time playing UI
    /// </summary>
    private void SetupUI()
    {
        //
        battleWonUI.anchoredPosition = new Vector2(0, -mainCanvas.rect.height);
        winUI.anchoredPosition = new Vector2(0, -mainCanvas.rect.height);
        //
        loseUI.anchoredPosition = new Vector2(mainCanvas.rect.width, 0);
        confirmUI.anchoredPosition = new Vector2(mainCanvas.rect.height, 0);
        //
        //battleWonUI.gameObject.SetActive(false);
        //winUI.gameObject.SetActive(false);
        //loseUI.gameObject.SetActive(false);
        //confirmUI.gameObject.SetActive(false);
    }
    public void DisplayBattleWinUI()
    {
        battleWonUI.gameObject.SetActive(true);
        battleWonUI.anchoredPosition = new Vector2(0, -mainCanvas.rect.height);
        Tween.UIAnchoredPosition(battleWonUI, Vector3.zero,0.3f).OnComplete(() =>
        {
            ShakeCamera(1,0.1f,2f);
        });
    }
    public void DisplayLoseUI()
    {
        loseUI.gameObject.SetActive(true);
        loseUI.anchoredPosition = new Vector2(mainCanvas.rect.height, 0);
        Tween.UIAnchoredPosition(loseUI, Vector3.zero,0.3f);
    }
}

public enum GameState
{
    Ready,
    PlayerTurn,
    EnemyTurn,
    Holdup,
}