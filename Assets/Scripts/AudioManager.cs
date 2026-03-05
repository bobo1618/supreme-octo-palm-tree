using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType { APPEAR, FLIP, MATCH, MISMATCH, WIN, LOSE }

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
	/// <summary>
	/// Class for mapping an SFX type to audio clips
	/// </summary>
	[System.Serializable]
	class SFXMapping { 
		public SFXType type;
		[SerializeField] List<AudioClip> clips;

		/// <summary>
		/// Returns a random audioclip from the list of assigned clips, or null if none are assigned
		/// </summary>
		public AudioClip GetClip() {
			if (clips.Count == 0) return null;
			return clips[Random.Range(0, clips.Count)];
		}
	}

	[SerializeField] AudioSource sfxSource, musicSource;
	[SerializeField] List<SFXMapping> sfxMapList;

	static AudioManager manager;

	// A mapping of SFX type to clips will be created for easy lookups
	Dictionary<SFXType, SFXMapping> sfxLookup = new();

	private void Awake() {
		manager = this;
		if (!sfxSource) sfxSource = GetComponent<AudioSource>();
		sfxMapList.ForEach(map => sfxLookup[map.type] = map);
	}

	private void OnDestroy() {
		if (manager == this) manager = null;
	}

	/// <summary>
	/// Play the requested SFX type, with an optional delay
	/// </summary>
	public static void PlaySFX(SFXType sfx, float delay = 0) {
		if (manager) manager.StartCoroutine(manager.PlaySFXCR(sfx, delay));
	}

	public static void SetMusic(bool toOn) {
		if (!manager || !manager.musicSource) return;
		if (toOn) manager.musicSource.Play();
		else manager.musicSource.Pause();
	}

	IEnumerator PlaySFXCR(SFXType sfx, float delay) {
		// Check if the sfx lookup comtains the requested type, and whether it has any clips assigned
		if (!sfxLookup.ContainsKey(sfx)) yield break;
		AudioClip clip = manager.sfxLookup[sfx].GetClip();
		if (!clip) yield break;

		if (delay > 0) yield return new WaitForSeconds(delay);
		manager.sfxSource.PlayOneShot(clip);
	}
}