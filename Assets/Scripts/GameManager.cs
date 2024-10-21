using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static int row = 8;

    public static int col = 8;

    //public static Transform[,] gridData = new Transform[width, height];
    public static SpriteRenderer[,] visualgrid = new SpriteRenderer[col, row];
    public static GameObject[,] cubeGrid = new GameObject[col, row];
    public static int[,] dataGrid = new int[col, row];

    [Header("Gameplay")] 
    [SerializeField] private ShapeListSO ezPack;
    [SerializeField] private ShapeListSO normalPack;
    [SerializeField] private ShapeListSO hardPack;
    [SerializeField] private Shape spawnableBlock;
    [SerializeField] private List<Sprite> randomSprite;
    [SerializeField] private List<Transform> blockPosition;
    [SerializeField] private List<Shape> currentBlock;
    [SerializeField] private bool ispause;

    [Header("UI")] 
    [SerializeField] private GameObject loseUI;

    [SerializeField] private TextMeshProUGUI muteText;
    private bool isMute;
    [Header("Score")] private Dictionary<int,( int, string)> comboScore;
    //[SerializeField] private int score;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int beforeStartStreak = 3;
    [SerializeField] private int currentChain = 0;
    [SerializeField] private int currentStreak;
    [SerializeField] private ComboTextUI comboTextEffect;
    private int currentScore;
    private int plusScore;
    [Header("Visual")] [SerializeField] private Camera cam;
    [SerializeField] private GameObject gridDebug;
    [SerializeField] private Transform visualBoard;
    [SerializeField] private Transform gameBoard;
    [SerializeField] private Color ghostColor;
    [SerializeField] private Color invalidColor;

    [Header("Audio/ SFX")] 
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private AudioSource _au;
    [SerializeField] private AudioSource clearPitch;
    [SerializeField] private AudioClip clearSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip streakSound;
    [SerializeField] private float pitchMult = 0.1f;
    private EmptySearch DFS;

    private void Awake()
    {
        instance = this;
        // for (int i = 0; i < dataGrid.GetLength(0); i++)
        // {
        //     for (int j = 0; j < dataGrid.GetLength(1); j++)
        //     {
        //         dataGrid[i, j] = 1;
        //     }
        // }
        DebugGrid();
        cubeGrid = new GameObject[col, row];
        dataGrid = new int[col, row];
        comboScore = new Dictionary<int, (int, string)>();
        comboScore.Add(1, (10,""));
        comboScore.Add(2, (30, "good!"));
        comboScore.Add(3, (60, "great!"));
        comboScore.Add(4, (100, "amazing!"));
        comboScore.Add(5, (150, "excellent!"));
        comboScore.Add(6, (300, "unbelievable!"));
        currentScore = 0;
        loseUI.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        // if (MainMenuDataTransfer.Instance == null)
        // {
        //     spawnableBlock = normalPack.spawnableBlock;
        // }
        // else
        // {
        //     switch (MainMenuDataTransfer.Instance.difficulty)
        //     {
        //         case 0:
        //             spawnableBlock = ezPack.spawnableBlock;
        //             break;
        //         case 1:
        //             spawnableBlock = normalPack.spawnableBlock;
        //             break;
        //         case 2:
        //             spawnableBlock = hardPack.spawnableBlock;
        //             break;
        //     }
        // }
        scoreText.text = "000000000";
        ispause = false;
        SpawnNewBlocks();
        DFS = new EmptySearch(row, col);
    }

    //
    //<SUMMARY> check if can put the shape on to the board, unpacking the object, clear line, calculate score, check game over after movement
    //
    public void CheckAddToGrid(Shape shape)
    {
        // check if can be put on position
        foreach (var children in shape._childBlock)
        {
            //get children position
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            //not validate to put in
            if (InBounds(roundedX, roundedY) == false || dataGrid[roundedX, roundedY] == 1)
            {
                ClearHighLight();
                shape.ReturnOriginalSize();
                Tween.LocalPosition(shape.transform, Vector2.zero, 0.4f, Ease.OutQuart);
                return;
            }
        }

        // add to the board
        foreach (var children in shape._childBlock)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            //add to board
            children.transform.parent = gameBoard;
            cubeGrid[roundedX, roundedY] = children.gameObject;
            dataGrid[roundedX, roundedY] = 1;
            children.transform.position = new Vector3(roundedX, roundedY);
            UpdateScore(1);
        }

        //un parent
        _au.PlayOneShot(placeSound);
        Destroy(shape.gameObject);
        currentBlock.Remove(shape);
        if (currentBlock.Count == 0)
        {
            SpawnNewBlocks();
        }
        ClearHighLight();
        //the flavor
        CheckLineAndRow(shape.transform.position);
        //Debug.Log("Total empty space " + DFS.countIslands(dataGrid));
        StartCoroutine(CheckAfterUpdate());
    }


    private IEnumerator CheckAfterUpdate()
    {
        yield return null;
        CheckGameOver();
    }
    //
    //<SUMMARY> check when block out of the board or not
    //
    private bool InBounds(int x, int y)
    {
        if (x < 0 || x >= col)
        {
            return false;
        }

        if (y < 0 || y >= row)
        {
            return false;
        }

        return true;
    }
    
    //duh
    public void HighLightCloset(Transform cube)
    {
        int roundedX = Mathf.RoundToInt(cube.position.x);
        int roundedY = Mathf.RoundToInt(cube.position.y);
        if (InBounds(roundedX, roundedY) == false)
        {
            return;
        }

        visualgrid[roundedX, roundedY].color = ghostColor;
    }


    //very duh
    public void ClearHighLight()
    {
        foreach (var crap in visualgrid)
        {
            crap.color = Color.clear;
        }
    }
    
    //
    //<SUMMARY> highlight the clearable line
    //
    public void HighLightClearableRow(int row)
    {

    }

    public void HighLightClearableCol(int col)
    {

    }

    //
    //<SUMMARY> check when block is put in which slot, start checking row and column of it
    //
    public void CheckLineAndRow(Vector3 comboPos)
    {
        bool haveClear = false;
        int lineClear = 0;
        //store all line
        List<List<GameObject>> howDoIGetThisFar = new List<List<GameObject>>();
        //checking
        for (int i = row - 1; i >= 0; i--)
        {
            List<GameObject> fuck = new List<GameObject>();
            if (HasRow(i))
            {
                AddRowToQueue(i, fuck);
                lineClear++;
                haveClear = true;
                howDoIGetThisFar.Add(fuck);
            }
        }

        for (int i = col - 1; i >= 0; i--)
        {
            List<GameObject> fuck = new List<GameObject>();
            if (HasColumn(i))
            {
                AddColumnToQueue(i, fuck);
                lineClear++;
                haveClear = true;
                howDoIGetThisFar.Add(fuck);
            }
        }

        //scoring
        CheckStreak(haveClear);
        lineClear = Mathf.Clamp(lineClear, 1, 6);
        plusScore = comboScore[lineClear].Item1;
        if (lineClear > 0)
        {
            string comboText = comboScore[lineClear].Item2;
            if (currentStreak > 0)
            {
                comboText += "\n" + "STREAK +" + 10*currentStreak;
            }
            comboTextEffect.gameObject.SetActive(true);
            comboTextEffect.SetText(comboText, comboPos);
        }
        //deleting
        if (howDoIGetThisFar.Count > 0)
        {
            StartCoroutine(PlayClearSound());
            foreach (var list in howDoIGetThisFar)
            {
                StartCoroutine(DeleteAnimation(list));
            }
        }
    }

    private IEnumerator PlayClearSound()
    {
        clearPitch.pitch = 1;
        int time = 8;
        for (int i = 0; i < time; i++)
        {
            clearPitch.PlayOneShot(clearSound,2f);
            clearPitch.pitch += pitchMult;
            yield return new WaitForSeconds(0.05f);
        }
    }
    private IEnumerator DeleteAnimation(List<GameObject> chain)
    {
        foreach (var child in chain)
        {
            if (child == null)
            {
                continue;
            }

            Tween.Scale(child.transform, Vector3.one * 0.1f, 0.05f, Ease.OutExpo).OnComplete(() =>
            {
                UpdateScore(plusScore + (currentStreak * 10));
                Destroy(child);
            });
            yield return new WaitForSeconds(0.05f);
        }
    }
    private void CheckStreak(bool clear)
    {
        if (clear)
        {
            currentChain++;
            if (currentChain % 3 == 0)
            {
                currentStreak++;
            }
        }
        else
        {
            currentChain = 0;
            currentStreak = 0;
        }
    }

    private void UpdateScore(int value)
    {
        currentScore += value;
        string formattedScore = currentScore.ToString("D9");
        scoreText.text = formattedScore;
    }

    void AddRowToQueue(int row, List<GameObject> queue)
    {
        //for every element in row, delete all of them
        for (int j = 0; j < col; j++)
        {
            queue.Add(cubeGrid[j, row]);
            dataGrid[j, row] = 0;
            //cubeGrid[j, row] = null;
        }

        //Debug.Log("Found row " + row);
    }

    void AddColumnToQueue(int col, List<GameObject> queue)
    {
        //for every element in row, delete all of them
        for (int j = 0; j < row; j++)
        {
            queue.Add(cubeGrid[col, j]);
            dataGrid[col, j] = 0;
            //cubeGrid[col, j] = null;
        }

        //Debug.Log("Found col " + col);
    }

    bool HasColumn(int i)
    {
        //check each index in the row
        for (int j = 0; j < row; j++)
        {
            //if one of them not have an object in the row, the row can not delete
            if (cubeGrid[i, j] == null)
            {
                return false;
            }
        }

        return true;
    }

    bool HasRow(int i)
    {
        //check each index in the row
        for (int j = 0; j < col; j++)
        {
            //if one of them not have an object in the row, the row can not delete
            if (cubeGrid[j, i] == null)
            {
                return false;
            }
        }

        return true;
    }

    public void DebugGrid()
    {
        //bool flip = false;
        for (int i = 0; i < col; i++)
        {
            //flip = !flip;
            for (int j = 0; j < row; j++)
            {
                GameObject grid = Instantiate(gridDebug);
                grid.transform.parent = visualBoard;
                grid.transform.position = new Vector2(i, j);
                //flip = !flip;
                grid.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                visualgrid[i, j] = grid.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void TestCombo()
    {
        CheckStreak(true);
        string comboText = comboScore[3].Item2;
        if (currentStreak > 0)
        {
            comboText += "\n" + "STREAK +" + 10*currentStreak;
        }
        comboTextEffect.gameObject.SetActive(true);
        comboTextEffect.SetText(comboText, Vector3.zero);
    }
    private void SpawnNewBlocks()
    {
        for (int i = 0; i < 3; i++)
        {
            Sprite visual = randomSprite[Random.Range(0, randomSprite.Count)];
            Shape newShape = Instantiate(spawnableBlock);
            newShape.FeedData(ezPack.spawnableData[Random.Range(0, ezPack.spawnableData.Count)]);
            newShape.transform.parent = blockPosition[i];
            newShape.transform.localPosition = Vector3.zero;
            for (int j = 0; j < newShape._childBlock.Count; j++)
            {
                newShape._childBlock[j].GetComponent<SpriteRenderer>().sprite = visual;
            }

            currentBlock.Add(newShape);
        }
    }

#if UNITY_EDITOR
    public void GetNearestGridPosition(Vector2 position)
    {
        float minDistance = float.MaxValue;

        for (int x = 0; x < col; x++)
        {
            for (int y = 0; y < row; y++)
            {
                float distance = Vector2.Distance(position, new Vector2(x, y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    visualgrid[x, y].color = new Color(visualgrid[x, y].color.r, visualgrid[x, y].color.g,
                        visualgrid[x, y].color.b, 0.2f);
                }
            }
        }

    }
    
#endif


    public void Retry()
    {
        currentScore = 0;
        scoreText.text = "000000000";
        string formattedScore = currentScore.ToString("D9");
        scoreText.text = formattedScore;
        loseUI.SetActive(false);
        foreach (var child in currentBlock)
        {
            Destroy(child.gameObject);
        }
        currentBlock = new List<Shape>();
        foreach (var child in cubeGrid)
        {
            Destroy(child);
        }
        cubeGrid = new GameObject[col, row];
        dataGrid = new int[col, row];
        SpawnNewBlocks();
    }


    // Using BurstCompile to compile a Job with burst
    // Set CompileSynchronously to true to make sure that the method will not be compiled asynchronously
    // but on the first schedule
    [BurstCompile(CompileSynchronously = true)]
    public struct CheckShapePlacementJob : IJob
    {
        public int boardWidth;
        public int boardHeight;
        public int shapeWidth;
        public int shapeHeight;

        [ReadOnly] public NativeArray<int> boardArray;

        [ReadOnly] public NativeArray<int> shapeArray;

        [WriteOnly] public NativeArray<bool> result;

        public void Execute()
        {
            //try putting shape at all position on grid
            for (int y = 0; y <= boardHeight - shapeHeight; y++)
            {
                for (int x = 0; x <= boardWidth - shapeWidth; x++)
                {

                    bool isvalid = true;
                    //checking all the child element get overlap on the grid
                    //if(one child is overlap skip to the next position on board
                    for (int shapeY = 0; shapeY < shapeHeight; shapeY++)
                    {
                        for (int shapeX = 0; shapeX < shapeWidth; shapeX++)
                        {
                            int boardIndex = (y + shapeY) * boardWidth + (x + shapeX);
                            int shapeIndex = shapeY * shapeWidth + shapeX;

                            //only checking child value of 1
                            if (shapeArray[shapeIndex] == 1)
                            {
                                //equal value mean overlap
                                if (shapeArray[shapeIndex] == boardArray[boardIndex])
                                {
                                    isvalid = false;
                                    break;
                                }
                            }
                        }

                        if (isvalid)
                        {
                            result[0] = isvalid;
                            break;
                        }
                    }
                }
            }

            result[0] = false;
        }
    }

    void Convert2DArrayToNativeArray(int[,] source, out NativeArray<int> destination)
    {
        int width = source.GetLength(0);
        int height = source.GetLength(1);
        destination = new NativeArray<int>(width * height, Allocator.TempJob);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                destination[y * width + x] = source[x, y];
            }
        }
    }


private void CheckGameOver()
{
    bool isGameOver = true;
    // Iterate through each shape to check
    foreach (var child in currentBlock)
    {
        bool canPlaceShape = false;

        // Iterate through each position on the board
        for (int x = 0; x <= col - child.boardArray.GetLength(0); x++)
        {
            for (int y = 0; y <= row - child.boardArray.GetLength(1); y++)
            {
                bool canPlaceCurrentPosition = true;

                // Check if the current position can fit the shape
                for (int i = 0; i < child.boardArray.GetLength(0); i++)
                {
                    for (int j = 0; j < child.boardArray.GetLength(1); j++)
                    {
                        // Check if placing this shape segment would overlap with an existing segment
                        if (child.boardArray[i, j] == 1)
                        {
                            int boardX = x + i;
                            int boardY = y + j;

                            // Check if out of bounds
                            if (boardX >= col || boardX < 0 || boardY >= row || boardY < 0)
                            {
                                continue;
                            }

                            // Check for overlap
                            if (dataGrid[boardX, boardY] == 1)
                            {
                                canPlaceCurrentPosition = false;
                                break;
                            }
                        }
                    }
                    // If one child is overlap
                    if (!canPlaceCurrentPosition)
                    {
                        break;
                    }
                }

                // If this shape can be placed, mark it and break out of the loop
                if (canPlaceCurrentPosition)
                {
                    canPlaceShape = true;
                    break;
                }
            }

            if (canPlaceShape)
            {
                break;
            }
        }

        // If at least one shape can be placed, game is not over
        if (canPlaceShape)
        {
            isGameOver = false;
            break;
        }
    }

    // Check if game is over
    if (isGameOver)
    {
        loseUI.SetActive(true);
    }
}

    public void Mute()
    {
        isMute = !isMute;
        if (isMute)
        {
            muteText.text = "Unmute";
            _mixer.SetFloat("SFXvol", -80f);
        }
        else
        {
            muteText.text = "Mute";
            _mixer.SetFloat("SFXvol", 0);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fontSize = 30;
            for (int i = 0; i < dataGrid.GetLength(0); i++)
            {
                for (int j = 0; j < dataGrid.GetLength(1); j++)
                {
                    Handles.Label(new Vector3(i,j), Convert.ToInt32(dataGrid[i,j]) + "", style);
                }
            }
        
    }
#endif

}
