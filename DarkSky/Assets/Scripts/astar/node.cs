using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A node designed to work with and speed up A*
/// </summary>
public class node {

	public float h; ///< Heuristic value
	public float g; ///< Movement cost
	public float f; ///< Heuristic value combined with the movement cost (h + g) 
	public node parent; ///< Parent of this node (How you can translate to this node)

	public bool walkable; ///< Records whether this node is walkable
	public bool ispath; ///< Records if this node is apart of a path
	public bool hasFood; ///< Records if a piece of food is on this node
	public Vector3 nodePos; ///< Position of node

	public bool onClosedList = false; ///< Is on the list of nodes that have been checked
	public bool onOpenList = false; ///< Is on the list of nodes that hasn't been checked

	//public List<node> neighbors;

	// Use this for initialization
    /// <summary>
    /// Constructs a default node
    /// </summary>
	public node(){
		parent = null;

		walkable = true;
		hasFood = false;
		ispath = false;

		onOpenList = false;
		onClosedList = false;

		g = 1000f;
		f = 1000f;
		h = 0;

		//neighbors = new List<node>();
	}
}
