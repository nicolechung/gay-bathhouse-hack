﻿using UnityEngine;
using System.Collections;

public class TowelMan : MonoBehaviour {

	public float speed;

	// states
	private bool isChasing;
	private bool isLoving;

	// behaviours
	private GBFlee flee;
	private GBSeek seek;
	private GBWander wander;

	private static int TOWEL_MAN_LAYER = 5;

	// Use this for initialization
	void Start () {
		flee = GetComponent ("GBFlee") as GBFlee;
		seek = GetComponent ("GBSeek") as GBSeek;
		wander = GetComponent ("GBWander") as GBWander;

		flee.enabled = false;
		seek.enabled = false;
		wander.enabled = true;

		gameObject.layer = TOWEL_MAN_LAYER;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Love() {
		// go to shower zone
		Debug.Log ("loving behaviour");
	}

	void Chase() {
		// seek 
		Debug.Log ("seek behaviour");
	}
}
