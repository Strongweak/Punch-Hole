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

    [BurstCompile]
    struct Move : IJob
    {
        public NativeArray<float> Input;
        public NativeArray<float> Output;

        public void Execute()
        {
            
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