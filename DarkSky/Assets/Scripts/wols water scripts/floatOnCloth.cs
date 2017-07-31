using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatOnCloth : MonoBehaviour {
	
	public int closestVertex = 0;
	public int clothPlaneRatio;
	public Transform water;
	private Cloth clothPlane;
	
	// Use this for initialization
	void Start () {
		clothPlane = water.GetComponent<Cloth> ();
		getClothPlaneWidth ();
		getClosestVertex ();
	}
	
	// Update is called once per frame
	void Update () {
		updateFloatingHeight ();
	}

	void getClosestVertex() {
		Debug.Log ("inital vertex find - slow");
		for (int i = 0; i < clothPlane.vertices.Length; i++) {
			float distance = Vector3.Distance (clothPlane.vertices[i] + water.transform.position, transform.position);
			float distanceToClosestVertext = Vector3.Distance (clothPlane.vertices[closestVertex] + water.transform.position, transform.position);
			//Debug.Log (i + " " + (clothPlane.vertices[i] + water.transform.position) + " vs " + transform.position );

			//update to get closest vertex
			if (distance < distanceToClosestVertext) {
				closestVertex = i;
			}
		}
	}

	void getClosestVertex(bool optimized) {
		if (optimized) {

			int up, down, left, right = -1;
			int nw, ne, sw, se = -1;
			List<int> validDirection = new List<int>();

			//up
			int temp = closestVertex - clothPlaneRatio;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) ) {  // ((temp+1)%clothPlaneRatio != 0 || temp == 0 || temp == clothPlane.vertices.Length-1)
				up = temp;
				validDirection.Add (temp);
			}

			//down
			temp = closestVertex + clothPlaneRatio;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) ) {
				down = temp;
				validDirection.Add (temp);
			}

			//left
			temp = closestVertex - 1;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) && (closestVertex + 1)%clothPlaneRatio != 1) {
				left = temp;
				validDirection.Add (temp);
			}

			//right
			temp = closestVertex + 1;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) && (closestVertex + 1)%clothPlaneRatio != 0) {
				right = temp;
				validDirection.Add (temp);
			}

			//north west
			temp = closestVertex - 1 - clothPlaneRatio;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) && (closestVertex + 1)%clothPlaneRatio != 1) {
				nw = temp;
				validDirection.Add (temp);
			}

			//north east
			temp = closestVertex + 1 - clothPlaneRatio;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) && (closestVertex + 1)%clothPlaneRatio != 0) {
				ne = temp;
				validDirection.Add (temp);
			}

			//south west
			temp = closestVertex - 1 + clothPlaneRatio;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) &&  (closestVertex + 1)%clothPlaneRatio != 1) {
				sw = temp;
				validDirection.Add (temp);
			}

			//south east
			temp = closestVertex + 1 + clothPlaneRatio;
			if ( (temp >= 0) && (temp < clothPlane.vertices.Length) &&  (closestVertex + 1)%clothPlaneRatio != 0) {
				se = temp;
				validDirection.Add (temp);
			}

			//check valid surrounding vertices
			foreach (int v in validDirection) {
				float distance = Vector3.Distance (clothPlane.vertices[v] + water.transform.position, transform.position);
				float distanceToClosestVertext = Vector3.Distance(clothPlane.vertices[closestVertex] + water.transform.position, transform.position);

				//update to get closest vertex
				if (distance < distanceToClosestVertext) {
					closestVertex = v;
				}
			}

		} else {
			getClosestVertex ();
		}
	}

	void getClothPlaneWidth () {
		clothPlaneRatio = (int)Mathf.Sqrt ((float)clothPlane.vertices.Length);
	}

	void updateFloatingHeight(){
		getClosestVertex (true);

		//set new position
		//Vector3 vertPosWorld = water.TransformPoint(clothPlane.vertices [closestVertex]);

		transform.position = new Vector3 (
			transform.position.x,
			clothPlane.vertices [closestVertex].y + water.transform.position.y,
			transform.position.z
		);


	}
}
