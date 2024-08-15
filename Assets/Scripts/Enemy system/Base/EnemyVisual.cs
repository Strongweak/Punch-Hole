using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IDragHandler
{
    public Vector3 originalScale;
    public float shrink = 0.2f;
    [SerializeField] private Enemy enemyParent;
    private RectTransform _rectTransform;
    public Image _image;
    private Vector3 _offset;
    private bool isHolding = false;
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    private void Update()
    {
        if (!isHolding)
        {
            _rectTransform.position = Vector3.Lerp(_rectTransform.position,
                enemyParent.GetComponent<RectTransform>().position, 10f * Time.deltaTime);
            
        }
    }

    private void OnEnable()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        Tween.Scale(transform,originalScale * shrink,0.7f,Ease.OutElastic);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out position);

        Tween.StopAll(onTarget: this);
        Tween.Scale(transform,originalScale,0.2f,Ease.OutExpo);
        enemyParent.HeadupTelegraph();
        _offset = transform.parent.TransformPoint(position) - _rectTransform.position;
        _offset.z = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        //_rectTransform.anchoredPosition = Vector2.zero;
        //Tween.UIAnchoredPosition(_rectTransform,Vector2.zero, 1f, Ease.OutElastic);
        Tween.Scale(transform,originalScale * shrink,0.2f,Ease.OutExpo);
        enemyParent.ReleaseHeadupTelegraph();
    }

    public void SetupParent(Enemy e)
    {
        enemyParent = e;
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out newPosition);
        _rectTransform.position = transform.parent.TransformPoint(newPosition) - _offset;
    }
}
