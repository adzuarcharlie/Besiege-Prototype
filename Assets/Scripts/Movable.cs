using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour
{
	/*
	this script is to put on moving elements (only used for targets for now, but it's applyable on anything)
	it will start on its spawn position and make goings and comings to the endPos
	*/

	[SerializeField] private Vector3 endPos;

	[SerializeField] private float deltaPerSecond = 2f; // speed

	private Vector3 startPos;

	private enum Direction
	{
		Forward, // forward = towards endPos
		Backward // backward = towards startPos
	}

	private Direction direction;

	private Vector3 startToEnd; // used to steer the movement and check when the piece arrives at end or start
	// registered so it's only calclated once

    void Start()
    {
		startPos = transform.position;
		direction = Direction.Forward;
		startToEnd = (endPos - startPos).normalized;
    }

    void Update()
    {
		// move the piece according to the current direction
		transform.position += (direction == Direction.Forward ? startToEnd : -startToEnd) * Time.deltaTime * deltaPerSecond;

		// according to the direction, check when the piece passed the correct point and change the direction
		if (direction == Direction.Forward)
		{
			if (Vector3.Dot(startToEnd, transform.position - endPos) > 0f)
			{
				direction = Direction.Backward;
			}
		}
		else
		{
			if (Vector3.Dot(startToEnd, transform.position - startPos) < 0f)
			{
				direction = Direction.Forward;
			}
		}
    }
}
