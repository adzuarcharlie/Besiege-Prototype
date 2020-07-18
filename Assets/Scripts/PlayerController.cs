using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	/*
	this script manages all player's input
	it is separated from the Player script for easier reading/writing
	*/

	Rigidbody rb = null; // rigidbody ref. Player must have a rigidbody before we add PlayerController

	[SerializeField] private float forwardForce = 20f;
	[SerializeField] private float backwardForce = 12f;
	[SerializeField] private float turnTorqueForce = 20f;

	public void SetForwardForce(float newForce)
	{
		forwardForce = newForce;
	}
	
	public void SetBackWardForce(float newForce)
	{
		backwardForce = newForce;
	}

	public void SetTurnTorqueForce(float newForce)
	{
		turnTorqueForce = newForce;
	}

	private Player.PhysicsData physicsData; // we need the player's physics data to know if it has an engine (called wheels in code because it was supposed to be wheels at the beginning of the project)

	// this function copies the player's physics data and checks if there is an engine
	public void SetPhysicsData(Player.PhysicsData data)
	{
		physicsData = data;

		if (!data.hasWheels)
		{
			// if the player doesn't have any move component, activate a UI message to explain him
			GameObject.Find("GameUI").GetComponent<GameUI>().ActivateWarningText();
		}
	}

	private Player.InputData inputData; // the input data contains all pieces' behaviours with their corresponding input

	public void SetInputData(Player.InputData data)
	{
		inputData = data;
	}

    void Start()
    {
		rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
		if (physicsData.hasWheels) // if there is an engine, the vehicle floats and we can move it
		{
			RaycastHit hit;
			// this manages the floating effect
			// the vehicle floats in the air, falling until a platform is beneath, then having a counter-balancing force to float when it arrives nearby
			if (Physics.Raycast(physicsData.lowestPiece.transform.position, -transform.up, out hit, 3f, 255))
			{
				rb.drag = 1f;
				if (rb.velocity.y < 5f)
				{
					rb.AddForce(transform.up * 25f);
				}
				if (Vector3.Distance(transform.up, hit.point) < 1f)
				{
					rb.velocity = Vector3.zero;
				}
			}
			else
			{
				rb.drag = 0.1f;
			}

			// move input
			if (Input.GetKey(KeyCode.Z))
			{
				rb.AddForce(transform.forward * forwardForce);
			}
			if (Input.GetKey(KeyCode.S))
			{
				rb.AddForce(-transform.forward * backwardForce);
			}
			if (Input.GetKey(KeyCode.Q))
			{
				rb.AddTorque(-Vector3.up * turnTorqueForce, ForceMode.VelocityChange);
			}
			if (Input.GetKey(KeyCode.D))
			{
				rb.AddTorque(Vector3.up * turnTorqueForce, ForceMode.VelocityChange);
			}
		}
		// the pieces can still be activated no matter if there is an engine or not
		foreach (PieceBehaviour behaviour in inputData.behaviours)
		{
			// for every activable piece, we check the key and the mouse button
			// if they're not the default options, call the correct functions
			// according to the key/button's state (just pressed, held down or released)
			KeyCode key = behaviour.GetKey();
			if (key != KeyCode.Escape)
			{
				if (Input.GetKey(key))
				{
					behaviour.OnInputDown();
				}
				if (Input.GetKeyDown(key))
				{
					behaviour.OnInputPressed();
				}
				if (Input.GetKeyUp(key))
				{
					behaviour.OnInputReleased();
				}
			}
			int mouseButton = behaviour.GetMouseButton();
			if (mouseButton != -1)
			{
				if (Input.GetMouseButton(mouseButton))
				{
					behaviour.OnInputDown();
				}
				if (Input.GetMouseButtonDown(mouseButton))
				{
					behaviour.OnInputPressed();
				}
				if (Input.GetMouseButtonUp(mouseButton))
				{
					behaviour.OnInputReleased();
				}
			}
		}
	}
}
