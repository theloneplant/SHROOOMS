using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {
	private float startTime;

	void Start() {
		startTime = Time.time;
	}

	void Update () {
		if (Time.time - startTime > 0.5f && Input.anyKey) {
			Application.LoadLevel("main");
		}
	}
}
