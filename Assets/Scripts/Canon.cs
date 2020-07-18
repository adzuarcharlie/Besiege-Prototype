using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : PieceBehaviour
{
	/*
	a canon is a piece that shoots a projectile when the input is pressed
	the direction of the projectile depends on the direction of the canon
	you can't directly change it, but its direction depends on where you put it

	if you put it above a piece, it will shoot forward
	if you put it under a piece, it will shoot backward
	if you put it on left or right side, it will shoot forward
	if you put it at the front of a piece, it will shoot downward
	if you put it at the back of a piece, it will shoot upward
	*/

	private float force = 200f;

	void Start()
    {
		mouseButton = 0; // 0 corresponds to left click
		// we don't need to set any key value as the default value is Escape, which means it won't be activated by any key
    }

	// we need to implement all PieceBehaviour's functions, even when they don't do anything
	public override void OnInputDown()
	{

	}

	public override void OnInputPressed()
	{
		if (Physics.OverlapSphere(transform.position + transform.forward, 0.25f).Length > 0)
		{ // if the spawn position is blocked by any collision (may it be a piece, a wall or a projectile)
			// it won't spawn the projectile
			return;
		}
		GameObject go = Instantiate(MainManager.GetProjectilePrefab(), transform.position + transform.forward, transform.rotation);
		go.GetComponent<Rigidbody>().velocity = transform.forward * force;
		go.name = MainManager.GetProjectilePrefab().name;
	}

	public override void OnInputReleased()
	{

	}
}
