using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;

using System;
using System.Text;

using Newtonsoft.Json.Linq;

public class Actions : MonoBehaviour {

	public static bool textMode = false;
	public InputField inputField;
	public Camera _camera;
	public GameObject WatsonSpeechToText;
	public GameObject WatsonTextToSpeech;
	public GameObject Picture;

	private List<GameObject> inventory = new List<GameObject> ();
	private Dictionary<GameObject, Transform> inventory_parents = new Dictionary<GameObject, Transform>(); 
	private GameObject selectedItemInventory = null;
	private bool goForward = false;
	public Text m_MyText;

	private bool pickCategory = false;

	private int duration = 30;

	private int commandExecuted = 0;

	// Initializations
	void Start () {
		commandExecuted = duration * (-1);

		// Starting instructions
		// StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak(
		// 				"You are now in a dungeon. To escape it, you must firstly find the key to unlock the door. " + 
		// 				"To control the environment, just say what you want to happen. To move around, use GO and STOP."));
		// m_MyText.text = "You are now in a dungeon. To escape it, you must firstly find the key to unlock the door. " + 
		// 				"To control the environment, just say what you want to happen. To move around, use GO and STOP.";
		
		// // wait for 350 frames before the next command
		// resetWaitTime(400);
	}

    // Update is called once per frame
    void Update() {
        if (!textMode && Input.GetKeyDown("t")) {
            textMode = true;
            // activate text input mode
            inputField.ActivateInputField();
        }

		if (Input.GetKeyDown("y"))
			Stop();

		// after "duration" frames, let the user speak again
		if (commandExecuted > duration * (-1) && canTalk()) {
			commandExecuted = duration * (-1);
			duration = 30;
			m_MyText.text = "You can now speak again.";

			// TODO: uncomment
			StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("You can now speak again."));
		}

		// Go forward only if "go" command was given
		if (goForward) {
			// Move to a point in front of the camera
			movePlayerToPoint.moveToPoint(_camera.transform.position + _camera.transform.forward * 0.01f);

			// If an object is in front of the camera, stop going forward
			if (ObjectBetween(_camera.transform.position, _camera.transform.position + _camera.transform.forward * 2f))
				goForward = false;
		}

		/* Move selectedItemInventory ahead of the player */
		if (selectedItemInventory) {
			// the new position for selectedItemInventory
			Vector3 newPos = _camera.transform.position + _camera.transform.forward * 2.0f;

			// keep item on top of ground and not too high
			newPos.y = Mathf.Clamp(newPos.y, _camera.transform.position.y * 0.25f, _camera.transform.position.y * 0.75f);
			
			// set position for selectedItemInventory
			selectedItemInventory.transform.position = newPos;

			// set rotation for selectedItemInventory
			selectedItemInventory.transform.rotation = new Quaternion( 0.0f, _camera.transform.rotation.y, 0.0f, _camera.transform.rotation.w );
		}
    }

	/* Returns true if the user can talk again: 
	 *	"duration" frames passed since the last coomand was executed 
	 */
	public bool canTalk() {
		return Time.frameCount - commandExecuted >= duration;
	}

	/* Returns True if an object is between the two positions given as parameters */
	private bool ObjectBetween(Vector3 startPos, Vector3 endPos) {
		RaycastHit hitInfo;

		// cast a ray from startPos to endPos and see if there is an object between then except the floor
		return Physics.Linecast(startPos, endPos, out hitInfo) && 
					hitInfo.collider.name != "Floor" && hitInfo.collider.name != selectedItemInventory.name;
	}

	/* Returns the object with given tag in camera's sight and in a given range (delta) */
    private GameObject getObjectInSight(string tag, float delta) {
		// get all objects with given tag
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);

		for (int i = 0; i < objs.Length; i++) 
			// find the object in range of player's sight

			if (inRangeSight (objs[i].GetComponent<BoxCollider>().bounds.center, delta, tag))
				// return the object 
				return objs[i];

		// no object was found
		return null;
	}

	/* Returns true if the given item is in range and in sight */
	private bool inRangeSight(Vector3 position, float delta, string tag) {
		Vector3 point = _camera.WorldToViewportPoint(position);
		RaycastHit hitInfo;

		if (tag != null) 			// is the given position in the FOV?
			return point.z > 0 && point.z < delta && point.x > 0 && point.x < 1 && point.y > 0 && point.y < 1 &&
				// and no other objects are in between the camera and the given position?
				Physics.Linecast(_camera.transform.position, position, out hitInfo) && hitInfo.collider.tag == tag;

		// if no tag was provided, do not check for tag match with the collider
		return point.z > 0 && point.z < delta && point.x > 0 && point.x < 1 && point.y > 0 && point.y < 1 &&
			Physics.Linecast(_camera.transform.position, position, out hitInfo);			
	}

	/* Method called when the "InputField" is changed */
	public void activateFocus(string input) {
		textMode = true;
	}

	/* Set credentials and call WitAI with a given message,
	 *	then parse the response into actions
	 */
	public IEnumerator callWitAI (string message) {
		// construct the headers needed to authenticate on wit.ai
		Dictionary<string, string> headers = new Dictionary<string,string>();
		headers.Add("Authorization", "Bearer QLLF3GMFKHF3L5ZHYPNO6KJU2D2ZP4EE");

		// send WWW request to URL
		WWW www = new WWW("https://api.wit.ai/message?v=20180521&q=" + message, null, headers);
		
		// so it won't stall the main thread
		yield return www;
				
		// parse and call methods returned from wit.ai
		parse_WIT_response(www.text);

		// update the time frame when the last command is executed
		commandExecuted = Time.frameCount;
	}

	/* Set the ending time of an action as current frame 
	 *	and reset duration to a given parameter
	 */
	private void resetWaitTime(int time) {
		commandExecuted = Time.frameCount;
		duration = time;
	}

    /* parse response received from WIT AI */
    public void parse_WIT_response(string WIT_response) {
        /* convert response to JSON object */
        JObject jObject = JObject.Parse(WIT_response);
		// m_MyText.text = jObject["_text"].ToString(); TODO????

        /* get entities from response */
        JToken jEntities = jObject["entities"];

        /* get actions and parameters, numbers and quiz types */
        JArray jActions = (JArray)jEntities["intent"];
        JArray jParameters = (JArray)jEntities["parameter"];
        JArray jNumbers = (JArray)jEntities["number"];
        JArray jQuiz = (JArray)jEntities["type_of_quiz"];

        // deactivate the text InputField 
        textMode = false;

		/* if a quiz category was given */
		if (pickCategory && jQuiz != null) {
			if (jQuiz.Count > 1) {
				// tell the user only one category is allowed
				StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("Your response must contain only one category."));
				m_MyText.text = "Your response must contain only one category.";
				
				// wait for 90 frames to speak the instructions
				resetWaitTime(90);
			} else {
				foreach (JToken Quiz in jQuiz) {
					string response = (string)(Quiz["value"]);
					// select the domain, set the responses and load the first image
					Picture.GetComponent<LoadImages>().selectQuizDomain(response);
					Picture.GetComponent<LoadImages>().setRandomResponses();
        			Picture.GetComponent<LoadImages>().loadNextImage();

					// instruct the user to the next steps he must take
					StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak(
						"To pass to the next room, you have to correctly guess the next pictures. Look at the options and answer with the number you think is correct."));
					m_MyText.text = "To pass to the next room, you have to correctly guess the next pictures. Look at the options and answer with the number you think is correct.";
				
					// wait for 90 frames to speak the instructions
					resetWaitTime(220);
				}
			}
		}

		 /* if a number was given */
        if (jNumbers != null) {
			if (jNumbers.Count > 1) {
				// tell the user only one number must be chosen
				StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("Your response must contain only one number."));
				m_MyText.text = "Your response must contain only one number.";
				
				// wait for 90 frames to speak the instructions
				resetWaitTime(90);
			} else {
				foreach (JToken JNumber in jNumbers) {
					int response = (int)(JNumber["value"]);
					if (Picture.GetComponent<LoadImages>().checkCorrect(response)) {
						// congratulate the user on success
						StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("Good."));
						m_MyText.text = "Good.";
						
						// wait for 30 frames to speak the instructions
						resetWaitTime(30);
					} else {
						// tell the user the answer is incorrect
						StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("Wrong guess. Try again."));
						m_MyText.text = "Wrong guess. Try again.";

						// wait for 90 frames to speak the instructions
						resetWaitTime(50);
					}
				}
			}
		}

        /* if an action exists */
        if (jActions != null)
            /* for each action */
            foreach (JToken jAction in jActions) {
                string action = (string)jAction["value"];

				/* get all objects in the message */
				if (jParameters != null) {
					foreach (JToken jParameter in jParameters) {
						string parameter = (string) jParameter[(string)jParameter["type"]];

						/* try to apply each action on each object */
						applyAction(action, parameter);
					}
				// Actions with NO parameters
				} else 
					// Apply action with NULL as parameter
					applyAction(action, null);
            }
    }

	/* method called at the end of InputField edit */
    public void parseInput(string input) {
		// set response guide for user
		m_MyText.text = "Waiting for response from WIT.ai";

		// send input to WIT to parse: replace space with %20
		StartCoroutine(callWitAI(input.Replace(" ", "%20")));
	}

	/* Call an action with a parameter */
	public void applyAction(string action, string parameter) {
		Debug.Log("***********" + action + "***********" + parameter + "***********");
        
        /* switch on actions */
        switch (action) {
		case "open":
			open (parameter);
			break;
		case "take":
			take (parameter);
			break;
		case "drop":
			DropCurrentItem ();
			break;
		case "unlock":
			unlock (parameter);
			break;
		case "go":
			Go(parameter);
			break;
		default:
			Debug.LogError ("Action unknown: " + action);
			break;
		}
	}

	/* Open the object with tag given as parameter */
	public void open(string parameter) {
		// switch on what to open
		switch (parameter) {
		case "door":
			// find the door which is in sight
			GameObject door = getObjectInSight("Door", 5f);

			// atempt to unlock the door only if there is one and it is locked
			if (door && !door.GetComponent<Door> ().locked)
				StartCoroutine (door.GetComponent<Door> ().Move ());
			break;
		default:
			Debug.LogError ("input unknown: Cannot open " + parameter);
			break;
		}
	}

	/* Take into hand/inventory the object with tag given as parameter */
	public void take(string parameter) {
		// get the object with given tag that is in sight
		GameObject object_to_take = getObjectInSight(FirstLetterToUpper(parameter), 5f);

		// only if the given object is found and is not already in inventory
		if (object_to_take && !inventory.Contains(object_to_take)) {
			// add key to inventory list
			inventory.Add (object_to_take);

			// drop current item form hand
			DropCurrentItem();
			
			// Select item
			selectedItemInventory = object_to_take;

			// save parent
			inventory_parents[selectedItemInventory] = selectedItemInventory.transform.parent;

			// Set parent as null
			selectedItemInventory.transform.SetParent(null);

			// Reactivate collider
			selectedItemInventory.GetComponent<BoxCollider>().isTrigger = true;

			// Destroy rigidBody for selected item
			Destroy (selectedItemInventory.GetComponent<Rigidbody>());
		}
	}

	/* Drops current item form hand */
	public void DropCurrentItem() {
		// if an item exists in the inventory
		if (selectedItemInventory) {
			// Add rigidBody to item deselected
			selectedItemInventory.AddComponent<Rigidbody>();
			selectedItemInventory.GetComponent<Rigidbody>().mass = 10000;

			// remove the collider
			selectedItemInventory.GetComponent<BoxCollider>().isTrigger = false;

			// reset the parent
			selectedItemInventory.transform.SetParent(inventory_parents[selectedItemInventory]);

			// remove item from inventory
			inventory.Remove(selectedItemInventory);
			inventory_parents.Remove(selectedItemInventory);

			// set current item to null
			selectedItemInventory = null;
		}
	}

	/* Unlock an object with the tag given as parameter */
	public void unlock(string parameter) {
		// switch on what to unlock
		switch (parameter) {
		case "door":
			// get the door in sight and in range
			GameObject door = getObjectInSight("Door", 5f);

			// if a door was found and is locked
			if (door && door.GetComponent<Door> ().locked)
				// unlock the door
				useInventoryKey (door);
			break;
		default:
			Debug.LogError ("input unknown: Cannot unlock " + parameter);
			break;
		}
	}

	/* Convert string to first letter uppercase */
	public string FirstLetterToUpper(string str) {
		if (str == null)
			return null;

		if (str.Length > 1)
			return char.ToUpper(str[0]) + str.Substring(1);

		return str.ToUpper();
	}

	/* Go to the object with tag given as parameter */
	public void Go(string parameter) {
		// if no parameter given, go forward
		if (parameter == null) {
			goForward = true;
			return;
		}

		// if tag given, go to object with tag
		GameObject go_to_object = getObjectInSight(FirstLetterToUpper(parameter), 15f);
		if (go_to_object) 
			movePlayerToPoint.moveToObject(go_to_object, 2f);
	}

	/* unlock the given door */
	public bool useInventoryKey(GameObject door) {
		// go through inventory
		for (int i = 0; i < inventory.Count; i++) {
			// find a key that can unlock the door
			if (inventory [i].tag == "Key" && door.GetComponent<Door> ().unlock (inventory [i])) {
				// destory the key
				Destroy(inventory[i]);

				// remove the key from inventory
				inventory.Remove (inventory [i]);

				// the door was unlocked -> success
				return true;
			}
		}
		// the door was not unlocked -> fail
		return false;
	}

	/* Stop the current movement */
	public void Stop() {
		// reset bool "goForward"
		goForward = false;

		// Move player to its current position
		movePlayerToPoint.moveToObject(gameObject, 2f);
	}

	/* define behavior on collision between player and objects */
	void OnTriggerEnter(Collider other) {
		switch (other.name) {
			case "Room1_enter":
				if (!pickCategory) {
					StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("Please pick a category: ACTORS, CARS, CARTOONS or PAINTINGS."));
					m_MyText.text = "Please pick a category: ACTORS, CARS, CARTOONS or PAINTINGS.";
					resetWaitTime(160);

					// let the user pick a category
					pickCategory = true;

					// make transition
					other.GetComponent<MakeTransitionToRooms>().enterRoom1();
				}
				break;
			case "Dungeon_exit":
				// end message
				StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("Congratulations. You have escaped the dungeon!"));
				m_MyText.text = "Congratulations. You have escaped the dungeon!";
				resetWaitTime(200);

				// remove dungeon
				other.GetComponent<MakeTransitionToRooms>().exitDungeon();

				// deactivate speech to Text
				if (WatsonSpeechToText.GetComponent<ExampleStreaming>())
					WatsonSpeechToText.GetComponent<ExampleStreaming>().Active = false;
				break;
			default:
				Debug.Log("Collision with: " + other.name);
				break;	
		}
    }
}