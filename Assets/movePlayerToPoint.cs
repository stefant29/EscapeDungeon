using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayerToPoint : MonoBehaviour {

	public float speed = 3f;
	public static Vector3 target;
	public static bool moveEnded = true;
	private static float EPSILON = 0.01f;

	void Start ()
	{
		/* set a default target */
		target = new Vector3 (0.0f, transform.position.y, 0.0f);
	}

	void Update ()
	{
		/* if player is at the target position, stop movement */
		if (!moveEnded && equals(target, transform.position, EPSILON)) 
			moveEnded = true;
	
		/* move towards the specified target */
		if (!moveEnded) 
			transform.position = Vector3.MoveTowards (transform.position, target, speed * Time.deltaTime);
	}

	/* start movement to given vector target */
	public static void moveToPoint(Vector3 target_vector, float range = 0.01f) {
		// set RANGE in which to look for
		EPSILON = range;
		// set the position x/z (y is constant - player's y)
		target.x = target_vector.x;
		target.z = target_vector.z;
		/* set moving not ended */
		moveEnded = false;
	}


	/* start movement to given object target */
	public static void moveToObject(GameObject target_object, float range = 0.01f) {
		// use target_object's position to call moveToPoint(Vector3)
		moveToPoint(target_object.transform.position, range);		
	}

	/* check if v1 is "CLOSE" to v2 */
	private bool equals(Vector3 v1, Vector3 v2, float epsiolon) {
		return Mathf.Abs (v1.x - v2.x) < epsiolon && Mathf.Abs (v1.y - v2.y) < epsiolon && Mathf.Abs (v1.z - v2.z) < epsiolon;
	}
}
