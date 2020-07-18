using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingCanon : PieceBehaviour
{
	/*
	a holding canon works like a canon, except you have to keep the key pressed to charge the shot
	and release it when it's correctly charged
	the direction of the projectile depends on the direction of the canon
	you can't directly change it, but its direction depends on where you put it

	if you put it above a piece, it will shoot forward
	if you put it under a piece, it will shoot backward
	if you put it on left or right side, it will shoot forward
	if you put it at the front of a piece, it will shoot downward
	if you put it at the back of a piece, it will shoot upward
	*/

	float currentForce;
	float maxForce = 200f; // the max force the projectile can be shot at
	float deltaPerSecond = 50f; // the speed of the charge
	// for instance, with a delta of 50 and a max of 200, it takes 4 seconds to charge

	void Start()
    {
		currentForce = 0f;
		mouseButton = 1; // the classic canon is activated by the left mouse click
		// this one is activated by the right mouse click
    }

	public override void OnInputDown()
	{
		currentForce += Time.deltaTime * deltaPerSecond;
		// as long as the input is pressed, charge the shot
	}

	// we need to implement all PieceBehaviour's functions, even when they don't do anything
	public override void OnInputPressed()
	{
		
	}

	public override void OnInputReleased()
	{
		// when the input is released, clamp the force to the max
		if (currentForce > maxForce)
		{
			currentForce = maxForce;
		}
		// if the spawn position is blocked, cancel the shot
		if (Physics.OverlapSphere(transform.position + transform.forward, 0.25f).Length > 0)
		{
			currentForce = 0f;
			return;
		}
		// else shoot the projectile
		GameObject go = Instantiate(MainManager.GetProjectilePrefab(), transform.position + transform.forward, transform.rotation);
		go.GetComponent<Rigidbody>().velocity = transform.forward * currentForce;
		go.name = MainManager.GetProjectilePrefab().name;
		currentForce = 0f;
	}

}
