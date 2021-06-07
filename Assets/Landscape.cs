using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landscape : MonoBehaviour
{
    
    public TerrainMesh terrainMesh;

    MeshFilter meshFilter;

    private void Start()
    {   
        System.Console.WriteLine("DONE");

        GameObject meshObj = new GameObject("mesh");
        meshObj.transform.parent = transform;

        meshObj.AddComponent<MeshRenderer>();
        meshFilter = meshObj.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh();
        terrainMesh.terrainMesh = meshFilter.sharedMesh;
    }

    public void Initialize()
    {
        
        
        if (terrainMesh.terrainMesh == null)
        {
            System.Console.WriteLine("DONE");
           
            GameObject meshObj = new GameObject("mesh");
            meshObj.transform.parent = transform;

            meshObj.AddComponent<MeshRenderer>();
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            terrainMesh.terrainMesh = meshFilter.sharedMesh;
            terrainMesh.renderer = meshObj.GetComponent<MeshRenderer>();
        }

        terrainMesh.Initialize();
    }

    public void GenerateLandscape()
    {
        Initialize();
      
    }

    public void GenerateTrees()
    {
        terrainMesh.GenerateTrees();
    }
    
    
}
