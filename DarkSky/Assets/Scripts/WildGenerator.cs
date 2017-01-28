using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildGenerator : MonoBehaviour {

    //public variables
    [Header("Object Genration and Positioning")]
    public List<GameObject> objects; //a list of all plants/trees/bushes/stones which will be scattered around the wild

    [Range(0, 100)]
    [Tooltip("Out of 100%")]
    public List<int> objectDensity; //a list containing the density for each object

    public List<float> scaleMin; //The minimum range in which an objects scale can change
    public List<float> scaleMax; //The maximum range in which an objects scale can change

    [Range(0, 360)]
    public float rotation = 360; //The range in which an objects rotation can change

    public float xIncrement = 1; //how frequently it tries to place an object on the x axis

    public float zIncrement = 1; //how frequently it tries to place an object on the z axis

    public float xJitter = 0.5f; //how much an object can move around it's chosen x position

    public float zJitter = 0.5f; //how much an object can move around it's chosen z position

    public float spawnRadiusOfPlayer = 5f; //how far an object must be to be created near the player

    [Header("Town Data")]
    public float spawnRadiusOfTowns = 5f; //how far an object must be to be created near a town
    public List<GameObject> towns = new List<GameObject>();

    [Header("Terrain Data")]
    public Vector3 terrainSize; //holds the size of the terrain

    public Vector3 terrainPosition; //holds the position of the terrain

    [Header("Day/Night Cycle")]
    [Tooltip("Number of seconds in 1 day = 86400")]
    public int dayLength = 60; //holds the length of a day

    [Tooltip("Number of seconds in 1 day = 86400")]
    public int nightLength = 60; //holds the length of a day

    public int CurrentTime; //The current time of the day

    public int updateTimeInterval = 1; //how frequently to update the time

    public int sunrise;
    public int midday;
    public int sunset;
    public int midnight;
    public int fullDay;

    public int wildResetTime = 0;

    public Material daySkybox;
    public Material nightSkybox;

    //private variables
    private List<GameObject> theWild = new List<GameObject>(); //a list of all the objects that exsist currently in the wild

    private int numberOfObjects;

    //components
    private Terrain terrain;
    private GameObject player;

	// Use this for initialization
	void Start () {
		//get components
        terrain = Terrain.activeTerrain;

        player = GameObject.FindGameObjectWithTag("Player");

        //set variables
        terrainSize = terrain.terrainData.size;
        terrainPosition = terrain.transform.position;
        numberOfObjects = objects.Count;

        sunrise = 0;                            //beginning of the day
        midday = dayLength / 2;                 //middle of the day
        sunset = dayLength;                     //end of the day
        midnight = dayLength + nightLength / 2; //middle of the night
        fullDay = dayLength + nightLength;    //The total time of a day
        CurrentTime = midday;                   //set time to mid-day

        //start coroutines
        StartCoroutine(updateTime(updateTimeInterval));

        //get towns
        GetTowns();

        //create wild
        StartCoroutine(generateWild());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator generateWild()
    {
        //loop through x of terrain
        for (float x = 0; x < terrainSize.x; x+= xIncrement)
        {
            //loop through z of terrain
            for (float z = 0; z < terrainSize.z; z+= zIncrement)
            {
                //pick random object
                int randObj = Random.Range(0, numberOfObjects);

                //generate terrain (height map)
                generateTerrain();

                //calculate chance of being created
                float RandChance = Random.Range(0, 100);

                //If random chance value is less than the pecentage spawn desity create the object that was chosen
                if(RandChance <= objectDensity[randObj])
                {
                     //calculater jitter values
                    float xJit = Random.Range(-xJitter, xJitter);
                    float zJit = Random.Range(-xJitter, xJitter);
                    
                    //set object position
                    Vector3 objPos = new Vector3(x + terrainPosition.x + xJit, terrain.SampleHeight(new Vector3(x + terrainPosition.x + xJit, 0, z + terrainPosition.z + zJit)), z + terrainPosition.z + zJit);

                    //check if object is too close to the player or a town
                    if(Vector3.Distance(player.transform.position, objPos) >= spawnRadiusOfPlayer && !NearATown(objPos))
                    {
                        //create object
                        GameObject newObj = Instantiate(objects[randObj], objPos, Quaternion.identity);

                        //randomly scale it
                        float randScale = Random.Range(scaleMin[randObj], scaleMax[randObj]);
                        newObj.transform.localScale = new Vector3(randScale, randScale, randScale);

                        //randomly rotate it
                        float yRotation = Random.Range(0, rotation);

                        newObj.transform.eulerAngles = new Vector3(newObj.transform.eulerAngles.x, yRotation, newObj.transform.eulerAngles.z);

                        //newObj.transform.Rotate(Vector3.up, yRotation); //may want to change to newObj.transform.up

                        //add to objects in the wild list
                        theWild.Add(newObj);
                    }
                }
                      
            }
        }
        yield return null;
    }

    void clearWild()
    {
        //destroy every object
        foreach(GameObject g in theWild)
        {
            Destroy(g);
        }

        //clear list
        theWild.Clear();
    }

    void generateTerrain()
    {
        //This example shows how to flatten your entire terrain to a height of 20...

        //TerrainData TD = terrain.terrainData;
        //float[,] HeightMap = new float[TD.heightmapWidth, TD.heightmapHeight];
        //for (int x = 0; x < TD.heightmapWidth; x++)
        //{
        //    for (int y = 0; y < TD.heightmapHeight; y++)
        //    {
        //        HeightMap[x, y] = 20;
        //    }
        //}

        //TD.SetHeights(0, 0, HeightMap);
    }

    void GetTowns()
    {
        GameObject[] TownsInScene = GameObject.FindGameObjectsWithTag("Town");

        //loop through adding all towns to town list
        foreach(GameObject g in TownsInScene)
        {
            towns.Add(g);
        }
    }

    bool NearATown(Vector3 objectBeingCreated)
    {
        float shortestDistanceToATown = float.MaxValue;

        foreach(GameObject g in towns)
        {
            float dist = Vector3.Distance(objectBeingCreated, g.transform.position);

            if(dist < shortestDistanceToATown)
            {
                shortestDistanceToATown = dist;
            }
        }

        if(shortestDistanceToATown <= spawnRadiusOfTowns)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    IEnumerator updateTime(int updateWait)
    {
        while (true)
        {
            //update time
            CurrentTime += updateWait;

            //full day is over
            if(CurrentTime == fullDay)
            {
                //reset the time
                CurrentTime = 0;
            }

            //starting a new day
            if(CurrentTime == wildResetTime)
            {
                //clear the wild
                clearWild();

                //regenrate the wild
                StartCoroutine(generateWild());
            }

            //is day time
            if (CurrentTime >= sunrise && CurrentTime <= sunset && RenderSettings.skybox != daySkybox)
            {
                RenderSettings.skybox = daySkybox;
                DynamicGI.UpdateEnvironment();
            }
            //is night time
            else if (CurrentTime < fullDay && CurrentTime > sunset && RenderSettings.skybox != nightSkybox)
            {
                RenderSettings.skybox = nightSkybox;
                DynamicGI.UpdateEnvironment();
            }

            yield return new WaitForSeconds(updateWait);
        }
    }
}
