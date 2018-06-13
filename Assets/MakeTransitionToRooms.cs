using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeTransitionToRooms : MonoBehaviour {


	public GameObject R1_FrontWall_solid;
	public GameObject R1_FrontWall_door;
	public GameObject R1_FrameWall_solid;
	public GameObject R1_FrameWall_door;

	public GameObject Dungeon;
	public GameObject Environment;

	public void enterRoom1() {
		R1_FrameWall_door.SetActive(false);
		R1_FrameWall_solid.SetActive(true);
	}

	public void exitRoom1() {
		Debug.Log("doneeee");
		Environment.SetActive(true);
		R1_FrontWall_door.SetActive(true);
		R1_FrontWall_solid.SetActive(false);
	}

	public void exitDungeon() {
		Dungeon.SetActive(false);
	}
}
