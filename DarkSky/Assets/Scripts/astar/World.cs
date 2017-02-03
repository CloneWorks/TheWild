using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles and constructs 3D world array of nodes used for reef navigation.
/// </summary>
public class World : MonoBehaviour {

	//defines world space
    public Transform dungeonMinBound;  ///< Position of the minimum bound of the reef scene
    public Transform dungeonMaxBound;  ///< Position of the maximum bound of the reef scene
	public Vector3 worldMin; ///< Position of the minimum world bound 
    public Vector3 worldMax; ///< Position of the maximum world bound 
	public Vector3 offSetDungeonMax;///< Off sets the max position to be where the array end as it only goes as far as it evenly divides into the wolrd space

	//defines the size each chunk of the world will be
	public float gridCellSize; ///< Size of each node that makes up the world
	public float gizmoSizeOffset = 0.1f; ///< Spacing between each node
	public float edgeOffSet = 0.01f; ///< Offset from the edge of the world

    public Vector3 cellAmount; ///<stores the number of cells in each axis direction (how deep each axis (x,y,z) is)
	
	public node[,,] worldArray; ///< Array of all the nodes in the world

    List<Vector3> neighborPos = new List<Vector3>(); ///< Store the positions of the current nodes neighbors

	/*
	void OnGUI()
	{
		Vector2 targetPos;
		targetPos = Camera.main.WorldToScreenPoint (transform.position);
		float offset = 200;
		for (int i = 0; i < cellAmount.x; i++) { //i is cycling through x
			for (int j = 0; j < cellAmount.y; j++) { //j is cycling through y
				GUI.Box (new Rect (targetPos.x - 30 + (60 * i) + offset , Screen.height - targetPos.y - 100 - (30 * j) - offset/1.5f, 60, 30), "g" + worldArray [i, j, 0].g);

				GUI.Box (new Rect (targetPos.x - 30 + (60 * i) + offset, Screen.height - targetPos.y - 100 - (30 * j) + (cellAmount.y * 30) + 30 - offset/1.5f, 60, 30) , "f" + worldArray [i, j, 0].f);

				GUI.Box (new Rect (targetPos.x - 30 + (60 * i) - (cellAmount.x * 30) - 300 + offset, Screen.height - targetPos.y - 100 - (30 * j) - offset/1.5f , 60, 30) , "h" + worldArray [i, j, 0].h);
			}
		}
	}
	*/

	

    /// <summary>
    /// Creates the world before anything else happens
    /// </summary>
	// void Awake(){ //happens before anything else
		// worldMin = dungeonMinBound.position;
		// worldMax = dungeonMaxBound.position;
		// createWorldArray ();
		// calculateDungeonMaxOffset ();
		// populateCollisions();
	// }
	
	 /// <summary>
    /// Creates the world before anything else happens
    /// </summary>
	public void AwakeMe(){ //happens before anything else
		worldMin = dungeonMinBound.position;
		worldMax = dungeonMaxBound.position;
		createWorldArray ();
		calculateDungeonMaxOffset ();
		populateCollisions();
	}

	// Use this for initialization
    /// <summary>
    /// On start constructs the list of neighbor directions
    /// </summary>
	void Start () {
		neighborPos.Add (new Vector3(-1,1,0));
		neighborPos.Add (new Vector3(0,1,0));
		neighborPos.Add (new Vector3(1,1,0));
		neighborPos.Add (new Vector3(-1,0,0));
		neighborPos.Add (new Vector3(1,0,0));
		neighborPos.Add (new Vector3(-1,-1,0));
		neighborPos.Add (new Vector3(0,-1,0));
		neighborPos.Add (new Vector3(1,-1,0));

		neighborPos.Add (new Vector3(-1,1,-1));
		neighborPos.Add (new Vector3(0,1,-1));
		neighborPos.Add (new Vector3(1,1,-1));
		neighborPos.Add (new Vector3(-1,0,-1));
		neighborPos.Add (new Vector3(1,0,-1));
		neighborPos.Add (new Vector3(-1,-1,-1));
		neighborPos.Add (new Vector3(0,-1,-1));
		neighborPos.Add (new Vector3(1,-1,-1));

		neighborPos.Add (new Vector3(-1,1,1));
		neighborPos.Add (new Vector3(0,1,1));
		neighborPos.Add (new Vector3(1,1,1));
		neighborPos.Add (new Vector3(-1,0,1));
		neighborPos.Add (new Vector3(1,0,1));
		neighborPos.Add (new Vector3(-1,-1,1));
		neighborPos.Add (new Vector3(0,-1,1));
		neighborPos.Add (new Vector3(1,-1,1));

		neighborPos.Add (new Vector3(0,0,-1));
		neighborPos.Add (new Vector3(0,0,1));
	}

    /// <summary>
    /// Draws gizmos in the scene view for each node in the world array
    /// </summary>
	
	/*void OnDrawGizmos() { //Selected
		Color blue = new Color(0, 0, 1, 0.1F);
		Color red = new Color(1, 0, 0, 0.5F);
		Color green = new Color (0,1,0,0.5f);
		Color yellow = new Color (1,1,0,1);
		Color purple = new Color (0.67f,0,1,1);//rgb(67%, 0%, 100%)	

		float chunkSizeHalf = gridCellSize / 2;
		//float gizmoSizeOffset = 0.1f;
		Vector3 gizmoSize = new Vector3 (gridCellSize - gizmoSizeOffset,gridCellSize - gizmoSizeOffset,gridCellSize - gizmoSizeOffset);
		
		for(int i = 0; i < cellAmount.x ; i++){ //i is cycling through x
			for(int j = 0; j < cellAmount.y; j++){ //j is cycling through y
				for (int k = 0; k < cellAmount.z; k++) { //k is cycling through z
					//calculate gizmo centre
					//Debug.Log("walkable val = " + worldArray[i,j].walkable);
					Vector3 centre = new Vector3 ((i * gridCellSize) + chunkSizeHalf + worldMin.x, (j * gridCellSize) + chunkSizeHalf + worldMin.y, (k * gridCellSize) + chunkSizeHalf + worldMin.z);
					if (worldArray [i, j, k].walkable == true ) { //&& !worldArray [i, j, k].ispath && !worldArray [i, j, k].hasFood
						Gizmos.color = blue;
						Gizmos.DrawCube (centre, gizmoSize);
					} else if (worldArray [i, j, k].walkable == false) { // && !worldArray [i, j, k].ispath && !worldArray [i, j, k].hasFood
						Gizmos.color = red;
						Gizmos.DrawCube (centre, gizmoSize);
					}
				}
			}
		}
	}*/



	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Converts a position in the game world into an position in the world array.
    /// </summary>
    /// <param name="world">A position in the game world</param>
    /// <returns>returns a possition that exsists in the world array</returns>
	public Vector3 WorldToArrayPosition(Vector3 world){ //returns the position in the worldArray in which the position you gave falls within
		//divide each axis by gridCellSize and drop the decimal points by casting to an int
		Vector3 arrayPos = new Vector3 ((int)(((world.x - worldMin.x)/gridCellSize)),
										(int)(((world.y - worldMin.y)/gridCellSize)),
										(int)(((world.z - worldMin.z)/gridCellSize)));
		//Debug.Log ("Checking this pos: " + arrayPos.x + "\t" + arrayPos.y + "\t" + arrayPos.z);
		return arrayPos;
	}

    /// <summary>
    /// Converts an array position into a game world position.
    /// </summary>
    /// <param name="arrayPos">The array position to convert</param>
    /// <param name="randomOffset">Use a random offset or not</param>
    /// <returns>Returns a world position</returns>
	public Vector3 ArrayToWorldPosition(Vector3 arrayPos, bool randomOffset){

		//times array positions by gridCellSize, this makes a world position at the bottom, front, left corner in world space that the array p[osition represented. 
		Vector3 worldPos = new Vector3( (arrayPos.x*gridCellSize) + worldMin.x,
		                                (arrayPos.y*gridCellSize) + worldMin.y,
										(arrayPos.z*gridCellSize) + worldMin.z);

		//we need to add half gridCellSize to positions to allign with the centre of world space that the array pos represents. 
		//or a random position within the world space that the array pos represented
		if(randomOffset){
			float randomX = Random.Range (worldPos.x,worldPos.x + gridCellSize);
			float randomY = Random.Range (worldPos.y,worldPos.y + gridCellSize);
			float randomZ = Random.Range (worldPos.z,worldPos.z + gridCellSize);
			worldPos = new Vector3 (randomX, randomY, randomZ);
		} else {
			float chunkSizeHalf = gridCellSize/2;
			worldPos = new Vector3( worldPos.x + chunkSizeHalf, 
			                  		worldPos.y + chunkSizeHalf,
									worldPos.z + chunkSizeHalf);
		}

		//returns the world position for the array position given
		return worldPos;
	}

    /// <summary>
    /// checks if a posision in worldArray is free 
    /// </summary>
    /// <param name="worldPos">World position to check</param>
    /// <returns>returns true if world position is free</returns>
	public bool checkArrayPosFree(Vector3 worldPos){ //checks if a posision in worldArray is free 
		
		bool freePos = false;

		//Debug.Log ("pos: " + pos.x + "\t" + pos.y + "\t" + pos.z + "\t minus this pos: " + worldMin.x + "\t" + worldMin.y + "\t" + worldMin.z);
		Vector3 arrayPos = WorldToArrayPosition (worldPos);
		//Debug.Log ("now checking this pos: " + arrayPos.x + " | " + arrayPos.y + " | " + arrayPos.z);
		if(worldArray[(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z].walkable == true){
			freePos = true;
		}
		
		return freePos;
	}

    /// <summary>
    /// Checks if a posision in worldArray is free 
    /// </summary>
    /// <param name="index">World array index position</param>
    /// <returns>returns true if index position is free</returns>
	public bool checkArrayPosFreeByIndex(Vector3 index){ //checks if a posision in worldArray is free 

		bool freePos = false;
		if ((index.x >= cellAmount.x) || (index.y >= cellAmount.y) || (index.z >= cellAmount.z) || (index.x < 0) || (index.y < 0) || (index.z < 0)) {
			return freePos; // node is out of bounds so don't bother using
		}
		if(worldArray[(int)index.x, (int)index.y, (int)index.z].walkable == true){
			freePos = true;
		}
		

		return freePos;
	}

    /// <summary>
    /// Sets a world array position to free
    /// </summary>
    /// <param name="worldPos">world position to set free</param>
	public void setArrayPosToFree(Vector3 worldPos){ //sets position in worldArray to a 0 indicating it is now free
		Vector3 arrayPos = WorldToArrayPosition (worldPos);
		worldArray[(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z].walkable = true;
	}
    /// <summary>
    /// sets a position to free from the array index position
    /// </summary>
    /// <param name="arrayPos">index position to set free</param>
	public void setArrayPosToFreeByIndex(Vector3 arrayPos){ //sets position in worldArray to a 1 indicating it is now full (filled by a fish)
		worldArray[(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z].walkable = true;
	}

    /// <summary>
    /// Sets a world array position to full
    /// </summary>
    /// <param name="worldPos">the world position to set full</param>
	public void setArrayPosToFull(Vector3 worldPos){ //sets position in worldArray to a 1 indicating it is now full (filled by a fish)
		Vector3 arrayPos = WorldToArrayPosition (worldPos);
		worldArray[(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z].walkable = false;
	}

    /// <summary>
    /// sets a position to full from the array index position
    /// </summary>
    /// <param name="arrayPos">the array position to set to full</param>
	public void setArrayPosToFullByIndex(Vector3 arrayPos){ //sets position in worldArray to a 1 indicating it is now full (filled by a fish)
		worldArray[(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z].walkable = false;
	}

    /// <summary>
    /// Sets array position to full with food
    /// </summary>
    /// <param name="worldPos">the world position that needs to update that it contains food</param>
	public void setArrayPosToFullFood(Vector3 worldPos){ //sets position in worldArray to a 2 indicating it is now full with a food item
		Vector3 arrayPos = WorldToArrayPosition (worldPos);
		worldArray[(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z].hasFood = true;
	}

    /// <summary>
    /// Creates the world array
    /// </summary>
	void createWorldArray(){

		//calculates the number of gridCellSize sized chunks that fit in the world space between world min and world max
		cellAmount.x = (int)((Mathf.Max (worldMax.x, worldMin.x) - Mathf.Min (worldMax.x, worldMin.x)) / gridCellSize);
		cellAmount.y = (int)((Mathf.Max (worldMax.y, worldMin.y) - Mathf.Min (worldMax.y, worldMin.y)) / gridCellSize);
		cellAmount.z = (int)((Mathf.Max (worldMax.z, worldMin.z) - Mathf.Min (worldMax.z, worldMin.z)) / gridCellSize);
		//Debug.Log ("CA = " + cellAmount);
		//creates appropriate sized array to represent the world space. 
		worldArray = new node[(int)cellAmount.x, (int)cellAmount.y, (int)cellAmount.z];

		for(int i = 0; i < cellAmount.x ; i++){ //i is cycling through x
			for(int j = 0; j < cellAmount.y; j++){ //j is cycling through y
				for (int k = 0; k < cellAmount.z; k++) { //k is cycling through z
					//setting all positions in the world representation array to be 0
					//meaning each spot is free/empty
					worldArray [i, j, k] = new node ();
					worldArray [i, j, k].nodePos = new Vector3 (i, j, k);
				}
			}
		}
	}

    /// <summary>
    /// Checks if world array positions contain collisions
    /// </summary>
	void populateCollisions(){

		float chunkSizeHalf = gridCellSize / 2;
		Vector3 cube = new Vector3 (chunkSizeHalf - edgeOffSet, chunkSizeHalf - edgeOffSet, chunkSizeHalf - edgeOffSet);

		for(int i = 0; i < cellAmount.x ; i++){ //i is cycling through x
			for(int j = 0; j < cellAmount.y; j++){ //j is cycling through y
				for (int k = 0; k < cellAmount.z; k++) {
					Vector3 worldPoint = ArrayToWorldPosition (new Vector3 (i, j, k), false); 
					if (Physics.CheckBox (worldPoint, cube, transform.rotation, (1 << LayerMask.NameToLayer ("Dungeon")))) {
						setArrayPosToFull (worldPoint);
					}
				}
			}
		}

	}

    /// <summary>
    /// Calculates the maximum offset of the reef
    /// </summary>
	void calculateDungeonMaxOffset (){
		float outOfRangeX = (dungeonMaxBound.position.x - dungeonMinBound.position.x) - (cellAmount.x*gridCellSize);
		float outOfRangeY = (dungeonMaxBound.position.y - dungeonMinBound.position.y) - (cellAmount.y*gridCellSize);
		float outOfRangeZ = (dungeonMaxBound.position.z - dungeonMinBound.position.z) - (cellAmount.z*gridCellSize);
		offSetDungeonMax = new Vector3 (outOfRangeX, outOfRangeY, outOfRangeZ);
	}

    /// <summary>
    /// Creates and returns a copy of the world array
    /// </summary>
    /// <returns>Returns a node array of the world</returns>
	public node[,,] copyWorldArray(){
		node[,,] copy = new node[(int)cellAmount.x, (int)cellAmount.y, (int)cellAmount.z]; //chunkAmounts must be calculated before use of this function

		for(int i = 0; i < cellAmount.x ; i++){ //i is cycling through x
			for(int j = 0; j < cellAmount.y; j++){ //j is cycling through y
				for (int k = 0; k < cellAmount.z; k++) { //k is cycling through z
					//setting each nodes walkable status and node position based on world array
					//only unique values need to be set, other values are already set upon creating node array
					copy [i, j, k] = new node ();
					copy [i, j, k].walkable = worldArray [i, j, k].walkable;
					copy [i, j, k].nodePos = worldArray [i, j, k].nodePos;
				}
			}
		}

		return copy;
	}

    /// <summary>
    /// Resets the world array back to default values
    /// </summary>
    /// <param name="worldArray">The world array to reset</param>
	public void resetWorldArray (node[,,] worldArray){ //should move this into world.cs
		for (int i = 0; i < cellAmount.x; i++) { //i is cycling through x
			for (int j = 0; j < cellAmount.y; j++) { //j is cycling through y
				for (int k = 0; k < cellAmount.z; k++) { //k is cycling through z
					this.worldArray [i, j, k].f = 10000;
					this.worldArray [i, j, k].g = 10000;
					this.worldArray [i, j, k].h = 0;
					this.worldArray [i, j, k].ispath = false;
					this.worldArray [i, j, k].parent = null;
					this.worldArray [i, j, k].hasFood = false;
					this.worldArray [i, j, k].onClosedList = false;
					this.worldArray [i, j, k].onOpenList = false;
				}
			}
		}
	}

    /// <summary>
    /// Gets a list of all the neighbors of a node
    /// </summary>
    /// <param name="node">The node you want the neighbours of</param>
    /// <returns>returns a list of nodes</returns>
	public List<node> getNeighbors(node node){
		List<node> neighbors = new List<node> ();

		foreach (Vector3 nextdoor in neighborPos){ //adds all the valid neighbors of current node to the neighbors list

			//checking and adding all valid neighbours
			int checkX = (int)(node.nodePos.x + nextdoor.x);
			int checkY = (int)(node.nodePos.y + nextdoor.y);
			int checkZ = (int)(node.nodePos.z + nextdoor.z);

			if ((checkX >= cellAmount.x) || (checkY >= cellAmount.y) || (checkZ >= cellAmount.z) || (checkX < 0) || (checkY < 0) || (checkZ < 0)) {
				continue; // node is out of bounds so don't bother using
			} else {
				node checknode = worldArray [checkX, checkY, checkZ];
				// checknode.walkable && !closedList.Contains(checknode) && !openList.Contains(checknode)
				if (checknode.walkable) { //checks if node is walkable 
					neighbors.Add (checknode);
				}
			}
		}
		return neighbors;
	}
}
