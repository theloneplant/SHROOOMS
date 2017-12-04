using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDeath : MonoBehaviour {
	public float duration = 1f;

	private float startTime;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - startTime > duration) {
			Destroy(this.gameObject);
		}
	}
}
