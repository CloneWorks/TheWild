using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticles : MonoBehaviour {

    public ParticleSystem psRipple;

	private GameManager gm;

	// Use this for initialization
	void Start () 
	{
		// Get access to game manager
		gm = FindObjectOfType<GameManager>();	
	}
	
	// Update is called once per frame
	void Update () {
		// If we leave the water turn particles off
		if (gm.Player != null && gm.Player.transform.position.y > gm.waterLevel)
		{
			//turn off
			psRipple.Clear ();
			psRipple.Stop();
		}
	}

    //entering
    void OnTriggerEnter(Collider other)
    {
		if (other.tag == "Water") {
			//move ripple up and down player to the water level
			psRipple.transform.position = new Vector3 (gm.Player.transform.position.x, gm.waterLevel , gm.Player.transform.position.z); //other.transform.position.y

			//turn on
			psRipple.Play ();
		}
    }

	void OnTriggerStay()
	{

	}

    //leaving
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
        {
            //turn off
			psRipple.Clear ();
            psRipple.Stop();
        }
    }
}
