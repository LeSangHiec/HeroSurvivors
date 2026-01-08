using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChunkData
{
    public Vector2Int chunkCoord;           // Tọa độ chunk (0,0), (1,0), etc.
    public bool isGenerated;                // Đã generate chưa?

    // Obstacles trong chunk này (for pooling)
    public List<GameObject> obstacles = new List<GameObject>();

    // Chunk bounds (world space)
    public Bounds chunkBounds;

    public ChunkData(Vector2Int coord, Bounds bounds)
    {
        chunkCoord = coord;
        chunkBounds = bounds;
        isGenerated = false;
    }

    public void ClearObstacles()
    {
        obstacles.Clear();
    }
}