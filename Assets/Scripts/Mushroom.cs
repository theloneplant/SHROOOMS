using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour {
	private const int maxCount = 60;

	[Header("Spores")]
	[Tooltip("Interval that spores are released")]
	[SerializeField]
	private float sporeInterval = 5f;
	[Tooltip("Minimum spores to release")]
	[SerializeField]
	private int minSpores = 0;
	[Tooltip("Maximum spores to release")]
	[SerializeField]
	private int maxSpores = 2;
	[Tooltip("Spore gameobject to release")]
	[SerializeField]
	private GameObject spore;
	[Tooltip("Spot to release spores")]
	[SerializeField]
	private GameObject sporePoint;
	[Tooltip("Particle effect for explosion")]
	[SerializeField]
	private GameObject sporeExplosion;

	[Header("State")]
	[Tooltip("Amount of hits it takes to kill")]
	[SerializeField]
	private float health = 3f;
	[Tooltip("Speed to move")]
	[SerializeField]
	private float speed = 0.9f;
	[Tooltip("The time it takes for the stages to finish growing")]
	[SerializeField]
	private float growTime = 5f;
	[Tooltip("Default stage that the mushroom starts at")]
	[SerializeField]
	private int defaultStage = 1;
	[Tooltip("Explosion radius that kills other mushrooms")]
	[SerializeField]
	private float blastRadius = 1f;
	[Tooltip("Explosion offset")]
	[SerializeField]
	private Vector3 blastOffset = new Vector3(0, 0.2f, 0);
	private float randomGrowth;

	[Header("Colliders")]
	[Tooltip("Small collider, the mushroom is consumable in this state")]
	[SerializeField]
	private CircleCollider2D smallCollider;
	[Tooltip("Medium collider, the mushroom warns that it'll get big soon")]
	[SerializeField]
	private CircleCollider2D mediumCollider;
	[Tooltip("Large collider, the mushroom will attack the player and shoot spores")]
	[SerializeField]
	private CircleCollider2D largeCollider;

	[Header("Sounds")]
	[Tooltip("Sound when a mushroom is hit")]
	[SerializeField]
	private AudioClip hitSound;
	[Tooltip("Sound when a mushroom is killed")]
	[SerializeField]
	private AudioClip killSound;
	[Tooltip("Sound when a mushroom explodes")]
	[SerializeField]
	private AudioClip blastSound;
	[Tooltip("Sound made when jumping towards the player")]
	[SerializeField]
	private AudioClip jumpSound;
	[Tooltip("Sound made when jumping towards the player")]
	[SerializeField]
	private float jumpInterval = 0.4f;
	private float jumpStartTime;



	private GameObject player;

	private Animator animator;
	private Rigidbody2D rb;
	private int stage;
	private float stageStartTime;
	private float sporeStartTime;

	public static bool isTrippin = false;
	private static int count = 0;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		stage = defaultStage;
		setCollider(stage);
		stageStartTime = Time.time;
		animator.SetInteger("stage", stage);
		randomGrowth = Random.Range(-growTime / 4, growTime / 4);
	}
	
	// Update is called once per frame
	void Update () {
		animator.SetBool("hasFace", isTrippin);
		GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
		if (stage < 3 && Time.time - stageStartTime > growTime + randomGrowth) {
			stageStartTime = Time.time;
			sporeStartTime = Time.time;
			stage++;
			setCollider(stage);
			animator.SetInteger("stage", stage);
			if (stage == 3) {
				jumpStartTime = Time.time + jumpInterval / 2f; // Delay by half of the animation cycle for first jump
			}
		}
		if (stage == 3) {
			// Chase player, shoot spores
			rb.AddForce((player.transform.position - transform.position).normalized * speed);

			if (Time.time - sporeStartTime > sporeInterval) {
				sporeStartTime = Time.time;
				int numSpores = Random.Range(minSpores, maxSpores);
				for (int i = 0; i < numSpores; i++) {
					CreateSpore();
				}
			}
		}

		if (stage == 3 && Time.time - jumpStartTime > jumpInterval) {
			SoundManager.PlaySound(jumpSound, true, 0.15f);
			jumpStartTime = Time.time;
		}

		animator.SetFloat("moveX", rb.velocity.normalized.x > 0 ? 1 : -1);
	}

	private void setCollider(int stage) {
		smallCollider.enabled = false;
		mediumCollider.enabled = false;
		largeCollider.enabled = false;
		switch (stage) {
			case 1:
				smallCollider.enabled = true;
				break;
			case 2:
				mediumCollider.enabled = true;
				break;
			case 3:
				largeCollider.enabled = true;
				break;
			default:
				break;
		}
	}

	public void CreateSpore() {
		if (count < maxCount) {
			Instantiate(spore, sporePoint.transform.position, Quaternion.identity);
			count++;
		}
	}

	public bool isConsumable() {
		return stage == 1;
	}

	public int getStage() {
		return stage;
	}

	public void Hit(float damage) {
		health -= damage;
		SoundManager.PlaySound(hitSound, true, 1.2f);
		if (health <= 0) {
			if (stage < 3) {
				CreateSpore();
				count--;
				Destroy(this.gameObject);
				return;
			}
			
			GameObject[] mushrooms = GameObject.FindGameObjectsWithTag("Mushroom");
			foreach (var mushroom in mushrooms) {
				if ( mushroom != this.gameObject && mushroom.GetComponent<Mushroom>() 
						&& (mushroom.transform.position - transform.position + blastOffset).magnitude < blastRadius) {
					mushroom.GetComponent<Mushroom>().CreateSpore();
					count--;
					Destroy(mushroom);
				}
			}

			Instantiate(sporeExplosion, sporePoint.transform.position, Quaternion.identity);
			Kill();
		}
	}

	public void Kill() {
		if (stage == 3) {
			SoundManager.PlaySound(killSound, true, 1);
			int numSpores = Random.Range(minSpores, maxSpores);
			for (int i = 0; i < numSpores; i++) {
				CreateSpore();
			}
		}
		if (stage == 2) {
			SoundManager.PlaySound(killSound, true, 1);
			CreateSpore();
		}
		count--;
		Destroy(this.gameObject);
	}

	public static bool makingSpores() {
		return count < maxCount;
	}

	private void OnDestroy() {
		Debug.Log("destroyed");
		isTrippin = false;
		count = 0;
	}
}
