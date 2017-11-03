using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    public Image fadeOverlay;
    public GameObject gameOverUI;

	// Use this for initialization
	void Start () {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
	}

    void OnGameOver() {
        StartCoroutine(Fade(Color.clear, Color.black, 1.0f));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float fadeDuration) {
        float speed = 1 / fadeDuration;
        float fadePercent = 0;

        while (fadePercent < 1) {
            fadePercent += Time.deltaTime * speed;
            fadeOverlay.color = Color.Lerp(from, to, fadePercent);
            yield return null;
        }
    }

    public void StartNewGame() {
        SceneManager.LoadScene("Game");
    }
}
