using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	public float speed = 10f;
	public float distanceMultiplier = 1f;
	public Vector2 direction = Vector2.down;
	public GameObject explosion;

	private Vector3 startPos;
	private float totalDistance;
	private GameObject particleEffect;
	private Rigidbody2D rb;

	// Use this for initialization
	void Start () {
		startPos = transform.position;
		totalDistance = speed * distanceMultiplier;
		rb = GetComponent<Rigidbody2D>();
		rb.velocity = direction.normalized * speed;
	}
	
	// Update is called once per frame
	void Update () {
		float distance = (transform.position - startPos).magnitude;
		if (distance > totalDistance) {
			Die();
		}
		rb.velocity = direction.normalized * speed;
	}

	private void Die() {
		// Create particle effect, then die
		GameObject hitEffect = Instantiate(explosion, transform.position, Quaternion.identity);
		hitEffect.GetComponent<Rigidbody2D>().velocity = rb.velocity;
		particleEffect = hitEffect;
		Destroy(this.gameObject);
	}

	private void OnCollisionEnter2D(Collision2D c) {
		Mushroom mushroom = c.gameObject.GetComponent<Mushroom>();
		if (mushroom) {
			mushroom.Hit(1f);
		}
		Die();
	}
}
