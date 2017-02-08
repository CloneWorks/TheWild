using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroySunAndMoon : MonoBehaviour {

    private static DontDestroySunAndMoon instance = null;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
        }
        //If instance already exists and it's not this:
        else if (instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        //when in a dungeon
        if(SceneManager.GetActiveScene().buildIndex == 3)
        {
            GetComponentInChildren<Light>().enabled = false;

            foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
        }
        else
        {
            GetComponentInChildren<Light>().enabled = true;

            foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        }
    }
}
