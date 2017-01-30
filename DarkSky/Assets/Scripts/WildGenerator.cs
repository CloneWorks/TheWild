using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildGenerator : MonoBehaviour {

    //public variables
    [Header("Time Access")]
    public WorldClock worldClock;

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

    //private variables
    private List<GameObject> theWild = new List<GameObject>(); //a list of all the objects that exsist currently in the wild

    private List<int> theWildObjectSaves = new List<int>(); //Holds numbers representing each object in the wild
    private List<Vector3> theWildLoactionSaves = new List<Vector3>(); //holds the locations of each object in the wild

    private bool wildReset = false; //ensures a world update only happens once per new day

    private int numberOfObjects;

    //components
    private Terrain terrain;
    private GameObject player;

    private GameManager gm;

	// Use this for initialization
	void Start () {
		//get components
        terrain = Terrain.activeTerrain;

        player = GameObject.FindGameObjectWithTag("Player");

        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        //set variables
        terrainSize = terrain.terrainData.size;
        terrainPosition = terrain.transform.position;

        numberOfObjects = objects.Count;

        //start coroutines
        StartCoroutine(checkWorldUpdate(worldClock.updateTimeInterval));

        //get towns
        GetTowns();

        //check if there is a save
        if(PlayerPrefs.GetInt("WildExsists") == 1)
        {
            Debug.Log("loading wild");

            //load wild
            terrain.terrainData = gm.terrain;

            for (int i = 0; i < gm.wildObjects.Count; i++ )
            {
                GameObject newObj = Instantiate(objects[gm.wildObjects[i]], gm.wildLocations[i], Quaternion.identity);
                theWild.Add(newObj);
            }

        }
        else
        {
            Debug.Log("creating wild");

            //create wild
            StartCoroutine(generateWild());

            //save wild
            gm.saveWild(terrain.terrainData, theWildObjectSaves, theWildLoactionSaves);

            //mark wild as exsisting
            PlayerPrefs.SetInt("WildExsists", 1);
        }
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator generateWild()
    {
        //generate terrain (height map)
        generateTerrain();

        //place towns
        placeTownsOnTerrain();

        //place player
        placePlayerOnTerrain();

        //loop through x of terrain
        for (float x = 0; x < terrainSize.x; x+= xIncrement)
        {
            //loop through z of terrain
            for (float z = 0; z < terrainSize.z; z+= zIncrement)
            {
                //pick random object
                int randObj = Random.Range(0, numberOfObjects);

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

                        //save data
                        theWildObjectSaves.Add(randObj);
                        theWildLoactionSaves.Add(newObj.transform.position);
                    }
                }
                      
            }
        }

        //place objects on terrain
        //placeWildObjectsOnTerrain();

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
        //The lower the numbers in the number range, the higher the hills/mountains will be...
        float divRange = Random.Range(6, 15);

        //The higher the numbers, the more hills/mountains there are
        float tileSize = Random.Range(0, 10);

        //Heights For Our Hills/Mountains
        float[,] hts = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                hts[i, k] = Mathf.PerlinNoise(((float)i / (float)terrain.terrainData.heightmapWidth) * tileSize, ((float)k / (float)terrain.terrainData.heightmapHeight) * tileSize) / divRange;
            }
        }

        terrain.terrainData.SetHeights(0, 0, hts);

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

    void placePlayerOnTerrain()
    {
        player.transform.position = new Vector3(player.transform.position.x, terrain.SampleHeight(new Vector3(player.transform.position.x, 0, player.transform.position.z)), player.transform.position.z);
        flattern(player.transform.position, 1);
    }

    void placeTownsOnTerrain()
    {
        foreach(GameObject g in towns)
        {
            Vector3 newPos = new Vector3(g.transform.position.x, terrain.SampleHeight(new Vector3(g.transform.position.x, 0, g.transform.position.z)) + 6, g.transform.position.z);
            Vector3 flatternPos = new Vector3(g.transform.position.x, terrain.SampleHeight(new Vector3(g.transform.position.x, 0, g.transform.position.z)), g.transform.position.z);
            
            g.transform.position = newPos;
            flattern(flatternPos, 5);
        }
    }

    void placeWildObjectsOnTerrain()
    {
        foreach(GameObject g in theWild)
        {
            g.transform.position = new Vector3(g.transform.position.x, terrain.SampleHeight(new Vector3(g.transform.position.x, 0, g.transform.position.z)) + 6, g.transform.position.z);
        }
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

    //only need if terrain is updated by itself (already called in the genrate wild method)
    IEnumerator updateObjectHeights()
    {
        yield return new WaitForSeconds(5);

        //place objects on terrain
        placeWildObjectsOnTerrain();
    }

    IEnumerator checkWorldUpdate(int updateWait)
    {
        while (true)
        {
            yield return new WaitForSeconds(updateWait/2);

            //check if wild needs resetting
            if (worldClock.IsNewDay() && !wildReset)
            {
                //clear the wild
                clearWild();

                //regenrate the wild
                StartCoroutine(generateWild());

                //mark reset
                wildReset = true;
            }
            
            if(!worldClock.IsNewDay())
            {
                wildReset = false;
            }
        }
    }

    public void flattern(Vector3 pos, int radius)
    {
        TerrainData terrData;

        float[,] heightmapData; // prepare a matrix with terrain points
        float ratio; // ratio between player position and terrain points
        float ratioY; //height ratio

        //update terrain
        terrData = Terrain.activeTerrain.terrainData;
        int terrRes = terrData.heightmapResolution;

        Vector3 terrSize = terrData.size;

        heightmapData = terrData.GetHeights(0, 0, terrRes, terrRes); // heights we will change during of walking

        ratio = (terrSize.x) / terrRes;
        ratioY = (terrSize.y) / terrRes;

        //work out players position (player pos + terrain offset)
        float playPosX = pos.x + (terrData.size.x / 2);
        float playPosZ = pos.z + (terrData.size.z / 2);

        //store players height
        float playPosY = pos.y / terrRes;

        //get terrain point based of player location
        int terrainPointZ = Mathf.CeilToInt(playPosX / ratio) - 1;
        int terrainPointX = Mathf.CeilToInt(playPosZ / ratio) - 1;

        //get terrain height based on player location
        float terrainPointY = (playPosY / ratioY);

        //set height of terrain point under play to players height
        heightmapData[terrainPointX, terrainPointZ] = terrainPointY;

        //get set heights in radius of player
        for (int x = terrainPointX - radius; x < terrainPointX + radius; x++)
        {
            for (int z = terrainPointZ - radius; z < terrainPointZ + radius; z++)
            {
                heightmapData[x, z] = terrainPointY;
            }
        }

        terrData.SetHeights(0, 0, heightmapData); // save terrain heights back
    }
}
