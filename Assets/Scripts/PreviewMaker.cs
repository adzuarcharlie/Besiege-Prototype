using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewMaker : MonoBehaviour
{
	/*
	we have previews of the pieces in the editor, this script takes care of creating them on runtime
	when the editor is launched (the renderTextures cannot be transferred from one scene to another)
	
	I could have simply taken screenshots of the pieces and made PNGs, but I wanted to implement
	a generic method, so that I don't have to add/modify files when adding/modifying pieces

	when asked to make a preview from any object, this script will spawn it in front of a camera
	in the correct orientation and then put the camera's render into a renderTexture that can later
	be asked by any other script to show the preview

	as there is one camera, we can only make one preview at a time.
	we also have to wait for a frame to init the object correctly, so each preview takes a frame to make.
	this script contains a system to make previews wait for their turn, even when they're all asked at
	the same frame.
	as we don't have a lot of pieces, all previews are supposedly done within less than half a second, so
	it's "invisible" to the player, but anyway there is also a system (in the Constructor script)
	that blocks showing the previews until they're all done
	*/

	[SerializeField] private new Camera camera; // the render camera, already placed in the scene
	[SerializeField] private Transform spawn; // the place where objects are to be spawned at

	private Dictionary<GameObject, RenderTexture> textures = null; // all previews, captured or waiting to be, linked with the object they're representing
	private Dictionary<GameObject, bool> isLoaded = null; // the state of the different objects (if they are rendered or not)

	private GameObject currentObj = null; // the object that is currently being rendered

	// this inits this script's data
	// this is not put in a Start() because we want to know when this is initialized
	public void Init()
	{
		if (textures == null)
		{
			textures = new Dictionary<GameObject, RenderTexture>();
		}
		if (isLoaded == null)
		{
			isLoaded = new Dictionary<GameObject, bool>();
		}
		camera.enabled = false; // the camera is only enabled when we're making a preview, for optimization (avoids having 2 cameras rendering in the editor)
	}

	// this function waits for the rendering to be available and then creates the preview
	private IEnumerator WaitForRender(GameObject obj)
	{
		while (currentObj) // as long as there is an object being rendered, wait
		{ // currentObj is initialized to null, so the first call of WaitForRender() won't wait
			yield return new WaitForEndOfFrame();
		}
		currentObj = obj; // once the rendering is available, we immediatly tell the script that there is an object being rendered
		camera.enabled = true; // then we activate the camera
		camera.targetTexture = textures[obj]; // we set the texture associated with the object as the target texture of the camera, so that it will render directly into it
		// then we spawn the object and make sure the camera is looking at it
		GameObject go = Instantiate(obj, spawn.position, spawn.rotation * obj.transform.rotation, spawn);
		camera.transform.LookAt(spawn);

		yield return new WaitForEndOfFrame(); // we have to wait for a frame for Unity to correctly spawn the object

		// we render the camera's view into the texture
		camera.forceIntoRenderTexture = true;
		camera.Render();
		textures[obj].Create();
		isLoaded[obj] = true; // the object's preview is now made
		Destroy(go); // we don't need this object anymore
		camera.targetTexture = null; // we make sure that nothing else will erase the texture
		currentObj = null; // the current object is rendered, so we tell the place is free
		camera.enabled = false; // we deactivate the camera again, for optimization
	}

	// this function is called by another script to require a preview, it adds the wanted object to the queue
	public bool MakePreview(GameObject obj)
	{
		if (textures.ContainsKey(obj)) // if the object already has a preview, there is no need of making another
		{
			return true;
		}
		// create the texture to the wanted size and format
		RenderTexture texture = new RenderTexture(90, 90, 24);
		texture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat;
		// add the object to the lists
		textures.Add(obj, texture);
		isLoaded.Add(obj, false);
		// then wait for the render to be done
		StartCoroutine(WaitForRender(obj));
		return false;
	}

	// this function tells if all previews are done or not
	// it's used in the Constructor's blocking system, it makes sure we don't show the previews if they're not ready
	public bool IsMakingPreview()
	{
		return isLoaded.ContainsValue(false);
	}

	// this function tells if a specified object is waiting to be rendered (not used so far, but this could come in handy)
	public bool IsMakingPreview(GameObject obj)
	{
		return textures.ContainsKey(obj) && !isLoaded[obj];
	}

	// this returns the preview of the wanted object if it is available
	public RenderTexture GetPreview(GameObject obj)
	{
		if (textures.ContainsKey(obj) && isLoaded[obj])
		{
			return textures[obj];
		}
		else
		{
			return null;
		}
	}
}
