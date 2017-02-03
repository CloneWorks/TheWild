using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A 2D version of Astar path finding algorithm. used for calculating paths on a single plane
/// </summary>
public class AStar2D : MonoBehaviour {

	public World world = null;//!< Stores a refrence to an instance of the world (holds all the gird that represents the world)

	public List<node> openList; //!< list of nodes that need to be checked
	public List<node> closedList; //!< list of nodes that have been checked

	private float diagonal_cost = 1.4142f; //!< 1.4142 constant cost of a directional movement
	private float double_diagonal_cost = 1.73f; //!< not used in 2D astar 1.73 constant cost of a double diagonal movement (a change in x, y, and z)
	private float normal_cost = 1.0f; //!< 1.0 constant cost of horizontal and verticle movements

	public node[,,] localWorldArray; //!< stops all objects using Astar from changing each others values by storing and using a local copy of the world array
	List<Vector3> neighborPos = new List<Vector3>();//!< List of vectors that helps locate a nodes neighbours

	
	
	
	
	
	
	
	
	
	List<node> path = new List<node>();

	public string Ax = "";
	public string Ay = "";
	
	public string Bx = "";
	public string By = "";
	
	public bool started = false;
	public GameObject floorTile = null;
	
	
    void OnGUI() {
        Ax = GUI.TextField(new Rect(50, 10, 160, 20), Ax, 25);
		Ay = GUI.TextField(new Rect(50, 10+20, 160, 20), Ay, 25);
		
		Bx = GUI.TextField(new Rect(50, 10+20+20, 160, 20), Bx, 25);
		By = GUI.TextField(new Rect(50, 10+20+20+20, 160, 20), By, 25);
		
		GUI.Label(new Rect(10, 10, 50, 20), "A X:");
		GUI.Label(new Rect(10, 10+20, 50, 20), "A Y:");
		  
		GUI.Label(new Rect(10, 10+20+20, 50, 20), "B X:");
		GUI.Label(new Rect(10, 10+20+20+20, 50, 20), "B Y:");
			
		 if (GUI.Button(new Rect(10+200, 10, 200, 80), "Calculate path")){
			 
			 path = new List<node>();
			 
			 cloneWorldArrayIntoLocal ();
			 started = true;
			 //quickLocalArrayReset();
			 
			 Vector3 startPos = new Vector3(int.Parse(Ax),0,int.Parse(Ay));
			 startPos = world.WorldToArrayPosition (startPos);
			 startPos.y = 0;
			 node start = localWorldArray [(int)startPos.x, (int)startPos.y, (int)startPos.z];
			 
			 Vector3 goalPos = new Vector3(int.Parse(Bx),1,int.Parse(By));
			 goalPos = world.WorldToArrayPosition (goalPos);
			 goalPos.y = 0;
			 node goal = localWorldArray [(int)goalPos.x, (int)goalPos.y, (int)goalPos.z];
			 
			 path = findPath(start, goal);
			 
			 foreach (node n in path){
				 if(n != null){
					Vector3 position = world.ArrayToWorldPosition(n.nodePos, false);
					position.y = 0;
					//piece must be in a good location, so place it:
					GameObject pieceToCreate = Instantiate(floorTile, position, floorTile.transform.rotation);
					pieceToCreate.transform.parent = transform;

					//pick a random rotation
					int yRotation = random90DegreeRotation();
					pieceToCreate.transform.eulerAngles = new Vector3(pieceToCreate.transform.eulerAngles.x, yRotation, pieceToCreate.transform.eulerAngles.z);
				 }
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
	/// Used for initialization. sets neighbours directions in neighbourPos list and makes a instance of the world array
	/// Only adds directions of the neighbours on a single plane
	/// </summary>
	void Start () {
		cloneWorldArrayIntoLocal ();
		//adding all posible directions to check for nodes in getNeighbours
		//used for checking and adding all valid neighbours of a single node
		
		//horizontal and verticle directions
		neighborPos.Add (new Vector3(0, 0, 1));
		neighborPos.Add (new Vector3(-1,0,0));
		neighborPos.Add (new Vector3(1,0,0));
		neighborPos.Add (new Vector3(0,0,-1));
		
		//diagonal directions
		//neighborPos.Add (new Vector3(-1, 0, 1));
		//neighborPos.Add (new Vector3(1,0,1));
		//neighborPos.Add (new Vector3(-1,0,-1));
		//neighborPos.Add (new Vector3(1,0,-1));

		//neighborPos.Add (new Vector3(-1,1,-1));
		//neighborPos.Add (new Vector3(0,1,-1));
		//neighborPos.Add (new Vector3(1,1,-1));
		//neighborPos.Add (new Vector3(-1,0,-1));
		//neighborPos.Add (new Vector3(1,0,-1));
		//neighborPos.Add (new Vector3(-1,-1,-1));
		//neighborPos.Add (new Vector3(0,-1,-1));
		//neighborPos.Add (new Vector3(1,-1,-1));

		//neighborPos.Add (new Vector3(-1,1,1));
		//neighborPos.Add (new Vector3(0,1,1));
		//neighborPos.Add (new Vector3(1,1,1));
		//neighborPos.Add (new Vector3(-1,0,1));
		//neighborPos.Add (new Vector3(1,0,1));
		//neighborPos.Add (new Vector3(-1,-1,1));
		//neighborPos.Add (new Vector3(0,-1,1));
		//neighborPos.Add (new Vector3(1,-1,1));


		//neighborPos.Add (new Vector3(0,0,-1));
		//neighborPos.Add (new Vector3(0,0,1));

		//precomputeNeighbors(localWorldArray); //failed doesn't work as neightbor g values are calculated on the fly, therefor speed increases are minumal
	}

	/*
	public void precomputeNeighbors(node[,,] array){
		for (int i = 0; i < world.cellAmount.x; i++) { //i is cycling through x
			for (int j = 0; j < world.cellAmount.y; j++) { //j is cycling through y
				for (int k = 0; k < world.cellAmount.z; k++) { //k is cycling through z
					array[i, j, k].neighbors.AddRange(neighbors(array[i, j, k], array[i, j, k]).ToList());
					//Debug.Log("yo = " + array [i, j, k].neighbors);
				}
			}
		}
	}*/

	//use update for debugging
	// Update is called once per frame
	//void Update () {
		/*
		if(Input.GetKeyDown(KeyCode.Space)){

			//Vector3 myPos = world.WorldToArrayPosition (transform.position);
			//world.worldArray [(int)myPos.x, (int)myPos.y, (int)myPos.z].walkable = false;

			//makes a copy of the current world and resets nodes values
			cloneWorldArrayIntoLocal ();

			Vector3 pos;
			do{
				pos = new Vector3 ( Random.Range (world.reefMinBound.position.x, world.reefMaxBound.position.x - world.offSetDungeonMax.x),
									Random.Range (world.reefMinBound.position.y, world.reefMaxBound.position.y - world.offSetDungeonMax.y), //gameArray.gridCellSize
									Random.Range (world.reefMinBound.position.z, world.reefMaxBound.position.z - world.offSetDungeonMax.z));
			} while(!world.checkArrayPosFree (pos));

			Vector3 startPos = world.WorldToArrayPosition (gameObject.transform.GetChild(0).position);
			node startNode = localWorldArray [(int)startPos.x, (int)startPos.y, (int)startPos.z];

			//setting random goal position
			Vector3 arrayPos = world.WorldToArrayPosition (pos);
			node endNode = localWorldArray [(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z]; //7, 3


			//startNode = localWorldArray[1,1,1];
			//endNode = localWorldArray[100,10,60];


			//System.DateTime startTime = System.DateTime.Now;

			List<node> path = findPath (startNode, endNode);

			//System.DateTime endTime = System.DateTime.Now;
			//Debug.Log ("Time taken = " + ( endTime.Subtract(startTime).Milliseconds ) );

		}
		*/
	//}

	//used for debugging
	
	void OnDrawGizmos() { //Selected
		if (started){
			
			
		//Color blue = new Color(0, 0, 1, 0.1F);
		//Color red = new Color(1, 0, 0, 0.5F);
		//Color green = new Color (0,1,0,0.5f);
		Color yellow = new Color (1,1,0,1);
		Color purple = new Color (0.67f,0,1,1);//rgb(67%, 0%, 100%)	

		float chunkSizeHalf = world.gridCellSize / 2;
		//float gizmoSizeOffset = 0.1f;
		Vector3 gizmoSize = new Vector3 (world.gridCellSize - world.gizmoSizeOffset,world.gridCellSize - world.gizmoSizeOffset,world.gridCellSize - world.gizmoSizeOffset);

		for(int i = 0; i < world.cellAmount.x ; i++){ //i is cycling through x
			for(int j = 0; j < world.cellAmount.y; j++){ //j is cycling through y
				for (int k = 0; k < world.cellAmount.z; k++) { //k is cycling through z
					//calculate gizmo centre
					//Debug.Log("walkable val = " + worldArray[i,j].walkable);
					Vector3 centre = new Vector3 ((i * world.gridCellSize) + chunkSizeHalf + world.worldMin.x, (j * world.gridCellSize) + chunkSizeHalf + world.worldMin.y, (k * world.gridCellSize) + chunkSizeHalf + world.worldMin.z);
					if (localWorldArray [i, j, k].ispath) {
						Gizmos.color = yellow;
						if (localWorldArray [i, j, k].hasFood) Gizmos.color = purple;
						Gizmos.DrawCube (centre, gizmoSize);
					}
				}
			}
		}
		
		}
	}

	/// <summary>
	/// Heuristic, manhattan distance.
	/// 
	/// change in x plus the change in y between checking node and end node
	/// </summary>
	/// <returns>The manhattan distance.</returns>
	/// <param name="start">Start node.</param>
	/// <param name="goal">Goal node.</param>
	public float heuristic_Manhattan_distance(node start, node goal){ //change in x plus the change in y between checking node and end node
		float heuristic = (int)(	Mathf.Abs (start.nodePos.x - goal.nodePos.x) + Mathf.Abs (start.nodePos.y - goal.nodePos.y)	+ Mathf.Abs (start.nodePos.z - goal.nodePos.z));
		start.h = heuristic;
		return heuristic;
	}

	// <summary>
	/// Clones the world array into local.
	/// </summary>
	public void cloneWorldArrayIntoLocal(){
		localWorldArray = world.copyWorldArray ();
	}

	/// <summary>
	/// Quick local array reset. saves resetting all nodes in the world array to their default values
	/// </summary>
	public void quickLocalArrayReset(){
		if(openList != null){
			foreach (node n in openList){
				n.f = 10000;
				n.g = 10000;
				n.h = 0;
				n.ispath = false;
				n.parent = null;
				n.hasFood = false;
				n.onClosedList = false;
				n.onOpenList = false;
			}
		}
		if(closedList != null){
			foreach (node n in closedList){
				n.f = 10000;
				n.g = 10000;
				n.h = 0;
				n.ispath = false;
				n.parent = null;
				n.hasFood = false;
				n.onClosedList = false;
				n.onOpenList = false;
			}
		}
	}

	/// <summary>
	/// Finds the path. Starts the 2D Astar algorithm and returns a path through the world array of nodes that avoids non-walkable spaces.
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="start">Start node.</param>
	/// <param name="goal">Goal node.</param>
	public List<node> findPath(node start, node goal){ //IEnumerator

		//resetArray ();
		//makes a copy of the current world and resets nodes values
		//cloneWorldArrayIntoLocal ();
		//world.resetWorldArray(localWorldArray);

		openList = new List<node> ();
		closedList = new List<node> ();
		openList.Add (start);
		start.onOpenList = true;

		start.g = 0;
		start.h = heuristic_Manhattan_distance (start, goal);
		start.f = start.g + start.h;

		node lowestF = new node (); //holds lowestFvalue node
		lowestF = null;

		while (openList.Count > 0){
			node current = new node();
			current = null;

			//didn't work out to increase performance
			//openList.Sort ((a, b) => a.f.CompareTo (b.f));
			//current = openList[0];

			//finds node on open list with lowest f value
			foreach (node check in openList) { //slecects the node in the openList with the lowest f cost
				if (current == null) {
					current = check;
				}
				if (check.f < current.f) {
					current = check;
				}
			}

			//goal state
			if (current == goal){ // or when closedList.Contains(goal)
				return construct_path (current); //current is the same as goal at this point
			}

			openList.Remove (current);
			current.onOpenList = false;
			closedList.Add (current);
			current.onClosedList = true;

			foreach (node neighbor in neighbors(current, goal)) { 
				//no need to do this check just slows the prgram down
				//if (neighbor.onClosedList) { //closedList.Contains (neighbor)
				//	continue;
				//}

				float tentative_gScore = current.g + directionalGCost (current, neighbor); // score to the new neighbor
				openList.Add(neighbor);
				neighbor.onOpenList = true;

				if((tentative_gScore >= neighbor.g)) {
					continue; 
				}
				neighbor.parent = current;
				neighbor.g = tentative_gScore;
				neighbor.f = neighbor.g + heuristic_Manhattan_distance (neighbor, goal);
			}

		}

		return null; // no path exist
	}

	/// <summary>
	/// Directionals cost, G cost. Adds a cost to a node based on the direction the algorithm went to get to that node
	/// </summary>
	/// <returns>The G cost.</returns>
	/// <param name="current">Current node.</param>
	/// <param name="neighbor">Neighbor node.</param>
	public float directionalGCost(node current, node neighbor){
		bool changeInX = neighbor.nodePos.x != current.nodePos.x; //A
		bool changeInY = neighbor.nodePos.y != current.nodePos.y; //B
		bool changeInZ = neighbor.nodePos.z != current.nodePos.z; //C

		if ((changeInX && changeInY) || (changeInY && changeInZ) || (changeInX && changeInZ)) { //checks if diagnal to node
			if (changeInX && changeInY && changeInZ) {
				//neighbor.hasFood = true;
				return double_diagonal_cost;
			} else {
				return diagonal_cost;
			}
		} else { //else it is orthogonal (horizontal or vertical)
			return normal_cost;
		}

	}

	/// <summary>
	/// Returns a list of the Neighbour nodes of the specified node. takes the goal to apply a heuristic to the neighbours
	/// </summary>
	/// <param name="node">Current Node being tested.</param>
	/// <param name="goal">Goal node.</param>
	public List<node> neighbors(node node, node goal){
		List<node> neighbors = new List<node> ();

		foreach (Vector3 nextdoor in neighborPos){ //adds all the valid neighbors of current node to the neighbors list

			//checking and adding all valid neighbours
			int checkX = (int)(node.nodePos.x + nextdoor.x);
			int checkY = (int)(node.nodePos.y + nextdoor.y);
			int checkZ = (int)(node.nodePos.z + nextdoor.z);

			if ((checkX >= world.cellAmount.x) || (checkY >= world.cellAmount.y) || (checkZ >= world.cellAmount.z) || (checkX < 0) || (checkY < 0) || (checkZ < 0)) {
				continue; // node is out of bounds so don't bother using
			} else {
				node checknode = localWorldArray [checkX, checkY, checkZ];
				// checknode.walkable && !closedList.Contains(checknode) && !openList.Contains(checknode)
				if (checknode.walkable && !checknode.onClosedList && !checknode.onOpenList) { //checks if node is walkable 
					neighbors.Add (checknode);

					checknode.g = node.g + directionalGCost(node, checknode);
					checknode.h = heuristic_Manhattan_distance (checknode, goal);
					checknode.f = checknode.g + checknode.h;

					checknode.parent = node;
				}
			}
		}

		return neighbors;
	}

	/// <summary>
	/// Constructs the path ready to be passd back to the function that required a path.
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="node">Node.</param>
	public List<node> construct_path(node node){ //contructs the path from following the parents 
		List<node> path = new List<node>();
		path.Add (node);

		while(node != null){
			node.ispath = true;
			node = node.parent;
			path.Add (node);
		}

		return path;
	}
}
