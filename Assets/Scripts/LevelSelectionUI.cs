using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionUI : MonoBehaviour
{
	/*
	this script manages the whole level selection menu
	*/

	[SerializeField] private Canvas canvas; // ref to the canvas used for the buttons (used for the load vehicle buttons, especially to know the scale of the window)

	[SerializeField] private GameObject loadables; // the panel for the vehicle load buttons
	[SerializeField] private Scrollbar scrollbar; // the scrollbar for the vehicle load buttons

	[SerializeField] private InputField nameField; // this input field is used as a text displaying the loaded vehicle's name (I used a input field for the bold effect when no vehicle is selected)

	private Dictionary<string, GameObject> loadVehicles = null; // every load button linked to the name of its vehicle (the name is used to load the save)
	private Dictionary<string, GameObject> eraseVehicles = null; // every erase button linked to the name of its vehicle (the name is used to delete the save)

	void Start()
	{
		GameObject prefab = MainManager.GetLevelButtonPrefab(); // the prefab for the level launch button

		string[] levels = MainManager.GetLevels(); // the levels are managed with an asset bundle (for updates), so we ask the MainManager for them
		int currentLevel = 1; // we'll create the buttons one by one and use this variable to know which level we're currently making the button for

		int levelCompletion = MainManager.GetLevelCompletion(); // we get the max level the player has finished to know which buttons are active

		// the load menu is closed by default
		loadables.SetActive(false);
		scrollbar.gameObject.SetActive(false);

		loadVehicles = new Dictionary<string, GameObject>();
		eraseVehicles = new Dictionary<string, GameObject>();

		if (!nameField) // security, we make sure the input field is linked to the script
		{
			Debug.LogWarning("NameField link is missing !");
		}
		else
		{
			if (MainManager.IsPlayerSelected()) // if there is a vehicle selected (the player saved/loaded a vehicle in the editor or he's already played a level and come back to the level selection)
			{ // then we get the save's name and store it into the name field
				string name = MainManager.GetPlayer().GetSaveName();
				nameField.text = name.Substring(0, name.LastIndexOf('.')); // what is after the . is the extension, we only want the name
			}
		}

		bool found;
		do // we loop as long as we find the corresponding level
		{ // for each we create the corresponding launch button and activate it if the player has gone far enough
			found = false;
			int index = -1;
			for (int i = 0; i < levels.Length && !found; i++) // we look in the level bundle if there is one with the correct name (levels can only be stored by names in asset bundles, unlike GameObjects)
			{
				if (levels[i].Contains("Level " + currentLevel))
				{
					index = i;
					found = true;
				}
			}
			if (found) // if the level is found, we create the corresponding launch button
			{
				GameObject button = Instantiate(prefab, new Vector3(/*index - 1 % 5, index - 1 / 5 (old formula)*/(((currentLevel - 1) % 5) + 1) * Screen.width / 6f, Screen.height / 2f - (currentLevel - 1) / 5 * Screen.height / 8f, 0f)/*this is a little formula to place the buttons in rows of 5*/, Quaternion.identity, transform);
				button.name = "Level " + currentLevel;
				button.GetComponent<Button>().onClick.AddListener(() =>
				{
					if (MainManager.IsPlayerSelected()) // the button only works if a vehicle is selected
					{
						SceneManager.LoadScene(levels[index]);
					}
				});
				button.GetComponentInChildren<Text>().text = button.name;
				// enable the button only if the player has completed the level before, or if he has completed the one just before
				if (currentLevel <= levelCompletion)
				{
					button.GetComponent<Image>().color = Color.green;
				}
				else if (currentLevel == levelCompletion + 1)
				{
					button.GetComponent<Image>().color = Color.blue; // the uncompleted level has a different color so that the player knows this is the one he has to play if he wants to continue
				}
				else
				{
					// if the player has not completed the level just before, then the current is unreachable
					button.GetComponent<Image>().color = Color.gray;
					button.GetComponent<Button>().interactable = false;
				}
				currentLevel++;
			}
		} while (found); // if no level is found, it means we've reached the last one
	}

	void Update()
	{
		// load buttons update
		if (canvas)
		{
			List<string> savesNames = MainManager.GetSavesNames(); // we get every registered save
			int index = 0;
			if (loadables.activeSelf)
			{
				RectTransform tr = loadables.GetComponent<RectTransform>(); // we use the panel as the reference point to place the buttons
				foreach (string name in savesNames)
				{
					// for every save we place the buttons at the correct place, using a little formula (considering the window scale, the panel size and the scrollbar state)
					if (loadVehicles.ContainsKey(name)) // security, we make sure there is a button for the save
					{
						loadVehicles[name].transform.position = new Vector3(tr.position.x + canvas.scaleFactor * (1.5f * tr.sizeDelta.x / 4f),
							tr.position.y - canvas.scaleFactor * (tr.sizeDelta.y / 10f) - canvas.scaleFactor * (index * tr.sizeDelta.y / 5f) +
							(savesNames.Count > 5 ? canvas.scaleFactor * scrollbar.value * (savesNames.Count - 5) * (tr.sizeDelta.y / 5f) : 0f));

						if (eraseVehicles.ContainsKey(name)) // security, even though both buttons are created together, you never know
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

	public void BackToMenu()
	{
		SceneManager.LoadScene("Menu");
	}

	public void BackToEditor()
	{
		SceneManager.LoadScene("ConstructionEditor");
	}

	public void RestartProgress()
	{
		if (MainManager.GetLevelCompletion() > 0) // if the player has not done any level, we don't need to restart the progress anyway
		{
			MainManager.RestartLevelCompletion(); // we ask the MainManager to restart the progress (because it's the MainManager script that manages the save system)
			SceneManager.LoadScene(SceneManager.GetActiveScene().name); // we could disable all launch level buttons except for the first one instead of reloading the scene, but this was faster to implement (this is a prototype after all)
		}
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
					LoadVehicle(name);
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
		if (MainManager.GetPlayer() && saveName == MainManager.GetPlayer().GetSaveName()) // if the vehicle being deleted is the one selected, we notify that no vehicle is selected anymore
		{ // we also have to check if the player is initialized ; as we init it when a vehicle is selected, we might delete a save before the player is initialized
			// in this case, anyway we know that we don't have to notify anything (as we're sure that no vehicle is selected)
			MainManager.PlayerIsDeselected();
			nameField.text = "";
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
	private void LoadVehicle(string name)
	{
		if (!nameField) // security, we make sure the name field is linked so we can display the chosen vehicle's name
		{
			Debug.LogWarning("NameField link is missing !");
			return;
		}
		nameField.text = name.Substring(0, name.LastIndexOf('.')); // display the vehicle's name
		Player player = MainManager.GetPlayer(); // we get the player to change its vehicle name and notify it that a vehicle has been selected
		if (!player) // if there is no player yet (as we init it when a vehicle is saved or loaded in the editor menu)
		{ // we init it here
			player = gameObject.AddComponent<Player>();
			MainManager.SetPlayer(player);
		}
		// change the player's vehicle's name and notify the MainManager that a vehicle is selected, so we can launch a level
		player.SetSaveName(name);
		MainManager.PlayerIsSelected();
	}
}
