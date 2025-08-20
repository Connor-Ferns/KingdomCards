using System;
using UnityEngine;

public class GridObject
{
    public event EventHandler OnValueChanged;

    private GridSystemXY<GridObject> gridSystem;
    private GridPosition gridPosition;
    private int value;
    private PlacedObject placedObject;
    
    // New terrain-related fields
    private bool isBuildable = true;
    private WorldGenerator.TerrainType terrainType = WorldGenerator.TerrainType.Grass;
    private bool hasNaturalResource = false;
    private ResourceManager.ResourceType naturalResourceType;
    private int naturalResourceAmount = 0;

    public GridObject(GridSystemXY<GridObject> gridSystem, GridPosition gridPosition)
    {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
    }

    public override string ToString()
    {
        string resourceInfo = hasNaturalResource ? $"\nResource: {naturalResourceType} x{naturalResourceAmount}" : "";
        return $"{gridPosition}\nTerrain: {terrainType}{resourceInfo}\n{placedObject}";
    }

    public void SetValue(int value)
    {
        this.value = value;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetValue()
    {
        return value;
    }

    public bool CanBuild()
    {
        // Can't build if terrain is not buildable or if there's already something here
        return isBuildable && placedObject == null;
    }

    public void SetPlacedObject(PlacedObject placedObject)
    {
        this.placedObject = placedObject;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public PlacedObject GetPlacedObject()
    {
        return placedObject;
    }

    public void ClearPlacedObject()
    {
        placedObject = null;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    // New terrain-related methods
    public void SetTerrainType(WorldGenerator.TerrainType terrain)
{
    terrainType = terrain;
    // Only water, mountains, and rivers are not buildable
    isBuildable = (terrain != WorldGenerator.TerrainType.Water && 
                  terrain != WorldGenerator.TerrainType.Mountain &&
                  terrain != WorldGenerator.TerrainType.River);
    OnValueChanged?.Invoke(this, EventArgs.Empty);
}

    public WorldGenerator.TerrainType GetTerrainType()
    {
        return terrainType;
    }

    public void SetBuildable(bool buildable)
    {
        isBuildable = buildable;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsBuildable()
    {
        return isBuildable;
    }

    public void SetNaturalResource(ResourceManager.ResourceType resourceType, int amount)
    {
        hasNaturalResource = true;
        naturalResourceType = resourceType;
        naturalResourceAmount = amount;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasNaturalResource()
    {
        return hasNaturalResource;
    }

    public ResourceManager.ResourceType GetNaturalResourceType()
    {
        return naturalResourceType;
    }

    public int GetNaturalResourceAmount()
    {
        return naturalResourceAmount;
    }

    // Method to harvest natural resources
    public int HarvestNaturalResource()
    {
        if (!hasNaturalResource)
            return 0;

        int harvested = naturalResourceAmount;
        naturalResourceAmount = 0;
        hasNaturalResource = false;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
        
        return harvested;
    }
}