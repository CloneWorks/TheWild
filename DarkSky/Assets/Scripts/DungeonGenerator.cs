using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {

    public GameObject player;

    [Tooltip("Entrance piece must be first, Exit piece must be second")]
    public List<GameObject> dungeonParts; //a list to hold all hallways and rooms

    public GameObject dungeonEntrance;
    public GameObject dungeonExit;

    public List<GameObject> dungeonRooms;
    public List<GameObject> dungeonHallways;

    public List<int> dungeonDoorways; //a list to hold the number of doors on a dungeon part
    public List<int> dungeonSpacingX; //a list that holds how far appart dungeons need to be to line up on the X
    public List<int> dungeonSpacingZ; //a list that holds how far appart dungeons need to be to line up on the Z

    public int floors; //number of floors you want the dungeon to have
    public int minNumberOfRooms; //minimum number of rooms you wish the dungeon to be on each floor
    public int maxNumberOfRooms; //maximum number of rooms you wish the dungeon to be on each floor

    public int worldArea;
    public int NumberOfAttemptsToCreateRooms;

    private List<GameObject> dungeon = new List<GameObject>(); //holds every part of the dungeon created so far
    private int numberOfDungeonParts; //Holds number of dungeon parts

	// Use this for initialization
	void Start () {
        //find player
        player = GameObject.FindGameObjectWithTag("Player");

        //get number of dungeon parts
        numberOfDungeonParts = dungeonParts.Count;

        //create the dungeon
        //createDungeonMethod0();
        createDungeonMethod1();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void createDungeonMethod1(){
        //set attempts
        int attempts = 0;

        //get a number of rooms
        int totalRooms = Random.Range(minNumberOfRooms, maxNumberOfRooms + 1);
        int currentRooms = 0;

        //values for holding random x and z
        int randX;
        int randZ;

        //vector to hold the new position
        Vector3 roomPos = Vector3.zero;

        //Gameobject to hold new dungeon room
        GameObject newRoom;

        //place entrance
        randX = Random.Range(-worldArea, worldArea);
        randZ = Random.Range(-worldArea, worldArea);
        roomPos = new Vector3(randX, 0, randZ);
        newRoom = Instantiate(dungeonEntrance, roomPos, dungeonEntrance.transform.rotation);
        newRoom.transform.parent = transform;
        dungeon.Add(newRoom);

        //place player at entrance
        player.transform.position = newRoom.transform.position;

        //place exit
        randX = Random.Range(-worldArea, worldArea);
        randZ = Random.Range(-worldArea, worldArea);
        roomPos = new Vector3(randX, 0, randZ);
        newRoom = Instantiate(dungeonExit, roomPos, dungeonExit.transform.rotation);
        newRoom.transform.parent = transform;
        dungeon.Add(newRoom);

        //attempt to randomly place rooms in the world
        while (attempts < NumberOfAttemptsToCreateRooms && currentRooms != totalRooms)
        {
            //Choose a random room
            int roomPicked = Random.Range(0, dungeonRooms.Count);

            //randomly pick and X and a Z position in the world area:
            randX = Random.Range(-worldArea, worldArea);
            randZ = Random.Range(-worldArea, worldArea);
            roomPos = new Vector3(randX, 0, randZ);

            //create a room at that point
            //Debug.Log(dungeonRooms[roomPicked].GetComponent<SphereCollider>().radius);
            if (!isCollisionInRadius(roomPos, (dungeonRooms[roomPicked].GetComponent<SphereCollider>().radius)))
            {
                newRoom = Instantiate(dungeonRooms[roomPicked], roomPos, dungeonRooms[roomPicked].transform.rotation);
            }
            
            //tally room
            currentRooms++;

            //tally attempt
            attempts++;

            //set parent to dungeon
            newRoom.transform.parent = transform;

            //add room to dungeon
            dungeon.Add(newRoom);
        }
    }

    public bool isCollisionInRadius(Vector3 center, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);

        if(hitColliders.Length != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void createDungeonMethod0(){
        //create a 3D array of ints related to dungeon parts 
        for (int i = 0; i < floors; i++) //loop through number of floors
        {
            //choose a number of rooms
            int totalRooms = Random.Range(minNumberOfRooms, maxNumberOfRooms + 1);

            int offsetX = 0;
            int offsetZ = 0;

            int previousPiece = 0;

            //loop through creating rooms
            for (int currentRooms = 0; currentRooms < totalRooms; currentRooms++)
            {
                //pick a room
                int piecePicked;

                //if it is the first room
                if (currentRooms == 0)
                {
                    //select entrance
                    piecePicked = 0;
                }
                //if it is the last room
                else if (currentRooms == totalRooms - 1)
                {
                    //select exit
                    piecePicked = 1;
                }
                //is a normal room
                else
                {
                    //get a list of rooms that have more doors than the remaining number of rooms
                    int remainingRooms = (totalRooms - currentRooms);

                    List<int> availableRooms = new List<int>();

                    int roomAvailable = 0;

                    foreach(int numberOfdoors in dungeonDoorways)
                    {
                        //if the room being checked has less doors than the number of rooms we still need to make
                        if(numberOfdoors < remainingRooms)
                        {
                            //add it as being available (needs to be less as one door will be used to connect this room with the next)
                            availableRooms.Add(roomAvailable);

                            //move to next room
                            roomAvailable++;
                        }
                    }

                    //select a random room which isn't the entrance or exit 
                    //piecePicked = Random.Range(2, numberOfDungeonParts);
                    piecePicked = availableRooms[Random.Range(0, availableRooms.Count)];
                }

                //get room position
                //add offsets only after the first piece is placed as first piece doesn't require its own offset
                Vector3 roomPos = Vector3.zero;

                if(currentRooms == 0)
                {
                    roomPos = new Vector3(offsetX, 0, offsetZ);
                }
                else
                {
                    int XorZ = Random.Range(0, 2);

                    //add pieces x or z to offsets
                    if (XorZ == 0)
                    {
                        offsetX += dungeonSpacingX[piecePicked] + dungeonSpacingX[previousPiece];
                    }
                    else
                    {
                        offsetZ += dungeonSpacingZ[piecePicked] + dungeonSpacingZ[previousPiece];
                    }

                    roomPos = new Vector3(offsetX, 0, offsetZ);
                }
                

                //create the room
                GameObject newRoom = Instantiate(dungeonParts[piecePicked], roomPos, dungeonParts[piecePicked].transform.rotation);

                //set parent to dungeon
                newRoom.transform.parent = transform;

                //rotate room till entrances meet

                //set previous piece
                previousPiece = piecePicked;

                //add room to dungeon
                dungeon.Add(newRoom);
            }
        }
    }
}
