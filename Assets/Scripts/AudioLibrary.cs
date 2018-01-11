using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLibrary : MonoBehaviour {
    public SoundGroup[] soundGroups;

    void Awake() {
        foreach (SoundGroup soundGroup in soundGroups) {
			Debug.Log("Adding " + soundGroup.groupName);
            groupDictionary.Add(soundGroup.groupName, soundGroup.clips);
        }
    }

    public AudioClip GetClipFromName(string name) {
        if (groupDictionary.ContainsKey(name)) {
            AudioClip[] sounds = groupDictionary[name];
            return sounds[Random.Range(0, sounds.Length)];
        }
        return null;
    }

    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();

    [System.Serializable]
    public class SoundGroup {
        public string groupName;
        public AudioClip[] clips;
    }
}
