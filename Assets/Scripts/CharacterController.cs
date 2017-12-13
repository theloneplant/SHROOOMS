using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;

public class CharacterController : Controller {
	[Header("Movement")]
	[Tooltip("The character's movement speed")]
	[SerializeField]
	private float moveSpeed = 1.0f;
	[Tooltip("Input distance before the character starts moving")]
	[SerializeField]
	private float moveThreshold = 0.1f;
	[Tooltip("Modifier to scale the speed of movement animations")]
	[SerializeField]
	private float moveAnimationModifier = 1.5f;
	private Vector2 moveInput;
	private Vector2 travelDirection;
	private Vector2 prevPosition;
	private float travelSpeed;

	// Aiming
	private Vector2 aimInput;

	// Shooting
	[Header("Shooting")]
	[Tooltip("Bullet to shoot")]
	[SerializeField]
	private GameObject bullet;
	[Tooltip("Position to shoot from")]
	[SerializeField]
	private GameObject shootPoint;
	[Tooltip("Line created when shooting")]
	[SerializeField]
	private GameObject aimLine;
	[Tooltip("Sound to play when you shoot")]
	[SerializeField]
	private AudioClip shootSound;
	[Tooltip("Sound to play when you die")]
	[SerializeField]
	private AudioClip deathSound;
	[Tooltip("Minimum time between shots")]
	[SerializeField]
	private float shootCooldown = 0.2f;
	private bool aiming;
	private float shootStartTime;
	// Use multiple variables as a buffer for flicking the controller
	private Vector2 aimDirection1;
	private Vector2 aimDirection2;
	private Vector2 aimDirection3;
	private Vector2 aimDirection4;
	private Vector2 aimDirection5;

	// Interacting
	[Header("Interacting")]
	[Tooltip("Sound to play after eating")]
	[SerializeField]
	private AudioClip eatSound;
	[Tooltip("Prompt when you get near small mushroom")]
	[SerializeField]
	private GameObject eatSign;
	[Tooltip("Eating radius that applies some effects")]
	[SerializeField]
	private float eatRadius = 0.6f;
	[Tooltip("Offset of eating hitbox")]
	[SerializeField]
	private Vector3 eatOffset = new Vector3(0, 0.2f, 0);
	[Tooltip("Cooldown before looking for another target to eat")]
	[SerializeField]
	private float eatCooldown = 0.1f;
	[Tooltip("Amount that the score will enlarge when you eat a mushroom")]
	[SerializeField]
	private float textEnlarge = 1.5f;
	private float eatStartTime;
	private GameObject closestMushroom = null;

	// Text
	[Header("Text")]
	[Tooltip("Death text")]
	[SerializeField]
	private Text deathText;
	[Tooltip("Text displayed when you're reaaaaaly high")]
	[SerializeField]
	private Text trippyText;
	[Tooltip("Text displayed to show the number of kills you got")]
	[SerializeField]
	private Text shrooomsText;

	[System.Serializable]
	public struct IntoxicationParams {
		public float bloomIntensity;
		public float motionBlurAmount;
		public float color;
		public float distortion;
		public float lowPass;
		public float speed;
	}

	// Intoxication
	[Header("Intoxication")]
	[Tooltip("Camera that will have the effects applied on it")]
	[SerializeField]
	private GameObject camera;
	[Tooltip("How long it takes to sober up at each level")]
	[SerializeField]
	private float soberTime = 5f;
	[Tooltip("Amount of bloom that will happen")]
	[SerializeField]
	private float bloomIntensity;
	[Tooltip("Mushrooms to eat continuously until level 1")]
	[SerializeField]
	private int level1Count = 1;
	[Tooltip("Mushrooms to eat continuously until level 2")]
	[SerializeField]
	private int level2Count = 2;
	[Tooltip("Mushrooms to eat continuously until level 3")]
	[SerializeField]
	private int level3Count = 3;
	[Tooltip("Mushrooms to eat continuously until level 4")]
	[SerializeField]
	private int level4Count = 4;
	[Tooltip("Mushrooms to eat continuously until level 5")]
	[SerializeField]
	private int level5Count = 5;
	[Tooltip("Parameters associated with level 0 intoxication")]
	[SerializeField]
	private IntoxicationParams level0;
	[Tooltip("Parameters associated with level 1 intoxication")]
	[SerializeField]
	private IntoxicationParams level1;
	[Tooltip("Parameters associated with level 2 intoxication")]
	[SerializeField]
	private IntoxicationParams level2;
	[Tooltip("Parameters associated with level 3 intoxication")]
	[SerializeField]
	private IntoxicationParams level3;
	[Tooltip("Parameters associated with level 4 intoxication")]
	[SerializeField]
	private IntoxicationParams level4;
	[Tooltip("Parameters associated with level 5 intoxication")]
	[SerializeField]
	private IntoxicationParams level5;
	private int intoxication = 0;
	private int mushroomsEaten = 0;
	private int shroooms = 0;
	private float mushroomStartTime;
	private bool intoxicationChanged = false;
	private IntoxicationParams targetIntoxication;
	private MotionBlur blur;
	private Bloom bloom;
	private LensAberrations lens;
	private TonemappingColorGrading color;

	private bool dead = false;

	private Animator animator;
	private Rigidbody2D rb;
	
	void Start () {
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		
		animator.SetFloat("lookX", 0);
		animator.SetFloat("lookY", -1);

		travelDirection = Vector2.down;
		prevPosition = transform.position;

		blur = camera.GetComponent<MotionBlur>();
		bloom = camera.GetComponent<Bloom>();
		lens = camera.GetComponent<LensAberrations>();
		color = camera.GetComponent<TonemappingColorGrading>();

		trippyText.gameObject.SetActive(false);
		deathText.gameObject.SetActive(false);
	}

	void Update() {
		InputManager.HandleInput(this);
		GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
		shrooomsText.text = "SHROOOMS: " + shroooms;
		float lerpScale = Mathf.Lerp(shrooomsText.gameObject.transform.localScale.x, 1, Time.deltaTime * 20);
		shrooomsText.gameObject.transform.localScale = new Vector3(lerpScale, lerpScale, lerpScale);

		if (dead) {
			camera.GetComponent<Camera>().orthographicSize = Mathf.Lerp(camera.GetComponent<Camera>().orthographicSize, 2f, Time.deltaTime);
		}

		// Debug screen shake
		if (Input.GetKeyDown("c")) {
			FollowCam.ScreenShake(0.1f, 4f, 0.2f, 5f);
		}

		HandleAiming();
		if (!aiming) {
			HandleMovement();
		}

		Vector2 newTravel = new Vector2(transform.position.x, transform.position.y) - prevPosition;
		travelSpeed = newTravel.magnitude;
		if (newTravel.magnitude > 0.01f) {
			travelDirection = newTravel.normalized;
			prevPosition = transform.position;
		}

		HandleIntoxication();

		if (closestMushroom && (closestMushroom.transform.position - transform.position).magnitude > eatRadius) {
			HandleDisplaySign();
		}
	}

	void FixedUpdate() {
		
	}

	//----- Controller Callbacks -----

	public override void Move(Vector2 input) {
		moveInput = input;
	}

	public override void Aim(Vector2 input) {
		aimInput = input;
	}

	public override void Interact() {
		HandleInteraction();
	}

	//----- Movement Logic -----

	void HandleMovement() {
		if (moveInput.magnitude > 1) moveInput = moveInput.normalized;

		if (moveInput.magnitude > moveThreshold) {
			animator.SetBool("idle", false);
			animator.SetFloat("lookX", moveInput.x);
			animator.SetFloat("lookY", moveInput.y);
			rb.velocity = moveInput * moveSpeed;
			animator.speed = moveInput.magnitude * moveAnimationModifier + 0.1f;
		}
		else {
			animator.SetBool("idle", true);
			rb.velocity = Vector2.zero;
			animator.speed = 1f;
		}
	}

	void HandleAiming() {
		if (aimInput.magnitude > 1) aimInput = aimInput.normalized;
		// Used to get the "pull back" effect of the slingshot
		Vector2 inverseAim = aimInput * -1;
		if (aimInput.magnitude > moveThreshold) {
			animator.SetFloat("lookX", inverseAim.x);
			animator.SetFloat("lookY", inverseAim.y);
			animator.SetBool("aiming", true);
			if (aiming) {
				aimDirection5 = aimDirection4;
				aimDirection4 = aimDirection3;
				aimDirection3 = aimDirection2;
				aimDirection2 = aimDirection1;
				aimDirection1 = inverseAim;
			}
			else {
				aimDirection5 = inverseAim;
				aimDirection4 = inverseAim;
				aimDirection3 = inverseAim;
				aimDirection2 = inverseAim;
				aimDirection1 = inverseAim;
			}
			rb.velocity = Vector2.zero;
			aiming = true;
			aimLine.SetActive(true);
			float angle = Mathf.Atan2(inverseAim.y, inverseAim.x) * Mathf.Rad2Deg;
			aimLine.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
		else {
			if (aiming && Time.time - shootStartTime > shootCooldown) {
				// Player released the joystick, shoot
				SoundManager.PlaySound(shootSound, true, 1);
				GameObject newBullet = Instantiate(bullet, shootPoint.transform.position, Quaternion.identity);
				Bullet bulletComp = newBullet.GetComponent<Bullet>();
				bulletComp.direction = aimDirection3.normalized;
				shootStartTime = Time.time;
			}
			aiming = false;
			animator.SetBool("aiming", false);
			aimLine.SetActive(false);
		}
	}

	void HandleInteraction() {
		if (closestMushroom && closestMushroom.GetComponent<Mushroom>() && closestMushroom.GetComponent<Mushroom>().getStage() == 1) {
			GameObject tmp = closestMushroom;
			closestMushroom.GetComponent<Mushroom>().Kill();
			closestMushroom = null;
			HandleDisplaySign();

			if (closestMushroom == tmp) {
				eatSign.SetActive(false);
				closestMushroom = null;
			}

			mushroomsEaten++;
			shroooms++;
			shrooomsText.gameObject.transform.localScale = new Vector3(textEnlarge, textEnlarge, textEnlarge);
			mushroomStartTime = Time.time;
			SoundManager.PlaySound(eatSound, true, 1);
			CheckIntoxicationLevel();
		}
		else {
			eatSign.SetActive(false);
		}
	}

	void CheckIntoxicationLevel() {
		int oldVal = intoxication;
		if (mushroomsEaten >= level5Count) {
			intoxication = 5;
		}
		else if (mushroomsEaten >= level4Count) {
			intoxication = 4;
		}
		else if (mushroomsEaten >= level3Count) {
			intoxication = 3;
		}
		else if (mushroomsEaten >= level2Count) {
			intoxication = 2;
		}
		else if (mushroomsEaten >= level1Count) {
			intoxication = 1;
		}
		else {
			intoxication = 0;
		}

		// Flag if the level increased, this will let bloom flash a bit
		intoxicationChanged = intoxication > oldVal;
	}

	void HandleIntoxication() {
		if (intoxication > 0 && Time.time - mushroomStartTime > soberTime) {
			intoxication--;
			mushroomStartTime = Time.time;
			switch(intoxication) {
				case 0:
					mushroomsEaten = 0;
					break;
				case 1:
					mushroomsEaten = level1Count;
					break;
				case 2:
					mushroomsEaten = level2Count;
					break;
				case 3:
					mushroomsEaten = level3Count;
					break;
				case 4:
					mushroomsEaten = level4Count;
					break;
				case 5:
					mushroomsEaten = level5Count;
					break;
				default:
					mushroomsEaten = 0;
					break;
			}
		}

		switch (intoxication) {
			case 0:
				targetIntoxication = level0;
				break;
			case 1:
				targetIntoxication = level1;
				break;
			case 2:
				targetIntoxication = level2;
				break;
			case 3:
				targetIntoxication = level3;
				break;
			case 4:
				targetIntoxication = level4;
				break;
			case 5:
				targetIntoxication = level5;
				break;
			default:
				targetIntoxication = level5;
				break;
		}

		if (intoxication >= 3) {
			Mushroom.isTrippin = true;
		}
		else {
			Mushroom.isTrippin = false;
		}

		HandleBloom();
		HandleBlur();
		HandleColor();
		HandleDistortion();
		HandleHue();
		HandleLowPass();
		HandleSpeed();
	}

	private void HandleBloom() {
		float intensity = Mathf.Lerp(bloom.settings.intensity, targetIntoxication.bloomIntensity, Time.deltaTime);
		
		if (intoxicationChanged) {
			intensity += intoxication / 2f;
			intoxicationChanged = false;
		}
		bloom.settings.intensity = intensity;
	}

	private void HandleBlur() {
		blur.settings.frameBlending = Mathf.Lerp(blur.settings.frameBlending, targetIntoxication.motionBlurAmount, Time.deltaTime);
	}

	private void HandleColor() {
		var settings = color.colorGrading;
		settings.basics.saturation = Mathf.Lerp(settings.basics.saturation, targetIntoxication.color + 1, Time.deltaTime);
		settings.basics.vibrance = Mathf.Lerp(settings.basics.vibrance, targetIntoxication.color, Time.deltaTime);
		color.colorGrading = settings;
	}

	private void HandleDistortion() {
		lens.distortion.amount = Mathf.Lerp(lens.distortion.amount, targetIntoxication.distortion, Time.deltaTime);
		lens.chromaticAberration.amount = Mathf.Lerp(lens.chromaticAberration.amount, targetIntoxication.distortion / 2f, Time.deltaTime);
		lens.distortion.scale = (lens.distortion.amount / level5.distortion) * 0.33f + 1; // Scale is between 1-1.33
	}

	private void HandleHue() {
		if (intoxication >= 5) {
			if (!dead) {
				trippyText.gameObject.SetActive(true);
			}
			var settings = color.colorGrading;
			settings.basics.hue = Time.time / 2f % 1 - 0.5f;
			color.colorGrading = settings;
		}
		else {
			var settings = color.colorGrading;
			settings.basics.hue = Mathf.Lerp(settings.basics.hue, 0f, Time.deltaTime);
			color.colorGrading = settings;
		}
	}

	private void HandleLowPass() {
		SoundManager.SetLowPass(Mathf.Lerp(SoundManager.GetLowPass(), targetIntoxication.lowPass, Time.deltaTime));
	}

	private void HandleSpeed() {
		moveSpeed = Mathf.Lerp(moveSpeed, targetIntoxication.speed, Time.deltaTime);
	}

	private void HandleDisplaySign() {
		if (Time.time - eatStartTime > eatCooldown) {
			eatStartTime = Time.time;
			closestMushroom = null;
			float minDist = 1000;

			GameObject[] mushrooms = GameObject.FindGameObjectsWithTag("Mushroom");
			foreach (var mushroom in mushrooms) {
				float distance = (mushroom.transform.position - transform.position + eatOffset).magnitude;
				Mushroom mushroomComponent = mushroom.GetComponent<Mushroom>();
				if (mushroomComponent && mushroomComponent.isConsumable() && distance < eatRadius) {
					if (distance < minDist) {
						closestMushroom = mushroom;
						minDist = distance;
					}
				}
			}
		}

		eatSign.SetActive(false);
		if (closestMushroom) {
			eatSign.transform.position = closestMushroom.transform.position;
			eatSign.SetActive(true);
		}
	}
	
	private void OnTriggerStay2D(Collider2D c) {
		HandleDisplaySign();
	}

	private void OnCollisionEnter2D(Collision2D c) {
		if (c.gameObject.GetComponent<Mushroom>() && c.gameObject.GetComponent<Mushroom>().getStage() == 3) {
			if (!dead) {
				SoundManager.PlaySound(deathSound, false, 1);
			}
			dead = true;
			InputManager.Disable();
			moveInput = Vector2.zero;
			trippyText.gameObject.SetActive(false);
			deathText.gameObject.SetActive(true);
		}
	}
}
