using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEngine.SceneManagement;

public static class MainManager
{
	/*
	this script manages all global data,
	mainly the asset bundles and the save system
	*/

	private static GameObject playerPrefab = null; // the prefab used to init the player
	public static GameObject GetPlayerPrefab()
	{
		return playerPrefab;
	}

	// we need to store some player data in this script to pass it from a scene to another
	// (for instance to load the vehicle selected in a menu into a scene)
	private static Player player = null;
	public static void SetPlayer(Player newPlayer)
	{
		player = newPlayer;
	}
	public static Player GetPlayer()
	{
		return player;
	}

	// all the following data are managed by asset bundles
	private static Material winMaterial = null;
	public static Material GetWinMaterial()
	{
		return winMaterial;
	}

	private static GameObject basePrefab = null;
	public static GameObject GetBasePrefab()
	{
		return basePrefab;
	}

	private static GameObject levelButtonPrefab = null;

	public static GameObject GetLevelButtonPrefab()
	{
		return levelButtonPrefab;
	}

	private static GameObject loadButtonPrefab = null;

	public static GameObject GetLoadButtonPrefab()
	{
		return loadButtonPrefab;
	}

	private static GameObject eraseButtonPrefab = null;
	public static GameObject GetEraseButtonPrefab()
	{
		return eraseButtonPrefab;
	}

	private static GameObject projectilePrefab = null;
	public static GameObject GetProjectilePrefab()
	{
		return projectilePrefab;
	}

	private static GameObject[] basics = null;
	public static GameObject[] GetBasicsPieces()
	{
		return basics;
	}

	private static GameObject[] movement = null;
	public static GameObject[] GetMovementPieces()
	{
		return movement;
	}

	private static GameObject[] weapons = null;
	public static GameObject[] GetWeaponsPieces()
	{
		return weapons;
	}

	private static GameObject[] misc = null;
	public static GameObject[] GetMiscPieces()
	{
		return misc;
	}

	private static GameObject[] colliders = null;
	public static GameObject[] GetColliders()
	{
		return colliders;
	}

	private static string[] levels = null;
	public static string[] GetLevels()
	{
		return levels;
	}

	private static bool isLoaded = false; // this bool is used to know if asset bundles and saves are loaded
	public static bool IsLoaded()
	{
		return isLoaded;
	}

	// this bool is used to know if a vehicle save has been selected,
	// so that we don't launch a level until we've selected one (used by the LevelSelectionUI script)
	private static bool isPlayerSelected = false;
	public static void PlayerIsSelected()
	{
		isPlayerSelected = true;
	}

	public static void PlayerIsDeselected()
	{
		isPlayerSelected = false;
	}

	public static bool IsPlayerSelected()
	{
		return isPlayerSelected;
	}

	private static string savePath; // the saves directory's path

	public static string GetSavePath()
	{
		return savePath;
	}

	private static List<string> savesNames = null; // the list of all vehicles saves

	public static List<string> GetSavesNames()
	{
		return savesNames;
	}

	// this function adds a vehicle save to the file referencing all saves
	public static void AddSaveName(string saveName)
	{
		if (!savesNames.Contains(saveName)) // security, we don't need to add the name if it is already in the list
		{ // (when the player registers a vehicle with a name that is already used, it rewrites the other save)
			savesNames.Add(saveName);
		}

		string path = savePath + "saves.txt"; // we get the path to the saves reference file

		// we don't bother adding a line at the end and checking if there is already a save with the same name
		// we simply rewrite the whole file with the names contained in the savesNames list
		StreamWriter writer = File.CreateText(path);
		
		foreach (string name in savesNames)
		{
			writer.WriteLine(name);
		}

		writer.Close();
	}

	//  this function deletes a vehicle save
	public static void RemoveSave(string saveName)
	{
		if (!savesNames.Contains(saveName)) // security, we make sure the save exists
		{
			return;
		}
		savesNames.Remove(saveName);
		// don't need to erase the line in the text file, because next time we will write the file it will be overwritten
		// even if we don't rewrite the file, we check during the saves reference file reading that every file exists (in LoadAssets())
		string path = savePath + saveName;

		File.Delete(path); // delete the save file
	}

	private static int levelCompletion; // the furthest level that the player has completed

	// this function resets the level completion and rewrites the level completion save file
	public static void RestartLevelCompletion()
	{
		levelCompletion = 0;
		string path = savePath + "levels.save";
		FileStream file = File.Create(path);
		BinaryWriter writer = new BinaryWriter(file);
		writer.Write(levelCompletion);
		writer.Close();
		file.Close();
	}

	public static int GetLevelCompletion()
	{
		return levelCompletion;
	}

	// this function is called when the player finishes a level,
	// it takes care of saving the level completion if needed
	public static void CompletedLevel(string levelName)
	{
		string strNumber = levelName.Substring(levelName.IndexOf(' ') + 1); // every level is called "Level X", so to get the number we only need to look after the " "
		int number = int.Parse(strNumber); // then we can convert the string to an int
		if (levelCompletion < number) // if the level completed is further than the current completion, then we rewrite the level completion file
		{
			levelCompletion = number;
			string path = savePath + "levels.save";
			FileStream file = File.Create(path);
			BinaryWriter writer = new BinaryWriter(file);
			writer.Write(levelCompletion);
			writer.Close();
			file.Close();
		}
	}

	// this function is called when the player finishes a level,
	// it takes care of loading the next level if there is one (else we go back to the level selection menu)
	public static void LoadNextLevel(string levelName)
	{
		string strNumber = levelName.Substring(levelName.IndexOf(' ') + 1); // every level is called "Level X", so to get the number we only need to look after the " "
		int number = int.Parse(strNumber); // then we can convert the string to an int
		if (levels.Length > number) // if the level completed is not the last level, we can load the next one
		{
			SceneManager.LoadScene(levels[number]); // the list start at index 0, where the first level is Level 1, so the number of the current level corresponds to the index of the next level
		}
		else
		{
			SceneManager.LoadScene("LevelSelection"); // if the level completed is the last level, we go back to the level selection menu
		}
	}

	// has to be called before LoadAssets() and ManageUpdates(),
	// it inits the saves directory file according to the project's location
	// and if it is played in editor or in a build version
	public static void InitSavePath()
	{
		savePath = Directory.GetCurrentDirectory() + (Application.isEditor ? "/Saves/" : "/../Saves/");
	}

	// this function loads all asset bundles and the saves (vehicles saves and level completion)
	public static void LoadAssets()
	{
		// we load materials first because the pieces need them
		AssetBundle materialsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "materials"));
		winMaterial = materialsBundle.LoadAsset<Material>("WinZoneMaterial");

		// every prefab is stored separatedly
		AssetBundle prefabBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "prefabs"));
		basePrefab = prefabBundle.LoadAsset<GameObject>("Base");
		playerPrefab = prefabBundle.LoadAsset<GameObject>("Player");
		levelButtonPrefab = prefabBundle.LoadAsset<GameObject>("Level 0");
		loadButtonPrefab = prefabBundle.LoadAsset<GameObject>("LoadButton");
		eraseButtonPrefab = prefabBundle.LoadAsset<GameObject>("eraseButton");
		projectilePrefab = prefabBundle.LoadAsset<GameObject>("Projectile");

		// we load every piece of a type at once and do it for each type
		AssetBundle basicsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "basics"));
		basics = basicsBundle.LoadAllAssets<GameObject>();

		AssetBundle movementBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "movement"));
		movement = movementBundle.LoadAllAssets<GameObject>();

		AssetBundle weaponsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "weapons"));
		weapons = weaponsBundle.LoadAllAssets<GameObject>();

		AssetBundle miscBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "misc"));
		misc = miscBundle.LoadAllAssets<GameObject>();

		AssetBundle levelsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "levels"));
		levels = levelsBundle.GetAllScenePaths();

		AssetBundle collidersBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "colliders"));
		colliders = collidersBundle.LoadAllAssets<GameObject>();

		// init saves
		savesNames = new List<string>();

		string path = savePath + "saves.txt";

		if (File.Exists(path)) // if the vehicles saves file exists, we can read it
		{
			StreamReader file = File.OpenText(path);

			while (!file.EndOfStream)
			{
				string saveName = file.ReadLine(); // each line corresponds to a vehicle name
				if (File.Exists(savePath + saveName)) // we verify that the corresponding vehicle file exists, else it means it has been deleted (manually or by RemoveSave())
				{
					savesNames.Add(saveName);
				}
			}

			file.Close();
		}
		else // else we create it and there is no vehicle saved yet
		{
			StreamWriter file = File.CreateText(path);
			file.Close();
		}

		// level completion
		path = savePath + "levels.save";

		if (File.Exists(path)) // if the level completion file exists, we can simply read the number contained within
		{ // as this file only register one number corresponding to the furthest completed level
			FileStream file = File.Open(path, FileMode.Open);
			BinaryReader reader = new BinaryReader(file);
			levelCompletion = reader.ReadInt32();
			reader.Close();
			file.Close();
		}
		else
		{
			levelCompletion = 0;
			// no need to create a file yet, only create when we've completed a level
		}

		isLoaded = true;
	}

	// has to be called before LoadAssets()
	// it looks in the version file if there is a new version of the asset bundles
	// and if there is, replaces all asset bundles by the new ones
	public static void ManageUpdate()
	{
		int currentVersion;
		string cfgPath = Path.Combine(Application.streamingAssetsPath, "version.cfg");
		StreamReader cfgFile;
		if (File.Exists(cfgPath)) // if there is a config file, we can read the version written in it
		{
			cfgFile = File.OpenText(cfgPath);
			currentVersion = int.Parse(cfgFile.ReadLine());
			cfgFile.Close();
		}
		else // else we consider it's the version 0 by default
		{
			currentVersion = 0;
		}

		string updatePath = savePath + "../Updates/version.cfg"; // then we look for the updated version file

		if (!File.Exists(updatePath)) // no update available
		{
			return;
		}

		StreamReader updateFile = File.OpenText(updatePath);
		int latestVersion = int.Parse(updateFile.ReadLine()); // we read the updated version number

		if (latestVersion == currentVersion) // is already updated
		{
			return;
		}

		// if there is an update to do, rewrite every streaming asset file
		File.Copy(savePath + "../Updates/basics", Path.Combine(Application.streamingAssetsPath, "basics"), true);
		File.Copy(savePath + "../Updates/basics.manifest", Path.Combine(Application.streamingAssetsPath, "basics.manifest"), true);

		File.Copy(savePath + "../Updates/colliders", Path.Combine(Application.streamingAssetsPath, "colliders"), true);
		File.Copy(savePath + "../Updates/colliders.manifest", Path.Combine(Application.streamingAssetsPath, "colliders.manifest"), true);

		File.Copy(savePath + "../Updates/levels", Path.Combine(Application.streamingAssetsPath, "levels"), true);
		File.Copy(savePath + "../Updates/levels.manifest", Path.Combine(Application.streamingAssetsPath, "levels.manifest"), true);

		File.Copy(savePath + "../Updates/materials", Path.Combine(Application.streamingAssetsPath, "materials"), true);
		File.Copy(savePath + "../Updates/materials.manifest", Path.Combine(Application.streamingAssetsPath, "materials.manifest"), true);

		File.Copy(savePath + "../Updates/misc", Path.Combine(Application.streamingAssetsPath, "misc"), true);
		File.Copy(savePath + "../Updates/misc.manifest", Path.Combine(Application.streamingAssetsPath, "misc.manifest"), true);

		File.Copy(savePath + "../Updates/movement", Path.Combine(Application.streamingAssetsPath, "movement"), true);
		File.Copy(savePath + "../Updates/movement.manifest", Path.Combine(Application.streamingAssetsPath, "movement.manifest"), true);

		File.Copy(savePath + "../Updates/prefabs", Path.Combine(Application.streamingAssetsPath, "prefabs"), true);
		File.Copy(savePath + "../Updates/prefabs.manifest", Path.Combine(Application.streamingAssetsPath, "prefabs.manifest"), true);

		File.Copy(savePath + "../Updates/StandaloneWindows", Path.Combine(Application.streamingAssetsPath, "StandaloneWindows"), true);
		File.Copy(savePath + "../Updates/StandaloneWindows.manifest", Path.Combine(Application.streamingAssetsPath, "StandaloneWindows.manifest"), true);

		File.Copy(savePath + "../Updates/weapons", Path.Combine(Application.streamingAssetsPath, "weapons"), true);
		File.Copy(savePath + "../Updates/weapons.manifest", Path.Combine(Application.streamingAssetsPath, "weapons.manifest"), true);

		StreamWriter cfgWriter = File.CreateText(cfgPath);
		cfgWriter.WriteLine(latestVersion.ToString()); // and register the new version number
		cfgWriter.Close();
	}
}
