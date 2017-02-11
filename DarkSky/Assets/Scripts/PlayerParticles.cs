using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticles : MonoBehaviour {

    public ParticleSystem psRipple;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //entering
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Water")
        {
            //move ripple up and down player to the water level
            psRipple.transform.position = new Vector3(psRipple.transform.position.x, other.transform.position.y, psRipple.transform.position.z);

            //turn on
            psRipple.Play();
        }
    }

    //leaving
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
        {
            //turn off
            psRipple.Stop();
        }
    }
}
