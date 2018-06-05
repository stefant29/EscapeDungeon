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
	public Canvas canvas;
	public Canvas messageCanvas;
	public static List<GameObject> inventory = new List<GameObject> ();

	public GameObject mini_key;

	public Text m_MyText;

	// Use this for initialization
	void Start () {
		MonoBehaviour.print("Start");
	}

    // Update is called once per frame
    void Update()
    {
        if (!textMode && Input.GetKeyDown("t"))
        {
            textMode = true;
            // activate text input mode
            inputField.ActivateInputField();
        }


		/* Draw lines from player (camera) to each door */
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("Door");
		Color[] colors = {Color.red, Color.black, Color.yellow};
		for (int i = 0; i < objs.Length; i++) {
			Vector3 fromPosition = _camera.transform.position;
 			Vector3 toPosition = objs[i].GetComponent<Renderer>().bounds.center;
 			Vector3 direction = toPosition - fromPosition;
			Debug.DrawRay(fromPosition, direction, colors[i]);
		}
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

	/* checks if item is in inventory 
	public bool containsInventory(string item) {
		for (int i = 0; i < inventory.Count; i++)
			if (inventory [i].name == item)
				return true;
		return false;
	}
	/* remove item from inventory 
	public void dropFromInventory(string item) {
		for (int i = 0; i < inventory.Count; i++)
			if (inventory [i].name == item) {
				inventory.Remove (inventory [i]);

				Debug.Log (inventory.Count);
				break;
			}
	}
	*/

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
	IEnumerator callWitAI (string message) {
		Debug.Log("Wit call: " + message);
		// construct the headers needed to authenticate on wit.ai
		Dictionary<string, string> headers = new Dictionary<string,string>();
		headers.Add("Authorization", "Bearer QLLF3GMFKHF3L5ZHYPNO6KJU2D2ZP4EE");

		// send WWW request to URL
		WWW www = new WWW("https://api.wit.ai/message?v=20180521&q="+message, null, headers);
		
		// so it won't stall the main thread
		yield return www;
		
		// parse and call methods returned from wit.ai
		parse_WIT_response(www.text);
	}

    /* parse response received from WIT */
    public void parse_WIT_response(string WIT_response)
    {
        /* convert response to JSON object */
        JObject jObject = JObject.Parse(WIT_response);
	
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
                //Debug.Log("jAction: " + action);

                if (action.Equals("unlock")) {
					applyAction(action, "door");
                    continue;
                }

                /* get all objects in the message */
                if (jParameters != null)
                    foreach (JToken jParameter in jParameters) {
                        string parameter = (string) jParameter[(string)jParameter["type"]];
                        //Debug.Log("jAction: " + action + ",  jobject_to_take: " + parameter);

                        /* try to apply each action on each object */
                        applyAction(action, parameter);
                    }
            }
    }

	/* method called at the end of InputField edit */
    public void parseInput(string input) {
		// send input to WIT to parse: replace space with %20
		StartCoroutine(callWitAI(input.Replace(" ", "%20")));
	}

	/* call an action with a parameter */
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
		case "unlock":
			unlock (parameter);
			break;
		case "go":
			go(parameter);
			break;
		default:
			Debug.LogError ("Action unknown: " + action);
			break;
		}
	}

	public void open(string parameter) {
		/* switch on what to open */
		switch (parameter) {
		case "door":
			GameObject door = getObjectInSight("Door", 5f);
			if (door && !door.GetComponent<Door> ().locked)
				StartCoroutine (door.GetComponent<Door> ().Move ());
			break;
		case "1":
			break;
		case "2":
			break;
		case "3":
			break;
		case "4":
			break;
		default:
			Debug.LogError ("input unknown: Cannot open " + parameter);
			break;
		}
	}

	public void take(string parameter) {
		/* switch on what to open */
		switch (parameter) {
		case "key":
			GameObject key = getObjectInSight("Key", 5f);
			if (key && !inventory.Contains(key)) {
				// move key to messageCanvas
				key.transform.SetParent(messageCanvas.transform);
				key.transform.position = messageCanvas.transform.position;
				key.transform.localScale = new Vector3(1,1,1);
				key.transform.rotation = Quaternion.identity;

				// add key to inventory list
				inventory.Add (key);
				break;
			}
			break;
		case "2":
			break;
		case "3":
			break;
		case "4":
			break;
		default:
			Debug.LogError ("input unknown: Cannot take " + parameter);
			break;
		}
	}


	public void unlock(string parameter) {
		/* switch on what to unlock */
		switch (parameter) {
		case "door":
			GameObject door = getObjectInSight("Door", 5f);
			if (door && door.GetComponent<Door> ().locked)
				useInventoryKey (door);

			break;
		case "1":
			break;
		case "2":
			break;
		case "3":
			break;
		case "4":
			break;
		default:
			Debug.LogError ("input unknown: Cannot unlock " + parameter);
			break;
		}
	}
	public string FirstLetterToUpper(string str)
	{
		if (str == null)
			return null;

		if (str.Length > 1)
			return char.ToUpper(str[0]) + str.Substring(1);

		return str.ToUpper();
	}

	public void go(string parameter) {

		GameObject go_to_object = getObjectInSight(FirstLetterToUpper(parameter), 15f);
			if (go_to_object)
				movePlayerToPoint.moveToObject(go_to_object, 1.2f);

		// /* switch on what to unlock */
		// switch (parameter) {
		// case "door":
		// 	GameObject door = getObjectInSight("Door", 15f);
		// 	if (door)
		// 		movePlayerToPoint.moveToObject(door, 1.2f);
		// 	break;
		// case "table":
		// 	GameObject table = getObjectInSight("Table", 15f);
		// 	if (table)
		// 		movePlayerToPoint.moveToObject(table, 1.2f);
		// 	break;
		// case "2":
		// 	break;
		// case "3":
		// 	break;
		// case "4":
		// 	break;
		// default:
		// 	Debug.LogError ("input unknown: Cannot go to " + parameter);
		// 	break;
		// }
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
}