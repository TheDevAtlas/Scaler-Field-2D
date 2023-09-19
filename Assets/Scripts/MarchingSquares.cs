using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquares : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int xSize; public int ySize;

    [Header("Terrain Value Settings")]
    public float scale; 
    public float valueCutoff;
    public int octaves = 4;
    public float persistence = 0.5f;

    [Header("Chunk Position")]
    public Vector2 chunkOffset;

    // Mesh Components //
    Mesh mesh; MeshFilter meshFilter; MeshCollider meshCollider;

    // Compute Shaders - Scripts That Can Run On The GPU //
    public ComputeShader marchingSquares;
    
    // Compute Buffers - Get Data Back From Marching Squares Compute Shader //
    ComputeBuffer triangleBuffer; ComputeBuffer triCountBuffer;

    // Value Texture - Holds Data For The Chunk - Can Be Saved, Modified, And Loaded //
    public RenderTexture valueTexture;
    public Texture2D newValues;

    public bool updateMesh;

    // Called On Start //
    void Start()
    {
        // Set Up Mesh //
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshFilter.mesh = mesh;

        // Generates New Chunk Data Texture - Only Run With Brand New Chunk //
        GenerateNewChunkData();

        // Calculates Mesh Verticies And Triangles And Sets Mesh //
        GenerateMesh();
        StartCoroutine(LoopGenerate());
    }

    IEnumerator LoopGenerate()
    {
        if(updateMesh)
        {
            GenerateNewChunkData();
            GenerateMesh();
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(LoopGenerate());
    }

    // Called When New Chunk Is Generated //
    void GenerateNewChunkData()
    {
        newValues = new Texture2D(xSize+2, ySize+2);
        for(int x = 0; x < xSize+2; x++){
            for(int y = 0; y < ySize+2; y++){
                float sample = 0.0f;
                float amplitude = 1.0f;
                float frequency = 1.0f;

                for (int octave = 0; octave < octaves; octave++)
                {
                    float perlinX = (chunkOffset.x * xSize + x) / scale * frequency;
                    float perlinY = (chunkOffset.y * ySize + y) / scale * frequency;

                    sample += Mathf.PerlinNoise(perlinX, perlinY) * amplitude;

                    amplitude *= persistence;
                    frequency *= 2.0f; // You can adjust this for different octave spacing.
                }

                sample = Mathf.Clamp01(sample);
                newValues.SetPixel(x,y, new Color(sample, sample, sample));
            }
        }

        newValues.Apply();

        valueTexture = new RenderTexture(xSize+2, ySize+2, 24);
        valueTexture.enableRandomWrite = true;
        valueTexture.Create();
        Graphics.Blit(newValues, valueTexture);
    }

    // Called When The Mesh Needs To Be Created //
    public void GenerateMesh()
    {
        print("Generate Mesh");
        // Create Buffers For Shader //
        triangleBuffer = new ComputeBuffer ((xSize-1) * (ySize-1) * 3, sizeof(float) * 3 * 3, ComputeBufferType.Append); // Number Of Squares * Max Triangles In Square //
        triCountBuffer = new ComputeBuffer (1, sizeof(int), ComputeBufferType.Raw); // Single Int - This Is The Easy One //

        // Set Variables In Marching Square Shader //
        triangleBuffer.SetCounterValue (0);
        marchingSquares.SetBuffer(0, "triangles", triangleBuffer);
        marchingSquares.SetFloat("valueCutoff", valueCutoff);
        marchingSquares.SetTexture(0, "valueTexture", valueTexture);

        // Run Marching Square Shader //
        marchingSquares.Dispatch(0, xSize / 8, ySize / 8, 1);

        // Get The Number Of Triangles ** SOLUTION FROM SEB LAG ** //
        int numTris = triangleBuffer.count;

        // Get Triangle Data From Shader ** SOLUTION FROM SEB LAG ** //
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        // Clear The Mesh For New Data //
        mesh.Clear();

        // Turn Shader Data Into Arrays For Mesh //
        Vector3[] vertices = new Vector3[numTris * 3];
        int[] meshTriangles = new int[numTris * 3];

        // Loop Through Each Triangle And Each Of It's Points //
        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }

        // Set New Mesh Data //
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    // ** STOLEN STRUCT FROM SEB LAG ** //
    struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 av;
        public Vector3 bv;
        public Vector3 cv;

        public Vector3 this [int i] {
            get {
                switch (i) {
                    case 0:
                        return av;
                    case 1:
                        return bv;
                    default:
                        return cv;
                }
            }
        }
    }
}
