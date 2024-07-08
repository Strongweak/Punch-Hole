using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class ComboTextUI : MonoBehaviour
{
    private TextMeshProUGUI _textMeshProUGUI;
    private Vector3 scale;
    private Tween currentTween;
    private void Awake()
    {
        scale = transform.localScale;
        transform.localScale = Vector3.zero;
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (currentTween.isAlive)
        {
            currentTween.Stop();
        }
    }

    public void SetText(string combo, Vector3 pos)
    {
        transform.position = pos + Vector3.up;
        _textMeshProUGUI.text = combo;
        Tween.Scale(transform, scale, 0.3f, Ease.OutElastic).OnComplete(() =>
        {
            Tween.Delay(2f, () =>
            {
                Tween.Scale(transform, Vector3.zero, 0.3f, Ease.InBack).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });
        });
    }
}
