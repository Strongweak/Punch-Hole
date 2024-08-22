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
    public static int Row = 8;
    public static int Col = 8;
    public static SpriteRenderer[,] Visualgrid;
    public static (SpriteRenderer, MaterialPropertyBlock)[,] HighlightGrid;
    public static (Block, int)[,] DataGrid;
    private GameObject _gridContainer;

    private GameObject _highlightGridContainer;
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
        if (_gridContainer != null)
        {
            Destroy(_gridContainer);
        }

        Row = 9;
        Col = 9;
        Visualgrid = new SpriteRenderer[Col, Row];
        HighlightGrid = new (SpriteRenderer, MaterialPropertyBlock)[Col, Row];
        DataGrid = new (Block, int)[Col, Row];

        _gridContainer = new GameObject();
        _gridContainer.name = "Grid container";
        _gridContainer.AddComponent<SortingGroup>().sortingOrder = -2;

        _highlightGridContainer = new GameObject();
        _highlightGridContainer.name = "highlight container";
        _highlightGridContainer.AddComponent<SortingGroup>().sortingOrder = -1;

        for (int i = 0; i < Col; i++)
        {
            for (int j = 0; j < Row; j++)
            {
                //grid
                GameObject grid = Instantiate(cell);
                grid.transform.parent = _gridContainer.transform;
                grid.transform.position = new Vector2(i, j);
                grid.GetComponent<SpriteRenderer>().color = gridColor;
                Visualgrid[i, j] = grid.GetComponent<SpriteRenderer>();

                //highlight
                GameObject high = Instantiate(respondCell);
                high.transform.parent = _highlightGridContainer.transform;
                high.transform.position = new Vector2(i, j);
                // Create a new MaterialPropertyBlock
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                propertyBlock.SetColor("_Color", normalColor);
                high.GetComponent<SpriteRenderer>().SetPropertyBlock(propertyBlock);
                HighlightGrid[i, j].Item1 = high.GetComponent<SpriteRenderer>();
                HighlightGrid[i, j].Item2 = propertyBlock;
            }
        }

        yield return new WaitForEndOfFrame();
        //Adjust camera to fit the board
        var bounds = new Bounds();
        foreach (var col in Visualgrid)
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
        if (_gridContainer != null)
        {
            Destroy(_gridContainer);
        }

        Row = y;
        Col = x;
        Visualgrid = new SpriteRenderer[Col, Row];
        _gridContainer = new GameObject();
        //bool flip = false;
        for (int i = 0; i < Col; i++)
        {
            //flip = !flip;
            for (int j = 0; j < Row; j++)
            {
                GameObject grid = Instantiate(cell);
                grid.transform.parent = _gridContainer.transform;
                grid.transform.position = new Vector2(i, j);
                //flip = !flip;
                grid.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                Visualgrid[i, j] = grid.GetComponent<SpriteRenderer>();
            }
        }
        //Adjust camera to fit the board
        var bounds = new Bounds();
        foreach (var col in Visualgrid)
        {
            bounds.Encapsulate(col.GetComponent<Collider2D>().bounds);
        }

        bounds.Expand(cameraExpand);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * cam.pixelHeight / cam.pixelWidth;

        cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
        cam.transform.position = bounds.center + new Vector3(0, 0, -10) + (Vector3)offset;

    }

    public void ClearCell(int x, int y)
    {
        DataGrid[x, y].Item2 = 0;
        //Destroy(DataGrid[x, y].Item1.gameObject);
        DataGrid[x, y].Item1?.gameObject.SetActive(false);
        DataGrid[x, y].Item1 = null;
    }
    public void CheckAddToGrid(Shape shape)
    {
        // check if can be put on position
        foreach (var children in shape._childBlock)
        {
            //get children position
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            //not validate to put in
            if (InBounds(roundedX, roundedY) == false || DataGrid[roundedX, roundedY].Item2 == 1)
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
            children.transform.parent = _gridContainer.transform;
            DataGrid[roundedX, roundedY].Item1 = children;
            DataGrid[roundedX, roundedY].Item2 = 1;
            children.transform.position = new Vector3(roundedX, roundedY);
            //UpdateScore(1);
        }

        //_au.PlayOneShot(placeSound);
        GameplayManager.Instance.currentBlock.Remove(shape);
        Destroy(shape.gameObject);
        ClearHighLight();
        Observer.Instance.TriggerEvent(ObserverConstant.OnPlacingShape);
    }

    /// <summary>
    /// check when block is put in which slot, start checking row and column of it
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckLineAndRow()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.Holdup);
        bool haveClear = false;
        int lineClear = 0;
        float totalDamage = 0;
        //checking
        for (int i = Row - 1; i >= 0; i--)
        {
            if (HasLine(i, false))
            {
                for (int j = 0; j < Col; j++)
                {
                    ClearCell(j, i);
                    totalDamage += GameplayManager.Instance.plusScore + (GameplayManager.Instance.currentStreak * 10);
                }

                lineClear++;
                haveClear = true;
            }
        }

        for (int i = Col - 1; i >= 0; i--)
        {
            if (HasLine(i, true))
            {
                for (int j = 0; j < Row; j++)
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
                yield return GameplayManager._delay;
            }
        }
        else
        {
            yield return null;
        }

        //Observer.Instance.TriggerEvent(ObserverConstant.OnPlacingShape);
        //Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.PlayerTurn);
    }

    public IEnumerator CheckLineAndRowAfterUpdate()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.Holdup);
        bool haveClear = false;
        int lineClear = 0;
        float totalDamage = 0;
        //checking
        for (int i = Row - 1; i >= 0; i--)
        {
            if (HasLine(i, false))
            {
                for (int j = 0; j < Col; j++)
                {
                    ClearCell(j, i);
                    totalDamage += GameplayManager.Instance.plusScore + (GameplayManager.Instance.currentStreak * 10);
                }

                lineClear++;
                haveClear = true;
            }
        }

        for (int i = Col - 1; i >= 0; i--)
        {
            if (HasLine(i, true))
            {
                for (int j = 0; j < Row; j++)
                {
                    ClearCell(i,j);
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
                enemy.Damage(totalDamage);
            }

            yield return GameplayManager._delay;
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
            for (int j = 0; j < Row; j++)
            {
                //if one of them not have an object in the row, the row can not delete
                if (DataGrid[index, j].Item1 == null)
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            //check each index in the row
            for (int j = 0; j < Col; j++)
            {
                //if one of them not have an object in the row, the row can not delete
                if (DataGrid[j, index].Item1 == null)
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
    void AddToQueue(int index, List<Block> queue, bool isColumn)
    {
        if (isColumn)
        {
            //for every element in row, delete all of them
            for (int j = 0; j < Row; j++)
            {
                queue.Add(DataGrid[index, j].Item1);
                DataGrid[index, j].Item2 = 0;
                //cubeGrid[col, j] = null;
            }
        }
        else
        {
            //for every element in row, delete all of them
            for (int j = 0; j < Col; j++)
            {
                queue.Add(DataGrid[j, index].Item1);
                DataGrid[j, index].Item2 = 0;
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
        if (x < 0 || x >= Col)
        {
            return false;
        }

        if (y < 0 || y >= Row)
        {
            return false;
        }

        return true;
    }

    //very duh
    public void ClearHighLight()
    {
        foreach (var crap in HighlightGrid)
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

        HighlightGrid[roundedX, roundedY].Item2.SetColor("_Color", ghostColor);
        HighlightGrid[roundedX, roundedY].Item1.SetPropertyBlock(HighlightGrid[roundedX, roundedY].Item2);
    }

    public void CheckOutOfMove()
    {
        bool isOutOfPossibleMove = true;
        // Iterate through each shape to check
        foreach (var child in GameplayManager.Instance.currentBlock)
        {
            bool canPlaceShape = false;

            // Iterate through each position on the board
            for (int x = 0; x <= Col - child.boardArray.GetLength(0); x++)
            {
                for (int y = 0; y <= Row - child.boardArray.GetLength(1); y++)
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
                                if (boardX >= Col || boardX < 0 || boardY >= Row || boardY < 0)
                                {
                                    continue;
                                }

                                // Check for overlap
                                if (DataGrid[boardX, boardY].Item2 == 1)
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

    public IEnumerator ClearBoard()
    {
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.Holdup);
        Debug.Log("OUT OF MOVE");
        GameplayManager.Instance.ShakeCamera(2f, 0.5f, 10f);
        yield return new WaitForSeconds(GameplayManager._gameplaySpeed * 2f);

        for (int i = 0; i < Col; i++)
        {
            for (int j = 0; j < Row; j++)
            {
                ClearCell(i,j);
                yield return new WaitForSeconds(GameplayManager._gameplaySpeed / 10f);
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
        if (DataGrid != null)
        {
            for (int i = 0; i < DataGrid.GetLength(0); i++)
            {
                for (int j = 0; j < DataGrid.GetLength(1); j++)
                {
                    Handles.Label(new Vector3(i - 0.5f, j + 0.5f), Convert.ToInt32(DataGrid[i, j].Item2) + "", style);
                }
            }
        }
    }
#endif
}