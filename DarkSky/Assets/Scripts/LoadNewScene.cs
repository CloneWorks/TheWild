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
        //set which scene to load
        PlayerPrefs.SetInt("SceneToLoad", newSceneNumber);

        //Load the loading scene
        SceneManager.LoadScene(1, LoadSceneMode.Single); 
    }
}
