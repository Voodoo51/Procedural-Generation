using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
[System.Serializable]
public class TerrainMesh
{
    [Header("General Settings")]
    public int diameter;

    [Header("Point Distribution Settings")]
    public float radius = 1;
    [Range(5,100)]
    public int rejectionSamples = 30;

    [Header("Generation Settings")]
    public int seed;
    [Range(0, 5)]
    public int steps;
    public float waterLevel;
    public float snowLevel;
    public float floorLevel = 10f;
    public float minimunGroundLevel;


    [Header("Noise Settings")]
    public Vector2 offset;
    public int octaves;
    public float scale;
    [Range(0, 1)]
    public float dampening;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;
    public float heightScale;
    float frequency;

    private Polygon polygon;
    private TriangleNet.Mesh mesh;
    private List<float> heights;
    public UnityEngine.Mesh terrainMesh;

    [Header("Color Settings")]
    public ColorSettings colorSettings;
    
    public Gradient gradient;


   
    

    [HideInInspector]
    public MeshRenderer renderer;

    List<Vector2> points;

    float minTerrainHeight;
    float maxTerrainHeight;

    public void Initialize()
    {
        radius = Mathf.Max(0.3f, radius);
       

        polygon = new Polygon();

        points = GeneratePoints(radius, new Vector2(diameter, diameter), rejectionSamples);
        foreach(Vector2 point in points)
        {
            polygon.Add(new Vertex(point.x, point.y));          
        }
        
        ConstraintOptions constraintOptions = new ConstraintOptions();
        constraintOptions.ConformingDelaunay = true;

        mesh = polygon.Triangulate(constraintOptions) as TriangleNet.Mesh;
        heights = new List<float>();

        ConstructMesh();
    }

    public void ConstructMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        Color[] colors;
        

        IEnumerator<Triangle> triangleEnum = mesh.Triangles.GetEnumerator();


        ShapeTerrain();
        for (int i = 0; i < mesh.Triangles.Count; i++)
        {
            if (!triangleEnum.MoveNext())
            {
                break;
            }


            Triangle currentTriangle = triangleEnum.Current;


            Vector3 v0 = new Vector3((float)currentTriangle.GetVertex(2).X, heights[currentTriangle.GetVertexID(2)], (float)currentTriangle.GetVertex(2).Y);
            Vector3 v1 = new Vector3((float)currentTriangle.GetVertex(1).X, heights[currentTriangle.GetVertexID(1)], (float)currentTriangle.GetVertex(1).Y);
            Vector3 v2 = new Vector3((float)currentTriangle.GetVertex(0).X, heights[currentTriangle.GetVertexID(0)], (float)currentTriangle.GetVertex(0).Y);

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
        
        colors = new Color[vertices.Count];
       

        if (colorSettings == ColorSettings.Random)
        {
            for (int i = 0; i < triangles.Count - 2; i += 3)
            {

                Color color = EvaluateColor();
                colors[i] = color;
                colors[i + 1] = color;
                colors[i + 2] = color;

            }
        }
        else if (colorSettings == ColorSettings.HeightGradient)
        {
            for (int i = 0; i < triangles.Count - 2; i += 3)
            {
                float height = Mathf.InverseLerp(minTerrainHeight + waterLevel, maxTerrainHeight + snowLevel, (vertices[i].y + vertices[i + 1].y + vertices[i + 2].y)/3);
               
                Color color = EvaluateColor(height);
                colors[i] = color;
                colors[i + 1] = color;
                colors[i + 2] = color;
            }

        }

            terrainMesh.Clear();
        terrainMesh.SetVertices(vertices);
        terrainMesh.triangles = triangles.ToArray();
        terrainMesh.colors = colors;
        terrainMesh.uv = uvs.ToArray();
        terrainMesh.RecalculateNormals();





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
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, diameter, points, grid)) 
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

    private bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, float diameter, List<Vector2> points, int[,] grid)
    {
        Vector2 center = new Vector2(sampleRegionSize.x / 2, sampleRegionSize.y / 2);
        if (Vector2.Distance(candidate, center) > diameter/2)
        {
            return false;
        }
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

    private void ShapeTerrain()
    {

        minTerrainHeight = float.PositiveInfinity;
        maxTerrainHeight = float.NegativeInfinity;
        List<float> range = new List<float>();
 
        if(steps > 0)
        {
            float addition = 0;
            while (addition < 0.98f)
            {
                
                range.Add(addition);
                addition += 1f / steps;
            }
            
            
            range.Add(1);
           

        }

        for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for(int o = 0; o < octaves; o++)
                {

                    float xValue = (float)mesh.VerticesDict[i].X / scale * frequency;
                    float yValue = (float)mesh.VerticesDict[i].Y / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(xValue + offset.x + seed, yValue + offset.y + seed) * 2 - 1;
                    perlinValue *= dampening;

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }



    
            noiseHeight = (noiseHeight < 0f) ? noiseHeight * heightScale / floorLevel : noiseHeight * heightScale;
            
            if (noiseHeight > maxTerrainHeight)
            {
                maxTerrainHeight = noiseHeight;
            }
            else if (noiseHeight < minTerrainHeight)
            {
                minTerrainHeight = noiseHeight;
            }

            if (steps > 0)
            {
                
             
                heights.Add(Mathf.Max(Cubify(range,minimunGroundLevel), Cubify(range, noiseHeight)));
                
                
            }
            else
            {
                heights.Add(Mathf.Max(minimunGroundLevel,noiseHeight));
            }

           
        }
        
        

    }

    public float Cubify(List<float> range ,float noiseHeight)
    {
        for (int i = 0; i < range.Count; i++)
        {
            range[i] += (int)noiseHeight;
        }

        float betweenMinValue = range[0];
        float betweenMaxValue = range[1];


        for(int i = 1; i < range.Count; i++)
        {
            if(noiseHeight > range[i])
            {
                betweenMaxValue = range[i];
                betweenMinValue = range[i-1];
            }
            
        }

        float differenceMax = betweenMaxValue - noiseHeight;
        float differenceMin = noiseHeight - betweenMinValue;

        for (int i = 0; i < range.Count; i++)
        {
            range[i] -= (int)noiseHeight;
        }

        return differenceMax > differenceMin ? betweenMinValue : betweenMaxValue;
    }

    public void GenerateTrees()
    {
        for(int i = 0; i < terrainMesh.vertices.Length; i++)
        {
            if (terrainMesh.colors[i].Equals(gradient.colorKeys[2]))
            {
                RaycastHit hitInfo;
                Ray ray = new Ray(new Vector3(terrainMesh.vertices[i].x, heightScale, terrainMesh.vertices[i].z),Vector3.down);
                if (Physics.Raycast(ray, out hitInfo))
                {
                    //MonoBehaviour.Instantiate(tree, hitInfo.point, Quaternion.Euler(hitInfo.normal));
                }
            }
        }
    }

    private Color EvaluateColor(float height = 0)
    {

        switch(colorSettings)
        {
            case ColorSettings.Random:
                return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            case ColorSettings.HeightGradient:
                return gradient.Evaluate(height);

        }

        

        return Color.magenta;


    }


    public enum ColorSettings {Random, HeightGradient };

}
