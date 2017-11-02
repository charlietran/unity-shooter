using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    float speed = 10;
    float damage = 1;
    float lifetime = 3.0f;
    float collisionBufferWidth = 0.1f;

    public LayerMask collisionMask;
     
    public void SetSpeed (float newSpeed) {
        speed = newSpeed;
    }

    private void Start() {
        // Make sure our bullets don't live forever
        Destroy(gameObject, lifetime);
        CheckInitialCollisions();
    }

    void Update () {
        float moveDistance = speed * Time.deltaTime;

        // Before we move the projectile, check if it would hit something this frame
        CheckCollisions(moveDistance);

        // If we haven't hit something, move the projectile
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    void CheckInitialCollisions() {
        // Find all the colliders touching our projectile at spawn time
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);
        if (initialCollisions.Length > 0) {
            OnHitObject(initialCollisions[0]);
        }
    }

    void CheckCollisions(float moveDistance) {
        // Create a ray at the projectile's current position and orientation
        Ray ray = new Ray(transform.position, transform.forward);

        // This will hold our "hit", if it exists
        RaycastHit hit;

        // Cast a ray to see if we hit any colliders. If we do, store the location in the "hit" var
        bool hitSomething = Physics.Raycast(
            ray, 
            out hit,
            // We add the collision buffer width here to compensate for the fact that enemies are moving as well
            // So if the enemy is within the projectile's move distance plus our slight buffer, we call it a hit
            moveDistance + collisionBufferWidth, 
            collisionMask, 
            QueryTriggerInteraction.Collide
        );

        if (hitSomething) {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit) {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        DamageObject(damageableObject);
    }

    // This overloaded OnHitObject is for CheckInitialCollisions, when we won't have a RaycastHit
    void OnHitObject(Collider collider) {
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        DamageObject(damageableObject);
    }

    void DamageObject(IDamageable damageableObject) {
        if (damageableObject != null) {
            damageableObject.TakeDamage(damage);
        }

        // gameObject is a reference to the current game object (the projectile)
        // Destroy the projectile when it hits something
        GameObject.Destroy (gameObject);
    }

}
