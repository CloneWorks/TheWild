using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildGenerator : MonoBehaviour {

    //public variables
    public List<GameObject> objects; //a list of all plants/trees/bushes/stones which will be scattered around the wild

    [Range(0.1f, 1)]
    public List<float> objectDensity; //a list containing the density for each object

    [Range(0.1f, 2)]
    public float scale = 1; //The range in which an objects scale can change

    [Range(0, 360)]
    public float rotation = 360; //The range in which an objects rotation can change

    public float xIncrement = 1; //how frequently it tries to place an object on the x axis

    public float zIncrement = 1; //how frequently it tries to place an object on the z axis

    public Vector3 terrainSize; //holds the size of the terrain

    public Vector3 terrainPosition; //holds the position of the terrain

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

    public int timeReset;

    //private variables
    List<GameObject> theWild = new List<GameObject>(); //a list of all the objects that exsist currently in the wild

    //components
    Terrain terrain;

	// Use this for initialization
	void Start () {
		//get components
        terrain = Terrain.activeTerrain;

        //set variables
        terrainSize = terrain.terrainData.size;
        terrainPosition = terrain.transform.position;

        sunrise = 0;                            //beginning of the day
        midday = dayLength / 2;                 //middle of the day
        sunset = dayLength;                     //end of the day
        midnight = dayLength + nightLength / 2; //middle of the night
        timeReset = dayLength + nightLength;    //The total time of a day
        CurrentTime = midday;                   //set time to mid-day

        //start coroutines
        StartCoroutine(updateTime(updateTimeInterval));

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
                //select object

                //chance of being created
                    //create object
                    GameObject newObj = Instantiate(objects[0], new Vector3(x + terrainPosition.x, terrain.SampleHeight(new Vector3(x + terrainPosition.x, 0, z + terrainPosition.z)), z + terrainPosition.z), Quaternion.identity);

                    //randomly scale it
                    float randScale = Random.Range(0.1f, scale);
                    newObj.transform.localScale = new Vector3(randScale, randScale, randScale);

                    //randomly rotate it
                    float yRotation = Random.Range(0, rotation);

                    newObj.transform.eulerAngles = new Vector3(newObj.transform.eulerAngles.x, yRotation, newObj.transform.eulerAngles.z);

                    //newObj.transform.Rotate(Vector3.up, yRotation); //may want to change to newObj.transform.up

                    //add to objects in the wild list
                    theWild.Add(newObj);
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

    IEnumerator updateTime(int updateWait)
    {
        while (true)
        {
            //update time
            CurrentTime += updateWait;

            //starting a new day
            if(CurrentTime == timeReset)
            {
                //reset the time
                CurrentTime = 0;

                //clear the wild
                clearWild();

                //regenrate the wild
                StartCoroutine(generateWild());
            }

            yield return new WaitForSeconds(updateWait);
        }
    }
}
