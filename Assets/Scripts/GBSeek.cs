﻿using UnityEngine;
using System.Collections;

public class GBSeek : MonoBehaviour {

	private bool flag; // make it only call once

	public float collisionDistance = 2;
	public float speed = 1;
	private float direction;
	public GameObject Target;
	private string state;
	private bool hasHitTarget;
	private static bool DEBUG = true;
	private List<Vector2> directions;
	private float rotationRange = 5;
	public int LayerToMask = 8;
	private Vector2 seekDirection;


	// Use this for initialization
	void Start () {
		flag = true;

		// start out seeking if the Target is within range
		state = "seek";
		hasHitTarget = false;
		directions = new List<Vector2> ();
		StartCoroutine (Seek ());
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (enabled && flag) {
			flag = false;
		}

		if (enabled)
		{
			switch (state) {
				
			case "start-move":
				state = "avoid";
				StartCoroutine (AvoidMove ());
				
				break;
				
				
			case "start-rotate":
				state = "rotate";
				StartCoroutine (rotateAround ());
				break;

			case "seek":
				rigidbody2D.AddForce(seekDirection * speed);
				break;
			default: 
				
				break;
			}
		}
	}

	void OnCollisionEnter2D (Collision2D col)
	{
		
		if (col.gameObject.name == "Player") {
			hasHitTarget = true;
		}
	}
	
	IEnumerator Seek ()
	{
		// check if there is something in the way of the target
		while (!hasHitTarget && state=="seek") {
			bool seek;
			rotateToTarget ();
			seek = seekCheck ();
			if (seek) {   
				transform.Translate (Vector2.up * speed * Time.smoothDeltaTime);
			} else {
				state = "start-rotate";
			}
			yield return null;
		}
	}
	
	IEnumerator AvoidMove ()
	{
		bool obstacle;
		bool seek;
		directions.Clear ();
		while (state=="avoid") {
			
			transform.Translate (Vector2.up * speed * Time.smoothDeltaTime);
			
			// check if there is something in the way of the target
			seek = seekCheck (); // checks for something in front of the Target game object
			obstacle = obstacleCheck (); // checks for an obstacle directly in front
			
			// inside here, if there is an obstacle, change state to "rotate"
			if (obstacle) {
				state = "start-rotate";
				yield return null;
			}
			
			// if it's okay to see and there is no obstacle
			if (seek && !obstacle) {
				state = "seek";
				StartCoroutine (Seek ());
			} 
			
			yield return null;
		}
		
	}
	
	RaycastHit2D hasObstacles (Vector2 direction, string colorString)
	{
		RaycastHit2D[] hits;
		RaycastHit2D[] hitsLeft;
		RaycastHit2D[] hitsRight;
		
		
		Vector2 directionLeft;
		
		Vector2 directionRight;
		// move the origin of the Raycast so that it's outside of the collider
		
		Color color;
		// move the origin of the Raycast so that it's outside of the collider
		
		switch (colorString) {
		case "blue":
			color = Color.cyan;
			break;
			
		case "red":
			color = Color.red;
			break;
			
		case "yellow":
			color = Color.yellow;
			break;
			
		default:
			color = Color.black;
			break;
		}
		
		
		if (DEBUG) {
			Debug.DrawRay (transform.position, direction * collisionDistance, color);
		}
		
		
		hits = Physics2D.RaycastAll (transform.position, direction, collisionDistance, 1 << LayerToMask);
		
		Vector2 left = new Vector2 (-0.3F, 0);
		Vector2 leftOrigin = new Vector2 (transform.position.x, transform.position.y) + left;
		directionLeft = direction + left;
		hitsLeft = Physics2D.RaycastAll (leftOrigin, directionLeft, collisionDistance, 1 << LayerToMask);
		if (DEBUG) {
			Debug.DrawRay (leftOrigin, directionLeft * collisionDistance, color);
		}
		
		
		Vector2 right = new Vector2 (0.3F, 0);
		Vector2 rightOrigin = new Vector2 (transform.position.x, transform.position.y) + right;
		directionRight = direction + right;
		hitsRight = Physics2D.RaycastAll (rightOrigin, directionRight, collisionDistance, 1 << LayerToMask);
		if (DEBUG) {
			Debug.DrawRay (rightOrigin, directionRight * collisionDistance, color);
		}
		
		// is there a collision?
		foreach (RaycastHit2D hit in hits) {
			if (hit && hit.collider) {
				return hit;
			}
		}
		
		foreach (RaycastHit2D hit in hitsLeft) {
			if (hit && hit.collider) {
				return hit;
			}
		}
		
		foreach (RaycastHit2D hit in hitsRight) {
			if (hit && hit.collider) {
				return hit;
			}
		}
		
		RaycastHit2D falsey = new RaycastHit2D();
		return falsey;
		
	}
	
	bool seekCheck ()
	{
		
		Vector2 direction = (Target.transform.position - transform.position).normalized;
		
		bool hasObstacle = hasObstacles (direction, "blue");
		
		return !hasObstacle;
		
	}
	
	bool obstacleCheck ()
	{
		Vector2 direction = transform.up; /* works for a top-down game, for a platformer try transform.forward */
		bool hasObstacle = hasObstacles (direction, "yellow");
		
		return hasObstacle;
	}
	
	void rotateToTarget ()
	{
		float zRotation = Mathf.Atan2 ((Target.transform.position.y - transform.position.y), (Target.transform.position.x - transform.position.x)) * Mathf.Rad2Deg - 90;
		transform.eulerAngles = new Vector3 (0, 0, zRotation);
	}
	
	// need optional last direction
	IEnumerator rotateAround ()
	{
		RaycastHit2D obstacle;
		while (state=="rotate") {
			if (DEBUG) Debug.Log ("--state: rotate--");
			float randomAngle = Random.Range (-rotationRange, rotationRange);
			Vector3 directionChange = new Vector3 (randomAngle, 0, 0);
			Vector3 direction;
			Vector3 originalDirection;  
			
			originalDirection = Target.transform.position;
			direction = originalDirection + directionChange;
			obstacle = hasObstacles (direction, "yellow");
			if (DEBUG) {
				Debug.Log ("---obstacle?:---");
				Debug.Log (!obstacle && !obstacle.collider);
			}
			
			if (!obstacle || !obstacle.collider) {
				if (DEBUG) {
					Debug.Log ("---no obstacle, moving:---");
				}        
				Vector3 directionPoint = transform.position + direction.normalized; 
				Vector3 vectorToTarget =  directionPoint - transform.position;      
				float zRotation = Mathf.Atan2 (vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90;
				transform.eulerAngles = new Vector3 (0, 0, zRotation);
				// if there is no obstacle, then actually rotate in that direction
				state = "start-move";
				yield return null;
			} else {
				// usually when the seeker is right against the object, bump it out
				// transform.Translate (obstacle.normal * speed * Time.smoothDeltaTime); 
				seekDirection = obstacle.normal;
				state = "seek";
				yield return null;
			} 
		}
	}


	// set up Event Listeners
	void onEnable() 
	{
		
	}
	
	// remove Event Listeners
	void onDisable() 
	{
		
	}
	
	// remove Event Listeners
	void onDestroy() 
	{
		
	}
}
