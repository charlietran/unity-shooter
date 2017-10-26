using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    float speed = 10;
    float damage = 1;
    public LayerMask collisionMask;
     
    public void SetSpeed (float newSpeed)
    {
        speed = newSpeed;
    }

    void Update ()
    {
        float moveDistance = speed * Time.deltaTime;

        // Before we move the projectile, check if it would hit something this frame
        CheckCollisions(moveDistance);

        // If we haven't hit something, move the projectile
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    void CheckCollisions(float moveDistance)
    {
        // Create a ray at the projectile's current position and orientation
        Ray ray = new Ray(transform.position, transform.forward);

        // This will hold our "hit", if it exists
        RaycastHit hit;

        // Cast a ray to see if we hit any colliders. If we do, store the location in the "hit" var
        bool hitSomething = Physics.Raycast(ray, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide);

        if (hitSomething) {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit)
    {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
         
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hit);
        }

        // gameObject is a reference to the current game object (the projectile)
        // Destroy the projectile once it hits something
        GameObject.Destroy (gameObject);
    }

}
