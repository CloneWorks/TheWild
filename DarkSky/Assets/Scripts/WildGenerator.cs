using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // used for Sum of array

public class WildGenerator : MonoBehaviour {

    //public variables
    [Header("Time Access")]
    public WorldClock worldClock; //Holds a copy of the world clock

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

    public float waterLevel = 1.0f; //how hight the water level sits

    public float sandLevel; //the height of the sand, usually a little above the water

    public GameObject water; //holds the water

    public float raiseTerrain = 0.1f; //a fixed amount to add to terrain height

    [Header("Town Data")]
    public float spawnRadiusOfTowns = 5f; //how far an object must be to be created near a town
    public List<GameObject> towns = new List<GameObject>();

    [Header("Terrain Data")]
    public Terrain islandMask; //holds data which will make generation into an island

    public Vector3 terrainSize; //holds the size of the terrain

    public Vector3 terrainPosition; //holds the position of the terrain

    //private variables
    public List<GameObject> theWild = new List<GameObject>(); //a list of all the objects that exsist currently in the wild

    //saved data about wild objects
    private List<int> theWildObjectSaves = new List<int>(); //Holds numbers representing each object in the wild
    private List<Vector3> theWildLoactionSaves = new List<Vector3>(); //holds the locations of each object in the wild
    private List<float> theWildScaleSaves = new List<float>(); //holds the scale of each object in the wild
    private List<float> theWildRotationSaves = new List<float>(); //holds the y rotation of each object in the wild

    private int numberOfObjects;    //hold number of different objects to be placed throughout the wild

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

        worldClock = GameObject.FindGameObjectWithTag("Clock").GetComponent<WorldClock>();

        //set variables
        terrainSize = terrain.terrainData.size;
        terrainPosition = terrain.transform.position;

        numberOfObjects = objects.Count;

        //start coroutines
        StartCoroutine(checkWorldUpdate(worldClock.updateTimeInterval)); // <---------------------------this is triggering when it shouldn't be

        //get towns
        GetTowns();

        //check if world needs update
        if (worldResetCheck())
        {
            //regenrate the wild
            generateWild();

            //mark reset
            gm.wildReset = false;
        }
        else
        {
            //Load or generate world
            createWorld();
        }
	}

    void OnApplicationQuit()
    {
        //flattern terrain when quiting
        terrainHeight(0);
    }

	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        //rotateWildObjectsOnTerrain();
    }

    void createWorld()
    {
        //check if there is a save
        if (PlayerPrefs.GetInt("WildExsists") == 1)
        {
            //load save
            loadWild();
        }
        //no save so create one
        else if (PlayerPrefs.GetInt("WildExsists") != 1)
        {
            //create wild
            generateWild();
        }
    }

    public void loadWild()
    {
        Debug.Log("loading wild");

        //load wild (this may not be needed as terrain data will save on the stored heightmap)
        //terrain.terrainData.SetHeights(0, 0, gm.terrain.GetHeights(0, 0, gm.terrain.heightmapWidth, gm.terrain.heightmapHeight));  //terrain.terrainData.GetHeights();

        //place towns
        placeTownsOnTerrain();

        //place player
        placePlayerOnTerrain();

        //place water
        water.transform.position = new Vector3(water.transform.position.x, waterLevel, water.transform.position.z);

        //texture terrain
        textureTerrain();

        //re-populate objects
        for (int i = 0; i < gm.wildObjects.Count; i++)
        {
            //create object
            GameObject newObj = Instantiate(objects[gm.wildObjects[i]], gm.wildLocations[i], Quaternion.identity);

            //scale it
            newObj.transform.localScale = new Vector3(gm.wildScales[i], gm.wildScales[i], gm.wildScales[i]);

            //rotate it
            newObj.transform.eulerAngles = new Vector3(newObj.transform.eulerAngles.x, gm.wildRotations[i], newObj.transform.eulerAngles.z);

            theWild.Add(newObj);

            newObj.transform.parent = transform;
        }

        //rotate each object
        rotateWildObjectsOnTerrain();
    }

    public void generateWild()
    {
        Debug.Log("creating wild");

        //clear all lists
        clearWild();

        //generate terrain (height map)
        generateTerrain();

        //place towns
        placeTownsOnTerrain();

        //place player
        placePlayerOnTerrain();

        //place water
        water.transform.position = new Vector3(water.transform.position.x, waterLevel, water.transform.position.z);

        //texture terrain
        textureTerrain();

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
                    if(Vector3.Distance(player.transform.position, objPos) >= spawnRadiusOfPlayer && !NearATown(objPos) && objPos.y > sandLevel)
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

                        newObj.transform.parent = transform;

                        //save data
                        theWildObjectSaves.Add(randObj);
                        theWildLoactionSaves.Add(newObj.transform.position);
                        theWildScaleSaves.Add(randScale);
                        theWildRotationSaves.Add(yRotation);

                    }
                }         
            }  
        }

        //rotate each object
        rotateWildObjectsOnTerrain();

        //ensure lists are clear for new data
        //clearWild();

        //save wild data into game manager
        gm.saveWild(terrain.terrainData, theWildObjectSaves, theWildLoactionSaves, theWildScaleSaves, theWildRotationSaves);

    }

    //clears all the objects in the wild and wipes saved data
    void clearWild()
    {
        //destroy every object
        foreach(GameObject g in theWild)
        {
            Destroy(g);
        }

        //clear list
        theWild.Clear();

        //clear local saves
        theWildObjectSaves.Clear();
        theWildLoactionSaves.Clear();

        //clear game manager saves
        gm.clearSaves();
    }

    //creates a random terrain using perlin noise
    void generateTerrain()
    {
        //The lower the numbers in the number range, the higher the hills/mountains will be...
        float divRange = Random.Range(6, 15);

        //The higher the numbers, the more hills/mountains there are
        float tileSize = Random.Range(0, 10);

        //grab island shape data
        float[,] islandData = islandMask.terrainData.GetHeights(0,0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);

        //Heights For Our Hills/Mountains
        float[,] hts = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                hts[i, k] = Mathf.PerlinNoise(((float)i / (float)terrain.terrainData.heightmapWidth) * tileSize, ((float)k / (float)terrain.terrainData.heightmapHeight) * tileSize) / divRange;

                //make island shape by multiplying by island mask terrain data
                hts[i, k] = (hts[i, k] + raiseTerrain) * islandData[i, k];
            }
        }

        terrain.terrainData.SetHeights(0, 0, hts);
    }

    void textureTerrain()
    {
        TerrainData terrainData = terrain.terrainData;

        float[, ,] originalSplatData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
         
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y/(float)terrainData.alphamapHeight;
                float x_01 = (float)x/(float)terrainData.alphamapWidth;
                 
                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );

                //get a more detailed height using a ray cast -----------------------------------------------------------------------------------------------------------> My RAY
                Vector3 start = new Vector3(y + terrain.transform.position.x - terrainData.alphamapLayers, 1000, x + terrain.transform.position.z - terrainData.alphamapLayers);

                RaycastHit rcHit = new RaycastHit();
                LayerMask mask = 1 << 10;

                Ray ray = new Ray(start, Vector3.down);

                float height2 = 0;

                if (Physics.Raycast(ray, out rcHit, 2000, mask))
                {
                    height2 = rcHit.point.y;
                }
                //-------------------------------------------------------------------------------------------------------------------------------------------------------> End of My Ray

                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01,x_01);
      
                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01,x_01);
                 
                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];
                 
                // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT
     
                // Texture[0] has constant influence
                splatWeights[0] = 1f;

                //if(height > sandLevel)
                //{
                //    splatWeights[0] = 1f;
                //}

                // Texture[1] is stronger at lower altitudes
                splatWeights[1] = 0; //Mathf.Clamp01((terrainData.heightmapHeight - height));

                //if (y < terrainData.alphamapHeight / 5 || x < terrainData.alphamapWidth/5)
                //{
                //    splatWeights[1] = 1.0f;
                //}

                //if(steepness < 5)
                //{
                //    splatWeights[1] = 0.8f;
                //}

                //if (height <= sandLevel)
                //{
                //    splatWeights[1] = 1 + 1.0f * Mathf.Clamp01(steepness);
                //}

                //if (height <= sandLevel + 2)
               // {
                //    splatWeights[1] = 1.0f * ((sandLevel + 2) - height);
                //}

                if(height2 < sandLevel)
                {
                    splatWeights[1] = 1.0f;
                }

                // Texture[2] stronger on flatter terrain
                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                // Subtract result from 1.0 to give greater weighting to flat surfaces
                splatWeights[2] = 0;

                //splatWeights[2] = 1.0f - Mathf.Clamp01(steepness*steepness/(terrainData.heightmapHeight/5.0f));
                 
                // Texture[3] increases with height but only on surfaces facing positive Z axis 
                splatWeights[3] = 0;//Mathf.Clamp01(steepness) * 10; //height * (Mathf.Clamp01(normal.z) + Mathf.Clamp01(normal.x) + Mathf.Clamp01(normal.y));  //height * Mathf.Clamp01(normal.z);
                
                //if(steepness > 24 && height > sandLevel)
                //{
                //    splatWeights[3] = 1.0f * (steepness) - 24;
                //}

                if (steepness > 24 && height2 >= sandLevel)
                {
                    splatWeights[3] = 1.0f * (steepness) - 24;
                }

                //load stones from original terrain
                splatWeights[4] = originalSplatData[x, y, 4] * 10.0f;

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();
                 
                // Loop through each terrain texture
                for(int i = 0; i<terrainData.alphamapLayers; i++)
                {
                     
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;
                     
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
      
        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    float convertWorldHeightToHeightmapHeight(float y)
    {
        //conver height into terrain height
        TerrainData terrData = Terrain.activeTerrain.terrainData;
        int terrRes = terrData.heightmapResolution;
        Vector3 terrSize = terrData.size;
        float ratioY = (terrSize.y) / terrRes;

        //store height in scale with terrain resolution
        float PosY = y / terrRes;

        //get terrain height based on location scaling in the y ratio
        float terrainPointY = (PosY / ratioY);

        return terrainPointY;
    }

    //raises player to the height of the terrain at their position
    void placePlayerOnTerrain()
    {
        float terrainHeight = terrain.SampleHeight(new Vector3(player.transform.position.x, 0, player.transform.position.z));

        if(player.transform.position.y < terrainHeight){
            player.transform.position = new Vector3(player.transform.position.x, terrainHeight , player.transform.position.z);
            flattern(player.transform.position, 1);
        }
        
    }

    //raises all towns to the terrain height at their position
    void placeTownsOnTerrain()
    {
        foreach(GameObject g in towns)
        {
            Vector3 newPos = new Vector3(g.transform.position.x, terrain.SampleHeight(new Vector3(g.transform.position.x, 0, g.transform.position.z)), g.transform.position.z);
            Vector3 flatternPos = new Vector3(g.transform.position.x, terrain.SampleHeight(new Vector3(g.transform.position.x, 0, g.transform.position.z)), g.transform.position.z);
            
            g.transform.position = newPos;
            flattern(flatternPos, 5);
        }
    }

    //raises all wild objects to the terrain height at their position
    void placeWildObjectsOnTerrain()
    {
        foreach(GameObject g in theWild)
        {
            g.transform.position = new Vector3(g.transform.position.x, terrain.SampleHeight(new Vector3(g.transform.position.x, 0, g.transform.position.z)), g.transform.position.z);
        }
    }

    void rotateWildObjectsOnTerrain()
    {
        Debug.Log("Here");
        //make platform adjust terrain rotation
        RaycastHit rcHit;

        //Make raycast direction down
        Ray ray;

        //set a mask which will ensure the ray only collides with terrain
        LayerMask mask = 1 << 10;

        //int tallyObjectsChecked = 0;

        //int tallyRaysHit = 0;

        foreach(GameObject g in theWild)
        {
            //Debug.Log("Here!");
            int offsetHeight = 100;

            Vector3 start = new Vector3(g.transform.position.x, g.transform.position.y + offsetHeight, g.transform.position.z);

            ray = new Ray(start, Vector3.down);

            if (Physics.Raycast(ray, out rcHit, 1000, mask))
            {
                //Debug.Log("HIT!");
                //this is for getting distance from object to the ground
                float GroundDis = rcHit.distance;
                //with this you rotate object to adjust with terrain
                g.transform.rotation = Quaternion.FromToRotation(g.transform.up, rcHit.normal);

                //finally, this is for putting object IN the ground
                g.transform.position = new Vector3(rcHit.point.x, rcHit.point.y, rcHit.point.z);

                //tallyRaysHit++;
            }

            //tallyObjectsChecked++;
        }

        //Debug.Log(tallyObjectsChecked);
        //Debug.Log(tallyRaysHit);
    }

    //collects all towns in the scene
    void GetTowns()
    {
        GameObject[] TownsInScene = GameObject.FindGameObjectsWithTag("Town");

        //loop through adding all towns to town list
        foreach(GameObject g in TownsInScene)
        {
            towns.Add(g);
        }
    }

    //checks if an objects position is near a town
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

    //continuously checks if the world needs updating
    IEnumerator checkWorldUpdate(int updateWait)
    {
        while (true)
        {
            yield return new WaitForSeconds(updateWait);

            //Debug.Log("Time being checked is: " + worldClock.CurrentTime);
            if (worldResetCheck())
            {
                //regenrate the wild
                generateWild();

                //mark reset
                gm.wildReset = false;
            }
            else
            {
                gm.wildReset = false;
            }
        }
    }

    public bool worldResetCheck()
    {
        //check if wild needs resetting
        if (gm.wildReset)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //flatterns an area of terrain around a position
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

    public void terrainHeight(float height)
    {
        float[,] hts = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                hts[i, k] = height;
            }
        }

        terrain.terrainData.SetHeights(0, 0, hts);
    }

    //never worked and isn't used
    public void oldTerrainCode()
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
}
