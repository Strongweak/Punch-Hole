using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stat")]
    public EnemySO enemyData;
    protected string _name;
    [SerializeField] protected float maxHealth;
    public int _delayAfterMove;
    protected EnemyTier _tier;
    public float _currentHealth;
    public int _currentMoveCount;
    public bool _isDead;
    private Material _mat;
    [Header("Respond")] 
    [SerializeField] private Image healthbarShader;
    [SerializeField] protected EnemyVisual enemyVisual;
    [SerializeField] private TextMeshProUGUI numberText;

    public Sequence currentSequence = new Sequence();
    protected virtual void Start()
    {
        _isDead = false;
        _name = enemyData.name;
        maxHealth = enemyData.health;
        _delayAfterMove = enemyData.delayAfterMove;
        _mat = new Material(healthbarShader.material);
        healthbarShader.material = _mat;
        _tier = enemyData.tier;
        _currentHealth = maxHealth;
        _currentMoveCount = _delayAfterMove;
        numberText.text = _currentMoveCount.ToString();
        healthbarShader.material.SetFloat("_Percentage", _currentHealth/ maxHealth);
        Setup();
    }
    
    //warning the player
    //for example, swapping column, need to tell player the column gonna be swapped in the future move
    protected virtual void Telegraph()
    {
        
    }

    protected virtual void Setup(){

    }
    // how the enemy gonna behave
    public virtual IEnumerator EffectEvent()
    {
        yield return null;
    }
    private void Shake()
    {
        Tween.ShakeLocalRotation(enemyVisual.transform,Vector3.forward * 10f,0.5f, 20f).Group(Tween.ShakeScale(enemyVisual.transform,Vector3.one * 0.3f,0.5f, 20f));
    }
    public void UpdateText()
    {
        numberText.text = _currentMoveCount.ToString();
    }
    public virtual void Dead()
    {
        enemyVisual._image.color = Color.red;
    }

    public virtual IEnumerator Damage(float damage)
    {
        Shake();
        _currentHealth -= damage;
        healthbarShader.material.SetFloat("_Percentage", _currentHealth/ maxHealth);
        if (_currentHealth <= 0 && !_isDead)
        {
            _isDead = true;
            _currentHealth = 0;
            healthbarShader.material.SetFloat("_Percentage", _currentHealth/ maxHealth);
            yield return GameplayManager._delay; 
            Dead();
        }
        yield return GameplayManager._delay;
    }
    /// <summary>
    /// Do highlight on chosen enemy to know which movement warning belong to
    /// </summary>
    public virtual void HeadupTelegraph()
    {
        
    }
    public virtual void ReleaseHeadupTelegraph()
    {
        
    }

    public virtual void OnDestroy()
    {
        Destroy(enemyVisual.gameObject);
    }

    public virtual void OnSpawn()
    {
        
    }

    public void SetChildVisual(EnemyVisual visual)
    {
        enemyVisual = visual;
    }
}
