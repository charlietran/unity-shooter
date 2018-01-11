using System.Collections;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {
    public float startingHealth;
    public float health { get; protected set; }
    protected bool dead;

    public event System.Action OnDeath;
     
    protected virtual void Start() {
        health = startingHealth;
    }
    public virtual void TakeHit(float damage, Vector3 hitPosition, Vector3 hitDirection) {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage) {
        // Subtract the passed in damage from this entity's health
        health -= damage;

        // Die if health is 0 or below, and not already dead
        if (health <= 0 && !dead) {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    public virtual void Die() {
        dead = true;
        if (OnDeath != null) {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
