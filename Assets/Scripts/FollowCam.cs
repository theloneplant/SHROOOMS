using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour {
	public GameObject target;

	[Header("Aim")]
	[Tooltip("The distance that the camera moves when looking around")]
	public float aimDistance = 0f;
	[Tooltip("The speed that the camera moves when looking around")]
	public float aimSpeed = 5f;
	[Tooltip("The speed that the camera follows its target")]
	public float followSpeed = 5f;

	// Follow variables
	// TODO

	// Aim variables
	private Vector2 aimPosition;

	// Screen shake variables
	private struct Shake {
		public float intensity;
		public float duration;
		public float speed;
		public float attack;
		public float startTime;

		// Values for interpolating along perlin noise
		public Vector2 prevLocationX;
		public Vector2 targetLocationX;
		public Vector2 prevLocationY;
		public Vector2 targetLocationY;
		public float noiseStartTime;
	}
	private static List<Shake> screenShakes;
	
	void Start () {
		screenShakes = new List<Shake>();
		aimPosition = transform.position;
	}
	
	void Update () {
		Vector2 position = Vector2.zero;
		position += CalculateFollow();
		position += CalculateAim();
		position += CalculateScreenShake();

		transform.position = new Vector3(position.x, position.y, transform.position.z);
	}

	Vector2 CalculateFollow() {
		Vector3 delta = (target.transform.position - transform.position) * followSpeed * Time.deltaTime;
		return transform.position + delta;
	}

	Vector2 CalculateAim() {
		Vector2 offset = InputManager.aimDirection * aimDistance * -1;

		Vector2 offsetDelta = (offset - aimPosition) * aimSpeed * Time.deltaTime;
		aimPosition += offsetDelta;

		return aimPosition;
	}

	Vector2 CalculateScreenShake() {
		Vector2 offset = Vector2.zero;

		for(int i = 0; i < screenShakes.Count; i++) {
			float t = (Time.time - screenShakes[i].startTime) / screenShakes[i].duration;
			
			if (t > 1) {
				screenShakes.RemoveAt(i);
			}
			else {
				float tNoise = (Time.time - screenShakes[i].noiseStartTime) * screenShakes[i].speed;
				
				if (tNoise > 1) {
					Shake updatedShake = screenShakes[i];
					Vector2 newDirX = new Vector2(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f)).normalized * 10f;
					Vector2 newDirY = new Vector2(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f)).normalized * 10f;
					updatedShake.prevLocationX = screenShakes[i].targetLocationX;
					updatedShake.prevLocationY = screenShakes[i].targetLocationY;
					updatedShake.targetLocationX = screenShakes[i].prevLocationX + newDirX;
					updatedShake.targetLocationY = screenShakes[i].prevLocationY + newDirY;
					updatedShake.noiseStartTime = Time.time;
					screenShakes[i] = updatedShake;
					tNoise = 0;
				}

				float currentAttack = 1;
				if (screenShakes[i].attack > 0) {
					currentAttack = (Time.time - screenShakes[i].startTime) / screenShakes[i].attack;
					currentAttack = currentAttack > 1 ? 1 : currentAttack;
				}

				float currentIntensity = screenShakes[i].intensity * (1 - t);

				Vector2 currentLocationX = Vector2.Lerp(screenShakes[i].prevLocationX, screenShakes[i].targetLocationX, tNoise);
				Vector2 currentLocationY = Vector2.Lerp(screenShakes[i].prevLocationY, screenShakes[i].targetLocationY, tNoise);
				float noiseX = (Mathf.PerlinNoise(currentLocationX.x, currentLocationX.y) - 0.5f) * 2;
				float noiseY = (Mathf.PerlinNoise(currentLocationY.x, currentLocationY.y) - 0.5f) * 2;
				offset.x += noiseX * currentIntensity * currentAttack;
				offset.y += noiseY * currentIntensity * currentAttack;
			}
		}
		return offset;
	}

	public static void ScreenShake(float intensity, float duration, float attack = 0f, float speed = 10f) {
		Shake screenShake = new Shake();
		screenShake.intensity = intensity;
		screenShake.speed = speed;
		screenShake.attack = attack;
		screenShake.duration = duration;
		screenShake.startTime = Time.time;
		screenShake.prevLocationX = new Vector2(Random.Range(0f, 10f), Random.Range(0f, 10f));
		screenShake.prevLocationY = new Vector2(Random.Range(0f, 10f), Random.Range(0f, 10f));
		screenShake.noiseStartTime = Time.time - (1 / speed);
		screenShakes.Add(screenShake);
	}
}
