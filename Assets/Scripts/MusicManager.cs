using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string activeSceneName;

    void Start() {
    }

	void OnEnable() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.name != activeSceneName) {
			activeSceneName = scene.name;
			Invoke("PlayMusic", 0.2f);
		}

    }

    void PlayMusic() {
        AudioClip clip = null;
        if (activeSceneName == "Menu") {
            clip = menuTheme;
        } else if (activeSceneName == "Game") {
            clip = mainTheme;
        }

        if (clip != null) {
            AudioManager.instance.PlayMusic(clip, 2);
        }
    }

}