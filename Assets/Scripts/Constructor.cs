using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Constructor : MonoBehaviour
{
	/*
	this script manages the editor menu
	from the UI to the controls
	*/

	// player ref
	private GameObject playerGO = null;
	private Player player = null;

	[SerializeField] private Transform panel; // used to access the buttons refs
	private Button[] piecesButtons = null; // UI buttons we get so we can disable them when clicked

	private PreviewMaker previewMaker = null;

	private RawImage[] images; // pieces previews

	private bool arePreviewsReady; // the PreviewMaker needs several frames per preview to make, so we make sure it's ready before to use previews

	private InputField nameField = null; // the input field used to define the vehicle's name

	[SerializeField] private Canvas canvas; // ref to the canvas used for the buttons (used for the load vehicle buttons, especially to know the scale of the window)

	[SerializeField] private GameObject loadables; // the panel for the vehicle load buttons
	[SerializeField] private Scrollbar scrollbar; // the scrollbar for the vehicle load buttons

	private Dictionary<string, GameObject> loadVehicles = null; // every load button linked to the name of its vehicle (the name is used to load the save)
	private Dictionary<string, GameObject> eraseVehicles = null; // every erase button linked to the name of its vehicle (the name is used to delete the save)

	private GameObject selectedPiece = null; // the piece currently being placed by the player

	private int selectedPieceIndex; // the index of the selected piece, used so we don't have to search all the lists for the correct piece

	private Quaternion selectedPieceBaseRotation; // the original rotation of the selected piece, can be useful to place some pieces

	private enum PieceType
	{
		basic,
		movement,
		weapon,
		misc
	}

	private PieceType type; // the type of the pieces currently being displayed in the UI, used to know which piece the player is taking when he selects one

	void Start()
	{
		// base construction
		playerGO = Instantiate(MainManager.GetPlayerPrefab(), new Vector3(0f, 5f, 0f), Quaternion.identity);
		playerGO.name = "Player";
		player = playerGO.AddComponent<Player>();
		player.SetSaveName("great vehicle.save");
		player.CreateFromVoid(playerGO.transform);

		MainManager.SetPlayer(player); // save the player data ref in the manager

		// init camera
		Camera.main.GetComponent<ConstructionCamera>().SetTarget(player.transform);
		Camera.main.transform.position = playerGO.transform.position - playerGO.transform.forward * 10f + playerGO.transform.up * 3f;

		// init UI
		piecesButtons = panel.GetComponentsInChildren<Button>();

		// for now, we know we don't have more than 5 pieces, so we can simply place
		// the images in the scene and activate the ones we need with the wanted preview
		images = panel.GetComponentsInChildren<RawImage>();
		DisableImages();

		previewMaker = GameObject.Find("@PreviewMaker").GetComponent<PreviewMaker>();

		// we ask the preview maker for a preview of every piece
		previewMaker.Init();
		GameObject[] obj = MainManager.GetBasicsPieces();
		for (int i = 0; i < obj.Length; i++)
		{
			previewMaker.MakePreview(obj[i]);
		}
		obj = MainManager.GetMovementPieces();
		for (int i = 0; i < obj.Length; i++)
		{
			previewMaker.MakePreview(obj[i]);
		}
		obj = MainManager.GetWeaponsPieces();
		for (int i = 0; i < obj.Length; i++)
		{
			previewMaker.MakePreview(obj[i]);
		}
		obj = MainManager.GetMiscPieces();
		for (int i= 0; i < obj.Length; i++)
		{
			previewMaker.MakePreview(obj[i]);
		}
		arePreviewsReady = false;
		StartCoroutine(WaitForPreviews()); // this coroutine will turn the arePreviewsReady to true when the preview maker has finished previews

		nameField = GameObject.Find("ConstructorUI").GetComponentInChildren<InputField>();

		if (!loadables || !scrollbar)
		{
			Debug.LogWarning("LOAD CANNOT WORK BECAUSE PANEL OR SCROLLBAR LINK IS MISSING");
		}
		else
		{
			loadables.SetActive(false);
			scrollbar.gameObject.SetActive(false);
		}

		// simply create the list, fill it when we want to load a save
		loadVehicles = new Dictionary<string, GameObject>();
		eraseVehicles = new Dictionary<string, GameObject>();
	}

	void Update()
	{
		RaycastHit hit;
		// if there is a selected piece, the player can wether remove a piece by right-clicking on it (the next if)
		// wether place the selected piece by left-clicking on a piece
		if (selectedPiece)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			{
				if (hit.collider.gameObject.tag == "Piece")
				{
					// align the selected piece to the pointed piece
					selectedPiece.transform.position = hit.collider.transform.position + hit.normal;
					selectedPiece.transform.up = hit.normal;

					selectedPiece.transform.Rotate(selectedPieceBaseRotation.eulerAngles);

					// if the player left-clicks, place the selected piece
					if (Input.GetMouseButtonDown(0))
					{
						SetCollidersEnables(true); // we enable the collider so that it can be detected by the future raycasts and other pieces can be put over it
						selectedPiece.transform.parent = playerGO.transform; // attach the piece to the player object
						InstantiateSelectedPiece(selectedPieceIndex); // we immediatly create another instance of the selected piece, because we suppose the player might want to keep using this one
					}
				}
				else
				{
					// if the player is not pointing at a piece (which means he's pointing at the ground), then we simply place the piece where the raycast ends (so that the player sees the game isn't frozen)
					selectedPiece.transform.position = hit.point + hit.normal * 0.5f;
				}
			}

		}
		// if there is no selected piece, the player can only remove a piece by right-clicking on it
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
		{
			if (hit.collider.gameObject.tag == "Piece" && hit.collider.name != "Base") // we can only remove a piece (not the base)
			{
				if (Input.GetMouseButtonDown(1))
				{
					Remove(hit.collider.gameObject);
				}
			}
		}

		// update the load and erase buttons' positions
		if (canvas)
		{
			List<string> savesNames = MainManager.GetSavesNames();
			int index = 0;
			if (loadables.activeSelf)
			{
				RectTransform tr = loadables.GetComponent<RectTransform>();
				foreach (string name in savesNames)
				{
					if (loadVehicles.ContainsKey(name))
					{
						loadVehicles[name].transform.position = new Vector3(tr.position.x + canvas.scaleFactor * (1.5f * tr.sizeDelta.x / 4f),
							tr.position.y - canvas.scaleFactor * (tr.sizeDelta.y / 10f) - canvas.scaleFactor * (index * tr.sizeDelta.y / 5f) +
							(savesNames.Count > 5 ? canvas.scaleFactor * scrollbar.value * (savesNames.Count - 5) * (tr.sizeDelta.y / 5f) : 0f));

						if (eraseVehicles.ContainsKey(name))
						{
							eraseVehicles[name].transform.position = new Vector3(tr.position.x + canvas.scaleFactor * (3.5f * tr.sizeDelta.x / 4f),
								tr.position.y - canvas.scaleFactor * (tr.sizeDelta.y / 10f) - canvas.scaleFactor * (index * tr.sizeDelta.y / 5f) +
								(savesNames.Count > 5 ? canvas.scaleFactor * scrollbar.value * (savesNames.Count - 5) * (tr.sizeDelta.y / 5f) : 0f));
						}
						else
						{
							Debug.LogError("We should have a erase button for " + name);
						}
						index++;
					}
				}
			}
		}
	}

	// this function displays basic pieces' previews
	// it also deselects the selected piece and disables the button that calls this
	public void ShowBasics()
	{
		if (arePreviewsReady)
		{
			EnableButtons("Basics"); // enable all buttons except the basics one
			DisableImages();
			Deselect();
			GameObject[] obj = MainManager.GetBasicsPieces();
			for (int i = 0; i < obj.Length; i++)
			{
				images[i].texture = previewMaker.GetPreview(obj[i]);
				images[i].enabled = true;
			}
			type = PieceType.basic;
		}
	}

	// this function displays movement pieces' previews
	// it also deselects the selected piece and disables the button that calls this
	public void ShowMovement()
	{
		if (arePreviewsReady)
		{
			EnableButtons("Movement"); // enable all buttons except the movement one
			DisableImages();
			Deselect();
			GameObject[] obj = MainManager.GetMovementPieces();
			for (int i = 0; i < obj.Length; i++)
			{
				images[i].texture = previewMaker.GetPreview(obj[i]);
				images[i].enabled = true;
			}
			type = PieceType.movement;
		}
	}

	// this function displays weapons pieces' previews
	// it also deselects the selected piece and disables the button that calls this
	public void ShowWeapons()
	{
		if (arePreviewsReady)
		{
			EnableButtons("Weapons"); // enable all buttons except the weapons one
			DisableImages();
			Deselect();
			GameObject[] obj = MainManager.GetWeaponsPieces();
			for (int i = 0; i < obj.Length; i++)
			{
				images[i].texture = previewMaker.GetPreview(obj[i]);
				images[i].enabled = true;
			}
			type = PieceType.weapon;
		}
	}

	// this function displays miscellaneous pieces' previews
	// it also deselects the selected piece and disables the button that calls this
	public void ShowMisc()
	{
		if (arePreviewsReady)
		{
			EnableButtons("Misc"); // enable all buttons except the miscellaneous one
			DisableImages();
			Deselect();
			GameObject[] obj = MainManager.GetMiscPieces();
			for (int i = 0; i < obj.Length; i++)
			{
				images[i].texture = previewMaker.GetPreview(obj[i]);
				images[i].enabled = true;
			}
			type = PieceType.misc;
		}
	}

	// this function activates all pieces buttons but one (it can still activate all buttons if none corresponds to the given string)
	private void EnableButtons(string buttonToDisable)
	{
		foreach (Button b in piecesButtons)
		{
			if (b.name == buttonToDisable)
			{
				b.interactable = false;
			}
			else
			{
				b.interactable = true;
			}
		}
	}

	public void BackToMenu()
	{
		SceneManager.LoadScene("Menu");
	}

	// this function notifies when the preview maker has finished its work
	private IEnumerator WaitForPreviews()
	{
		while (previewMaker.IsMakingPreview())
		{
			yield return new WaitForEndOfFrame();
		}
		arePreviewsReady = true;
	}

	// this function disables all pieces previews
	private void DisableImages()
	{
		foreach (RawImage image in images)
		{
			image.enabled = false;
		}
	}

	// this function is called by the pieces previews buttons
	// it destroys the former selected piece and creates an instance of the new one
	public void SelectPiece(int index)
	{
		if (selectedPiece)
		{
			Destroy(selectedPiece);
		}
		selectedPieceIndex = index;
		InstantiateSelectedPiece(index);
	}

	// this function creates an instance of the selected piece
	private void InstantiateSelectedPiece(int index)
	{
		// instantiate the correct piece according to the selected type and index
		switch (type)
		{
			case PieceType.basic:
				selectedPiece = Instantiate(MainManager.GetBasicsPieces()[index]);
				selectedPiece.name = MainManager.GetBasicsPieces()[index].name;
				selectedPieceBaseRotation = selectedPiece.transform.rotation;
				break;
			case PieceType.movement:
				selectedPiece = Instantiate(MainManager.GetMovementPieces()[index]);
				selectedPiece.name = MainManager.GetMovementPieces()[index].name;
				selectedPieceBaseRotation = selectedPiece.transform.rotation;
				break;
			case PieceType.weapon:
				selectedPiece = Instantiate(MainManager.GetWeaponsPieces()[index]);
				selectedPiece.name = MainManager.GetWeaponsPieces()[index].name;
				selectedPieceBaseRotation = selectedPiece.transform.rotation;
				break;
			case PieceType.misc:
				selectedPiece = Instantiate(MainManager.GetMiscPieces()[index]);
				selectedPiece.name = MainManager.GetMiscPieces()[index].name;
				selectedPieceBaseRotation = selectedPiece.transform.rotation;
				break;
			default:
				return;
		}
		SetCollidersEnables(false); // disable the selected piece's collision so that it doesn't stop the raycast used to place it
	}

	// this function activates/deactivates the selected piece's collision
	// the collision needs to be disabled for the raycast used to place the piece, and then reactivated when placed
	// so that we can put other pieces over it
	private void SetCollidersEnables(bool enable)
	{
		Collider[] colliders = selectedPiece.GetComponentsInChildren<Collider>();
		foreach (Collider collider in colliders)
		{
			collider.enabled = enable;
		}
	}

	private void Remove(GameObject piece)
	{
		Destroy(piece);
	}

	// when we want to save a vehicle, we first remove all pieces that are unattached to the main body
	// as we have to wait for a frame so Unity correctly destroys the GameObjects, this is done in a coroutine
	// so we use a bool to know when we can save
	private bool hasFinishedRemovingPieces = true;

	// this function removes all pieces that are unattached to the main body, and notifies when it's done
	// first it gets the base, then looks for every piece attached to it, then for every piece
	// attached to the newly found ones, again and again until there is no new piece found
	private IEnumerator RemoveUnattachedPieces()
	{
		hasFinishedRemovingPieces = false;
		yield return new WaitForEndOfFrame();
		Transform[] allPieces = playerGO.GetComponentsInChildren<Transform>(); // get all pieces
		List<GameObject> attachedPieces = new List<GameObject>(); // this is the list of the pieces we have to keep
		foreach (Transform tr in allPieces)
		{
			// first we only put the base into the list
			if (tr.name == "Base")
			{
				attachedPieces.Add(tr.gameObject);
				break;
			}
		}
		if (attachedPieces.Count > 0) // normally there is always a base, but a security is always good
		{
			List<Transform> newPieces = new List<Transform>(); // these are all the pieces attached to the last pieces discovered
			newPieces.Add(attachedPieces[0].transform);
			while (newPieces.Count > 0) // loop as long as we discover new pieces
			{
				newPieces = GetAttachedPieces(newPieces, allPieces); // get the pieces attached to the last pieces discovered
				for (int i = 0; i < newPieces.Count; i++) // then for every piece found, we check if they are new
				{
					if (attachedPieces.Contains(newPieces[i].gameObject)) // we always have pieces that were found earlier, so we remove them to know how many are new (to know if we loop again)
					{
						newPieces.RemoveAt(i);
						i--;
					}
					else
					{ // if it's a new piece, we add it to the keep-list
						attachedPieces.Add(newPieces[i].gameObject);
					}
				}
			}
		}

		// finally, we look every piece, and if they're not in the keep-list we destroy them
		for (int i = 0; i < allPieces.Length; i++)
		{
			// we also check it's a piece, because there might be objects that are not pieces and we want to keep them
			if (allPieces[i].tag == "Piece" && !attachedPieces.Contains(allPieces[i].gameObject))
			{
				Destroy(allPieces[i].gameObject);
			}
		}
		yield return new WaitForEndOfFrame(); // we wait for a frame to be sure that Unity has correctly destroyed the objects
		hasFinishedRemovingPieces = true; // and we notify that this function is done, so we can save
	}

	// this function return every piece among the allPieces list that is close enough to the piecesToCheck list
	private List<Transform> GetAttachedPieces(List<Transform> piecesToCheck, Transform[] allPieces)
	{
		List<Transform> attachedPieces = new List<Transform>();
		foreach (Transform pieceToCheck in piecesToCheck) // for every piece we want to check
		{
			foreach (Transform tr in allPieces) // we look all pieces
			{
				// if one of them is at a correct distance (we know all pieces have a size of 1 for now) and it's not the same piece
				if (Vector3.Distance(tr.position, pieceToCheck.position) < 1.1f && tr != pieceToCheck && tr.tag == "Piece")
				{ // then this piece is attached to it
					attachedPieces.Add(tr);
				}
			}
		}
		return attachedPieces;
	}

	// this function removes the selected piece
	public void Deselect()
	{
		if (selectedPiece)
		{
			Destroy(selectedPiece);
			selectedPiece = null;
		}
	}

	// this function removes all pieces that are unattached from the main body and then saves the vehicle
	public void Save()
	{
		if (nameField.text == "") // first we make sure the vehicle has a name
		{
			return;
		}
		StartCoroutine(RemoveUnattachedPieces()); // we remove all unattached pieces
		StartCoroutine(SaveWhenReady()); // we save when RemoveUnattachedPieces() has finished
	}

	public void SelectLevel()
	{
		SceneManager.LoadScene("LevelSelection");
	}

	// this function is called by the load button
	// it opens the load menu
	// it also creates the vehicles load/erase buttons when needed (the first time this function is called and every time there is a new or a deleted save)
	public void StartLoad()
	{
		if (!loadables || !scrollbar) // security, we make sure the panel and scroll bar are linked
		{
			return;
		}
		loadables.SetActive(!loadables.activeSelf); // activate/deactivate the panel, it will automatically display/hide buttons as they're its children
		scrollbar.gameObject.SetActive(!scrollbar.gameObject.activeSelf); // activate/deactivate the scroll bar so the player can scroll if there are many saves
		scrollbar.value = 0f; // put the scroll bar at the start

		List<string> names = MainManager.GetSavesNames();
		foreach (string name in names)
		{
			// for every save, if the buttons don't exist already, we create them
			if (!loadVehicles.ContainsKey(name))
			{
				GameObject button = Instantiate(MainManager.GetLoadButtonPrefab(), loadables.transform);
				string realName = name.Substring(0, name.LastIndexOf('.'));
				button.name = realName;
				button.GetComponentInChildren<Text>().text = realName;
				loadVehicles.Add(name, button);
				button.GetComponent<Button>().onClick.AddListener(() =>
				{
					StartCoroutine(LoadVehicle(name));
				});
			}
			if (!eraseVehicles.ContainsKey(name))
			{
				GameObject button = Instantiate(MainManager.GetEraseButtonPrefab(), loadables.transform);
				string realName = name.Substring(0, name.LastIndexOf('.'));
				button.name = realName;
				eraseVehicles.Add(name, button);
				button.GetComponent<Button>().onClick.AddListener(() =>
				{
					EraseVehicle(name);
				});
			}
		}

		scrollbar.size = 5f / loadVehicles.Count; // we set the bar to the correct size, 5 buttons can be placed in the panel at the same time
	}

	// this function deletes the asked vehicle's save and also corresponding load/erase buttons
	private void EraseVehicle(string saveName)
	{
		if (saveName == player.GetSaveName()) // if the vehicle being deleted is the one being built, we notify that no vehicle is selected anymore
		{ // unlike LevelSelectionUI's EraseVehicle(), we don't need to check if the player is initialized, as we initialize it in the Start()
			MainManager.PlayerIsDeselected();
		}

		// we delete the load and erase buttons corresponding to the vehicle
		GameObject button = loadVehicles[saveName];
		loadVehicles.Remove(saveName);
		Destroy(button);
		button = eraseVehicles[saveName];
		eraseVehicles.Remove(saveName);
		Destroy(button);

		MainManager.RemoveSave(saveName); // we ask the MainManager to delete the save, because it's the MainManager script that manages the save system

		scrollbar.size = 5f / loadVehicles.Count; // we update the scroll bar's size
	}

	// this function loads the asked vehicle
	// unlike LevelSelectionUI's LoadVehicle() this is a coroutine because we have to remove current pieces before to load the vehicle
	private IEnumerator LoadVehicle(string saveName)
	{
		ErasePieces(); // first we have to remove all pieces currently placed
		yield return new WaitForEndOfFrame(); // we wait for a frame for Unity to correctly destroy the objects
		player.SetSaveName(saveName);
		player.Load(player.transform, false); // then we load the vehicle (we don't need to instantiate the base as there is already one)
		nameField.text = saveName.Substring(0, saveName.LastIndexOf('.')); // display the vehicle's name. We don't have to check if the input field is linked, because a warning is display on Start() if it isn't
		MainManager.PlayerIsSelected(); // notify the MainManager that a vehicle is selected so we can launch a level
	}

	// this function removes all pieces except the base
	private void ErasePieces()
	{
		Transform[] pieces = player.GetComponentsInChildren<Transform>();
		for (int i = 0; i < pieces.Length; i++)
		{
			if (pieces[i].name != "Player" && pieces[i].name != "Base") // we don't want to remove the root object or the base
			{
				Destroy(pieces[i].gameObject);
			}
		}
	}

	// this function waits for unattached pieces to be removed before saving the vehicle
	private IEnumerator SaveWhenReady()
	{
		while (!hasFinishedRemovingPieces)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForEndOfFrame(); // we wait another frame to be really sure
		player.SetSaveName(nameField.text + ".save");
		player.Save();
		MainManager.PlayerIsSelected(); // notify the MainManager that a vehicle is selected so we can launch a level

		// close load menu
		loadables.SetActive(false);
		scrollbar.gameObject.SetActive(false);
	}
}
