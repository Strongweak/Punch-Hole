using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Block : MonoBehaviour ,IPointerDownHandler, IPointerUpHandler,IDragHandler
{
    public List<SpriteRenderer> visual;
    public ShapeData _data;
    public int[,] boardArray;
    public AudioSource au;
    [SerializeField] private AudioClip pickSound;
    private const float skrink = 0.7f;
    private Vector3 originalScale;

    private void OnEnable()
    {
        if (_data != null)
        {
            boardArray = new int[_data.row, _data.col];

            for (int i = 0; i < _data.row; i++)
            {
                for (int j = 0; j < _data.col; j++)
                {
                    boardArray[i, j] = Convert.ToInt32(_data.board[i].col[j]);
                }
            }
        }    
    }

    private void Start()
    {
        au = GetComponent<AudioSource>();
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        Tween.Scale(transform,originalScale * skrink,0.2f,Ease.OutBack);

    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        visual = transform.GetComponentsInChildren<SpriteRenderer>().ToList();
        foreach (var child in visual)
        {
            if (child.GetComponent<BoxCollider2D>() == null)
            {
                child.AddComponent<BoxCollider2D>();
            }
        }
    }
#endif


    public void OnPointerDown(PointerEventData eventData)
    {
        au.PlayOneShot(pickSound);
        Tween.Scale(transform,originalScale,0.2f,Ease.OutExpo);
        //Debug.Log("Hold");
    }
    
    //<TODO> check if can be placed , destroy the parent, release the child to the grid, if not return to the old position
    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Release");
        GameManager.instance.CheckAddToGrid(this);
    }

    public void ReturnOriginalSize()
    {
        Tween.Scale(transform,originalScale * skrink,0.2f,Ease.OutExpo);
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out newPosition);
        GameManager.instance.ClearHighLight();

        transform.position = transform.parent.TransformPoint(newPosition);
        foreach (var child in visual)
        {
            GameManager.instance.HighLightCloset(child.transform);
        }

    }
    private void OnDrawGizmos()
    {

        foreach (var child in visual)
        {
            int roundedX = Mathf.RoundToInt(child.transform.position.x);
            int roundedY = Mathf.RoundToInt(child.transform.position.y);
            Handles.Label(child.transform.position, "[" + roundedX+ "," + roundedY +"]");
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
                    Handles.Label(transform.position + new Vector3(i,j), Convert.ToInt32(boardArray[i,j]) + "", style);
                }
            }
        }
    }
}
