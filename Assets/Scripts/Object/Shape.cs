using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Shape : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public List<Block> _childBlock;
    public ShapeData _data;
    public int[,] boardArray;
    public AudioSource au;
    [SerializeField] private AudioClip pickSound;
    private const float skrink = 0.7f;
    private Vector3 originalScale;
    [SerializeField] private Sprite defaultSprite;
    private Tween _currentTween;
    private void OnEnable()
    {
        // if (_data != null)
        // {
        //     boardArray = new int[_data.row, _data.col];
        //     for (int i = 0; i < _data.row; i++)
        //     {
        //         for (int j = 0; j < _data.col; j++)
        //         {
        //             boardArray[i, j] = Convert.ToInt32(_data.board[i].col[j]);
        //         }
        //     }
        // }
    }

    private void Start()
    {
        au = GetComponent<AudioSource>();
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        Tween.Scale(transform, originalScale * skrink, 0.2f, Ease.OutBack);

    }
    public void FeedData(ShapeData data)
    {
        _data = data;
        boardArray = new int[_data.row, _data.col];
        for (int i = 0; i < _data.row; i++)
        {
            for (int j = 0; j < _data.col; j++)
            {
                boardArray[i, j] = Convert.ToInt32(_data.board[i].col[j]);
            }
        }
        SetupVisual();
    }
    public void SetupVisual()
    {
        _childBlock = new List<Block>();
        if (_data != null)
        {
            for (int i = 0; i < _data.row; i++)
            {
                for (int j = 0; j < _data.col; j++)
                {
                    if (_data.board[i].col[j])
                    {
                        GameObject newBlock = ObjectPool.instance.GetObject("Block");
                        newBlock.transform.parent = this.transform;
                        newBlock.transform.localPosition = new Vector3(i, j, 0) - new Vector3(1, 1, 0);
                        _childBlock.Add(newBlock.GetComponent<Block>());
                    }
                }
            }
        }
    }
    // #if UNITY_EDITOR
    //     private void OnValidate()
    //     {
    //         visual = transform.GetComponentsInChildren<SpriteRenderer>().ToList();
    //         foreach (var child in visual)
    //         {
    //             if (child.GetComponent<BoxCollider2D>() == null)
    //             {
    //                 child.AddComponent<BoxCollider2D>();
    //             }
    //
    //             if (defaultSprite != null)
    //             {
    //                 child.sprite = defaultSprite;
    //             }
    //         }
    //     }
    // #endif

    private Vector2 offset;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameplayManager.Instance._state != GameState.PlayerTurn)
        {
            return;
        }
        au.PlayOneShot(pickSound);
        Tween.StopAll(onTarget: this);
        Tween.Scale(transform, originalScale, 0.2f, Ease.OutExpo);
        //Debug.Log("Hold");
        Vector2 position = Camera.main.ScreenToWorldPoint(eventData.position);
        offset = position - (Vector2)transform.position + Vector2.up * 3f;
        //Tween.Position(transform,position,0.2f,Ease.OutExpo);
    }

    //<TODO> check if can be placed , destroy the parent, release the child to the grid, if not return to the old position
    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameplayManager.Instance._state != GameState.PlayerTurn)
        {
            return;
        }
        //GameManager.instance.CheckAddToGrid(this);
        GridSystem.Instance.CheckAddToGrid(this);
        //ReturnOriginalSize();
        GridSystem.Instance.ClearHighLight();
    }

    public void ReturnOriginalSize()
    {
        Tween.Scale(transform, originalScale * skrink, 0.2f, Ease.OutExpo);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (GameplayManager.Instance._state != GameState.PlayerTurn)
        {
            return;
        }
        GridSystem.Instance.ClearHighLight();
        Vector2 position = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position = position - offset + Vector2.up * 3f;
        foreach (var child in _childBlock)
        {
            //GameManager.instance.HighLightCloset(child.transform);
            GridSystem.Instance.HighLightCloset(child.transform);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_childBlock.Count > 0)
        {
            foreach (var child in _childBlock)
            {
                int roundedX = Mathf.RoundToInt(child.transform.position.x);
                int roundedY = Mathf.RoundToInt(child.transform.position.y);
                Handles.Label(child.transform.position, "[" + roundedX + "," + roundedY + "]");
            }
        }
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontSize = 30;

        if (_data != null)
        {
            for (int i = 0; i < boardArray.GetLength(0); i++)
            {
                for (int j = 0; j < boardArray.GetLength(1); j++)
                {
                    Handles.Label(transform.position + new Vector3(i, j), Convert.ToInt32(boardArray[i, j]) + "", style);
                }
            }
        }
    }
#endif
}
