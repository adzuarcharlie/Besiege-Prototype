using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadZone : MonoBehaviour
{
	[SerializeField] GameObject[] objectsToLoad = null; // the list of objects to load when the zone is reached
	// it can also be a parent object used to group a whole environment part

	private void OnTriggerEnter(Collider other)
	{
		// when something hits the load zone, verify it's the player
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null)
		{
			// if the player reached the load zone, load every object that has to be by activating them
			// we could also deactivate this zone for optimization (not trying to activate the list every time the player enters this zone)
			// but as I only have few objects for now, that was not necessary at all
			foreach (GameObject go in objectsToLoad)
			{
				go.SetActive(true);
			}
		}
	}

	/*
	 note : if you want several loading zones (like in level 3),
	 you can simply put the next loading zone in the list of objects to load (that's what I did)
	 */
}
