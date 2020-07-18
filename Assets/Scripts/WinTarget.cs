using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTarget : MonoBehaviour
{
	/*
	there are two types of levels : the levels where you need to reach a certain zone
	and the levels where you need to shoot all targets

	this script is used for the levels where you need to shoot targets
	it works when linked to the targets
	*/

	[SerializeField] private GameObject[] targets = null; // this is the list of targets that need to be shot
	// the script needs at least one target to work

	private Dictionary<GameObject, bool> isTouched = null; // this is the list of targets and their state (if they have been shot or not)

	private void Start()
	{
		// at the beginning of the level, we store every target and set them to false (= "not shot")
		isTouched = new Dictionary<GameObject, bool>();
		foreach (GameObject target in targets)
		{
			isTouched.Add(target, false);
		}
	}

	// this function is called by a target when it gets shot
	// it updates its state in the list and then checks if all targets have been shot
	public void TargetIsTouched(GameObject target)
	{
		if (isTouched.ContainsKey(target)) // verify the target is in the list (the function can be called by a non-linked target !)
		{
			isTouched[target] = true; // update the state of the target
			if (!isTouched.ContainsValue(false))
			{
				// if all targets have been shot, we activate the win state
				GameObject.Find("Player").GetComponent<Player>().EnterWinState();
			}
		}
	}
}
