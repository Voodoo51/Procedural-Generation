using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
[System.Serializable]
public class Sphere 
{
    [Header("General Settings")]
    [Range(5, 40)]
    public int size;

    [Header("Point Distribution Settings")]
    public float radius = 1;
    [Range(5, 100)]
    public int rejectionSamples = 30;

    [Header("Generation Settings")]
    public int seed;
    public int octaves;
    public float scale;
    public float dampening;
    public float persistence;
    public float lacunarity;
    public float heightScale;

    [Header("Noise Settings")]
    public Vector2 offset;
    public float frequency;

    private Polygon polygon;
    private TriangleNet.Mesh mesh;
    private List<float> heights;
    public UnityEngine.Mesh terrainMesh;


    public TerrainType[] terrainTypes;

    Texture2D texture;
    const int textureResolution = 50;




    public void Initialize()
    {
        radius = Mathf.Max(0.3f, radius);


        polygon = new Polygon();
        List<Vector2> points = GeneratePoints(radius, Vector2.one, rejectionSamples);
        foreach (Vector2 point in points)
        {
            polygon.Add(new Vertex(point.x, point.y));
        }

        ConstraintOptions constraintOptions = new ConstraintOptions();
        constraintOptions.ConformingDelaunay = true;

        mesh = polygon.Triangulate(constraintOptions) as TriangleNet.Mesh;

        ConstructMesh();
    }

    public void ConstructMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        IEnumerator<Triangle> triangleEnum = mesh.Triangles.GetEnumerator();

        for (int i = 0; i < mesh.Triangles.Count; i++)
        {
            if (!triangleEnum.MoveNext())
            {
                break;
            }


            Triangle currentTriangle = triangleEnum.Current;


            currentTriangle.GetVertexID(2);
            Vector3 v0 = new Vector3((float)currentTriangle.GetVertex(2).X, 0, (float)currentTriangle.GetVertex(2).Y);
            Vector3 v1 = new Vector3((float)currentTriangle.GetVertex(1).X, 0, (float)currentTriangle.GetVertex(1).Y);
            Vector3 v2 = new Vector3((float)currentTriangle.GetVertex(0).X, 0, (float)currentTriangle.GetVertex(0).Y);

            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);


            var normal = Vector3.Cross(v1 - v0, v2 - v0);

            for (int x = 0; x < 3; x++)
            {
                normals.Add(normal);
                uvs.Add(Vector3.zero);
            }
        }

        foreach (Vector3 point in vertices)
        {
            point.Normalize();
        }
        terrainMesh.Clear();
        terrainMesh.SetVertices(vertices);
        terrainMesh.triangles = triangles.ToArray();
        terrainMesh.RecalculateNormals();
        terrainMesh.uv = uvs.ToArray();




    }

    private List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {


        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        float center = sampleRegionSize.x / 2;

        spawnPoints.Add(sampleRegionSize / 2);
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2 * radius);
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }

        }

       
        return points;

    }

    private bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {

        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }


    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
