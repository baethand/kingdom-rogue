using System;
using System.Collections;
using UnityEngine;

// Базовый класс для сущностей с здоровьем
public abstract class EntityWithHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float startHealth = 6f;
    
    public float currentHealth { get; protected set; }
    public bool alive { get; protected set; } = true;

    protected virtual void Awake() {
        currentHealth = startHealth;
    }

    public virtual void getDamage(float damage) {
        if (!alive) return;

        Debug.Log("| " + gameObject.name + " | получен урон | currentHealth: " + currentHealth + " |  damage: " + damage + " |");
        currentHealth = currentHealth  - damage;

        if (currentHealth <= 0) {
            die();
        }
    }

    protected virtual void die()
    {
        alive = false;
    }

    public virtual void heal(float amount)
    {
        if (!alive) return;

        currentHealth = Mathf.Min(currentHealth + amount, startHealth);
    }
}