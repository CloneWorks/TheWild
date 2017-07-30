using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

	public GameObject Player;
	public Scene CurrentScene;

	public WildGenerator TheWild;
	public float waterLevel = 15.0f;

    public WorldClock time;
	public GameObject WorldTime;

    public TerrainData terrain; 
    public List<int> wildObjects;       //store objects index number
    public List<Vector3> wildLocations; //store objects location

    public List<float> wildScales;        //store objects scale
    public List<float> wildRotations; //store objects Quaternion.Euler rotation

    public bool wildReset = false; //ensures a world update only happens once per new day

    public Vector3 playerPos = Vector3.zero; //store players spawn position
	public Vector3 playerRot = Vector3.zero; //store players spawn rotation

	public GameObject OurTimeSystem;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;

			// Must be first time creating this, we need our world clock, sun and moon!
			OurTimeSystem = (GameObject)Instantiate(WorldTime);
			time = OurTimeSystem.GetComponentInChildren<WorldClock>();
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

	void OnLevelWasLoaded()
	{
		// Load/Find player object for this scene
		Player = GameObject.Find("Player");

		// Get info about the current scene.
		CurrentScene = SceneManager.GetActiveScene ();

		if (CurrentScene.buildIndex == 0)
		{
			// Get the wild
			TheWild = FindObjectOfType<WildGenerator>();
		}

		// Retrieve the name of this scene.
		string sceneName = CurrentScene.name;
		Debug.Log ("Scene Name: " + sceneName);

		// Change lighting for dungeon
		if (sceneName == "Dungeon" || sceneName == "loadingScene") {
			// Hide the sun and moon
			foreach (Renderer rend in GameObject.Find ("SunAndMoon").gameObject.GetComponentsInChildren<Renderer> ()) 
			{
				rend.enabled = false;
			}
			GameObject.Find ("SunLight").GetComponent<Light> ().enabled = false;
			GameObject.Find ("Reflection Probe").GetComponent<ReflectionProbe> ().enabled = false;
			GameObject.Find ("Light Probe Group").GetComponent<LightProbeGroup> ().enabled = false;
		} 
		else 
		{
			// Make sure it's visible
			foreach (Renderer rend in GameObject.Find ("SunAndMoon").gameObject.GetComponentsInChildren<Renderer> ()) 
			{
				rend.enabled = true;
			}
			GameObject.Find ("SunLight").GetComponent<Light> ().enabled = true;
			GameObject.Find ("Reflection Probe").GetComponent<ReflectionProbe> ().enabled = true;
			GameObject.Find ("Light Probe Group").GetComponent<LightProbeGroup> ().enabled = true;
		}
	}

    void Update()
    {
		// Load objects which might not have been set yet
		if (Player == null) 
		{
			Player = GameObject.Find ("Player");
		}

		if (CurrentScene == null)
		{
			CurrentScene = SceneManager.GetActiveScene ();	
		}
			
		// Check for the new day
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
