using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private BoxCollider2D col;
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("HIT");
        _renderer.color = new Color(_renderer.color.r,_renderer.color.g,_renderer.color.b,0.2f);

    }
    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("HIT");
        _renderer.color = new Color(_renderer.color.r,_renderer.color.g,_renderer.color.b,0.2f);

    }
    private void OnTriggerExit2D(Collider2D other)
    {
        _renderer.color = new Color(_renderer.color.r,_renderer.color.g,_renderer.color.b,0f);
    }

    // private void OnCollisionEnter2D(Collision2D other)
    // {
    //     Debug.Log("HIT");
    // }
    //
    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     _renderer.color = new Color(_renderer.color.r,_renderer.color.g,_renderer.color.b,0f);
    // }
}
