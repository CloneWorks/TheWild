using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewScene : MonoBehaviour {

    public int newSceneNumber;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        //when leaving the wild save the player location
        if(newSceneNumber != 0)
        {
            //save players wild position
            GameManager gm = FindObjectOfType<GameManager>();

			gm.playerPos = gm.Player.transform.position - gm.Player.transform.forward * 2f; 

			//Old position stuff which was a little buggy:
			//other.transform.position - (other.transform.forward * 2f); //new Vector3(other.transform.position.x + 2f, other.transform.position.y, other.transform.position.z);
            //gm.playerPos.y += 0.05f;
        }
        
        //set which scene to load
        PlayerPrefs.SetInt("SceneToLoad", newSceneNumber);

        //Load the loading scene
        SceneManager.LoadScene(1, LoadSceneMode.Single); 
    }
}
