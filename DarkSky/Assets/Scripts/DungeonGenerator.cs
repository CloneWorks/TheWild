using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DungeonGenerator : MonoBehaviour {

	public World world; //links the world generator (grid for making halls)
	public AStar2D aStar;
	
    public GameObject player;

    public Transform dungeonMaxBound;
    public Transform dungeonMinBound;

    public float gridCellSize;

    public List<GameObject> doors = new List<GameObject>();

    [Tooltip("Entrance piece must be first, Exit piece must be second")]
    public List<GameObject> dungeonParts; //a list to hold all hallways and rooms

    public GameObject dungeonEntrance;
    public GameObject dungeonExit;

    public List<GameObject> dungeonRooms;
    public List<GameObject> dungeonHallways;

    public List<Vector3> hallwayLocations;

    public List<int> dungeonDoorways; //a list to hold the number of doors on a dungeon part
    public List<int> dungeonSpacingX; //a list that holds how far appart dungeons need to be to line up on the X
    public List<int> dungeonSpacingZ; //a list that holds how far appart dungeons need to be to line up on the Z

    public int floors; //number of floors you want the dungeon to have
    public int minNumberOfRooms; //minimum number of rooms you wish the dungeon to be on each floor
    public int maxNumberOfRooms; //maximum number of rooms you wish the dungeon to be on each floor

    public int worldArea;
    public int NumberOfAttemptsToCreateRooms;

    public List<GameObject> dungeon = new List<GameObject>(); //holds every part of the dungeon created so far
    private int numberOfDungeonParts; //Holds number of dungeon parts
    private int hallwayRadius; //Holds the radius of hallway pieces

	// Use this for initialization
	void Start () {
        //find player
        player = GameObject.FindGameObjectWithTag("Player");

        //get number of dungeon parts
        numberOfDungeonParts = dungeonParts.Count;

        //get hallway data
        hallwayRadius = (int)dungeonHallways[0].GetComponentInChildren<SphereCollider>().radius;

        //set grid size
        gridCellSize = hallwayRadius * 2;

        //set min and max bound to worldArea
        int actualWorldArea = worldArea * (int)gridCellSize * 2;
        dungeonMaxBound.position = new Vector3(actualWorldArea + gridCellSize/2, gridCellSize, actualWorldArea + gridCellSize/2);
        dungeonMinBound.position = new Vector3(-actualWorldArea - gridCellSize/2, 0, -actualWorldArea - gridCellSize/2);

        //create the dungeon
        //createDungeonMethod0();
        createDungeonMethod1();

        //debugDungeonLocations();
		
		//create world grid for generating hallways
		world.gridCellSize = gridCellSize;

		world.AwakeMe();

        //create the first piece of hallway facing out of the room
        //placeFirstHallwayPieces();

        aStar.StartMe();

        markHallways();

        addDoorwaysAsHallways();

		createHallways();
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

        //vector to hold the new position
        Vector3 roomPos = Vector3.zero;

        //Gameobject to hold new dungeon room
        GameObject newRoom = null;

        //place entrance
        newRoom = createDungeonPiece(dungeonEntrance, newPosition(worldArea));
        dungeon.Add(newRoom);
        addDoors(newRoom);

        //place player at entrance
        player.transform.position = newRoom.transform.position;

        //align players Y rotation with entrance and camera
        player.transform.eulerAngles = new Vector3(player.transform.eulerAngles.x, newRoom.transform.eulerAngles.y, player.transform.eulerAngles.z);
        Camera.main.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, newRoom.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z); //<--------this doesn't seem to work

        //place exit
        newRoom = createDungeonPiece(dungeonExit, newPosition(worldArea));
        dungeon.Add(newRoom);
        addDoors(newRoom);

        //attempt to randomly place rooms in the world
        while (attempts < NumberOfAttemptsToCreateRooms && currentRooms != totalRooms)
        {
            //Choose a random room
            int roomPicked = Random.Range(0, dungeonRooms.Count);

            //randomly pick an X and a Z position in the world area:
            roomPos = newPosition(worldArea);

            //create a room at that point if there are no collisions with it
            float roomRadius = dungeonRooms[roomPicked].GetComponentInChildren<SphereCollider>().radius;

            if (!isCollisionInRadius(roomPos, (int)roomRadius))
            {
                newRoom = Instantiate(dungeonRooms[roomPicked], roomPos, dungeonRooms[roomPicked].transform.rotation);

                //pick a random rotation
                float yRotation = random90DegreeRotation();
                newRoom.transform.eulerAngles = new Vector3(newRoom.transform.eulerAngles.x, yRotation, newRoom.transform.eulerAngles.z);

                //tally room
                currentRooms++;

                //set parent to dungeon
                newRoom.transform.parent = transform;

                //add room to dungeon
                dungeon.Add(newRoom);

                //add rooms doors to door list
                addDoors(newRoom);
            }

            //tally attempt
            attempts++;
        }
    }

    public void placeFirstHallwayPieces()
    {
        Vector3 newPos = Vector3.zero;
        int yRotation = 0;

        foreach(GameObject door in doors){
            
			newPos = getFirstDoorPiecePosition(door);

            yRotation = getFirstDoorPieceRotation(door);

            GameObject newHallway = Instantiate(dungeonHallways[0], newPos, dungeonHallways[0].transform.rotation);
            newHallway.transform.eulerAngles = new Vector3(newHallway.transform.eulerAngles.x, yRotation, newHallway.transform.eulerAngles.z);
			
			world.setArrayPosToFull(newPos); //set node as being full
            world.setArrayPosAsHallway(newPos); //mark node as being a hallway piece
        }
    }

    /// <summary>
    /// Gives a direction of the door from its room parent
    /// </summary>
    /// <param name="door"></param>
    /// <returns></returns>
    public string getDoorDirection(GameObject door)
    {
        Vector3 A = door.transform.position;
        Vector3 B = door.transform.parent.transform.position;

        Vector3 AB = (A - B).normalized;

        Vector3 up = new Vector3(0, 0, 1);
        Vector3 down = new Vector3(0, 0, -1);
        Vector3 left = new Vector3(-1, 0, 0);
        Vector3 right = new Vector3(1, 0, 0);

        if(AB == up)
        {
            //Debug.Log("Door is facing up!");
            return "up";
        }

        if(AB == down){
            //Debug.Log("Door is facing down!");
            return "down";
        }

        if(AB == left){
            //Debug.Log("Door is facing left!");
            return "left";
        }

        if (AB == right)
        {
            //Debug.Log("Door is facing right!");
            return "right";
        }

        return null;
    }
	
    public int getFirstDoorPieceRotation(GameObject door){
        int yRotation = 0;

        string direction = getDoorDirection(door);

        if (direction == "up")
        {
            yRotation = 0;
        }
        else if (direction == "down")
        {
            yRotation = 0;
        }
        else if (direction == "left")
        {
            yRotation = 90;
        }
        else if (direction == "right")
        {
            yRotation = 90;
        }

        return yRotation;
    }

	public Vector3 getFirstDoorPiecePosition(GameObject door){
		
		Vector3 newPos = Vector3.zero;
		
		string direction = getDoorDirection(door);

		if(direction == "up")
		{
			 newPos = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z + hallwayRadius);
		}
		else if(direction == "down")
		{
			newPos = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z - hallwayRadius);
		}
		else if(direction == "left")
		{
			newPos = new Vector3(door.transform.position.x - hallwayRadius, door.transform.position.y, door.transform.position.z);
		}
		else if(direction == "right")
		{
			newPos = new Vector3(door.transform.position.x + hallwayRadius, door.transform.position.y, door.transform.position.z);
		}
		
		return newPos;
	}

    public void addDoorwaysAsHallways()
    {
        foreach(GameObject door in doors)
        {
            //get the position which needs to be marked as part of the hallway
            Vector3 pos = getDoorAsHallwayPosition(door);

            //mark as hallway
            world.setArrayPosAsHallway(pos);
        }
    }

    public Vector3 getDoorAsHallwayPosition(GameObject door)
    {
        Vector3 newPos = Vector3.zero;

        string direction = getDoorDirection(door);

        if (direction == "up")
        {
            newPos = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z - hallwayRadius);
        }
        else if (direction == "down")
        {
            newPos = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z + hallwayRadius);
        }
        else if (direction == "left")
        {
            newPos = new Vector3(door.transform.position.x + hallwayRadius, door.transform.position.y, door.transform.position.z);
        }
        else if (direction == "right")
        {
            newPos = new Vector3(door.transform.position.x - hallwayRadius, door.transform.position.y, door.transform.position.z);
        }

        return newPos;
    }

    public Vector3 getFirstHallwayPiecePosition(GameObject door)
    {

        Vector3 newPos = Vector3.zero;

        string direction = getDoorDirection(door);

        if (direction == "up")
        {
            newPos = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z + hallwayRadius + hallwayRadius*2);
        }
        else if (direction == "down")
        {
            newPos = new Vector3(door.transform.position.x, door.transform.position.y, door.transform.position.z - hallwayRadius - hallwayRadius * 2);
        }
        else if (direction == "left")
        {
            newPos = new Vector3(door.transform.position.x - hallwayRadius - hallwayRadius * 2, door.transform.position.y, door.transform.position.z);
        }
        else if (direction == "right")
        {
            newPos = new Vector3(door.transform.position.x + hallwayRadius + hallwayRadius * 2, door.transform.position.y, door.transform.position.z);
        }

        return newPos;
    }

    /// <summary>
    /// Forces the piece to be created. Good for entrance and exit as they must exsist, also good for other rooms that
    /// need to exsist as part of level design. (might want to include the radius of the object it collided with rather than its own!!)
    /// </summary>
    /// <param name="dungeonPiece"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject createDungeonPiece(GameObject dungeonPiece, Vector3 position)
    {
        
        int pieceRadius = (int)dungeonPiece.GetComponentInChildren<SphereCollider>().radius;
        int newX;
        int newZ;

        //Debug.Log("Position of " + dungeonPiece.name + "is: " + position);

        //while there is a collision in radius of dungeonpiece move it
        while (isCollisionInRadius(position, pieceRadius)) //while (collisionObjDistance != int.MaxValue) //
        {
            //calculate a new X position
            if(Random.Range(0,2) == 0)
            {
                newX = (int)position.x + (int)(hallwayRadius * 2) + (pieceRadius*2); //+ (int)collisionObjDistance;
            }
            else
            {
                newX = (int)position.x - (int)(hallwayRadius * 2) - (pieceRadius * 2); //- (int)collisionObjDistance;
            }

            //calculate a new Z position
            if (Random.Range(0, 2) == 0)
            {
                newZ = (int)position.z + (int)(hallwayRadius * 2) + (pieceRadius * 2); //+ (int)collisionObjDistance;
            }
            else
            {
                newZ = (int)position.z - (int)(hallwayRadius * 2) - (pieceRadius * 2); //- (int)collisionObjDistance;
            }

            //update position to the newly calulated one
            position = new Vector3(newX, 0, newZ);

            //Debug.Log(dungeonPiece.name + " moved here: " + position);
        }

        //piece must be in a good location, so place it:
        GameObject pieceToCreate = Instantiate(dungeonPiece, position, dungeonPiece.transform.rotation);
        pieceToCreate.transform.parent = transform;

        //pick a random rotation
        int yRotation = random90DegreeRotation();
        pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);

        return pieceToCreate;
    }

    public void addDoors(GameObject room)
    {
        foreach(Transform child in room.GetComponentsInChildren<Transform>())
        {
            if(child.tag == "Doorway")
            {
                doors.Add(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Returns a random rotation out of (0, 90, 180, and 270)
    /// </summary>
    /// <returns></returns>
    public int random90DegreeRotation()
    {
        return (int)(Random.Range(0, 4) * 90);
    }

    /// <summary>
    /// Takes a range such as the world size and returns a random point either side of 0 for that area
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Vector3 newPosition(int range)
    {
        int randX = positionFitsHallway(Random.Range(-range, range + 1));
        int randZ = positionFitsHallway(Random.Range(-range, range + 1));
        return new Vector3(randX, 0, randZ);
    }

    /// <summary>
    /// Ensures that a new dungeon object isn't being placed inside another dungeon object and is leaving enough gap to be connected by a hallway
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public bool isCollisionInRadius(Vector3 center, int radius)
    {
        int layerMask = 1 << 8; //only look for collisions on dungeon layer

        Collider[] hitColliders = Physics.OverlapSphere(center, radius + (hallwayRadius*2) * 2, layerMask);

        if(hitColliders.Length != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the closest collided dungeon object
    /// </summary>
    /// <returns></returns>
    public int DistanceToClosestObjectsInCollisionRadius(Vector3 center, int radius)
    {
        int layerMask = 1 << 8; //only look for collisions on dungeon layer

        Collider[] hitColliders = Physics.OverlapSphere(center, radius + (hallwayRadius * 2), layerMask);

        int closest = int.MaxValue;

        foreach(Collider c in hitColliders)
        {
            int distanceBetweenObjectAndMe = Mathf.RoundToInt(Vector3.Distance(c.transform.position, center));

            if(distanceBetweenObjectAndMe < closest)
            {
                closest = distanceBetweenObjectAndMe;
            }
        }

        return closest;
    }

    /// <summary>
    /// Ensures that the randomly chosen position fits on a grid of hallway sizes
    /// </summary>
    /// <param name="myPos"></param>
    /// <returns></returns>
    public int positionFitsHallway(int myPos)
    {
        int hallwaySize = (int)(hallwayRadius * 2);
        if ((myPos % hallwaySize) == 0)
        {
            return myPos;
        }
        else
        {
            return myPos * hallwaySize;
        }
    }

    public void debugDungeonLocations()
    {
        bool notOnGrid = false;

        foreach(GameObject g in dungeon)
        {
            //if x or z position doesn't divide into hallway size the piece isn't on the grid
            if (g.transform.position.x % (hallwayRadius * 2) != 0 || g.transform.position.z % (hallwayRadius * 2) != 0)
            {
                notOnGrid = true;
            }
        }

        if(notOnGrid)
        {
            Debug.Log("A dungeon piece isn't on a grid loaction.");
        }
        else
        {
            Debug.Log("All dungeon pieces are on the grid.");
        }
    }

	
	public List<node> path; ///< Holds a list of nodes in the generated path
	
    public void markHallways()
    {
       // AStar2D aStar = new AStar2D();
		
       List<GameObject> tempDoors = new List<GameObject>();
	   
	   foreach (GameObject Obj in doors){
		   tempDoors.Add(Obj);
	   }

        while (tempDoors.Count > 0)
        {
			
			path = new List<node>();

			//quicker reset look to function for more details
			aStar.quickLocalArrayReset();
		
			GameObject doorA = tempDoors[Random.Range(0, tempDoors.Count)];
			tempDoors.Remove(doorA);
			
			
			
			GameObject doorB = null;
			if (tempDoors.Count < 1){ //quite messy might change so the while runs while tempdoor count is greater than 2 then have a seperate if to handle the last doors	
				
				//method 1 - 	more chance of causing none linking sections of the dungeon
				//				however, has less chance of creating competing paths
				//doorB = getClosestDoor(doorA, doors);
				
				
				
				//method 2 - 	less chance of causing none linking sections of the dungeon
				//				however, has more chance of creating competing paths
				do{
					doorB = doors[Random.Range(0, doors.Count)];
				} while ((doorB.transform.parent != doorA.transform.parent) && (doorA != doorB));

			} else {
				//Debug.Log("YO" + tempDoors.Count);
				doorB = getClosestDoor(doorA, tempDoors);
				if(doorB == null){
					//Debug.Log("running");
					do{
						doorB = doors[Random.Range(0, doors.Count)];
					} while ((doorB.transform.parent != doorA.transform.parent) && (doorA != doorB));
				}
			}
			//tempDoors.Remove(doorB);

            Vector3 doorAPos = getFirstDoorPiecePosition(doorA); //if we want all the first pieces facing out of the room use: getFirstHallwayPiecePosition(doorA);
			int Ax = (int)Mathf.Round(doorAPos.x);
			int Az = (int)Mathf.Round(doorAPos.z);

            Vector3 doorBPos = getFirstDoorPiecePosition(doorB); //if we want all the first pieces facing out of the room use: getFirstHallwayPiecePosition(doorB);
			int Bx = (int)Mathf.Round(doorBPos.x);
			int Bz = (int)Mathf.Round(doorBPos.z);
		
            Vector3 startPos = new Vector3(Ax, 0, Az);
            startPos = world.WorldToArrayPosition(startPos);
            startPos.y = 0;
            node start = aStar.localWorldArray[(int)startPos.x, (int)startPos.y, (int)startPos.z];

            Vector3 goalPos = new Vector3(Bx, 1, Bz);
            goalPos = world.WorldToArrayPosition(goalPos);
            goalPos.y = 0;
            node goal = aStar.localWorldArray[(int)goalPos.x, (int)goalPos.y, (int)goalPos.z];

            path = aStar.findPath(start, goal);

			if(path != null){
				foreach (node n in path)
				{
					if (n != null)
					{
						Vector3 position = world.ArrayToWorldPosition(n.nodePos, false);
						position.y = 0;

                        //mark as piece of hallway
                        world.setArrayPosAsHallway(world.ArrayToWorldPosition(n.nodePos, false)); //sets a node as being part of a hallway

						//piece must be in a good location, so store it:
                        hallwayLocations.Add(position);
					}
				}
			}
			//Destroy (doorA);
			//Destroy (doorB);
        }
    }

    public void createHallways()
    {
        //hold the new game object
        GameObject pieceToCreate;

        //basic bytes       (4 variations)
        byte up = 8;
        byte down = 4;
        byte left = 2;
        byte right = 1;

        //two directions    (6 variations)
        byte upDown = 12;
        byte upLeft = 10;
        byte upRight = 9;
        byte downLeft = 6;
        byte downRight = 5;
        byte leftRight = 3;


        //three directions  (4 variations)
        byte upLeftRight = 11;
        byte downLeftRight = 7;
        byte upRightDown = 13;
        byte upLeftDown = 14;

        //four directions   (1 variation)
        byte fourWay = 15;

        //none              (1 variation)
        byte none = 0;

        //holds the byte value of the sections around
        byte hallwaysAroundMe = 0;

        foreach (Vector3 position in hallwayLocations)
        {
            //set hallway surroundings bit integer:
            //check if hallway above
            if (world.checkHallwayPiece(new Vector3(position.x, position.y, position.z + gridCellSize)))
            {
                //set fist bit = 1000 = above
                hallwaysAroundMe += up;
            }
            
            //check if hallway below
            if (world.checkHallwayPiece(new Vector3(position.x, position.y, position.z - gridCellSize)))
            {
                //set second bit = 0100 = below
                hallwaysAroundMe += down;
            }

            
            //check if hallway left
            if (world.checkHallwayPiece(new Vector3(position.x - gridCellSize, position.y, position.z)))
            {
                //set third bit = 0010 = left
                hallwaysAroundMe += left;
            }
            
            //check if hallway right
            if (world.checkHallwayPiece(new Vector3(position.x + gridCellSize, position.y, position.z)))
            {
                //set fourth bit = 0001 = right
                hallwaysAroundMe += right;
            }

            //check what's around and choose a piece and rotation based on condition
            //one direction
            if (hallwaysAroundMe == up) 
            {
                //make straight
                pieceToCreate = Instantiate(dungeonHallways[0], position, dungeonHallways[0].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 0;
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if (hallwaysAroundMe == down) 
            {
                //make straight
                pieceToCreate = Instantiate(dungeonHallways[0], position, dungeonHallways[0].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 0;
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if (hallwaysAroundMe == left) 
            {
                //make straight
                pieceToCreate = Instantiate(dungeonHallways[0], position, dungeonHallways[0].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 90;
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if (hallwaysAroundMe == right) 
            {
                //make straight
                pieceToCreate = Instantiate(dungeonHallways[0], position, dungeonHallways[0].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 90;
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            //twoDirections
            if(hallwaysAroundMe == upDown)
            {
                //make straight
                pieceToCreate = Instantiate(dungeonHallways[0], position, dungeonHallways[0].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 0;
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == upLeft)
            {
                //make corner
                pieceToCreate = Instantiate(dungeonHallways[1], position, dungeonHallways[1].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 0; //was 270
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == upRight)
            {
                //make corner
                pieceToCreate = Instantiate(dungeonHallways[1], position, dungeonHallways[1].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 90; //was 0
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == downLeft)
            {
                //make corner
                pieceToCreate = Instantiate(dungeonHallways[1], position, dungeonHallways[1].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 270; //was 180
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == downRight)
            {
                //make corner
                pieceToCreate = Instantiate(dungeonHallways[1], position, dungeonHallways[1].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 180; //was 90
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == leftRight)
            {
                //make straight
                pieceToCreate = Instantiate(dungeonHallways[0], position, dungeonHallways[0].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 90;
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            //three directions
            if(hallwaysAroundMe == upLeftRight)
            {
                //make T-Section
                pieceToCreate = Instantiate(dungeonHallways[2], position, dungeonHallways[2].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 90; //was 0
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == downLeftRight)
            {
                //make T-Section
                pieceToCreate = Instantiate(dungeonHallways[2], position, dungeonHallways[2].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 270; //was 180
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == upRightDown)
            {
                //make T-Section
                pieceToCreate = Instantiate(dungeonHallways[2], position, dungeonHallways[2].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 180; //was 90
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            if(hallwaysAroundMe == upLeftDown)
            {
                //make T-Section
                pieceToCreate = Instantiate(dungeonHallways[2], position, dungeonHallways[2].transform.rotation);
                pieceToCreate.transform.parent = transform;

                //pick a rotation
                int yRotation = 0; //was 270
                pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
            }

            //four directions
            if (hallwaysAroundMe == fourWay)
            {
                //make floor
                pieceToCreate = Instantiate(dungeonHallways[3], position, dungeonHallways[3].transform.rotation);
                pieceToCreate.transform.parent = transform;
            }

            //nothing around
            if (hallwaysAroundMe == none)
            {
                //make floor
                pieceToCreate = Instantiate(dungeonHallways[3], position, dungeonHallways[3].transform.rotation);
                pieceToCreate.transform.parent = transform;
            }

            //reset hallways around me
            hallwaysAroundMe = 0;


            
        }
        
    }

	//gets the closest door thats not on the same room to the door you pass in
	public GameObject getClosestDoor(GameObject doorA, List<GameObject> doors){
		float dist = float.MaxValue;
		GameObject doorB = null;
		
		foreach(GameObject tempDoorB in doors){
			if(tempDoorB != null){
				float tempDist = Vector3.Distance(doorA.transform.position, tempDoorB.transform.position);
				if(tempDist < dist){
					if(tempDoorB.transform.parent != doorA.transform.parent){
						dist = tempDist;
						doorB = tempDoorB;
					}
				}
			}
		}
		
		return doorB;
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
