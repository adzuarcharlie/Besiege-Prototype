using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	private void Start()
	{
		// this script is settled in the main menu, so we're sure it's the first script to be launched
		// thus we use it to load everything that we need
		if (!MainManager.IsLoaded()) // we make sure that we don't load again when the player returns to the main menu
		{
			MainManager.InitSavePath(); // first, we init the correct save path according to the project's place (we also have to check if we are in the editor or in a build)
			MainManager.ManageUpdate(); // we check for updates, and if there is one, do it
			MainManager.LoadAssets(); // after the update check, load the assets
			// these are mainly vehicle pieces, but also the vehicles saves and the levels completion
		}
	}

	// these functions are called by the corresponding buttons and simply load the correct scene
	public void Construction()
	{
		SceneManager.LoadScene("ConstructionEditor");
	}

	public void LevelSelection()
	{
		SceneManager.LoadScene("LevelSelection");
	}

	public void Quit()
	{
		Application.Quit();
	}
}
