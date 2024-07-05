using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public static int height = 8;
    public static int width = 8;
    //public static Transform[,] gridData = new Transform[width, height];
    public static SpriteRenderer[,] visualgrid = new SpriteRenderer[width, height];
    public static GameObject[,] cubeGrid = new GameObject[width, height];

    [Header("Gameplay")] 
    [SerializeField] private List<Block> spawnableBlock;
    [SerializeField] private List<Sprite> randomSprite;
    [SerializeField] private List<Transform> blockPosition;
    [SerializeField] private List<Block> currentBlock;
    [SerializeField] private bool isGameOver;
    [SerializeField] private bool ispause;

    [Header("Score")]
    private Dictionary<int,int> comboScore;
    [SerializeField] private int score;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int beforeStartStreak = 3;
    [SerializeField] private int currentChain = 0;
    [SerializeField] private int currentStreak;
    private int currentScore;
    private int plusScore;
    [Header("Visual")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject gridDebug;
    [SerializeField] private Transform visualBoard;
    [SerializeField] private Transform gameBoard;
    [SerializeField] private Color ghostColor;
    [SerializeField] private Color invalidColor;
    private void Awake()
    {
        instance = this;
        DebugGrid();
        comboScore = new Dictionary<int, int>();
        comboScore.Add(1,10);
        comboScore.Add(2,30);
        comboScore.Add(3,60);
        comboScore.Add(4,100);
        comboScore.Add(5,150);
        comboScore.Add(6,300);
        currentScore = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "000000000";
        ispause = false;
        isGameOver = false;
        SpawnNewBlocks();
    }
    
    public void CheckAddToGrid(Block block)
    {
        // check if can be put on position
        foreach (var children in block.visual)
        {
            //get children position
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            //not validate to put in
            if (InBounds(roundedX, roundedY) == false || cubeGrid[roundedX, roundedY] != null)
            {
                ClearHighLight();
                block.ReturnOriginalSize();
                Tween.LocalPosition(block.transform, Vector2.zero, 0.4f, Ease.OutQuart);
                return;
            }
        }
        // add to the board
        foreach (var children in block.visual)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            //add to board
            children.transform.parent = gameBoard;
            cubeGrid[roundedX, roundedY] = children.gameObject;
            children.transform.position = new Vector3(roundedX, roundedY);
            UpdateScore(1);
        }
        //un parent
        Destroy(block.gameObject);
        //the flavor
        CheckLineAndRow();
        ClearHighLight();
        currentBlock.Remove(block);
        if (currentBlock.Count == 0)
        {
            SpawnNewBlocks();
        }
    }
    //
    //<SUMMARY> check when block out of the board or not
    //
    private bool InBounds (int x, int y)
    {
        if (x < 0 || x >= width)
        {
            return false;
        }
        
        if (y < 0 || y >= height)
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
        if (InBounds(roundedX,roundedY) == false)
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
    public void CheckLineAndRow()
    {
        bool haveClear = false;
        int lineClear= 0;
        //store all line
        List<List<GameObject>> howDoIGetThisFar = new List<List<GameObject>>();
        //checking
        for (int i = height-1; i >= 0 ; i--)
        {
            List<GameObject> fuck = new List<GameObject>();
            if (HasRow(i))
            {
                AddRowToQueue(i, fuck);
                lineClear++;
                haveClear = true;
            }
            howDoIGetThisFar.Add(fuck);
        }
        for (int i = width-1; i >= 0 ; i--)
        {
            List<GameObject> fuck = new List<GameObject>();
            if (HasColumn(i))
            {
                AddColumnToQueue(i, fuck);
                lineClear++;
                haveClear = true;
            }
            howDoIGetThisFar.Add(fuck);
        }

        //scoring
        CheckStreak(haveClear);
        lineClear = Mathf.Clamp(lineClear, 1, 6);
        plusScore = comboScore[lineClear];
        //deleting
        foreach (var list in howDoIGetThisFar)
        {
             StartCoroutine(DeleteAnimation(list));
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
            yield return new WaitForSeconds(0.03f);
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
        for (int j = 0; j < width; j++)
        {
            queue.Add(cubeGrid[j, row]);
            //cubeGrid[j, row] = null;
        }
        Debug.Log("Found row " + row);
    }

    void AddColumnToQueue(int col, List<GameObject> queue)
    {
        //for every element in row, delete all of them
        for (int j = 0; j < height; j++)
        {
            queue.Add(cubeGrid[col, j]);
            //cubeGrid[col, j] = null;
        }
        Debug.Log("Found col " + col);
    }
    bool HasColumn(int i)
    {
        //check each index in the row
        for(int j = 0; j < height; j++)
        {
            //if one of them not have an object in the row, the row can not delete
            if(cubeGrid[i,j] == null)
            {
                return false;
            }
        }
        return true;
    }
    bool HasRow(int i)
    {
        //check each index in the row
        for(int j = 0; j < width; j++)
        {
            //if one of them not have an object in the row, the row can not delete
            if(cubeGrid[j,i] == null)
            {
                return false;
            }
        }
        return true;
    }

    public void DebugGrid()
    {
        //bool flip = false;
        for (int i = 0; i < width; i++)
        {
            //flip = !flip;
            for (int j = 0; j < height; j++)
            {
                GameObject grid = Instantiate(gridDebug);
                grid.transform.parent = visualBoard;
                grid.transform.position = new Vector2(i, j);
                //flip = !flip;
                grid.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
                visualgrid[i, j] = grid.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void GenerateCross()
    {
        for (int i = 0; i < width; i++)
        {
            if (i == 4)
            {
                continue;
            }
            GameObject newshit = Instantiate(gridDebug);
            newshit.transform.position = new Vector2(i,4);
            cubeGrid[i, 4] = newshit.gameObject;

        }
        for (int i = 0; i < height; i++)
        {
            if (i == 4)
            {
                continue;
            }
            GameObject newshit = Instantiate(gridDebug);
            newshit.transform.position = new Vector2(4,i);
            cubeGrid[4,i] = newshit.gameObject;

        }
    }

    private void SpawnNewBlocks()
    {
        for (int i = 0; i < 3; i++)
        {
            Sprite visual = randomSprite[Random.Range(0, randomSprite.Count)];
            Block newBlock = Instantiate(spawnableBlock[Random.Range(0,spawnableBlock.Count)]);
            newBlock.transform.parent = blockPosition[i];
            newBlock.transform.localPosition = Vector3.zero;
            for (int j = 0; j < newBlock.visual.Count; j++)
            {
                newBlock.visual[j].GetComponent<SpriteRenderer>().sprite = visual;
            }
            currentBlock.Add(newBlock);
        }
    }
    
    public void GetNearestGridPosition(Vector2 position)
    {
        float minDistance = float.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distance = Vector2.Distance(position, new Vector2(x, y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    visualgrid[x,y].color = new Color(visualgrid[x,y].color.r,visualgrid[x,y].color.g,visualgrid[x,y].color.b,0.2f);
                }
            }
        }
        
    }
    public void CalculateCameraCenter()
    {
        var horizontal = width * cam.pixelHeight / cam.pixelWidth;
        var vertical = height;

        var size = Mathf.Max(horizontal, vertical) * 0.4f;
        Vector3 center = new Vector3(width / 2f, height / 2f, -10);


        cam.transform.position = center;
        cam.orthographicSize = size;
    }

    public void Retry()
    {
        score = 0;
        scoreText.text = "000000000";
        string formattedScore = currentScore.ToString("D9");
        scoreText.text = formattedScore;
    }
}



[Serializable]
public enum Combo
{
    nothing,
    Holding
}