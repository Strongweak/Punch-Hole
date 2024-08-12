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
    [SerializeField] private GameObject informationBox;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        enemyParent = transform.parent.GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        Tween.Scale(transform,originalScale * shrink,0.7f,Ease.OutElastic);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        informationBox.SetActive(true);
        Tween.Scale(transform,originalScale,0.2f,Ease.OutExpo);
        enemyParent.HeadupTelegraph();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        informationBox.SetActive(false);
        //_rectTransform.anchoredPosition = Vector2.zero;
        Tween.UIAnchoredPosition(_rectTransform,Vector2.zero, 1f, Ease.OutElastic);
        Tween.Scale(transform,originalScale * shrink,0.2f,Ease.OutExpo);
        enemyParent.ReleaseHeadupTelegraph();
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta;
        informationBox.SetActive(false);
    }
}
