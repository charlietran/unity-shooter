using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour {

	public GameObject flashHolder;
	public Sprite[] flashSprites;
	public SpriteRenderer[] spriteRenderers;

	public float flashTime;

	void Start() {
		Deactivate();
	}

	public void Activate () {
		// Set the holding game object to active
		flashHolder.SetActive(true);

		// Grab one of our defined sprites at random
		int flashSpriteIndex = Random.Range(0, flashSprites.Length);

		// For each of our renderers, set its active sprite to the random one we chose
		for (int i = 0; i < spriteRenderers.Length; i++) {
			spriteRenderers[i].sprite = flashSprites[flashSpriteIndex];
		}

		// Deactivate the muzzle flash object after our defined flashTime
		Invoke("Deactivate", flashTime);
	}
	
	public void Deactivate () {
		flashHolder.SetActive(false);
	}
}
