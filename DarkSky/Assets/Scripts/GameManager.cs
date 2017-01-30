using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

    public WorldClock time;

    public TerrainData terrain; 
    public List<int> wildObjects;       //store objects index number
    public List<Vector3> wildLocations; //store objects location

    public List<float> wildScales;        //store objects scale
    public List<float> wildRotations; //store objects Quaternion.Euler rotation

    public bool wildReset = false; //ensures a world update only happens once per new day

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
        if (time.IsNewDay() && !wildReset)
        {
            wildReset = true;
        }

    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("WildExsists", 0);
    }

    public void saveWild(TerrainData t, List<int> o, List<Vector3> v, List<float> s, List<float> r)
    {
        //store all data
        terrain = t;
        wildObjects = o;
        wildLocations = v;
        wildScales = s;
        wildRotations = r;

        //mark wild as exsisting
        PlayerPrefs.SetInt("WildExsists", 1);
    }

    public void clearSaves()
    {
        terrain = null;
        wildObjects.Clear();
        wildLocations.Clear();
        wildScales.Clear();
        wildRotations.Clear();
    }
}
