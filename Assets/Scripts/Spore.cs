using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spore : MonoBehaviour {
	public float startHeight = 2f;
	public float speed = 0.1f;
	public float verticalSpeed = 0.2f;
	public float maxFallSpeed = 0.2f;
	public float gravity = -0.3f;
	public float wiggleAmount = 1f;
	public float wiggleSpeed = 5f;

	private Vector2 direction;
	private float currentSpeed;
	private float spriteVelocity;

	public GameObject sporeSprite;
	public GameObject mushroom;
	private SpriteRenderer sprite;

	// Use this for initialization
	void Start () {
		sporeSprite.transform.parent = transform;
		sporeSprite.transform.localPosition = new Vector3(0, startHeight, 0);
		direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
		sprite = sporeSprite.GetComponent<SpriteRenderer>();
		spriteVelocity = verticalSpeed;
		currentSpeed = Random.Range(0, speed);
	}
	
	// Update is called once per frame
	void Update () {
		/* 
		 * Use this object's position so that the child can move up and down without affecting sorting order
		 * This will make a pseudo 3d effect, since this object can move around the ground and the child
		 * can move up and down in "3d space"
		 */
		sprite.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;

		// Set velocity and wiggle
		float wiggle = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
		Vector3 velocity = direction * currentSpeed;
		velocity.x += wiggle;
		transform.position += velocity * Time.deltaTime;

		spriteVelocity += gravity * Time.deltaTime;
		sporeSprite.transform.position += new Vector3(0, spriteVelocity * Time.deltaTime, 0);

		if (sporeSprite.transform.localPosition.y <= 0) {
			GameObject newMushroom = Instantiate(mushroom, transform.position, Quaternion.identity);
			Destroy(this.gameObject);
		}
	}
}
