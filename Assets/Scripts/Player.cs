using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class Player : MonoBehaviour
{
	/*
	this scripts manages the player's vehicle's pieces, from their loading
	(instantiate the pieces, store their behaviour, create the input and physics data)
	to their saving, and also the win state
	*/

	private string saveName = "great vehicle.save"; // the vehicle's name, used to save/load it

	public string GetSaveName()
	{
		return saveName;
	}

	public void SetSaveName(string newSaveName)
	{
		saveName = newSaveName;
	}

	// the data used to regroup a piece's info during loading
	private struct PieceData
	{
		public string name; // the name is used to know what piece it is
		public Vector3 position;
		public Quaternion rotation;
	}

	public struct PhysicsData // contains all data useful for physics and controls
	{
		public bool hasWheels; // does the vehicle have an engine, if not it can't move
		public GameObject lowestPiece; // the lowest piece is used for the floating effect's raycast (in the PLayerController's Update())
	}

	private PhysicsData physics;

	public PhysicsData GetPhysicsData()
	{
		return physics;
	}

	public struct InputData // contains all activable pieces
	{
		public List<PieceBehaviour> behaviours;
	}

	private InputData input;

	public InputData GetInputData()
	{
		return input;
	}

	// this function is called at the Constructor's Start() (so when we enter the editor menu), it creates the vehicle's base
	public void CreateFromVoid(Transform spawnPoint)
	{
		Instantiate(MainManager.GetBasePrefab(), spawnPoint.position, spawnPoint.rotation, transform).name = MainManager.GetBasePrefab().name;
	}

	// this function loads all the pieces from a vehicle save, along with their behaviour, and also creates the corresponding input and physics data
	public void Load(Transform spawnPoint, bool instantiateBase = true)
	{
		FileStream file = File.Open(MainManager.GetSavePath() + saveName, FileMode.Open);

		if (file == null || !file.CanRead)
		{
			Debug.LogError("COULD NOT OPEN FILE " + MainManager.GetSavePath() + saveName);
			return;
		}

		// we get all pieces prefabs
		GameObject[] basics = MainManager.GetBasicsPieces();
		GameObject[] movement = MainManager.GetMovementPieces();
		GameObject[] weapons = MainManager.GetWeaponsPieces();
		GameObject[] misc = MainManager.GetMiscPieces();

		// we get or init the base
		GameObject baseGO;

		if (instantiateBase)
		{
			baseGO = Instantiate(MainManager.GetBasePrefab(), spawnPoint.position, spawnPoint.rotation, transform);
			baseGO.name = MainManager.GetBasePrefab().name;
		}
		else
		{
			baseGO = transform.Find("Base").gameObject;
		}

		BinaryReader reader = new BinaryReader(file);

		int nbPieces = reader.ReadInt32();
		PieceData piece = new PieceData();

		// init PhysicsData
		physics.hasWheels = false;
		physics.lowestPiece = baseGO;

		// init InputData
		input.behaviours = new List<PieceBehaviour>();

		for (int index = 0; index < nbPieces; index++)
		{
			// for every piece, we read the data contained in the file
			piece.name = reader.ReadString();
			piece.position.x = reader.ReadSingle();
			piece.position.y = reader.ReadSingle();
			piece.position.z = reader.ReadSingle();
			piece.rotation.x = reader.ReadSingle();
			piece.rotation.y = reader.ReadSingle();
			piece.rotation.z = reader.ReadSingle();
			piece.rotation.w = reader.ReadSingle();

			bool instanced = false;
			GameObject go = null;
			// then we look in every list of pieces until we've found the corresponding one
			// when it is found, we instantiate it
			for (int i = 0; i < basics.Length && !instanced; i++)
			{
				if (basics[i].name == piece.name)
				{
					go = Instantiate(basics[i], piece.position, piece.rotation, transform);
					go.name = basics[i].name;
					instanced = true;
				}
			}
			for (int i = 0; i < movement.Length && !instanced; i++)
			{
				if (movement[i].name == piece.name)
				{
					go = Instantiate(movement[i], piece.position, piece.rotation, transform);
					go.name = movement[i].name;
					instanced = true;
					physics.hasWheels = true; // if  the player has at least one engine, it can move
				}
			}
			for (int i = 0; i < weapons.Length && !instanced; i++)
			{
				if (weapons[i].name == piece.name)
				{
					go = Instantiate(weapons[i], piece.position, piece.rotation, transform);
					go.name = weapons[i].name;
					instanced = true;
				}
			}
			for (int i = 0; i < misc.Length && !instanced; i++)
			{
				if (misc[i].name == piece.name)
				{
					go = Instantiate(misc[i], piece.position, piece.rotation, transform);
					go.name = misc[i].name;
					instanced = true;
				}
			}
			if (instanced) // set input data
			{
				PieceBehaviour behaviour = go.GetComponent<PieceBehaviour>();
				if (behaviour) // if there is a behaviour attached to the piece, we add it to the input data
				{
					input.behaviours.Add(behaviour);
				}

				if ((go.transform.position.x - physics.lowestPiece.transform.position.x) * (go.transform.position.x - physics.lowestPiece.transform.position.x)
					+ (go.transform.position.z - physics.lowestPiece.transform.position.z) * (go.transform.position.z - physics.lowestPiece.transform.position.z)
					< 0.1f && go.transform.position.y < physics.lowestPiece.transform.position.y)
				{ // we check if the instantiated piece is below everything else
					physics.lowestPiece = go; // if it is, this piece becomes the reference point for the floating effect's raycast
				}
			}
		}

		reader.Close();
		file.Close();
	}

	// this function is called on spawn, it turns the pieces' box colliders into their correct colliders
	// all pieces have a box collider in the editor, as we use normals to place other pieces around them
	// we then look among the colliders asset bundle if there is a collider for the specified piece
	public void LoadGamePiecesBehaviour()
	{
		Transform[] pieces = GetComponentsInChildren<Transform>(); // we get all the vehicle's pieces
		GameObject[] colliders = MainManager.GetColliders(); // we get the colliders asset bundle
		foreach (Transform piece in pieces)
		{
			foreach (GameObject collider in colliders)
			{
				if (collider.name == piece.name + "_Collider") // for every piece, if there is a collider for it
				{
					// we replace the old collider by the asset bundle's one
					Destroy(piece.GetComponent<Collider>());
					StartCoroutine(AddComponentWhenReady(piece.gameObject, collider));
				}
			}
		}
	}

	// we wait for a frame to be sure that the old collider has been destroyed correctly
	private IEnumerator AddComponentWhenReady(GameObject piece, GameObject colliderPiece)
	{
		yield return new WaitForEndOfFrame();
		piece.AddComponent(colliderPiece.GetComponent<Collider>().GetType());
	}

	// this function registers all the vehicle's pieces' data into the corresponding file
	public void Save()
	{
		FileStream file = File.Create(MainManager.GetSavePath() + saveName); // if a file already exists, the File.Create() will simply rewrite it
		if (file == null || !file.CanRead)
		{
			Debug.LogError("COULD NOT CREATE FILE " + MainManager.GetSavePath() + saveName);
			return;
		}

		Transform[] pieces = GetComponentsInChildren<Transform>(); // we get all pieces

		// the noise corresponds to the objects we don't want to save,
		// such as the root component, which is not a piece, and the base
		int noise = 0;
		foreach (Transform tr in pieces)
		{
			if (tr.tag != "Piece" || tr.name == "Base")
			{
				noise++;
			}
		}

		BinaryWriter writer = new BinaryWriter(file);

		writer.Write(pieces.Length - noise); // we register the number of pieces there is, so that we know how many we'll have to load

		foreach (Transform tr in pieces)
		{
			// then for every correct piece we simply register the name (to know what piece this is),
			// the position and the rotation (useful for pieces like weapons or boosters)
			if (tr.tag == "Piece" && tr.name != "Base")
			{
				writer.Write(tr.name);
				writer.Write(tr.position.x);
				writer.Write(tr.position.y);
				writer.Write(tr.position.z);
				writer.Write(tr.rotation.x);
				writer.Write(tr.rotation.y);
				writer.Write(tr.rotation.z);
				writer.Write(tr.rotation.w);
			}
		}

		writer.Close();
		file.Close();

		// we add the save to a file that references all saves, so that we only have to read this file to know all saved vehicles
		MainManager.AddSaveName(saveName);
	}

	// this function copies a vehicle's data ; from the name only we can load all pieces, physics and input data
	public void Copy(Player player)
	{
		saveName = player.saveName;
		physics = player.physics;
	}

	// this function is called when the current level is completed, it saves progress and changes level
	public void EnterWinState()
	{
		if (GetComponent<PlayerController>().enabled) // security, we make sure we only do this once
		{
			GetComponent<PlayerController>().enabled = false; // there is no need to keep controlling the vehicle once the level is done (and it's used as a security)
			GameObject.Find("GameUI").GetComponent<GameUI>().ActivateWinText(); // we activate a text to notify the player he's completed the level
			MainManager.CompletedLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); // we ask the MainManager to save the progress
			StartCoroutine(EnableWin(3f)); // we wait for a few seconds before loading the next level
		}
	}

	// this is called by EnterWinState(), we wait for a few seconds to make sure the player understands he's completed the level
	private IEnumerator EnableWin(float time)
	{
		yield return new WaitForSeconds(time);
		MainManager.LoadNextLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); // we ask the MainManager to load the next level, as it knows which level is the next (if there is one)
	}
}