using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour {

    int surfaceIndex;

    public float waterLevel = 20.0f;
    public AudioClip dirtGrass;
    public AudioClip stone;
    public AudioClip water;
    public AudioClip sand;

    private Animator animator;
    private Rigidbody rigid;
    private AudioSource footsteps;

	// Use this for initialization
	void Start () {
        //Get Animator Controller
        animator = GetComponent<Animator>();

        //get rigid body
        rigid = GetComponent<Rigidbody>();

        //get audio source
        footsteps = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

        //if player is moving
        if(rigid.velocity.magnitude > 0)
        {
            
            
            //if running or walking
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
            {
                surfaceIndex = TerrainSurface.GetMainTexture(transform.position);

                if (transform.position.y > waterLevel) 
                {
                    if(surfaceIndex == 0) //is grass
                    {
                        footsteps.clip = dirtGrass;
                    }

                    if(surfaceIndex == 1) //is sand
                    {
                        footsteps.clip = sand;
                    }

                    if (surfaceIndex == 2) //dirt
                    {
                        footsteps.clip = dirtGrass;
                    }

                    if (surfaceIndex == 3) //stone
                    {
                        footsteps.clip = stone;
                    }

                    if (surfaceIndex == 4) //stone path
                    {
                        footsteps.clip = stone;
                    }
                }
                else //must be in the water (might want to update later to ensure he's not swimming)
                {
                    footsteps.clip = water;
                }

                //play sound if not playing
                if (!footsteps.isPlaying)
                {
                    footsteps.Play();
                }
            }
            else
            {
                //no longer in motion stop footsteps
                if(footsteps.isPlaying)
                {
                    footsteps.Stop();
                }
            }
        }
        else
        {
            //no longer in motion stop footsteps
            if (footsteps.isPlaying)
            {
                footsteps.Stop();
            }
        }
	}
}
