using UnityEngine;
using System.Collections;

public class Flatter : MonoBehaviour
{

    TerrainData terrData;

    float[,] heightmapData; // prepare a matrix with terrain points
    float ratio; // ratio between player position and terrain points

    // Use this for initialization
    void Start()
    {

        terrData = Terrain.activeTerrain.terrainData;
        int terrRes = terrData.heightmapResolution;
        Vector3 terrSize = terrData.size;

        heightmapData = terrData.GetHeights(0, 0, terrRes, terrRes); // heights we will change during of walking

        ratio = terrSize.x / terrRes;

        Debug.Log("heightmapData=" + heightmapData + " terrRes=" + terrRes + " terrSize=" + terrSize + " ratio=" + ratio);
    }

    // Update is called once per frame
    void Update()
    {

        int terrainPointX = Mathf.CeilToInt(transform.position.x / ratio);
        int terrainPointZ = Mathf.CeilToInt(transform.position.z / ratio);

        Debug.Log(" terrainPointX=" + terrainPointX + " x terrainPointZ=" + terrainPointZ + ": set height to 0.0");

        heightmapData[terrainPointZ, terrainPointX] = 0.0f; // move terrain point to 0 (example)

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