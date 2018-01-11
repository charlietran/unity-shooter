using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
	public AudioClip mainTheme;
	public AudioClip menuTheme;

	void Start() {
		AudioManager.instance.PlayMusic(menuTheme, 2);
	}
}