using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public Transform player;
    public float chunkSize;

    public MarchingSquares chunkPrefab;

    List<MarchingSquares> chunks = new List<MarchingSquares>();

    void Start(){
        for(int i = -1; i <= 1; i++){
            for(int j = -1; j <= 1; j++){
                MarchingSquares newChunk = Instantiate(chunkPrefab.gameObject, new Vector3(i * chunkSize, j * chunkSize, 0f), Quaternion.identity).GetComponent<MarchingSquares>();
                newChunk.chunkOffset = new Vector2(i + 1, j + 1);
                chunks.Add(newChunk);
            }
        }
    }

    public void UpdateChunk(Vector2 playerPos){
        Vector2 s = playerPos;
        playerPos += new Vector2(32, 32);
        playerPos /= chunkSize;
        Vector2Int chunkPos = new Vector2Int((int)playerPos.x, (int)playerPos.y);

        foreach(var chunk in chunks)
        {
            if (chunk.chunkOffset == new Vector2(chunkPos.x, chunkPos.y))
            {
                Texture2D oldTexture = chunk.newValues;
                for(int i = -4; i <= 0; i++)
                {
                    for (int j = -4; j <= 0; j++)
                    {
                        
                        oldTexture.SetPixel((int)s.x + i - chunkPos.x * 32, (int)s.y + j - chunkPos.y * 32, new Color(0, 0, 0));
                    }
                }
                

                oldTexture.Apply();

                RenderTexture valueTexture = new RenderTexture(32 + 2, 32 + 2, 24);
                valueTexture.enableRandomWrite = true;
                valueTexture.Create();

                Graphics.Blit(oldTexture, valueTexture);
                chunk.valueTexture = null;
                chunk.valueTexture = valueTexture;
                chunk.GenerateMesh();
            }
        }
    }
}
