using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour {
	private AudioLowPassFilter lowPass;
	// Use this for initialization
	void Start () {
		lowPass = GetComponent<AudioLowPassFilter>();
		lowPass.cutoffFrequency = SoundManager.GetLowPass();
	}
	
	// Update is called once per frame
	void Update () {
		lowPass.cutoffFrequency = SoundManager.GetLowPass();
	}
}
