using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class InputManager : MonoBehaviour {
	// Singleton
	public static InputManager instance { get; private set; }

	private void Awake() {
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
		}
		else {
			instance = this;
		}
	}

	// Platforms & constants
	private const string WINDOWS = "Windows";
	private const string MAC_OS = "MacOS";

	private const float JOYSTICK_THRESHOLD = 0.1f;
	private const float BUTTON_THRESHOLD = 0.5f;

	// Public readonly input variables
	public static Vector2 moveDirection		{ get; private set; }
	public static Vector2 aimDirection		{ get; private set; }
	public static bool isMouseActive		{ get; private set; }

	private static string platform;
	private static string moveX, moveY;
	private static string aimX, aimY;
	private static string interact;
	private static string start;
	private static Vector3 prevMousePosition;
	private static bool isDisabled;

	void Start () {
		moveDirection = Vector2.zero;
		aimDirection = Vector2.zero;
		isMouseActive = false;
		prevMousePosition = Input.mousePosition;
		
		if (Application.platform == RuntimePlatform.WindowsPlayer 
				|| Application.platform == RuntimePlatform.WindowsEditor) {
			platform = WINDOWS;
		}
		else if (Application.platform == RuntimePlatform.OSXPlayer
				|| Application.platform == RuntimePlatform.OSXEditor) {
			platform = MAC_OS;
		}
		else {
			platform = WINDOWS;
		}

		moveX = platform + "MoveX";
		moveY = platform + "MoveY";
		aimX = platform + "AimX";
		aimY = platform + "AimY";
		interact = platform + "Interact";
		start = platform + "Start";
	}

	public static void HandleInput(Controller controller) {
		Vector2 controllerMove = new Vector2(Input.GetAxis(moveX), Input.GetAxis(moveY));
		Vector2 controllerAim = new Vector2(Input.GetAxis(aimX), Input.GetAxis(aimY));
		
		Vector2 keyboardMove = new Vector2(Input.GetAxis("KeyboardMoveX"), Input.GetAxis("KeyboardMoveY"));
		
		if (isDisabled) {
			if (GetButton(start)) {
				Application.LoadLevel("title");
			}
			return;
		}

		// Move input
		if (controllerMove.magnitude > JOYSTICK_THRESHOLD) {
			moveDirection = controllerMove;
			isMouseActive = false;
		}
		else if (keyboardMove.magnitude > JOYSTICK_THRESHOLD) {
			moveDirection = keyboardMove.normalized;
			isMouseActive = true;
		}
		else {
			moveDirection = Vector2.zero;
		}
		// Aim input
		if (controllerAim.magnitude > JOYSTICK_THRESHOLD) {
			aimDirection = controllerAim;
			isMouseActive = false;
		}
		else {
			float x = Input.GetAxis("KeyboardAimX");
			float y = Input.GetAxis("KeyboardAimY");
			aimDirection = new Vector2(x, y);
		}

		// Check mouse activity
		if ((Input.mousePosition - prevMousePosition).magnitude > 0) {
			isMouseActive = true;
		}

		controller.Move(moveDirection);
		controller.Aim(aimDirection);
		prevMousePosition = Input.mousePosition;
		
		if (Input.GetButtonDown(interact)) {
			Debug.Log("Interact");
			controller.Interact();
		}
	}

	private static bool GetButton(string axisName) {
		return Input.GetAxis(axisName) > BUTTON_THRESHOLD;
	}

	public static void Disable() {
		isDisabled = true;
	}

	private void OnDestroy() {
		isDisabled = false;
	}
}
