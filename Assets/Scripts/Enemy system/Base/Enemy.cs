using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stat")]
    public EnemySO enemyData;
    protected string name;
    [SerializeField] protected float maxHealth;
    public int delayAfterMove;
    protected EnemyTier tier;
    public float currentHealth;
    public int currentMoveCount;
    public bool isDead;

    [Header("Respond")] 
    [SerializeField] private Image healthbarShader;
    [SerializeField] protected EnemyVisual enemyVisual;
    [SerializeField] private GameObject informationBox;
    [SerializeField] private TextMeshProUGUI descriptionTex;
    [SerializeField] private TextMeshProUGUI numberText;

    public Sequence currentSequence = new Sequence();
    protected virtual void Start()
    {
        isDead = false;
        name = enemyData.name;
        maxHealth = enemyData.health;
        delayAfterMove = enemyData.delayAfterMove;
        tier = enemyData.tier;
        currentHealth = maxHealth;
        currentMoveCount = delayAfterMove;
        numberText.text = currentMoveCount.ToString();
        descriptionTex.text = enemyData.description;
        healthbarShader.material.SetFloat("_Percentage", currentHealth/ maxHealth);
        Telegraph();
    }
    
    //warning the player
    //for example, swapping column, need to tell player the column gonna be swapped in the future move
    protected virtual void Telegraph()
    {
        
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
        numberText.text = currentMoveCount.ToString();
    }
    protected virtual void Dead()
    {
        enemyVisual._image.color = Color.red;
    }

    public virtual IEnumerator Damage(float damage)
    {
        Shake();
        currentHealth -= damage;
        healthbarShader.material.SetFloat("_Percentage", currentHealth/ maxHealth);
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            currentHealth = 0;
            healthbarShader.material.SetFloat("_Percentage", currentHealth/ maxHealth);
            yield return new WaitForSeconds(GameplayManager.Instance.gameplaySpeed); 
            Dead();
        }
        yield return new WaitForSeconds(GameplayManager.Instance.gameplaySpeed);
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
        
    }
}
