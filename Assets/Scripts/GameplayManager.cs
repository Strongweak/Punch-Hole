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
    public static float _gameplaySpeed = 0.2f;
    public static WaitForSeconds _delay = new WaitForSeconds(_gameplaySpeed);

    #region Player stat

    [Header("Player stat")] [SerializeField]
    private int _maxLife;

    [SerializeField] private ShapeListSO spawnableBlock;
    public int maxVisibleShape;
    public List<Block> currentBlock;
    private int currentlife;

    #endregion

    #region Enemy stat

    [Header("Enemy")] [SerializeField] private int maxNumberOfEnemies = 3;
    public List<Enemy> currentEnemies;
    [SerializeField] private List<Enemy> enemyprefab;

    #endregion

    #region Scoring

    [Header("Score")] [SerializeField] private ScoringSO comboScore;
    private int currentScore;
    [SerializeField] private int beforeStartStreak = 3;
    [SerializeField] private int currentChain = 0;
    [HideInInspector] public int currentStreak;
    public float plusScore = 1f;

    #endregion

    #region Camera

    public Camera cam;

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
        currentlife = _maxLife;
        StartCoroutine(SetupGameplay());
        Observer.Instance.AddObserver(ObserverConstant.OnPlacingShape, o =>
        {
            StartCoroutine(GameplayFlow());
            //StartCoroutine(CheckEnemyTurn());
        });
        Observer.Instance.AddObserver(ObserverConstant.OnStateChange, o => ChangeState(o));
        ChangeState(GameState.PlayerTurn);
        GameplayUI.Instance.SetupUI();
        StartCoroutine(SpawnEnemy(maxNumberOfEnemies));
    }

    /// <summary>
    /// The main gameplay flow must be manage here, trigger after shape put on board
    /// player turn => [placing block] => player does effect(damage, line clear...) => check enemy health
    /// => check enemy turn => enemy movement => spawn set of shapes (if out of shape) => check game over => player turn
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameplayFlow()
    {
        Debug.Log("PHASE: line clear phase");
        yield return StartCoroutine(GridSystem.Instance.CheckLineAndRow());
        if (currentBlock.Count == 0)
        {
            Debug.Log("PHASE: Check enemy turn");
            yield return StartCoroutine(EnemyTurnCountdown());
        }
        Debug.Log("PHASE: check enemy stat");
        yield return StartCoroutine(CheckCurrentEnemyStat());
        Debug.Log("PHASE: line clear phase");
        yield return StartCoroutine(GridSystem.Instance.CheckLineAndRowAfterUpdate());
        if (currentBlock.Count == 0)
        {
            Debug.Log("PHASE: spawn new block");
            yield return StartCoroutine(SpawnNewBlocks());
        }
        Debug.Log("PHASE: check gameover");
        GridSystem.Instance.CheckOutOfMove();
        yield return _delay;
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
        yield return null;
        for (int i = 0; i < maxVisibleShape; i++)
        {
            if (GameplayUI.Instance.worldSpaceTransforms[i].transform.childCount == 0)
            {
                Block newBlock =
                    Instantiate(spawnableBlock.spawnableBlock[Random.Range(0, spawnableBlock.spawnableBlock.Count)]);
                newBlock.transform.parent = GameplayUI.Instance.worldSpaceTransforms[i];
                newBlock.transform.localPosition = Vector3.zero;
                currentBlock.Add(newBlock);
            }
        }

        yield return _delay;
    }

    #region Enemy management

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
            Enemy enemy = Instantiate(enemyprefab[Random.Range(0, enemyprefab.Count)],
                GameplayUI.Instance.enemyContainer);
            GameplayUI.Instance.SpawnEnemyVisualForEnemy(enemy);
            currentEnemies.Add(enemy);
            yield return _delay;
        }
    }

    /// <summary>
    /// Manage the movement of enemies, enemies will taking turn one after another
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckCurrentEnemyStat()
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
            yield return _delay;
            currentEnemies.Remove(enemy);
            Destroy(enemy.gameObject);
        }

        if (currentEnemies.Count == 0)
        {
            //
            Debug.Log("Wave cleared");
            Victory();
        }

        yield return _delay;
    }

    private IEnumerator EnemyTurnCountdown()
    {
        foreach (var enemy in currentEnemies)
        {
            enemy._currentMoveCount--;
            enemy.UpdateText();
            //not dead, and ready to strike
            if (enemy._currentMoveCount <= 0 && !enemy._isDead)
            {
                yield return _delay;
                yield return StartCoroutine(enemy.EffectEvent());
                Debug.Log(enemy.name + " is done move");
                enemy._currentMoveCount = enemy._delayAfterMove;
                enemy.UpdateText();
            }
        }
    }

    #endregion

    /// <summary>
    /// Setup the gameplay space for the rest of the play section
    /// include, camera, grid, block position...
    /// </summary>
    /// <returns></returns>
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

    #region Gameplay management

    private void Victory()
    {
        GameplayUI.Instance.DisplayBattleWinUI();
        foreach (var block in currentBlock)
        {
            currentBlock.Remove(block);
            Destroy(block.gameObject);
        }

        StartCoroutine(GridSystem.Instance.ClearBoard());
    }

    private IEnumerator SetupGameplay()
    {
        //Generate grid
        yield return StartCoroutine(GridSystem.Instance.GenerateGrid());
        //create world position of ui for the block to spawn
        yield return StartCoroutine(GameplayUI.Instance.SetupWorldPosition());
        //now we spawn the block
        StartCoroutine(SpawnNewBlocks());
    }

    public void NextLevel()
    {
        GameplayUI.Instance.DisplayBattleWinUI(false);
        StartCoroutine(SpawnEnemy(maxNumberOfEnemies));
        StartCoroutine(SpawnNewBlocks());
    }

    public void RemoveBlock(Block block)
    {
        
    }
    #endregion
}

public enum GameState
{
    Ready,
    PlayerTurn,
    EnemyTurn,
    Holdup,
}