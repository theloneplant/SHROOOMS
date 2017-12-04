using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public GameObject soundPrefab;
	private AudioLowPassFilter lowPass;

	// Singleton
	public static SoundManager instance { get; private set; }

	private void Awake() {
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
		}
		else {
			instance = this;
		}
	}

	// Use this for initialization
	void Start () {
		lowPass = GetComponent<AudioLowPassFilter>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void SetLowPass(float amount) {
		instance.lowPass.cutoffFrequency = amount;
	}

	public static float GetLowPass() {
		return instance.lowPass.cutoffFrequency;
	}

	public static void PlaySound(AudioClip source, bool randomPitch = false, float volume = 0.5f) {
		float pitch = 1;
		if (randomPitch) {
			pitch = Random.Range(0.9f, 1.1f);
		}
		GameObject sound = Instantiate(instance.soundPrefab, Vector3.zero, Quaternion.identity);
		AudioSource audio = sound.GetComponent<AudioSource>();
		audio.clip = source;
		audio.volume = volume;
		audio.pitch = pitch;
		audio.Play();
	}
}
