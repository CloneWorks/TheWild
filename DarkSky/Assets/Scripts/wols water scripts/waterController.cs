using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterController : MonoBehaviour {

	public float height;
	public float seconds;
	private float startHeight;
	
	// Use this for initialization
	void Start () {
		startHeight = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
	   float speed = (height/2)/seconds;
	   transform.position = new Vector3(transform.position.x, startHeight+(height*Mathf.Sin(speed*Time.time)), transform.position.z);
	}
}
