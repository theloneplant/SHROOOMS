using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
	public virtual void Move(Vector2 input) { }

	public virtual void Aim(Vector2 input) { }

	public virtual void Interact() { }
}
