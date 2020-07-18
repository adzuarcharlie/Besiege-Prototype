using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
	/*
	this works along with the WinTarget script

	when this script is put on an object, the object becomes a target
	it then needs to be linked to the WinTarget script of the level
	*/
	private void OnCollisionEnter(Collision collision)
	{
		// when something hits the target, verify if it is a projectile
		if (collision.gameObject.name == MainManager.GetProjectilePrefab().name) // for now, we only have one type of projectile
		{ // if we want to implement more projectiles, all we need to do is set a list<GameObject> instead of a simple GameObject as the projectile prefab, and then check the list for the collision.gameObject.name instead of a single object
		  // if this is indeed a projectile, then we look for the WinTarget object to update the state of this target
			GameObject.Find("WinTarget").GetComponent<WinTarget>().TargetIsTouched(gameObject);
			// we also change the material as a feedback for the player to know he's touched the target
			GetComponent<MeshRenderer>().material = MainManager.GetWinMaterial();
		}
	}
}