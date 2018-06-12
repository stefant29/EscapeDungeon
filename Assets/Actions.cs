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

	public static List<GameObject> inventory = new List<GameObject> ();
	private GameObject selectedItemInventory = null;
	private bool goForward = false;
	public Text m_MyText;

	private int commandExecuted = -20;

	// Use this for initialization
	void Start () {}

	public bool canTalk() {
		return Time.frameCount - commandExecuted >= 20;
	}
    // Update is called once per frame
    void Update() {
        if (!textMode && Input.GetKeyDown("t")) {
            textMode = true;
            // activate text input mode
            inputField.ActivateInputField();
        }

		if (Input.GetKeyDown("y")) {
			Stop();
        }

		if (!canTalk()) {
			Debug.Log("set to false");
			// TODO: uncomment
			// WatsonSpeechToText.GetComponent<ExampleStreaming>().Active = false;
		}

		if (commandExecuted > -20 && canTalk()) {
			commandExecuted = -20;
			m_MyText.text = "You can now speak again.";
			StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("You can now speak again."));

			// TODO: uncomment
			// WatsonSpeechToText.GetComponent<ExampleStreaming>().Active = true;
		}

		// Go forward
		if (goForward) {
			// Move to a point in front of the camera
			movePlayerToPoint.moveToPoint(_camera.transform.position + _camera.transform.forward * 0.01f);

			// If an object is in front of the camera, stop going forward
			if (ObjectBetween(_camera.transform.position, _camera.transform.position + _camera.transform.forward * 2f))
			// if (ObjectBetween(_camera.transform.position, _camera.transform.forward, 0.1f))
				goForward = false;
		}

		/* Draw lines from player (camera) to each door */
		/* 
			GameObject[] objs = GameObject.FindGameObjectsWithTag ("Door");
			Color[] colors = {Color.red, Color.black, Color.yellow};
			for (int i = 0; i < objs.Length; i++) {
				Vector3 fromPosition = _camera.transform.position;
				Vector3 toPosition = objs[i].GetComponent<Renderer>().bounds.center;
				Vector3 direction = toPosition - fromPosition;
				Debug.DrawRay(fromPosition, direction, colors[i]);
			}
		*/

		/* Move selectedItemInventory ahead of the player */
		if (selectedItemInventory) {
			Vector3 newPos = _camera.transform.position + _camera.transform.forward * 2.0f;
			// keep item on top of ground and not too high
			newPos.y = Mathf.Clamp(newPos.y, _camera.transform.position.y * 0.25f, _camera.transform.position.y * 0.75f);
			
			selectedItemInventory.transform.position = newPos;
			selectedItemInventory.transform.rotation = new Quaternion( 0.0f, _camera.transform.rotation.y, 0.0f, _camera.transform.rotation.w );
		}
    }

	/* returns True if an object is between the two positions given as parameters */
	private bool ObjectBetween(Vector3 startPos, Vector3 endPos) {
		RaycastHit hitInfo;
		return Physics.Linecast(startPos, endPos, out hitInfo) && hitInfo.collider.name != "Floor";
	}

	private bool ObjectBetween(Vector3 startPos, Vector3 direction, float radius) {
		RaycastHit hitInfo;
		return Physics.SphereCast(startPos, radius, direction, out hitInfo, 1f) && hitInfo.collider.name != "Floor";
	}

    GameObject getObjectInSight(string tag, float delta) {
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
		for (int i = 0; i < objs.Length; i++) {
			/* find the object in range of player's sight */
			if (inRangeSight (objs[i].GetComponent<BoxCollider>().bounds.center, delta, tag)) {
				/* return the object */
				Debug.Log ("Object " + objs[i].name + " found.");	
				return objs[i];
			}
		}
		return null;
	}

	/* returns true if the given item is in range and in sight */
	public bool inRangeSight(Vector3 position, float delta, string tag) {
		Vector3 point = _camera.WorldToViewportPoint(position);
		RaycastHit hitInfo;

		// is the given position in the FOV?
		return point.z > 0 && point.z < delta && point.x > 0 && point.x < 1 && point.y > 0 && point.y < 1 &&
			// and no other objects are in between the camera and the given position?
			Physics.Linecast(_camera.transform.position, position, out hitInfo) && hitInfo.collider.tag == tag;
	}

	/* method called when the "InputField" is changed */
	public void activateFocus(string input) {
		textMode = true;
	}

	/* replace spaces with %20 */
	public IEnumerator callWitAI (string message) {
		Debug.Log("Called Wit.AI WITH: " + message);
		// Debug.Log("Wit call: " + message);
		// construct the headers needed to authenticate on wit.ai
		Dictionary<string, string> headers = new Dictionary<string,string>();
		headers.Add("Authorization", "Bearer QLLF3GMFKHF3L5ZHYPNO6KJU2D2ZP4EE");

		// send WWW request to URL
		WWW www = new WWW("https://api.wit.ai/message?v=20180521&q="+message, null, headers);
		
		// so it won't stall the main thread
		yield return www;
		

		
		// parse and call methods returned from wit.ai
		parse_WIT_response(www.text);

		// Set the current frame as time when the last action was executed
		commandExecuted = Time.frameCount;
		Debug.Log("2");
	}

    /* parse response received from WIT */
    public void parse_WIT_response(string WIT_response) {
        /* convert response to JSON object */
        JObject jObject = JObject.Parse(WIT_response);
		Debug.Log("response from WIT: " + jObject);
	
		m_MyText.text = jObject["_text"].ToString();



        /* get entities from response */
        JToken jEntities = jObject["entities"];

        /* get actions and parameters */
        JArray jActions = (JArray)jEntities["intent"];
        JArray jParameters = (JArray)jEntities["parameter"];

        // deactivate the text InputField 
        textMode = false;

		Debug.Log(jObject);

		// TODO: "unlock and open the door" -> GOOD
		// TODO: "open and unlock the door" -> JUST UNLOCK!!!!
		// rezolvare in ordine: chiar daca se primeste "open and unlock", rezolva mai intai UNLOCK, apoi OPEN

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
		StartCoroutine(WatsonTextToSpeech.GetComponent<ExampleTextToSpeech>().Speak("Wait for a moment."));

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
		/* switch on what to open */
		switch (parameter) {
		case "door":
			GameObject door = getObjectInSight("Door", 5f);
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
		GameObject object_to_take = getObjectInSight(FirstLetterToUpper(parameter), 5f);
		if (object_to_take && !inventory.Contains(object_to_take)) {
			// add key to inventory list
			inventory.Add (object_to_take);

			// drop current item form hand
			DropCurrentItem();
			
			// Select item
			selectedItemInventory = object_to_take;

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
		if (selectedItemInventory) {
			// Add rigidBody to item deselected
			selectedItemInventory.AddComponent<Rigidbody>();
			selectedItemInventory.GetComponent<Rigidbody>().mass = 10000;
			// remove the collider
			selectedItemInventory.GetComponent<BoxCollider>().isTrigger = false;
			// remove item from inventory
			inventory.Remove(selectedItemInventory);
			// set current item to null
			selectedItemInventory = null;
		}
	}

	public void unlock(string parameter) {
		/* switch on what to unlock */
		switch (parameter) {
		case "door":
			GameObject door = getObjectInSight("Door", 5f);
			// Debug.Log("Dooor: " + door);
			if (door && door.GetComponent<Door> ().locked)// {
				// Debug.Log("if locked");
				useInventoryKey (door);
			// } else {
				// Debug.Log("ELSE LOCKED");
			// }
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
			Debug.Log("Go Forward");
			goForward = true;
			return;
		}

		// if tag given, go to object with tag
		GameObject go_to_object = getObjectInSight(FirstLetterToUpper(parameter), 15f);
		if (go_to_object) {
			Debug.Log("Go 1");
			movePlayerToPoint.moveToObject(go_to_object, 2f);
		} else
			Debug.Log("Go 2");
	}

	/* unlock the given door */
	public bool useInventoryKey(GameObject door) {
		for (int i = 0; i < inventory.Count; i++) {
			if (inventory [i].tag == "Key" && door.GetComponent<Door> ().unlock (inventory [i])) {
				Debug.Log("destroying: " + inventory[i]);
				Destroy(inventory[i]);
				inventory.Remove (inventory [i]);
				return true;
			}
		}
		return false;
	}

	/* Stop the current movement */
	public void Stop() {
		// reset bool "goForward"
		goForward = false;

		// Move player to its current position
		movePlayerToPoint.moveToObject(gameObject, 2f);
	}
}