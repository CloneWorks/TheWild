using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles crab interaction and animation.
/// </summary>
public class crabInteract : MonoBehaviour {
    public Animator anim; ///< Holds the crabs animator component

    public Transform reefMinBound; ///< Holds the min bound position that the crab can move within
    public Transform reefMaxBound; ///< Holds the max bound position that the crab can move within

	private Transform currentNode; ///< Holds the position of the current node in the path
	private GameObject nextNode; ///< Hold the node data of the next node in the path

	public bool isMoveing = false; ///< Keeps track of when the crab is moving
	public bool pauseMoving = false; ///< Used to interrupt moving

	float CrabTurningSpeed = 1f; ///< The speed at which the crab turns
	float crabSwimmingSpeed = 0.025f; ///< The speed at which the crab moves
	float zRotation = 0; ///< stops twisting on its z axis
    public World gameArray; ///<object that holds a list of all nodes in the world

    public List<node> path; ///< Holds a list of nodes in the generated path
    public int pathPos; ///< Counter for moving through path, keeps trak of current position in the path list
	public AStar2D pathFind;///< Allows the fish to use A* path finding

    int crabStuckFrames = 0; ///< Number of frames the crab has been stuck for
    bool facingCurrentNode = false; ///< Used for making crab turn more aggressavly to a future node while moving to their current node (path smoothing)
    private float fishTurningSpeedOnPath = 8f; ///< The turning speed used when path smoothing

	// Use this for initialization
    /// <summary>
    /// Ran on initialization
    /// </summary>
	void Start () {
		nextNode = new GameObject();
	}
	
	// Update is called once per frame
    /// <summary>
    /// Called once per frame, handles setting default animation and when to path calculate and traverse
    /// </summary>
	void Update () {
        //if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
       // {
        //    anim.Play("Walk");
      //  }

		if (isMoveing == false) {
			calculatePath ();
			//calculateNewPos ();
		} else {
			moveCrabOnPath ();
			//moveCrab ();
		}

		//if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")) {
		//	pauseMoving = false;
		//}
	}

    /// <summary>
    /// When crab is tapped plays the crabs tap animation 
    /// </summary>
    void tap()
    {
        //anim.Play("Hide");
		pauseMoving = true;
    }

    /// <summary>
    /// When crab is held plays the crabs hold animation
    /// </summary>
    void hold()
    {
        //anim.Play("Hide");
    }

    /// <summary>
    /// Generates a path using A* and the world nodes.
    /// </summary>
	private void calculatePath(){
		path = new List<node>();

		//quicker reset look to function for more details
		pathFind.quickLocalArrayReset();

		Vector3 pos; 
		do{ //gets final destination
			pos = new Vector3 ( Random.Range (reefMinBound.position.x, reefMaxBound.position.x - gameArray.offSetDungeonMax.x),
				0, 
				Random.Range (reefMinBound.position.z, reefMaxBound.position.z - gameArray.offSetDungeonMax.z));
		} while(!gameArray.checkArrayPosFree (pos));

		Vector3 startPos = gameArray.WorldToArrayPosition (transform.position);
		startPos.y = 0; //fixing the crab to the bottom layer of y
		//int rightIsBad = 0;
		//while (!gameArray.checkArrayPosFreeByIndex (startPos)) { //stops fish from getting stuck inside objects 
		//	if (rightIsBad < 10) {
		//		if (gameArray.checkArrayPosFreeByIndex (startPos + Vector3.right)) {
		//			startPos = startPos + Vector3.right;
		//		} else {
		//			rightIsBad++;
		//		}
		//	} else {
		//		startPos = startPos + Vector3.left;
		//	}
		//}

		//if sea dweller is stuck in a none wealkable zone it will find the next free stop to calculate its next path from
		if (!gameArray.checkArrayPosFreeByIndex (startPos)) {
			int rightIsBad = 0;
			int leftIsBad = 0;
			int forwardIsBad = 0;
			int backIsBad = 0;
			while (!gameArray.checkArrayPosFreeByIndex (startPos)) { //stops fish from getting stuck inside objects 
				if (!gameArray.checkArrayPosFreeByIndex (startPos + (Vector3.right * rightIsBad))) {
					rightIsBad++;
				} else {
					startPos = startPos + (Vector3.right * rightIsBad);
					continue;
				}

				if (!gameArray.checkArrayPosFreeByIndex (startPos + (Vector3.left * leftIsBad))) {
					leftIsBad++;
				} else {
					startPos = startPos + (Vector3.left * leftIsBad);
					continue;
				}

				if (!gameArray.checkArrayPosFreeByIndex (startPos + (Vector3.back * backIsBad))) {
					backIsBad++;
				} else {
					startPos = startPos + (Vector3.back * backIsBad);
					continue;
				}

				if (!gameArray.checkArrayPosFreeByIndex (startPos + (Vector3.forward * forwardIsBad))) {
					forwardIsBad++;
				} else {
					startPos = startPos + (Vector3.forward * forwardIsBad);
					continue;
				}
			}
		}


		node startNode = pathFind.localWorldArray [(int)startPos.x, (int)startPos.y, (int)startPos.z];

		//setting random goal position
		Vector3 arrayPos = gameArray.WorldToArrayPosition (pos);
		arrayPos.y = 0;
		node endNode = pathFind.localWorldArray [(int)arrayPos.x, (int)arrayPos.y, (int)arrayPos.z]; //7, 3

		//goalNode = endNode;
		path = pathFind.findPath (startNode, endNode);
		if (path != null) {
			pathPos = path.Count - 3;
		} else { //if no path exist to get to the picked location
			return;
		}
		if (pathPos > 0) {
			gameArray.setArrayPosToFullByIndex (path[pathPos].nodePos);

			Vector3 NewPos = gameArray.ArrayToWorldPosition(path[pathPos].nodePos,false); 
			Vector3 terrainSample2 = new Vector3(NewPos.x, 0, NewPos.z);
			float terrainHeight2 = Terrain.activeTerrain.SampleHeight(terrainSample2) + Terrain.activeTerrain.GetPosition().y;
			terrainHeight2 = gameArray.WorldToArrayPosition (NewPos).y;
			NewPos.y = terrainHeight2;

			nextNode.transform.position = NewPos; //sets the nextNodes position to the validated position 
			currentNode = nextNode.transform;//sets the fishes new node (destination)
			isMoveing = true; //tells the fish it now moving
		}
	}

    //legacy path generator
    //private void calculateNewPos(){

    //    //get terrain height where the crab is at
    //    //Vector3 terrainSample = new Vector3(transform.position.x, 0, transform.position.z);
    //    //float terrainHeight = Terrain.activeTerrain.SampleHeight(terrainSample) + Terrain.activeTerrain.GetPosition().y;
    //    //terrainHeight = gameArray.WorldToArrayPosition (terrainHeight);

    //    //Debug.Log ("reef max y = " + reefMaxBound.position.y);
    //    //minus game array chunk size offset for array based on world max (array goes 0-n game area goes to 0-n+1) e.g 10-20 = 10 values == 0,1,2,3,4,5,6,7,8,9 instead of 0,1,2,3,4,5,6,7,8,9,10
    //    Vector3 pos = new Vector3 ( Random.Range (reefMinBound.position.x, reefMaxBound.position.x - gameArray.offSetDungeonMax.x),
    //                                0, 
    //                                Random.Range (reefMinBound.position.z, reefMaxBound.position.z - gameArray.offSetDungeonMax.z));

    //    Vector3 terrainSample2 = new Vector3(pos.x, 0, pos.z);
    //    float terrainHeight2 = Terrain.activeTerrain.SampleHeight(terrainSample2) + Terrain.activeTerrain.GetPosition().y;
    //    terrainHeight2 = gameArray.WorldToArrayPosition (pos).y;
    //    pos.y = terrainHeight2;


    //    if (gameArray.checkArrayPosFree(pos)) { //checks if new pos is free
    //        if(currentNode == nextNode.transform) gameArray.setArrayPosToFree(currentNode.transform.position);
    //        nextNode.transform.position = pos; //sets the nextNodes position to the validated position 
    //        currentNode = nextNode.transform;//sets the fishes new node (destination)
    //        gameArray.setArrayPosToFull (nextNode.transform.position);//updates worldArray with new position to full
    //        isMoveing = true; //tells the fish it now moving
    //    }
    //}

    /// <summary>
    /// Respnsible for moving the crab along its path.
    /// </summary>
	private void moveCrabOnPath(){
		if (!pauseMoving) {
			if ((currentNode.position - transform.position) != Vector3.zero && !facingCurrentNode) { //stops error from Quaternion.lerp and there is no single vector direction
				gameArray.setArrayPosToFullByIndex (path [pathPos + 1].nodePos); //sets current node a crab is on to full until they begion moving to the next node
				Quaternion targetRotation = Quaternion.LookRotation (currentNode.position - transform.position);
				float str = Mathf.Min (CrabTurningSpeed * Time.deltaTime, 1f);
				Quaternion myRotration = gameObject.transform.rotation;
				transform.rotation = Quaternion.Lerp (myRotration, targetRotation, str);
				gameObject.transform.eulerAngles = new Vector3 (gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, zRotation);
			}

			if (Vector3.Angle (transform.forward, currentNode.position - transform.position) < 10 || facingCurrentNode) { //normally 20
				facingCurrentNode = true;
				gameArray.setArrayPosToFreeByIndex (path [pathPos + 1].nodePos);
				transform.position = Vector3.MoveTowards (transform.position, currentNode.transform.position, crabSwimmingSpeed);

				if (pathPos > 0) {//start to face future node before getting to current
					Vector3 futureNodePos = gameArray.ArrayToWorldPosition (path [pathPos - 1].nodePos, false);

					Vector3 terrainSample = new Vector3(futureNodePos.x, 0, futureNodePos.z);
					float terrainHeight = Terrain.activeTerrain.SampleHeight(terrainSample) + Terrain.activeTerrain.GetPosition().y;
					terrainHeight = gameArray.WorldToArrayPosition (currentNode.position).y;
					futureNodePos = new Vector3(futureNodePos.x ,terrainHeight, futureNodePos.z);

					Quaternion targetRotation = Quaternion.LookRotation (futureNodePos - transform.position);
					float str = Mathf.Min (fishTurningSpeedOnPath * Time.deltaTime, 1f);

					Quaternion myRotration = gameObject.transform.rotation;
					transform.rotation = Quaternion.Lerp (myRotration, targetRotation, str);

					//fixed z rotation so fish dont rotate in un natural angles
					gameObject.transform.eulerAngles = new Vector3 (gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, zRotation);
				}

			} else {
				//do nothing when crab isn't facing the direction it needs to move in
			}
			//sets moving to false as the fish is at its desired location
			if (transform.position == currentNode.position || currentNode == null) {
				if (pathPos > 0) {
					if (gameArray.checkArrayPosFreeByIndex (path [pathPos - 1].nodePos)) {
						//gameArray.setArrayPosToFreeByIndex (path [pathPos].nodePos);
						pathPos--;
						gameArray.setArrayPosToFullByIndex (path [pathPos].nodePos);
						currentNode.position = gameArray.ArrayToWorldPosition (path [pathPos].nodePos, false);

						Vector3 terrainSample2 = new Vector3(currentNode.position.x, 0, currentNode.position.z);
						float terrainHeight2 = Terrain.activeTerrain.SampleHeight(terrainSample2) + Terrain.activeTerrain.GetPosition().y;
						terrainHeight2 = gameArray.WorldToArrayPosition (currentNode.position).y;
						currentNode.position = new Vector3(currentNode.position.x ,terrainHeight2, currentNode.position.z);

					} else {//if the next position isn't free in x frames time try new path
						crabStuckFrames++;
						if(crabStuckFrames >= 10){ //release fish
							gameArray.setArrayPosToFreeByIndex (path [pathPos].nodePos);
							isMoveing = false;
							facingCurrentNode = false;
						}
						crabStuckFrames = crabStuckFrames % 10;
					}
				} else {
					gameArray.setArrayPosToFreeByIndex (path [pathPos].nodePos);
					isMoveing = false;
					facingCurrentNode = false;
				}
			}
		}
	}

    ////legacy path moving
    //private void moveCrab(){
    //    if (!pauseMoving) {
    //        Quaternion targetRotation = Quaternion.LookRotation (currentNode.position - transform.position);
    //        float str = Mathf.Min (CrabTurningSpeed * Time.deltaTime, 1f);

    //        Quaternion myRotration = gameObject.transform.rotation;
    //        transform.rotation = Quaternion.Lerp (myRotration, targetRotation, str);

    //        gameObject.transform.eulerAngles = new Vector3 (gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, zRotation);

    //        if (Vector3.Angle (transform.forward, currentNode.position - transform.position) < 10) { //normally 20
    //            //fishanimat.Play ("fast"); //swimFast
    //            transform.position = Vector3.MoveTowards (transform.position, currentNode.transform.position, crabSwimmingSpeed);
    //        } else {
    //            //fishanimat.Play ("slow"); //swimSlow
    //        }
    //        //sets moving to false as the fish is at its desired location
    //        if (transform.position == currentNode.position || currentNode == null) {
    //            isMoveing = false;
    //        }
    //    }
    //}
}
