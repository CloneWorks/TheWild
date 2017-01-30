using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

    public TerrainData terrain; 
    public List<int> wildObjects;
    public List<Vector3> wildLocations;

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

        Debug.Log(PlayerPrefs.GetInt("WildExsists"));
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("WildExsists", 0);
    }

    public void saveWild(TerrainData t, List<int> o, List<Vector3> v)
    {
        terrain = t;
        wildObjects = o;
        wildLocations = v;
    }
}
