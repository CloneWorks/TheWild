using UnityEngine;
using System.Collections;

public class Flatter : MonoBehaviour
{

    TerrainData terrData;

    float[,] heightmapData; // prepare a matrix with terrain points
    float ratio; // ratio between player position and terrain points
    float ratioY; //height ratio

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //update terrain
        terrData = Terrain.activeTerrain.terrainData;
        int terrRes = terrData.heightmapResolution;

        Vector3 terrSize = terrData.size;

        heightmapData = terrData.GetHeights(0, 0, terrRes, terrRes); // heights we will change during of walking

        ratio = (terrSize.x) / terrRes;
        ratioY = (terrSize.y) / terrRes;

        //work out players position (player pos + terrain offset)
        float playPosX = transform.position.x + (terrData.size.x / 2);
        float playPosZ = transform.position.z + (terrData.size.z / 2);

        //store players height
        float playPosY = transform.position.y/terrRes;

        //get terrain point based of player location
        int terrainPointZ = Mathf.CeilToInt(playPosX / ratio) - 1;
        int terrainPointX = Mathf.CeilToInt(playPosZ / ratio) - 1;

        //get terrain height based on player location
        float terrainPointY = (playPosY / ratioY);

        //set height of terrain point under play to players height
        heightmapData[terrainPointX, terrainPointZ] = terrainPointY;

        //get set heights in radius of player
        int radius = 5;

        for (int x = terrainPointX - radius; x < terrainPointX + radius; x++ )
        {
            for (int z = terrainPointZ - radius; z < terrainPointZ + radius; z++)
            {
                heightmapData[x, z] = terrainPointY;
            }
        }

    }


    void OnGUI()
    {
        if (Input.GetKey(KeyCode.H))
        {
            //Debug.Log("Saving");

            terrData.SetHeights(0, 0, heightmapData); // save terrain heights back
        }
    }
}