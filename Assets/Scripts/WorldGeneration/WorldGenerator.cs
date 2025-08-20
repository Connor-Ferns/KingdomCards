using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [System.Serializable]
    public enum TerrainType
    {
        Water,
        Grass,
        Forest,
        Mountain,
        Sand,
        River
    }

    [System.Serializable]
    public class TerrainTile
    {
        public TerrainType type;
        public bool isBuildable;
        public bool hasResource;
        public ResourceManager.ResourceType resourceType;
        public int resourceAmount;
        
        public TerrainTile(TerrainType t)
        {
            type = t;
            // Can build on grass, sand, and forest (just not on water, mountains, rivers)
            isBuildable = (t != TerrainType.Water && 
                        t != TerrainType.Mountain && 
                        t != TerrainType.River);
            hasResource = false;
        }
    }

    [System.Serializable]
    public class ResourceDensity
    {
        [Range(0f, 1f)] public float woodDensity = 0.3f;
        [Range(0f, 1f)] public float ironDensity = 0.2f;
        [Range(1, 10)] public int woodAmountMin = 2;
        [Range(1, 10)] public int woodAmountMax = 5;
        [Range(1, 10)] public int ironAmountMin = 1;
        [Range(1, 10)] public int ironAmountMax = 4;
    }

    [System.Serializable]
    public class GuaranteedResources
    {
        [Header("Minimum resources per player territory")]
        public int minWoodNodes = 3;
        public int minIronNodes = 2;
    }

    [Header("Generation Settings")]
    [SerializeField] private int seed = 12345;
    [SerializeField] private bool useRandomSeed = true;
    
    [Header("Continental Terrain Settings")]
    [SerializeField] private float continentNoiseScale = 0.05f;  // Large scale for continent shape
    [SerializeField] private float detailNoiseScale = 0.15f;     // Small scale for terrain details
    [SerializeField] private float waterLevel = 0.35f;           // Lower = more land
    [SerializeField] private float mountainLevel = 0.75f;        // Higher = fewer mountains
    [SerializeField] private float forestLevel = 0.55f;          // Forest threshold
    
    [Header("River Settings")]
    [SerializeField] private bool generateRivers = true;
    [SerializeField] private int riverCount = 2;
    [SerializeField] private float riverWidth = 0.8f;
    
    [Header("Resource Distribution")]
    [SerializeField] private ResourceDensity resourceDensity = new ResourceDensity();
    [SerializeField] private GuaranteedResources guaranteedResources = new GuaranteedResources();
    
    [Header("Visual Prefabs")]
    [SerializeField] private GameObject grassPrefab;
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private GameObject mountainPrefab;
    [SerializeField] private GameObject sandPrefab;
    [SerializeField] private GameObject riverPrefab;
    
    [Header("Resource Visual Prefabs")]
    [SerializeField] private GameObject woodResourcePrefab;
    [SerializeField] private GameObject ironResourcePrefab;
    
    private TerrainTile[,] worldMap;
    private LevelGrid levelGrid;
    private GameObject terrainParent;
    private GameObject resourceParent;

    void Start()
    {
        levelGrid = LevelGrid.Instance;
        GenerateWorld();
    }

    public void GenerateWorld()
    {
        // Clean up any existing terrain
        CleanupWorld();
        
        // Initialize
        if (useRandomSeed)
            seed = Random.Range(0, 100000);
        
        Random.InitState(seed);
        Debug.Log($"Generating world with seed: {seed}");
        
        int width = levelGrid.GetWidth();
        int height = levelGrid.GetHeight();
        worldMap = new TerrainTile[width, height];
        
        // Create parent objects for organization
        terrainParent = new GameObject("Terrain");
        resourceParent = new GameObject("Resources");
        
        // Step 1: Generate continental landmass
        GenerateContinentalTerrain(width, height);
        
        // Step 2: Add rivers if enabled
        if (generateRivers)
            GenerateRivers(width, height);
        
        // Step 3: Apply terrain rules (beaches, forest clustering)
        ApplyTerrainRules(width, height);
        
        // Step 4: Clear starting areas for both players
        ClearStartingAreas(width, height);
        
        // Step 5: Place resources with guaranteed minimums
        PlaceResourcesWithGuarantees(width, height);
        
        // Step 6: Spawn visual tiles
        SpawnTerrainVisuals(width, height);
        
        // Step 7: Update grid objects with terrain data
        UpdateGridObjects(width, height);
        
        Debug.Log("World generation complete!");
    }

    void GenerateContinentalTerrain(int width, int height)
    {
        float offsetX = Random.Range(0f, 10000f);
        float offsetY = Random.Range(0f, 10000f);
        
        // Create continent shape using low-frequency noise
        float[,] continentShape = new float[width, height];
        float[,] terrainDetail = new float[width, height];
        
        // Generate continent shape and terrain detail separately
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Large-scale continent shape
                continentShape[x, y] = Mathf.PerlinNoise(
                    x * continentNoiseScale + offsetX,
                    y * continentNoiseScale + offsetY
                );
                
                // Small-scale terrain details
                terrainDetail[x, y] = Mathf.PerlinNoise(
                    x * detailNoiseScale + offsetX + 1000f,
                    y * detailNoiseScale + offsetY + 1000f
                );
            }
        }
        
        // Add gradient from center to create more continental feel
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxDistance = Mathf.Max(width, height) * 0.6f;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate distance from center
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float distanceGradient = 1f - (distance / maxDistance);
                distanceGradient = Mathf.Clamp01(distanceGradient);
                
                // Combine continent shape with detail and gradient
                float combinedNoise = (continentShape[x, y] * 0.6f + 
                                       terrainDetail[x, y] * 0.2f + 
                                       distanceGradient * 0.2f);
                
                TerrainType terrain;
                
                if (combinedNoise < waterLevel)
                {
                    terrain = TerrainType.Water;
                }
                else if (combinedNoise > mountainLevel)
                {
                    terrain = TerrainType.Mountain;
                }
                else if (combinedNoise > forestLevel)
                {
                    terrain = TerrainType.Forest;
                }
                else
                {
                    terrain = TerrainType.Grass;
                }
                
                worldMap[x, y] = new TerrainTile(terrain);
            }
        }
        
        // Ensure map edges are water for island feel
        for (int x = 0; x < width; x++)
        {
            worldMap[x, 0] = new TerrainTile(TerrainType.Water);
            worldMap[x, height - 1] = new TerrainTile(TerrainType.Water);
        }
        for (int y = 0; y < height; y++)
        {
            worldMap[0, y] = new TerrainTile(TerrainType.Water);
            worldMap[width - 1, y] = new TerrainTile(TerrainType.Water);
        }
    }

    void GenerateRivers(int width, int height)
    {
        for (int i = 0; i < riverCount; i++)
        {
            // Start river from a mountain or high ground
            List<Vector2Int> mountainPositions = new List<Vector2Int>();
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (worldMap[x, y].type == TerrainType.Mountain)
                    {
                        mountainPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            if (mountainPositions.Count == 0)
                continue;
            
            // Pick random mountain as river source
            Vector2Int riverStart = mountainPositions[Random.Range(0, mountainPositions.Count)];
            
            // Flow river downhill towards nearest water
            Vector2Int current = riverStart;
            int maxSteps = 50;
            int steps = 0;
            
            while (steps < maxSteps)
            {
                // Mark current position as river
                if (worldMap[current.x, current.y].type != TerrainType.Water &&
                    worldMap[current.x, current.y].type != TerrainType.Mountain)
                {
                    worldMap[current.x, current.y] = new TerrainTile(TerrainType.River);
                    worldMap[current.x, current.y].isBuildable = false;
                }
                
                // Find next position (prefer moving towards water)
                Vector2Int next = FindNextRiverPosition(current, width, height);
                if (next == current || worldMap[next.x, next.y].type == TerrainType.Water)
                    break;
                
                current = next;
                steps++;
            }
        }
    }

    Vector2Int FindNextRiverPosition(Vector2Int current, int width, int height)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(1, 1), new Vector2Int(-1, -1),
            new Vector2Int(1, -1), new Vector2Int(-1, 1)
        };
        
        foreach (var dir in directions)
        {
            Vector2Int next = current + dir;
            if (next.x > 0 && next.x < width - 1 && next.y > 0 && next.y < height - 1)
            {
                neighbors.Add(next);
            }
        }
        
        // Prefer moving towards water or lower elevation
        neighbors = neighbors.OrderBy(n => 
        {
            if (worldMap[n.x, n.y].type == TerrainType.Water) return 0;
            if (worldMap[n.x, n.y].type == TerrainType.Grass) return 1;
            if (worldMap[n.x, n.y].type == TerrainType.Forest) return 2;
            return 3;
        }).ToList();
        
        return neighbors.Count > 0 ? neighbors[0] : current;
    }

    void ApplyTerrainRules(int width, int height)
    {
        // Create a copy to read from while we modify the original
        TerrainTile[,] tempMap = new TerrainTile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tempMap[x, y] = new TerrainTile(worldMap[x, y].type);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Rule 1: Add sand beaches next to water
                if (tempMap[x, y].type == TerrainType.Grass)
                {
                    if (HasAdjacentTerrain(x, y, TerrainType.Water, tempMap))
                    {
                        if (Random.Range(0f, 1f) < 0.7f) // 70% chance for beaches
                        {
                            worldMap[x, y] = new TerrainTile(TerrainType.Sand);
                        }
                    }
                }
                
                // Rule 2: Cluster forests together
                if (tempMap[x, y].type == TerrainType.Forest)
                {
                    int forestNeighbors = CountAdjacentTerrain(x, y, TerrainType.Forest, tempMap);
                    if (forestNeighbors < 2 && Random.Range(0f, 1f) < 0.3f)
                    {
                        // Isolated forests might become grassland
                        worldMap[x, y] = new TerrainTile(TerrainType.Grass);
                    }
                }
            }
        }
    }

    void ClearStartingAreas(int width, int height)
    {
        // Clear player starting area (bottom center)
        int playerStartY = 1;
        int playerStartX = width / 2;
        ClearAreaForBase(playerStartX, playerStartY, 3);
        
        // Clear AI starting area (top center)
        int aiStartY = height - 2;
        int aiStartX = width / 2;
        ClearAreaForBase(aiStartX, aiStartY, 3);
    }

    void ClearAreaForBase(int centerX, int centerY, int radius)
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (IsValidPosition(x, y))
                {
                    // Make center grassland for building
                    worldMap[x, y] = new TerrainTile(TerrainType.Grass);
                    worldMap[x, y].hasResource = false; // Clear any resources
                }
            }
        }
    }

    void PlaceResourcesWithGuarantees(int width, int height)
    {
        // Define player territories
        int midY = height / 2;
        
        // Player territory (bottom half)
        PlaceGuaranteedResourcesInArea(0, 0, width, midY, "Player");
        
        // AI territory (top half)
        PlaceGuaranteedResourcesInArea(0, midY, width, height, "AI");
        
        // Place additional resources based on density settings
        PlaceAdditionalResources(width, height);
    }

    void PlaceGuaranteedResourcesInArea(int startX, int startY, int endX, int endY, string areaName)
    {
        Debug.Log($"Placing guaranteed resources for {areaName} territory");
        
        // Place guaranteed wood in forests
        PlaceGuaranteedResource(
            ResourceManager.ResourceType.Wood,
            guaranteedResources.minWoodNodes,
            TerrainType.Forest,
            startX, startY, endX, endY
        );
        
        // Place guaranteed iron in mountains
        PlaceGuaranteedResource(
            ResourceManager.ResourceType.Iron,
            guaranteedResources.minIronNodes,
            TerrainType.Mountain,
            startX, startY, endX, endY
        );
    }

    void PlaceGuaranteedResource(ResourceManager.ResourceType resourceType, int count, 
                                 TerrainType terrain, int startX, int startY, int endX, int endY)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();
        
        // Find valid positions in the specified area
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (worldMap[x, y].type == terrain && !worldMap[x, y].hasResource)
                {
                    validPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        // If not enough valid positions, convert some grass to the needed terrain
        if (validPositions.Count < count)
        {
            for (int x = startX; x < endX && validPositions.Count < count; x++)
            {
                for (int y = startY; y < endY && validPositions.Count < count; y++)
                {
                    if (worldMap[x, y].type == TerrainType.Grass && !worldMap[x, y].hasResource)
                    {
                        worldMap[x, y] = new TerrainTile(terrain);
                        validPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }
        
        // Place the resources
        for (int i = 0; i < count && i < validPositions.Count; i++)
        {
            Vector2Int pos = validPositions[i];
            worldMap[pos.x, pos.y].hasResource = true;
            worldMap[pos.x, pos.y].resourceType = resourceType;
            
            // Set amount based on resource type
            switch (resourceType)
            {
                case ResourceManager.ResourceType.Wood:
                    worldMap[pos.x, pos.y].resourceAmount = Random.Range(
                        resourceDensity.woodAmountMin, 
                        resourceDensity.woodAmountMax + 1
                    );
                    break;
                case ResourceManager.ResourceType.Iron:
                    worldMap[pos.x, pos.y].resourceAmount = Random.Range(
                        resourceDensity.ironAmountMin, 
                        resourceDensity.ironAmountMax + 1
                    );
                    break;
            }
        }
    }



    void PlaceAdditionalResources(int width, int height)
    {
        // Place additional resources based on density settings
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (worldMap[x, y].hasResource)
                    continue; // Skip if already has resource
                
                float roll = Random.Range(0f, 1f);
                
                // Wood in forests
                if (worldMap[x, y].type == TerrainType.Forest && roll < resourceDensity.woodDensity)
                {
                    worldMap[x, y].hasResource = true;
                    worldMap[x, y].resourceType = ResourceManager.ResourceType.Wood;
                    worldMap[x, y].resourceAmount = Random.Range(
                        resourceDensity.woodAmountMin, 
                        resourceDensity.woodAmountMax + 1
                    );
                }
                // Iron in mountains
                else if (worldMap[x, y].type == TerrainType.Mountain && roll < resourceDensity.ironDensity)
                {
                    worldMap[x, y].hasResource = true;
                    worldMap[x, y].resourceType = ResourceManager.ResourceType.Iron;
                    worldMap[x, y].resourceAmount = Random.Range(
                        resourceDensity.ironAmountMin, 
                        resourceDensity.ironAmountMax + 1
                    );
                }  
            }
        }
    }

    void SpawnTerrainVisuals(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridPosition gridPos = new GridPosition(x, y);
                Vector3 worldPos = levelGrid.GetWorldPosition(gridPos);
                GameObject prefab = GetTerrainPrefab(worldMap[x, y].type);
                
                if (prefab != null)
                {
                    GameObject tile = Instantiate(prefab, worldPos, Quaternion.identity, terrainParent.transform);
                    tile.name = $"Terrain_{x}_{y}_{worldMap[x, y].type}";
                }
                
                // Spawn resource visual if present
                if (worldMap[x, y].hasResource)
                {
                    GameObject resourcePrefab = GetResourcePrefab(worldMap[x, y].resourceType);
                    if (resourcePrefab != null)
                    {
                        Vector3 resourcePos = worldPos + new Vector3(0, 0, -0.1f); // Slightly above terrain
                        GameObject resource = Instantiate(resourcePrefab, resourcePos, Quaternion.identity, resourceParent.transform);
                        resource.name = $"Resource_{x}_{y}_{worldMap[x, y].resourceType}_{worldMap[x, y].resourceAmount}";
                    }
                }
            }
        }
    }

    void UpdateGridObjects(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridPosition gridPos = new GridPosition(x, y);
                GridObject gridObject = levelGrid.GetGridObject(gridPos);
                
                // Set terrain type in grid object
                gridObject.SetTerrainType(worldMap[x, y].type);
                
                // Set buildability
                gridObject.SetBuildable(worldMap[x, y].isBuildable);
                
                // Set natural resources if present
                if (worldMap[x, y].hasResource)
                {
                    gridObject.SetNaturalResource(
                        worldMap[x, y].resourceType,
                        worldMap[x, y].resourceAmount
                    );
                }
            }
        }
    }

    // Helper functions
    bool HasAdjacentTerrain(int x, int y, TerrainType terrain, TerrainTile[,] map)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int nx = x + dx;
                int ny = y + dy;
                
                if (IsValidPosition(nx, ny) && map[nx, ny].type == terrain)
                {
                    return true;
                }
            }
        }
        return false;
    }

    int CountAdjacentTerrain(int x, int y, TerrainType terrain, TerrainTile[,] map)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int nx = x + dx;
                int ny = y + dy;
                
                if (IsValidPosition(nx, ny) && map[nx, ny].type == terrain)
                {
                    count++;
                }
            }
        }
        return count;
    }

    int GetDistanceToTerrain(int x, int y, TerrainType terrain, TerrainTile[,] map, int maxDistance)
    {
        for (int distance = 1; distance <= maxDistance; distance++)
        {
            for (int dx = -distance; dx <= distance; dx++)
            {
                for (int dy = -distance; dy <= distance; dy++)
                {
                    if (Mathf.Abs(dx) != distance && Mathf.Abs(dy) != distance) continue;
                    
                    int nx = x + dx;
                    int ny = y + dy;
                    
                    if (IsValidPosition(nx, ny) && map[nx, ny].type == terrain)
                    {
                        return distance;
                    }
                }
            }
        }
        return -1;
    }

    bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < worldMap.GetLength(0) && y >= 0 && y < worldMap.GetLength(1);
    }

    GameObject GetTerrainPrefab(TerrainType type)
    {
        switch (type)
        {
            case TerrainType.Water: return waterPrefab;
            case TerrainType.Grass: return grassPrefab;
            case TerrainType.Forest: return forestPrefab;
            case TerrainType.Mountain: return mountainPrefab;
            case TerrainType.Sand: return sandPrefab;
            case TerrainType.River: return riverPrefab ?? waterPrefab; // Use water if no river prefab
            default: return grassPrefab;
        }
    }

    GameObject GetResourcePrefab(ResourceManager.ResourceType type)
    {
        switch (type)
        {
            case ResourceManager.ResourceType.Wood: return woodResourcePrefab;
            case ResourceManager.ResourceType.Iron: return ironResourcePrefab;
            default: return null;
        }
    }

    void CleanupWorld()
    {
        if (terrainParent != null)
            DestroyImmediate(terrainParent);
        if (resourceParent != null)
            DestroyImmediate(resourceParent);
    }

    // Public methods for other systems to query the world
    public bool IsBuildable(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return false;
        
        return worldMap[x, y].isBuildable;
    }

    public bool IsBuildable(GridPosition gridPos)
    {
        return IsBuildable(gridPos.x, gridPos.y);
    }

    public TerrainType GetTerrainAt(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return TerrainType.Water;
        
        return worldMap[x, y].type;
    }

    public TerrainType GetTerrainAt(GridPosition gridPos)
    {
        return GetTerrainAt(gridPos.x, gridPos.y);
    }

    // Debug method to regenerate world with new seed
    [ContextMenu("Regenerate World")]
    public void RegenerateWorld()
    {
        useRandomSeed = true;
        GenerateWorld();
    }

    // Debug method to regenerate with specific seed
    public void RegenerateWorldWithSeed(int newSeed)
    {
        useRandomSeed = false;
        seed = newSeed;
        GenerateWorld();
    }
}