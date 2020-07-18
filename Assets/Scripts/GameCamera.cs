using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
	/*
	this script defines the camera behaviour in the play mode
	it simply always look at the player's vehicle and stays
	at the same distance behind it

	PS : the camera can target any object even though it's only used for the player's vehicle so far
  	*/

	private Transform target = null;

	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
	}

	// we separate the 3 axes to adjust Y and Z correctly (X is always set to 0 here, it's ready to be changed though in case we need to)
	public float distanceZ = 15f;

	public void SetDistanceZ(float newDistanceZ)
	{
		distanceZ = newDistanceZ;
	}

	private float distanceY = 5f;

	public void SetDistanceY(float newDistanceY)
	{
		distanceY = newDistanceY;
	}

	private float distanceX = 0f;

	public void SetDistanceX(float newDistanceX)
	{
		distanceX = newDistanceX;
	}

	void Update()
	{
		transform.rotation = target.rotation; // the camera always looks in the target's direction
		// (it doesn't directly look at the target, as we want to see what's forward)

		transform.position = target.position - target.forward * distanceZ + target.up * distanceY + target.right * distanceX;
		// we always move the camera so it is placed at the wanted distance from the target
	}
}
