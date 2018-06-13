using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Make transitions between rooms.
 */
public class MakeTransitionToRooms : MonoBehaviour {

	// room 1 walls
	public GameObject R1_FrontWall_solid;
	public GameObject R1_FrontWall_door;
	public GameObject R1_FrameWall_solid;
	public GameObject R1_FrameWall_door;

	// Dungeon and Environment
	public GameObject Dungeon;
	public GameObject Environment;

	/* Remove wall with door and replace it with the frame with picture. */
	public void enterRoom1() {
		// remove wall with door
		R1_FrameWall_door.SetActive(false);

		// activate wall with frame and picture
		R1_FrameWall_solid.SetActive(true);
	}

	/* Remove simple wall and replace it with wall and door. 
	 * Add the outside environment.
	 */
	public void exitRoom1() {
		// set the environment as active
		Environment.SetActive(true);

		// activate wall with door
		R1_FrontWall_door.SetActive(true);

		// remove simple wall
		R1_FrontWall_solid.SetActive(false);
	}

	/* Exit the dungeon: remove the dungeon from the environment. */
	public void exitDungeon() {
		Dungeon.SetActive(false);
	}
}
