using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConstructionCamera : MonoBehaviour
{
	/*
	this script defines the camera behaviour in the construction mode
	you can simply turn it around the vehicle by click any mouse button
	and moving the mouse around

	PS : the camera can target any object even though it's only used for the player's vehicle so far
	 */

	[SerializeField] float sensivityX = 5f;
	[SerializeField] float sensivityY = 5f;

	private Transform target = null;

	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
	}

	// these are the data used to check the input
	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private EventSystem m_EventSystem;
	private PointerEventData m_PointerEventData;

	private bool isUsingScroller; // this is a special condition
	// as there is a scroller (for the load option)
	// we have to check if the player is using it before moving the camera

	void Update()
    {
		transform.LookAt(target);

		if (Input.GetMouseButtonDown(0)) // if the player starts pressing the mouse left button
		{
			m_PointerEventData = new PointerEventData(m_EventSystem);
			m_PointerEventData.position = Input.mousePosition;

			//Create a list of Raycast Results
			List<RaycastResult> results = new List<RaycastResult>();

			m_Raycaster.Raycast(m_PointerEventData, results);

			// we check every object that the mouse is pointing at
			foreach (RaycastResult result in results)
			{
				if (result.gameObject.name == "Scrollbar")
				{
					isUsingScroller = true; // if there is the scroll bar in the list, then we save the result to abort this function until the player stops using the scroll bar
					break;
				}
			}
		}
		if (!Input.GetMouseButton(0)) // when the player stops pressing the mouse left button, it means he's stopped using the scroll bar
		{
			isUsingScroller = false;
		}

		if (isUsingScroller)
		{ // while the player is using the scroll bar, abort this function as he's not willing to move the camera
			return;
		}
		// if the player is not using the scroll bar but still pressing a mouse button, then he's moving the camera
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			float horizontal = Input.GetAxis("Mouse X");
			float vertical = -Input.GetAxis("Mouse Y");

			transform.RotateAround(target.position, target.up, sensivityX * horizontal * Time.deltaTime);
			transform.RotateAround(target.position, target.right, sensivityY * vertical * Time.deltaTime);
		}
	}
}
