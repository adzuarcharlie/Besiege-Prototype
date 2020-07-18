using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	/*
	this script, when put anywhere in the level, takes care of spawning the player's vehicle
	by loading the correct vehicle save and initializing all required data
	*/

	private GameObject playerGO = null;
	private Player player = null;

	void Start()
    {
		// init player
		playerGO = Instantiate(MainManager.GetPlayerPrefab(), new Vector3(0f, 5f, 0f), Quaternion.identity);
		playerGO.name = "Player";
		player = playerGO.AddComponent<Player>();
		player.Copy(MainManager.GetPlayer()); // send the player data we saved in the editor
		player.Load(playerGO.transform); // load the vehicle from the selected save (we're sure there is a selected save because else we cannot launch a level)
		player.LoadGamePiecesBehaviour(); // load the pieces' correct collisions (all pieces have a cube collision in the editor, so we can use normals)

		// add game behaviour to the player (which has none by default so we're not bothered in the editor)
		Rigidbody rb = playerGO.AddComponent<Rigidbody>();
		rb.drag = 1f;
		rb.angularDrag = 3f;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		StartCoroutine(ResetCenter(rb));
		PlayerController controller = playerGO.AddComponent<PlayerController>();
		controller.SetPhysicsData(player.GetPhysicsData());
		controller.SetForwardForce(20f);
		controller.SetBackWardForce(12f);
		controller.SetTurnTorqueForce(0.2f);

		controller.SetInputData(player.GetInputData()); // link the player's pieces behaviours input to the controller

		// link the camera to the player
		Camera.main.GetComponent<GameCamera>().SetTarget(player.transform);
		Camera.main.transform.position = playerGO.transform.position - playerGO.transform.forward * 10f + playerGO.transform.up * 3f;
	}

	// as we added pieces (and thus new collisions) to the rigidbody, the center of mass may change
	private IEnumerator ResetCenter(Rigidbody rb)
	{
		yield return new WaitForEndOfFrame();
		rb.ResetCenterOfMass();
	}
}
