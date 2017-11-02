using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(UnityEngine.AI.NavMeshAgent))]
public class Enemy : LivingEntity {
    public enum EnemyState { Idle, Chasing, Attacking }
    EnemyState currentState;

    UnityEngine.AI.NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material enemyMaterial;
    Color originalColor;

    float attackDistanceThreshold = 0.5f;   // Minimum distance between enemy's and target's colliders for an attack to be triggered
    float timeBetweenAttacks = 1.0f;        // Minimum time to elapse before enemy can attack again
    float damage = 1.0f;                    // Amount of damage (health subtraction) applied to the target

    float nextAttackTime;                   // Holds a running counter for when is the next time this enemy can attack
    float enemyCollisionRadius;             // Radius of the Enemy's transform 
    float targetCollisionRadius;            // Radius of the player's transform

    bool hasTarget;                         // Tracks whether the player is still alive

	protected override void Start () {
        base.Start();

        // Set out pathfinder to the NavMeshAgent component defined on the Enemy
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();

        // Get our current material and its starting color
        enemyMaterial = GetComponent<Renderer>().material;
        originalColor = enemyMaterial.color;


        // Set the target and target entity to the Player
        GameObject targetObject = GameObject.FindGameObjectWithTag("Player");
        if (targetObject != null) {
            target = targetObject.transform;
            ChaseTarget();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (hasTarget) {
            AttackCheck();
        }
    }

    void OnTargetDeath() {
        hasTarget = false;
        currentState = EnemyState.Idle;
    }

    void ChaseTarget() {
        hasTarget = true;

        // By default, Enemy should be chasing
        currentState = EnemyState.Chasing;
        targetEntity = target.GetComponent<LivingEntity>();
        targetEntity.OnDeath += OnTargetDeath;

        // Get the radius of the Enemy and the target's game objects
        enemyCollisionRadius = GetComponent<CapsuleCollider>().radius;
        targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

        // Start a coroutine for refreshing the Enemy's navigation against the Player's position
        StartCoroutine(UpdatePath());
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
        bool hasAppliedDamage = false;

        while (attackPercent <= 1) {
            if (attackPercent >= 0.5 && !hasAppliedDamage) {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
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
        float refreshRate = 0.5f;

        while (hasTarget) {
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
