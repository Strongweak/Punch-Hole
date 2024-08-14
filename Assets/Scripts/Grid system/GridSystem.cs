using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;
    [SerializeField] private GameObject cell;
    [SerializeField] private GameObject respondCell;
    [SerializeField] private Camera cam;
    [SerializeField] private float cameraExpand = 1f;
    [SerializeField] private Vector2 offset;
    public static int row = 8;
    public static int col = 8;
    public static SpriteRenderer[,] visualgrid;
    public static (SpriteRenderer, MaterialPropertyBlock)[,] highlightGrid;
    public static (GameObject, int)[,] dataGrid;
    private GameObject gridContainer;

    private GameObject highlightGridContainer;

    //private GameObject dangerIndicatorGridContainer;
    //need to rework this
    [SerializeField] private Color ghostColor;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color gridColor;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator GenerateGrid()
    {
        if (gridContainer != null)
        {
            Destroy(gridContainer);
        }

        row = 9;
        col = 9;
        visualgrid = new SpriteRenderer[col, row];
        highlightGrid = new (SpriteRenderer, MaterialPropertyBlock)[col, row];
        dataGrid = new (GameObject, int)[col, row];

        gridContainer = new GameObject();
        gridContainer.name = "Grid container";
        gridContainer.AddComponent<SortingGroup>().sortingOrder = -2;

        highlightGridContainer = new GameObject();
        highlightGridContainer.name = "highlight container";
        highlightGridContainer.AddComponent<SortingGroup>().sortingOrder = -1;
        
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                //grid
                GameObject grid = Instantiate(cell);
                grid.transform.parent = gridContainer.transform;
                grid.transform.position = new Vector2(i, j);
                grid.GetComponent<SpriteRenderer>().color = gridColor;
                visualgrid[i, j] = grid.GetComponent<SpriteRenderer>();

                //highlight
                GameObject high = Instantiate(respondCell);
                high.transform.parent = highlightGridContainer.transform;
                high.transform.position = new Vector2(i, j);
                // Create a new MaterialPropertyBlock
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                propertyBlock.SetColor("_Color", normalColor);
                high.GetComponent<SpriteRenderer>().SetPropertyBlock(propertyBlock);
                highlightGrid[i, j].Item1 = high.GetComponent<SpriteRenderer>();
                highlightGrid[i, j].Item2 = propertyBlock;
            }
        }

        yield return StartCoroutine(AdjustCameraToFitGrid());
    }

    private IEnumerator AdjustCameraToFitGrid()
    {
        // Wait for the end of the frame to ensure all objects are fully initialized
        yield return new WaitForEndOfFrame();

        var bounds = new Bounds();
        foreach (var col in visualgrid)
        {
            bounds.Encapsulate(col.GetComponent<Collider2D>().bounds);
        }

        bounds.Expand(cameraExpand);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * cam.pixelHeight / cam.pixelWidth;

        cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
        cam.transform.position = bounds.center + new Vector3(0, 0, -10) + (Vector3)offset;
    }

    public void GenerateGrid(int x, int y)
    {
        if (gridContainer != null)
        {
            Destroy(gridContainer);
        }

        row = y;
        col = x;
        visualgrid = new SpriteRenderer[col, row];
        gridContainer = new GameObject();
        //bool flip = false;
        for (int i = 0; i < col; i++)
        {
            //flip = !flip;
            for (int j = 0; j < row; j++)
            {
                GameObject grid = Instantiate(cell);
                grid.transform.parent = gridContainer.transform;
                grid.transform.position = new Vector2(i, j);
                //flip = !flip;
                grid.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                visualgrid[i, j] = grid.GetComponent<SpriteRenderer>();
            }
        }

        StartCoroutine(AdjustCameraToFitGrid());
    }
    
    public void ClearCell(int x, int y)
    {
        dataGrid[x, y].Item2 = 0;
        Destroy(dataGrid[x, y].Item1);
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
            if (InBounds(roundedX, roundedY) == false || dataGrid[roundedX, roundedY].Item2 == 1)
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
            children.transform.parent = gridContainer.transform;
            dataGrid[roundedX, roundedY].Item1 = children.gameObject;
            dataGrid[roundedX, roundedY].Item2 = 1;
            children.transform.position = new Vector3(roundedX, roundedY);
            //UpdateScore(1);
        }

        //_au.PlayOneShot(placeSound);
        GameplayManager.Instance.currentBlock.Remove(block);
        Destroy(block.gameObject);
        ClearHighLight();
        //the flavor
        StartCoroutine(CheckLineAndRow());
    }

    /// <summary>
    /// check when block is put in which slot, start checking row and column of it
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckLineAndRow()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.Holdup);
        bool haveClear = false;
        int lineClear = 0;
        float totalDamage = 0;
        //checking
        for (int i = row - 1; i >= 0; i--)
        {
            if (HasLine(i, false))
            {
                for (int j = 0; j < col; j++)
                {
                    ClearCell(j, i);
                    totalDamage += GameplayManager.Instance.plusScore + (GameplayManager.Instance.currentStreak * 10);
                }

                lineClear++;
                haveClear = true;
            }
        }

        for (int i = col - 1; i >= 0; i--)
        {
            if (HasLine(i, true))
            {
                for (int j = 0; j < row; j++)
                {
                    ClearCell(i, j);
                    totalDamage += GameplayManager.Instance.plusScore + (GameplayManager.Instance.currentStreak * 10);
                }

                lineClear++;
                haveClear = true;
            }
        }

        if (haveClear)
        {
            foreach (var enemy in GameplayManager.Instance.currentEnemies)
            {
                yield return StartCoroutine(enemy.Damage(totalDamage));
                yield return new WaitForSeconds(GameplayManager.Instance.gameplaySpeed);
            }
        }
        else
        {
            yield return null;
        }

        Observer.Instance.TriggerEvent(ObserverConstant.OnPlayerMove);
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.PlayerTurn);
    }

    public IEnumerator CheckLineAndRowAfterUpdate()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.Holdup);
        bool haveClear = false;
        int lineClear = 0;
        float totalDamage = 0;
        //checking
        for (int i = row - 1; i >= 0; i--)
        {
            if (HasLine(i, false))
            {
                for (int j = 0; j < col; j++)
                {
                    dataGrid[j, i].Item2 = 0;
                    totalDamage += GameplayManager.Instance.plusScore + (GameplayManager.Instance.currentStreak * 10);
                    Destroy(dataGrid[j, i].Item1);
                }

                lineClear++;
                haveClear = true;
            }
        }

        for (int i = col - 1; i >= 0; i--)
        {
            if (HasLine(i, true))
            {
                for (int j = 0; j < row; j++)
                {
                    dataGrid[i, j].Item2 = 0;
                    totalDamage += GameplayManager.Instance.plusScore + (GameplayManager.Instance.currentStreak * 10);
                    Destroy(dataGrid[i, j].Item1);
                }

                lineClear++;
                haveClear = true;
            }
        }

        if (haveClear)
        {
            foreach (var enemy in GameplayManager.Instance.currentEnemies)
            {
                enemy.Damage(totalDamage);
            }

            yield return new WaitForSeconds(GameplayManager.Instance.gameplaySpeed);
        }
        else
        {
            yield return null;
        }

        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.PlayerTurn);
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
                //UpdateScore(plusScore + (currentStreak * 10));
                Destroy(child);
            });
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// Check if found any line can be clear
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isColumn"></param>
    /// <returns></returns>
    bool HasLine(int index, bool isColumn)
    {
        if (isColumn)
        {
            //check each index in the row
            for (int j = 0; j < row; j++)
            {
                //if one of them not have an object in the row, the row can not delete
                if (dataGrid[index, j].Item1 == null)
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            //check each index in the row
            for (int j = 0; j < col; j++)
            {
                //if one of them not have an object in the row, the row can not delete
                if (dataGrid[j, index].Item1 == null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Store all clearable block to ready to delete
    /// </summary>
    /// <param name="index"></param>
    /// <param name="queue"></param>
    /// <param name="isColumn"></param>
    void AddToQueue(int index, List<GameObject> queue, bool isColumn)
    {
        if (isColumn)
        {
            //for every element in row, delete all of them
            for (int j = 0; j < row; j++)
            {
                queue.Add(dataGrid[index, j].Item1);
                dataGrid[index, j].Item2 = 0;
                //cubeGrid[col, j] = null;
            }
        }
        else
        {
            //for every element in row, delete all of them
            for (int j = 0; j < col; j++)
            {
                queue.Add(dataGrid[j, index].Item1);
                dataGrid[j, index].Item2 = 0;
                //cubeGrid[j, row] = null;
            }
        }
    }

    /// <summary>
    /// check when block out of the board or not
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
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

    //very duh
    public void ClearHighLight()
    {
        foreach (var crap in highlightGrid)
        {
            crap.Item2.SetColor("_Color", normalColor);
            crap.Item1.SetPropertyBlock(crap.Item2);
        }
    }

    public void HighLightCloset(Transform cube)
    {
        int roundedX = Mathf.RoundToInt(cube.position.x);
        int roundedY = Mathf.RoundToInt(cube.position.y);
        if (InBounds(roundedX, roundedY) == false)
        {
            return;
        }

        highlightGrid[roundedX, roundedY].Item2.SetColor("_Color", ghostColor);
        highlightGrid[roundedX, roundedY].Item1.SetPropertyBlock(highlightGrid[roundedX, roundedY].Item2);
    }

    public void CheckOutOfMove()
    {
        bool isOutOfPossibleMove = true;
        // Iterate through each shape to check
        foreach (var child in GameplayManager.Instance.currentBlock)
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
                                if (dataGrid[boardX, boardY].Item2 == 1)
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
                isOutOfPossibleMove = false;
                break;
            }
        }

        // Check if out of move
        if (isOutOfPossibleMove)
        {
            StartCoroutine(ClearBoard());
        }
    }

    private IEnumerator ClearBoard()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.Holdup);
        Debug.Log("OUT OF MOVE");
        GameplayManager.Instance.ShakeCamera(2f, 0.5f, 10f);
        yield return new WaitForSeconds(GameplayManager.Instance.gameplaySpeed * 2f);

        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                dataGrid[i, j].Item2 = 0;
                Destroy(dataGrid[i, j].Item1);
                yield return new WaitForSeconds(GameplayManager.Instance.gameplaySpeed / 10f);
            }
        }

        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.PlayerTurn);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fontSize = 30;
        if (dataGrid != null)
        {
            for (int i = 0; i < dataGrid.GetLength(0); i++)
            {
                for (int j = 0; j < dataGrid.GetLength(1); j++)
                {
                    Handles.Label(new Vector3(i - 0.5f, j + 0.5f), Convert.ToInt32(dataGrid[i, j].Item2) + "", style);
                }
            }
        }
    }
#endif
}