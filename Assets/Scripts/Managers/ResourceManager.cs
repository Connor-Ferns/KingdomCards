using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [System.Serializable]
    public enum ResourceType
    {
        Food,
        Wood,
        Iron
    }

    [System.Serializable]
    public class ResourceAmount
    {
        public ResourceType resourceType;
        public int amount;

        public ResourceAmount(ResourceType type, int value)
        {
            resourceType = type;
            amount = value;
        }
    }

    [Header("Starting Resources")]
    [SerializeField] private int startingFood = 10;
    [SerializeField] private int startingIron = 5;
    [SerializeField] private int startingWood = 10;

    // Current resource amounts
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    // Events for UI updates
    public event Action<ResourceType, int> OnResourceChanged;
    public event Action OnResourcesGenerated;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one ResourceManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeResources();
    }

    private void Start()
    {
        // Subscribe to turn events
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnEnd += GenerateResources;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnEnd -= GenerateResources;
        }
    }

    private void InitializeResources()
    {
        resources[ResourceType.Food] = startingFood;
        resources[ResourceType.Iron] = startingIron;
        resources[ResourceType.Wood] = startingWood;

        // Notify UI of initial values
        foreach (var resource in resources)
        {
            OnResourceChanged?.Invoke(resource.Key, resource.Value);
        }
    }

    public int GetResource(ResourceType resourceType)
    {
        return resources.ContainsKey(resourceType) ? resources[resourceType] : 0;
    }

    public bool HasEnoughResources(List<ResourceAmount> costs)
    {
        foreach (var cost in costs)
        {
            if (GetResource(cost.resourceType) < cost.amount)
            {
                return false;
            }
        }
        return true;
    }

    public bool TrySpendResources(List<ResourceAmount> costs)
    {
        if (!HasEnoughResources(costs))
        {
            Debug.Log("Not enough resources!");
            return false;
        }

        foreach (var cost in costs)
        {
            AddResource(cost.resourceType, -cost.amount);
        }

        return true;
    }

    public void AddResource(ResourceType resourceType, int amount)
    {
        if (!resources.ContainsKey(resourceType))
        {
            resources[resourceType] = 0;
        }

        resources[resourceType] = Mathf.Max(0, resources[resourceType] + amount);
        OnResourceChanged?.Invoke(resourceType, resources[resourceType]);

        if (amount > 0)
        {
            Debug.Log($"Gained {amount} {resourceType}. Total: {resources[resourceType]}");
        }
    }

    public void AddResources(List<ResourceAmount> resourcesToAdd)
    {
        foreach (var resource in resourcesToAdd)
        {
            AddResource(resource.resourceType, resource.amount);
        }
    }

    private void GenerateResources()
    {
        Debug.Log("Generating resources from buildings...");

        // Find all buildings on the grid and generate resources
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int y = 0; y < LevelGrid.Instance.GetHeight(); y++)
            {
                GridPosition gridPos = new GridPosition(x, y);
                GridObject gridObject = LevelGrid.Instance.GetGridObject(gridPos);

                if (gridObject != null && gridObject.GetPlacedObject() != null)
                {
                    PlacedObject building = gridObject.GetPlacedObject();
                    PlacedObjectTypeSO buildingType = building.GetPlacedObjectTypeSO();

                    // Generate resources from this building
                    if (buildingType.resourceGeneration != null && buildingType.resourceGeneration.Count > 0)
                    {
                        AddResources(buildingType.resourceGeneration);
                    }
                }
            }
        }

        OnResourcesGenerated?.Invoke();
    }

    // Helper methods for common operations
    public bool CanAfford(List<ResourceAmount> costs)
    {
        return HasEnoughResources(costs);
    }

    public void DebugPrintResources()
    {
        foreach (var resource in resources)
        {
            Debug.Log($"{resource.Key}: {resource.Value}");
        }
    }
}