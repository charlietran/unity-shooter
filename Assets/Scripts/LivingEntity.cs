using System.Collections;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {
    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;
     
    protected virtual void Start() {
        health = startingHealth;
    }
    public void TakeHit(float damage, RaycastHit hit) {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage) {
        // Subtract the passed in damage from this entity's health
        health -= damage;

        // Die if health is 0 or below, and not already dead
        if (health <= 0 && !dead) {
            Die();
        }
    }

    protected void Die() {
        dead = true;
        if (OnDeath != null) {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
