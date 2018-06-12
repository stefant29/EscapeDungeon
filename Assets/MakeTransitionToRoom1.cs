using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeTransitionToRoom1 : MonoBehaviour {


	public GameObject FrontWall_solid;
	public GameObject FrontWall_door;
	public GameObject FrameWall_solid;
	public GameObject FrameWall_door;

	public void makeTransitionStart() {
		

		FrameWall_door.SetActive(false);
		FrameWall_solid.SetActive(true);
	}

	public void makeTransitionLeaveRoom() {
		Debug.Log("doneeee");
		FrontWall_door.SetActive(true);
		FrontWall_solid.SetActive(false);
	}
}
