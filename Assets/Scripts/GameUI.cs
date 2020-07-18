using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
	private GameObject winText = null; // the text printed when the player has finished a level

	private GameObject warningText = null; // the text printed when the player doesn't have an engine on his vehicle, and thus cannot move

	void Start()
    {
		// this links the texts to this script, so it doesn't have to be done manually (prevents from forgetting)
		winText = transform.Find("WinText").gameObject;
		winText.SetActive(false);

		warningText = transform.Find("WarningText").gameObject;
		warningText.SetActive(false);
	}

	void Update()
    {
        if (Input.GetKey(KeyCode.R)) // pressing R reloads the level just like the reload button
		{
			ReloadLevel();
		}
    }

	// the following functions are called by the corresponding buttons in play mode
	// each of them calls the correct scene
	public void BackToMenu()
	{
		SceneManager.LoadScene("Menu");
	}

	public void BackToEditor()
	{
		SceneManager.LoadScene("ConstructionEditor");
	}

	public void BackToLevelSelection()
	{
		SceneManager.LoadScene("LevelSelection");
	}

	// simply call the current scene to reload
	public void ReloadLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	// this displays the victory text (called when the player wins)
	public void ActivateWinText()
	{
		winText.SetActive(true);
	}

	//this displays the warning text (called when there is no engine on the player's vehicle)
	public void ActivateWarningText()
	{
		warningText.SetActive(true);
	}
}
