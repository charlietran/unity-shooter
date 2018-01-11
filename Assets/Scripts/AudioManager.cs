using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public float masterVolumePercent = 0.5f;
    public float sfxVolumePercent = 1f;
    public float musicVolumePercent = 1f;
	public Transform audioListenerTransform;
	public Transform playerTransform;

    AudioSource[] musicSources;
    AudioSource globalSoundSource;
    int activeMusicSourceIndex;
    AudioLibrary library;

    public static AudioManager instance;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        } 

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSources = new AudioSource[2];
        for (int i = 0; i < 2; i++) {
            GameObject newMusicSource = new GameObject("Music source " + (i + 1));
            musicSources[i] = newMusicSource.AddComponent<AudioSource>();
            newMusicSource.transform.parent = transform;
        }

        GameObject globalSoundSourceObject = new GameObject("Global Sound Source");
        globalSoundSource = globalSoundSourceObject.AddComponent<AudioSource>();
        globalSoundSourceObject.transform.parent = transform;

        masterVolumePercent = PlayerPrefs.GetFloat("MasterVolume", masterVolumePercent);
        sfxVolumePercent = PlayerPrefs.GetFloat("SfxVolume", sfxVolumePercent);
        musicVolumePercent = PlayerPrefs.GetFloat("MusicVolume", musicVolumePercent);

        library = GetComponent<AudioLibrary>();
    }

	void Update() {
		if (playerTransform != null) {
			audioListenerTransform.position = playerTransform.position;
		}
	}

    public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(FadeMusic(fadeDuration));
    }

    IEnumerator FadeMusic(float duration) {
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * (1 / duration);
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }

    public enum AudioChannel {Master, Sfx, Music};
    public void SetVolume(float volumePercent, AudioChannel audioChannel) {
        switch(audioChannel) {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        foreach (AudioSource musicSource in musicSources) {
            musicSource.volume = musicVolumePercent * masterVolumePercent;
        }

        PlayerPrefs.SetFloat("MasterVolume", masterVolumePercent);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolumePercent);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumePercent);
    }

    public void PlaySound(AudioClip clip, Vector3 position) {
        if (clip != null) {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolumePercent * masterVolumePercent);
        }
    }

    public void PlaySound(string soundName, Vector3 position) {
        PlaySound(library.GetClipFromName(soundName), position);
    }

    public void PlaySound(string soundName) {
        globalSoundSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }
}
