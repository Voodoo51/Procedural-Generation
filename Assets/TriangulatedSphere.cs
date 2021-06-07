using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangulatedSphere : MonoBehaviour
{
    public Sphere terrainMesh;
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
        }

        terrainMesh.Initialize();
    }

    public void Generate()
    {
        Initialize();

    }
}
