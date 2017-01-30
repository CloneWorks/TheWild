using UnityEngine;
using System.Collections;

public class Flatter : MonoBehaviour
{

    TerrainData terrData;

    float[,] heightmapData; // prepare a matrix with terrain points
    float ratio; // ratio between player position and terrain points
    float ratioY; 

    // Use this for initialization
    void Start()
    {

        terrData = Terrain.activeTerrain.terrainData;
        int terrRes = terrData.heightmapResolution;
        Vector3 terrSize = terrData.size;

        heightmapData = terrData.GetHeights(0, 0, terrRes, terrRes); // heights we will change during of walking

        ratio = (terrSize.x) / terrRes;
        

        Debug.Log("heightmapData=" + heightmapData + " terrRes=" + terrRes + " terrSize=" + terrSize + " ratio=" + ratio);
    }

    // Update is called once per frame
    void Update()
    {
        //update terrain
        terrData = Terrain.activeTerrain.terrainData;
        int terrRes = terrData.heightmapResolution;
        int terrResY = terrData.heightmapHeight;

        Vector3 terrSize = terrData.size;

        heightmapData = terrData.GetHeights(0, 0, terrRes, terrRes); // heights we will change during of walking

        ratio = (terrSize.x) / terrResY;
        ratioY = (terrSize.y);

        //work out players position (player pos + terrain offset)
        float playPosX = transform.position.x + (terrData.size.x / 2);
        float playPosZ = transform.position.z + (terrData.size.z / 2);

        float playPosY = transform.position.y + (terrData.size.y);

        int terrainPointZ = Mathf.CeilToInt(playPosX / ratio);
        int terrainPointX = Mathf.CeilToInt(playPosZ / ratio);

        int terrainPointY = Mathf.CeilToInt(playPosY / ratioY);

        Debug.Log(" terrainPointX=" + terrainPointX + " x terrainPointZ=" + terrainPointZ + ": set height to: " + terrainPointY);

        heightmapData[terrainPointX, terrainPointZ] = terrainPointY; // move terrain point to 0 (example)

    }


    void OnGUI()
    {
        if (Input.GetKey(KeyCode.H))
        {
            Debug.Log("Saving");

            terrData.SetHeights(0, 0, heightmapData); // save terrain heights back
        }
    }
}