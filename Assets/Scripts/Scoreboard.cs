using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour {
    public static int score { get; private set; }
	float lastEnemyKilledTime;
	int streakCount;
	float streakExpiration = 1f;

    void Start() {
        Enemy.OnDeathStatic += OnEnemyKilled;
		FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled() {
		if (Time.time < lastEnemyKilledTime + streakExpiration) {
			streakCount = (int)Mathf.Clamp(streakCount + 1, 0, 8);
		} else {
			streakCount = 0;
		}

		lastEnemyKilledTime = Time.time;

        score += 5 + (int)Mathf.Pow(2, streakCount);
    }

	void OnPlayerDeath() {
		Enemy.OnDeathStatic -= OnEnemyKilled;
	}
}
