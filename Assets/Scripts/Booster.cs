using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : PieceBehaviour
{
	/*
	  a booster is a piece that constantly gives an impulse to the vehicle when the input is pressed
	  the direction of the impulse depends on the direction of the booster
	  you can't directly change it, but its direction depends on where you put it
	  
	  if you put it under a piece, it will give an impulse upwards
	  if you put it above a piece, it will give an impulse downwards
	  if you put it on the side of a piece, it will give an impulse towards the piece it's attached to

	  each booster will give an impulse, which means that if you put 2 boosters in the same direction,
	  the impulse will be twice as powerful

	  note : the force given isn't relative to the position in the rigidBody,
	  so if you put only one booster on the side of the vehicle,
	  like this :
					____
					|  |
	booster here -> ^---
																									 ^
	it won't turn (like it normally should with realistic physics), it will only add a force towards |
	*/

	private Rigidbody rb = null;

	private float force = 25f;
	
	void Start()
    {
		key = KeyCode.Space;
		mouseButton = -1; // -1 means this won't be activated by the mouse
		rb = transform.root.GetComponent<Rigidbody>();
    }

	public override void OnInputDown()
	{
		if (!rb) // the booster can only work with a rigidBody
		{
			Debug.LogError("Booster is not linked to a RigidBody");
			return;
		}
		rb.AddForce(transform.forward * force);
	}

	// we need to implement all PieceBehaviour's functions, even when they don't do anything
	public override void OnInputPressed()
	{

	}

	public override void OnInputReleased()
	{

	}
}