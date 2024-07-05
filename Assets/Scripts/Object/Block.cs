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
    [SerializeField] private const float skrink = 0.7f;
    [SerializeField] private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        Tween.Scale(transform,originalScale * skrink,0.2f,Ease.OutBack);
    }

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

    public void OnPointerDown(PointerEventData eventData)
    {
        Tween.Scale(transform,originalScale,0.2f,Ease.OutExpo);
        Debug.Log("Hold");
    }
    
    //<TODO> check if can be placed , destroy the parent, release the child to the grid, if not return to the old position
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Release");
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
    }
}
