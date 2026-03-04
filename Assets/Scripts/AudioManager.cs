using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType { FLIP, MATCH, UNMATCH, WIN, LOSE }

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
	[System.Serializable]
	class SFXMapping { 
		public SFXType type;
		[SerializeField] List<AudioClip> clips;

		public AudioClip GetClip() {
			if (clips.Count == 0) return null;
			return clips[Random.Range(0, clips.Count)];
		}
	}

	[SerializeField] List<SFXMapping> sfxMapList;

	static AudioManager manager;

	AudioSource source;
	Dictionary<SFXType, SFXMapping> sfxLookup = new();

	private void Awake() {
		manager = this;
		source = GetComponent<AudioSource>();
		sfxMapList.ForEach(map => sfxLookup[map.type] = map);
	}

	private void OnDestroy() {
		if (manager == this) manager = null;
	}

	public static void PlaySFX(SFXType sfx) {
		if (!manager || !manager.sfxLookup.ContainsKey(sfx)) return;
		manager.source.PlayOneShot(manager.sfxLookup[sfx].GetClip());
	}
}