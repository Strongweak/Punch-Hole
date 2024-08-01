using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy data")]
public class EnemySO : ScriptableObject
{
    public string name;
    public string description;
    public float health;
    public int delayAfterMove;
    public EnemyTier tier;
}

[Serializable]
public enum EnemyTier
{
    Tier1,
    Tier2,
    Tier3,
    Tier4,
    Boss,
    FinalBoss
}