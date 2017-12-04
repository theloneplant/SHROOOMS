using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SporeEmitter : MonoBehaviour {
	public float interval = 3f;
	public GameObject spore;
	public float width = 15f;
	public float height = 8f;

	private float startTime;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - startTime > interval) {
			if (Mushroom.makingSpores()) {
				Vector3 pos = new Vector3(Random.Range(-width / 2f, width / 2f), Random.Range(-height / 2f, height / 2f) - 4f, 0);
				GameObject instance = Instantiate(spore, pos, Quaternion.identity);
				instance.GetComponent<Spore>().startHeight = 5f;
				instance.GetComponent<Spore>().verticalSpeed = 0f;
			}
			startTime = Time.time;
		}
	}
}
