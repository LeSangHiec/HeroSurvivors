using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EndlessMapManager : MonoBehaviour
{
    public static EndlessMapManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private Grid grid;
    [SerializeField] private Vector2 cellSize = new Vector2(2, 2);

    [Header("Chunk Settings")]
    [SerializeField] private int chunkSize = 20;
    [SerializeField] private int viewDistance = 2;

    [Header("Tilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase groundTile;

    [Header("Pattern Settings")]
    [SerializeField] private bool usePattern = false;
    [SerializeField] private Vector3Int patternPosition = new Vector3Int(0, 0, 0);
    [SerializeField] private Vector3Int patternSize = new Vector3Int(20, 20, 1);

    [Header("Obstacles")]
    [SerializeField] private int minObstaclesPerChunk = 5;
    [SerializeField] private int maxObstaclesPerChunk = 10;
    [SerializeField] private float obstacleSpacing = 2f;

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    [Header("Debug")]
    [SerializeField] private bool showChunkBorders = true;
    [SerializeField] private bool logChunkActivity = true;

    // Chunk tracking
    private Dictionary<Vector2Int, ChunkData> loadedChunks = new Dictionary<Vector2Int, ChunkData>();
    private Vector2Int currentPlayerChunk;
    private Vector2Int lastPlayerChunk;

    // Pattern cache
    private TileBase[] cachedPattern;
    private bool patternCached = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Auto-find player
        if (player == null)
        {
            PlayerController pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        // Auto-find grid
        if (grid == null)
        {
            grid = FindAnyObjectByType<Grid>();
        }

        // Validate
        if (groundTilemap == null)
        {
            Debug.LogError("EndlessMapManager: Ground Tilemap not assigned!");
        }

        if (!usePattern && groundTile == null)
        {
            Debug.LogError("EndlessMapManager: Ground Tile not assigned!");
        }
    }

    void Start()
    {
        // Cache pattern if enabled
        if (usePattern)
        {
            CachePattern();
        }

        // Update chunks immediately
        UpdateChunks();
    }

    void Update()
    {
        if (player == null) return;

        // Check if player moved to new chunk
        currentPlayerChunk = GetChunkCoord(player.position);

        if (currentPlayerChunk != lastPlayerChunk)
        {
            UpdateChunks();
            lastPlayerChunk = currentPlayerChunk;
        }

        // Debug key
        if (Input.GetKeyDown(KeyCode.C))
        {
            LogChunkStats();
        }
    }

    // ========== PATTERN CACHING ==========

    void CachePattern()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("Ground Tilemap not assigned!");
            return;
        }

        BoundsInt bounds = new BoundsInt(patternPosition, patternSize);
        cachedPattern = groundTilemap.GetTilesBlock(bounds);
        patternCached = true;

        Debug.Log($"<color=green>✓ Pattern cached: {patternSize.x}x{patternSize.y} tiles</color>");

        // Validate pattern
        int nonNullCount = 0;
        foreach (TileBase tile in cachedPattern)
        {
            if (tile != null) nonNullCount++;
        }

        Debug.Log($"<color=cyan>Pattern contains {nonNullCount}/{cachedPattern.Length} tiles</color>");

        if (nonNullCount == 0)
        {
            Debug.LogWarning("⚠️ Pattern is empty! Did you paint tiles at the pattern position?");
            Debug.LogWarning($"Pattern Position: {patternPosition}, Size: {patternSize}");
        }
    }

    // ========== CHUNK COORDINATE ==========

    Vector2Int GetChunkCoord(Vector3 worldPosition)
    {
        float chunkWorldSize = chunkSize * cellSize.x;

        int x = Mathf.FloorToInt(worldPosition.x / chunkWorldSize);
        int y = Mathf.FloorToInt(worldPosition.y / chunkWorldSize);

        return new Vector2Int(x, y);
    }

    Vector3 GetChunkWorldPosition(Vector2Int chunkCoord)
    {
        float chunkWorldSize = chunkSize * cellSize.x;

        return new Vector3(
            chunkCoord.x * chunkWorldSize,
            chunkCoord.y * chunkWorldSize,
            0
        );
    }

    Bounds GetChunkBounds(Vector2Int chunkCoord)
    {
        Vector3 center = GetChunkWorldPosition(chunkCoord);
        float chunkWorldSize = chunkSize * cellSize.x;

        center += new Vector3(chunkWorldSize / 2f, chunkWorldSize / 2f, 0);

        Vector3 size = new Vector3(chunkWorldSize, chunkWorldSize, 1);

        return new Bounds(center, size);
    }

    // ========== CHUNK MANAGEMENT ==========

    void UpdateChunks()
    {
        // Get required chunks
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int chunkCoord = currentPlayerChunk + new Vector2Int(x, y);
                requiredChunks.Add(chunkCoord);
            }
        }

        // Load new chunks
        foreach (Vector2Int coord in requiredChunks)
        {
            if (!loadedChunks.ContainsKey(coord))
            {
                LoadChunk(coord);
            }
        }

        // Unload far chunks
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();

        foreach (var kvp in loadedChunks)
        {
            if (!requiredChunks.Contains(kvp.Key))
            {
                chunksToUnload.Add(kvp.Key);
            }
        }

        foreach (Vector2Int coord in chunksToUnload)
        {
            UnloadChunk(coord);
        }

        if (logChunkActivity)
        {
            Debug.Log($"<color=cyan>Loaded chunks: {loadedChunks.Count}</color>");
        }
    }

    // ========== LOAD CHUNK ==========

    void LoadChunk(Vector2Int chunkCoord)
    {
        Bounds bounds = GetChunkBounds(chunkCoord);
        ChunkData chunk = new ChunkData(chunkCoord, bounds);

        // Generate ground tiles
        GenerateGroundTiles(chunk);

        // Spawn obstacles
        SpawnObstacles(chunk);

        // Mark as generated
        chunk.isGenerated = true;

        // Add to loaded chunks
        loadedChunks.Add(chunkCoord, chunk);

        if (logChunkActivity)
        {
            Debug.Log($"<color=green>✓ Loaded chunk: {chunkCoord}</color>");
        }
    }

    // ========== UNLOAD CHUNK ==========

    void UnloadChunk(Vector2Int chunkCoord)
    {
        if (!loadedChunks.TryGetValue(chunkCoord, out ChunkData chunk))
        {
            return;
        }

        // Clear ground tiles
        ClearGroundTiles(chunk);

        // Despawn obstacles
        DespawnObstacles(chunk);

        // Remove from dictionary
        loadedChunks.Remove(chunkCoord);

        if (logChunkActivity)
        {
            Debug.Log($"<color=red>✗ Unloaded chunk: {chunkCoord}</color>");
        }
    }

    // ========== GROUND TILES GENERATION ==========

    void GenerateGroundTiles(ChunkData chunk)
    {
        if (groundTilemap == null)
        {
            Debug.LogError("Ground Tilemap not assigned!");
            return;
        }

        Vector3 chunkWorldPos = GetChunkWorldPosition(chunk.chunkCoord);
        Vector3Int startTilePos = groundTilemap.WorldToCell(chunkWorldPos);

        if (usePattern && patternCached && cachedPattern != null)
        {
            GenerateWithPattern(startTilePos);
        }
        else
        {
            GenerateWithSingleTile(startTilePos);
        }
    }

    void GenerateWithPattern(Vector3Int startTilePos)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                // Wrap pattern (repeat infinitely)
                int patternX = x % patternSize.x;
                int patternY = y % patternSize.y;

                // Convert 2D to 1D index
                int patternIndex = patternY * patternSize.x + patternX;

                // Get tile from cached pattern
                TileBase tile = cachedPattern[patternIndex];

                // Set tile position
                Vector3Int tilePos = startTilePos + new Vector3Int(x, y, 0);

                // Paint tile
                groundTilemap.SetTile(tilePos, tile);
            }
        }
    }

    void GenerateWithSingleTile(Vector3Int startTilePos)
    {
        if (groundTile == null)
        {
            Debug.LogWarning("Ground Tile not assigned!");
            return;
        }

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = startTilePos + new Vector3Int(x, y, 0);
                groundTilemap.SetTile(tilePos, groundTile);
            }
        }
    }

    void ClearGroundTiles(ChunkData chunk)
    {
        if (groundTilemap == null) return;

        Vector3 chunkWorldPos = GetChunkWorldPosition(chunk.chunkCoord);
        Vector3Int startTilePos = groundTilemap.WorldToCell(chunkWorldPos);

        // Clear tiles
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = startTilePos + new Vector3Int(x, y, 0);
                groundTilemap.SetTile(tilePos, null);
            }
        }
    }

    // ========== OBSTACLE SPAWNING ==========

    void SpawnObstacles(ChunkData chunk)
    {
        if (ObstaclePoolManager.Instance == null)
        {
            return;
        }

        // Use chunk coord as seed for consistent generation
        Random.InitState(chunk.chunkCoord.x * 10000 + chunk.chunkCoord.y);

        int obstacleCount = Random.Range(minObstaclesPerChunk, maxObstaclesPerChunk + 1);
        List<Vector2> spawnedPositions = new List<Vector2>();

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2 spawnPos = GetRandomPositionInChunk(chunk, spawnedPositions);

            // Random obstacle type
            string poolName = Random.value > 0.5f ? "Rock" : "Tree";

            GameObject obstacle = ObstaclePoolManager.Instance.SpawnObstacle(
                poolName,
                spawnPos,
                Quaternion.identity
            );

            if (obstacle != null)
            {
                chunk.obstacles.Add(obstacle);
                spawnedPositions.Add(spawnPos);
            }
        }

        // Reset random seed
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    Vector2 GetRandomPositionInChunk(ChunkData chunk, List<Vector2> existingPositions)
    {
        Vector3 chunkWorldPos = GetChunkWorldPosition(chunk.chunkCoord);
        float chunkWorldSize = chunkSize * cellSize.x;

        int maxAttempts = 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(0f, chunkWorldSize);
            float randomY = Random.Range(0f, chunkWorldSize);
            Vector2 candidatePos = new Vector2(
                chunkWorldPos.x + randomX,
                chunkWorldPos.y + randomY
            );

            // Check spacing
            bool tooClose = false;
            foreach (Vector2 existingPos in existingPositions)
            {
                if (Vector2.Distance(candidatePos, existingPos) < obstacleSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                return candidatePos;
            }
        }

        // Fallback: random position
        return new Vector2(
            chunkWorldPos.x + Random.Range(0f, chunkWorldSize),
            chunkWorldPos.y + Random.Range(0f, chunkWorldSize)
        );
    }

    void DespawnObstacles(ChunkData chunk)
    {
        if (ObstaclePoolManager.Instance == null) return;

        // Separate rocks and trees
        List<GameObject> rocks = new List<GameObject>();
        List<GameObject> trees = new List<GameObject>();

        foreach (GameObject obstacle in chunk.obstacles)
        {
            if (obstacle != null)
            {
                if (obstacle.name.Contains("Rock"))
                {
                    rocks.Add(obstacle);
                }
                else if (obstacle.name.Contains("Tree"))
                {
                    trees.Add(obstacle);
                }
            }
        }

        // Despawn by pool
        ObstaclePoolManager.Instance.DespawnObstacles("Rock", rocks);
        ObstaclePoolManager.Instance.DespawnObstacles("Tree", trees);

        chunk.ClearObstacles();
    }

    // ========== DEBUG ==========

    void LogChunkStats()
    {
        Debug.Log("========== ENDLESS MAP STATS ==========");
        Debug.Log($"Player Chunk: {currentPlayerChunk}");
        Debug.Log($"Loaded Chunks: {loadedChunks.Count}");
        Debug.Log($"Cell Size: {cellSize}");
        Debug.Log($"Chunk Size: {chunkSize} cells = {chunkSize * cellSize.x} world units");
        Debug.Log($"Use Pattern: {usePattern}");

        if (usePattern)
        {
            Debug.Log($"Pattern Position: {patternPosition}");
            Debug.Log($"Pattern Size: {patternSize.x}x{patternSize.y}");
            Debug.Log($"Pattern Cached: {patternCached}");

            if (cachedPattern != null)
            {
                int nonNullCount = 0;
                foreach (TileBase tile in cachedPattern)
                {
                    if (tile != null) nonNullCount++;
                }
                Debug.Log($"Pattern Tiles: {nonNullCount}/{cachedPattern.Length}");
            }
        }


       
    }

    void OnDrawGizmos()
    {
        if (!showChunkBorders || player == null) return;

        // Draw loaded chunks
        foreach (var kvp in loadedChunks)
        {
            Bounds bounds = kvp.Value.chunkBounds;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        // Draw player chunk
        Bounds playerChunkBounds = GetChunkBounds(currentPlayerChunk);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(playerChunkBounds.center, playerChunkBounds.size);

        // Draw view distance
        float viewWorldSize = (viewDistance * 2 + 1) * chunkSize * cellSize.x;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(player.position, new Vector3(viewWorldSize, viewWorldSize, 0));
    }

    // ========== PUBLIC GETTERS ==========

    public Vector2Int GetCurrentPlayerChunk() => currentPlayerChunk;
    public int GetLoadedChunkCount() => loadedChunks.Count;
    public Vector2 GetCellSize() => cellSize;
    public int GetChunkSize() => chunkSize;
    public bool IsUsingPattern() => usePattern;
}