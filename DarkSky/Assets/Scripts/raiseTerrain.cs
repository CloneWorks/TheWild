using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class raiseTerrain : MonoBehaviour {

    public Terrain terrain;

    public float heightChange;

	// Use this for initialization
	void Start () {
        increaseTerrainHeight(heightChange);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void increaseTerrainHeight(float height)
    {
        float[,] current = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        float[,] hts = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                hts[i, k] = height + current[i, k];
            }
        }

        terrain.terrainData.SetHeights(0, 0, hts);
    }
}
