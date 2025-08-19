using UnityEngine;

public class WorldGenerationTester : MonoBehaviour
{
    private WorldGenerator worldGenerator;
    
    [Header("Debug Controls")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode regenerateKey = KeyCode.G;
    [SerializeField] private KeyCode showInfoKey = KeyCode.I;
    
    private void Start()
    {
        worldGenerator = GetComponent<WorldGenerator>();
        if (worldGenerator == null)
        {
            worldGenerator = FindObjectOfType<WorldGenerator>();
        }
    }
    
    private void Update()
    {
        // Press G to regenerate world with new random seed
        if (Input.GetKeyDown(regenerateKey))
        {
            Debug.Log("Regenerating world with new seed...");
            worldGenerator.RegenerateWorld();
            AnalyzeGeneratedWorld();
        }
        
        // Press I to show world info
        if (Input.GetKeyDown(showInfoKey))
        {
            AnalyzeGeneratedWorld();
        }
        
        // Press number + Shift to regenerate with specific seed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    int seed = i * 11111; // Creates seeds like 0, 11111, 22222, etc.
                    Debug.Log($"Regenerating world with seed: {seed}");
                    worldGenerator.RegenerateWorldWithSeed(seed);
                    AnalyzeGeneratedWorld();
                }
            }
        }
    }
    
    private void AnalyzeGeneratedWorld()
    {
        if (!showDebugInfo) return;
        
        int width = LevelGrid.Instance.GetWidth();
        int height = LevelGrid.Instance.GetHeight();
        
        // Count terrain types
        int waterCount = 0;
        int grassCount = 0;
        int forestCount = 0;
        int mountainCount = 0;
        int sandCount = 0;
        int riverCount = 0;
        
        // Count resources
        int woodResources = 0;
        int ironResources = 0;
        int foodResources = 0;
        
        // Separate by territory
        int playerWood = 0, playerIron = 0, playerFood = 0;
        int aiWood = 0, aiIron = 0, aiFood = 0;
        int midY = height / 2;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridPosition gridPos = new GridPosition(x, y);
                GridObject gridObject = LevelGrid.Instance.GetGridObject(gridPos);
                
                // Count terrain
                switch (gridObject.GetTerrainType())
                {
                    case WorldGenerator.TerrainType.Water:
                        waterCount++;
                        break;
                    case WorldGenerator.TerrainType.Grass:
                        grassCount++;
                        break;
                    case WorldGenerator.TerrainType.Forest:
                        forestCount++;
                        break;
                    case WorldGenerator.TerrainType.Mountain:
                        mountainCount++;
                        break;
                    case WorldGenerator.TerrainType.Sand:
                        sandCount++;
                        break;
                    case WorldGenerator.TerrainType.River:
                        riverCount++;
                        break;
                }
                
                // Count resources
                if (gridObject.HasNaturalResource())
                {
                    ResourceManager.ResourceType resourceType = gridObject.GetNaturalResourceType();
                    bool isPlayerTerritory = y < midY;
                    
                    switch (resourceType)
                    {
                        case ResourceManager.ResourceType.Wood:
                            woodResources++;
                            if (isPlayerTerritory) playerWood++;
                            else aiWood++;
                            break;
                        case ResourceManager.ResourceType.Iron:
                            ironResources++;
                            if (isPlayerTerritory) playerIron++;
                            else aiIron++;
                            break;
                        case ResourceManager.ResourceType.Food:
                            foodResources++;
                            if (isPlayerTerritory) playerFood++;
                            else aiFood++;
                            break;
                    }
                }
            }
        }
        
        int totalTiles = width * height;
        
        Debug.Log("=== WORLD GENERATION ANALYSIS ===");
        Debug.Log($"Map Size: {width}x{height} = {totalTiles} tiles");
        Debug.Log("--- Terrain Distribution ---");
        Debug.Log($"Water: {waterCount} ({GetPercentage(waterCount, totalTiles)}%)");
        Debug.Log($"Grass: {grassCount} ({GetPercentage(grassCount, totalTiles)}%)");
        Debug.Log($"Forest: {forestCount} ({GetPercentage(forestCount, totalTiles)}%)");
        Debug.Log($"Mountain: {mountainCount} ({GetPercentage(mountainCount, totalTiles)}%)");
        Debug.Log($"Sand: {sandCount} ({GetPercentage(sandCount, totalTiles)}%)");
        Debug.Log($"River: {riverCount} ({GetPercentage(riverCount, totalTiles)}%)");
        
        int buildableTiles = grassCount + sandCount;
        Debug.Log($"Buildable Tiles: {buildableTiles} ({GetPercentage(buildableTiles, totalTiles)}%)");
        
        Debug.Log("--- Resource Distribution ---");
        Debug.Log($"Total Resources: Wood={woodResources}, Iron={ironResources}, Food={foodResources}");
        Debug.Log($"Player Territory: Wood={playerWood}, Iron={playerIron}, Food={playerFood}");
        Debug.Log($"AI Territory: Wood={aiWood}, Iron={aiIron}, Food={aiFood}");
        
        // Check balance
        float woodBalance = Mathf.Abs(playerWood - aiWood) / (float)Mathf.Max(1, woodResources);
        float ironBalance = Mathf.Abs(playerIron - aiIron) / (float)Mathf.Max(1, ironResources);
        float foodBalance = Mathf.Abs(playerFood - aiFood) / (float)Mathf.Max(1, foodResources);
        
        Debug.Log("--- Territory Balance ---");
        Debug.Log($"Wood Imbalance: {woodBalance:P0}");
        Debug.Log($"Iron Imbalance: {ironBalance:P0}");
        Debug.Log($"Food Imbalance: {foodBalance:P0}");
        
        if (woodBalance > 0.3f || ironBalance > 0.3f || foodBalance > 0.3f)
        {
            Debug.LogWarning("Resource distribution is imbalanced! Consider regenerating.");
        }
        else
        {
            Debug.Log("<color=green>Resource distribution is well balanced!</color>");
        }
    }
    
    private int GetPercentage(int value, int total)
    {
        if (total == 0) return 0;
        return Mathf.RoundToInt((value / (float)total) * 100);
    }
    
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        // Show controls in top-left corner
        int yPos = 10;
        int lineHeight = 20;
        
        GUI.Label(new Rect(10, yPos, 300, lineHeight), "=== World Generation Controls ===");
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 300, lineHeight), $"Press [{regenerateKey}] - New random world");
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 300, lineHeight), $"Press [{showInfoKey}] - Show world analysis");
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 300, lineHeight), "Press [Shift + 0-9] - Specific seed");
        yPos += lineHeight;
        
        GUI.Label(new Rect(10, yPos, 300, lineHeight), "Press [H] - Harvest resource at mouse");
    }
}