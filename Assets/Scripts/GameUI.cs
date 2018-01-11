using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    public Image fadeOverlay;
    public GameObject gameOverUI;
    public RectTransform newWaveBanner;
    public Text waveTitle;
    public Text waveEnemyCount;
    public AnimationCurve bannerAnimationCurve;

    Spawner spawner;

    void Awake () {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

	// Use this for initialization
	void Start () {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
	}

    void OnGameOver() {
        StartCoroutine(Fade(Color.clear, Color.black, 1.0f));
        gameOverUI.SetActive(true);
    }

    void OnNewWave(int waveNumber) {
        waveTitle.text = "Wave " + Utility.NumberToWords(waveNumber);
        string enemyCount = spawner.currentWave.enemyCount.ToString();
        if (enemyCount == "-1") {
            enemyCount = "Infinite";
        }
        waveEnemyCount.text = enemyCount + " Enemies";

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    IEnumerator AnimateNewWaveBanner() {
        float duration = 0.5f; // animation duration in seconds
        float speed = 1 / duration;
        float animationPercent = 0f;
        float delayTime = 1f;
        int direction = 1;

        newWaveBanner.gameObject.SetActive(true);

        while (animationPercent >= 0) {
            animationPercent += Time.deltaTime * speed * direction;

            if (animationPercent > 1) {
                animationPercent = 1;
                direction = -1;
                yield return new WaitForSeconds(delayTime);
            }

            // newWaveBanner.anchoredPosition = Vector2.up * Mathf.SmoothStep(-330, 45, animationPercent);
            float newPosition = Mathf.Lerp(-350, 45, bannerAnimationCurve.Evaluate(animationPercent));
            newWaveBanner.anchoredPosition = Vector2.up * newPosition;
            yield return null;
        }

        newWaveBanner.gameObject.SetActive(false);
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
