using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSounds : MonoBehaviour {

    int surfaceIndex;

    public float waterLevel = 20.0f;
    public AudioClip grass;
    public AudioClip dirt;
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
        if(rigid.velocity.magnitude > 0.1)
        {
            
            
            //if running or walking
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
            {
                if(Terrain.activeTerrain != null)
                {
                    surfaceIndex = TerrainSurface.GetMainTexture(transform.position);
                }
                else
                {
                    surfaceIndex = 3; //make stone the default sound
                }

                //while in the wild and above water
                if (transform.position.y > waterLevel) 
                {
                    if(surfaceIndex == 0) //is grass
                    {
                        footsteps.clip = grass;
                    }

                    if(surfaceIndex == 1) //is sand
                    {
                        footsteps.clip = sand;
                    }

                    if (surfaceIndex == 2) //is dirt
                    {
                        footsteps.clip = dirt;
                    }

                    if (surfaceIndex == 3) //is stone
                    {
                        footsteps.clip = stone;
                    }

                    if (surfaceIndex == 4) //is stone path
                    {
                        footsteps.clip = stone;
                    }
                }
                else //must be in the water (might want to update later to ensure he's not swimming)
                {
                    //but if in dungeon you're not
                    if (SceneManager.GetActiveScene().buildIndex == 3)
                    {
                        footsteps.clip = stone;
                    }
                    else
                    {
                        footsteps.clip = water;
                    }
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
