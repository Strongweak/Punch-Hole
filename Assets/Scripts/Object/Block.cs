using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Block : MonoBehaviour
{
    public EffectType _effectType;
    private Material _mat;
    private MaterialPropertyBlock _materialPropertyBlock;
    
    [Header("Custom physic")]
    [Header("Physic")]
    [SerializeField] private float _gravity;
    [SerializeField] private float _capFallVelocity = 60f;
    [SerializeField] private float _drag = 0.1f;
    [Header("Angular")]
    [SerializeField] private float _AngularDrag = 0.1f;
    public bool _isStatic;
    private Vector2 velocity;
    private Vector3 angularVel;
    private void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
    }
    private void Start()
    {
        _mat = GetComponent<SpriteRenderer>().material;
    }
    public void SetupEffectVisual()
    {
        switch (_effectType)
        {
            case EffectType.Normal:
                break;
            case EffectType.Bomb:
                break;
            case EffectType.Cross:
                break;
            case EffectType.Seeker:
                break;
            case EffectType.Rock:
                break;
            case EffectType.Weight:
                break;
            case EffectType.Weakpoint:
                break;
            case EffectType.Double:
                break;
            case EffectType.Bonus:
                break;
            default:
                //normal effect
                break;
        }
    }

    private void Update()
    {
        if (_isStatic)
        {
            velocity = Vector2.zero;
            angularVel = Vector3.zero;
            return;
        }
        velocity.y += _gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, -_capFallVelocity, _capFallVelocity);
        transform.Translate(velocity * Time.deltaTime);
        transform.Rotate(angularVel * Time.deltaTime);
        velocity  *= 1 - Time.deltaTime * _drag;
        angularVel *= 1 - Time.deltaTime * _AngularDrag;
    }

    public void AddForce(Vector2 direction)
    {
        velocity += direction;
    }

    public void AddTorque(Vector3 direction)
    {
        angularVel += direction;
    }
    private void OnDisable()
    {
        _isStatic = true;
        velocity = Vector2.zero;
        angularVel = Vector3.zero;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bound"))
        {
            gameObject.SetActive(false);
        }
    }
}

/// <summary>
/// Normal: duh
/// Bomb: explode and destroy 8 neighbour cells
/// Cross: destroy perpendicular line
/// Seeker: cell being clear and find the line having fewest empty space and move there, cause chain reaction
/// Rock: Must be clear 2 times
/// Weight: Gradually move along gravity, destroy the next cell (for boss)
/// Weakpoint: Enemies weak point, x2 damage when cleared (can be use for Fly, Moth...)
/// Double: x2 damage when cleared
/// Bonus: Score bonus
/// </summary>
public enum EffectType
{
    Normal,
    Bomb,
    Cross,
    Seeker,
    Rock,
    Weight,
    Weakpoint,
    Double,
    Bonus,
}

public enum Attribute{
    
}