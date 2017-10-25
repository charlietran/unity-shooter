using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(UnityEngine.AI.NavMeshAgent))]
public class Enemy : LivingEntity {
    UnityEngine.AI.NavMeshAgent pathfinder;
    Transform target;

	protected override void Start () {
        base.Start();
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();

        // Set the target to the Player's transform
        target = GameObject.FindGameObjectWithTag("Player").transform;
         
        // Start a coroutine for often to refresh the Enemy's target position
        StartCoroutine(UpdatePath());
	}
	
	// Update is called once per frame
	void Update () {
	}

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while (target != null)
        {
            Vector3 targetPosition = new Vector3(target.position.x, 0, target.position.z);
            if (!dead) {
                pathfinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
