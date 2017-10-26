using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(UnityEngine.AI.NavMeshAgent))]
public class Enemy : LivingEntity {
    public enum EnemyState { Idle, Chasing, Attacking }
    EnemyState currentState;

    UnityEngine.AI.NavMeshAgent pathfinder;
    Transform target;
    Material enemyMaterial;

    Color originalColor;

    float attackDistanceThreshold = 0.5f;
    float timeBetweenAttacks = 1.0f;

    float nextAttackTime;

    float enemyCollisionRadius;
    float targetCollisionRadius;

	protected override void Start () {
        base.Start();

        // Set out pathfinder to the NavMeshAgent component defined on the Enemy
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();

        // Get our current material and its starting color
        enemyMaterial = GetComponent<Renderer>().material;
        originalColor = enemyMaterial.color;

        // By default, Enemy should be chasing
        currentState = EnemyState.Chasing;

        // Set the target to the Player's transform
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // Get the radius of the Enemy and the target's game objects
        enemyCollisionRadius = GetComponent<CapsuleCollider>().radius;
        targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

        // Start a coroutine for refreshing the Enemy's navigation against the Player's position
        StartCoroutine(UpdatePath());
	}
	
	// Update is called once per frame
	void Update () {
        AttackCheck();
    }

    private void AttackCheck() {
        // Check that enough time has passed for us to Attack
        if (Time.time > nextAttackTime) {
            // Use pythagorean theorum to find distance to target. This is the squared distance between the centers of both game objects
            float squareDistanceToTarget = (target.position - transform.position).sqrMagnitude;

            // Our distance for triggering an attack is the radius of both our objects, plus our arbitrary attackDistanceThreshold
            float attackTriggerDistance = attackDistanceThreshold + enemyCollisionRadius + targetCollisionRadius;

            // Square our attack trigger distance for the pythagorean comparison
            float squareAttackTriggerDistance = Mathf.Pow(attackTriggerDistance, 2);

            // Check if our given hypotenuse squared is higher than our specified attack distance hypotenuse squared
            if (squareDistanceToTarget < squareAttackTriggerDistance) {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack() {
        // Disable pathfinding while we're attacking so it doesn't interfere ith our attack positioning
        pathfinder.enabled = false;
        currentState = EnemyState.Attacking;

        Vector3 originalPosition = transform.position;

        // Get a normalized vector for the direction to the target
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // The target position should be at the attack distance from the player
        Vector3 attackPosition = target.position - directionToTarget * (enemyCollisionRadius);

        float attackPercent = 0.0f;
        float attackSpeed = 3.0f;

        enemyMaterial.color = Color.red;

        while (attackPercent <= 1) {
            attackPercent += Time.deltaTime * attackSpeed;

            // Parabola equation: y = 4(-x^2 + x)
            float interpolation = (-Mathf.Pow(attackPercent, 2) + attackPercent) * 4;

            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        enemyMaterial.color = originalColor;
        currentState = EnemyState.Chasing;
        pathfinder.enabled = true;
    }

    // Sets the path to which Enemy should be attempting to navigate 
    IEnumerator UpdatePath() {
        float refreshRate = 0.25f;

        while (target != null)
        {
            if (currentState == EnemyState.Chasing) {
                // Get a normalized vector for the direction to the target
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                // The target position should be at the attack distance from the player
                Vector3 targetPosition = target.position - directionToTarget * (enemyCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);

                if (!dead) {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
