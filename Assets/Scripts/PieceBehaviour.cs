using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PieceBehaviour : MonoBehaviour
{
	/*
	this is a generic class for every piece that is activable with an input
	may it be a key or mouse button
	it has call-backs when the key/button is pressed, held or released
	*/

	protected KeyCode key = KeyCode.Escape; // default value

	public KeyCode GetKey()
	{
		return key;
	}

	protected int mouseButton = -1; // default value

	public int GetMouseButton()
	{
		return mouseButton;
	}

	public abstract void OnInputPressed(); // this is called once when the input is pressed

	public abstract void OnInputDown(); // this is called every frame the input is held

	public abstract void OnInputReleased(); // this is called once when the input is released
}