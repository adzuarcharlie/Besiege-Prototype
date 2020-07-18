using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinZone : MonoBehaviour
{
	/*
	there are two types of levels : the levels where you need to reach a certain zone
	and the levels where you need to shoot all targets
	
	this script is used for the levels where you need to reach a zone,
	it works if put on an object with a trigger
	*/

    void Start()
    {
		// set a transparency to the material so the player knows it's not a solid object
		Material mat = GetComponent<MeshRenderer>().material;
		Color color = mat.color;
		color.a = 0.2f;
		mat.color = color;
    }

	private void OnTriggerEnter(Collider other)
	{
		// when something hits the trigger, we check it's the player
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null)
		{
			// if this is the player, then we activate his win state
			player.EnterWinState();
		}
	}
}
