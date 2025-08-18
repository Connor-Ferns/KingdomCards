using System.Collections.Generic;
using CodeMonkey.Toolkit.TMousePosition;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    // Mode's that are in the system
    public enum Mode
    {
        Normal,     // Not doing anything special
        Building,   // Currently placing buildings
    }

    [Header("Setup")]
    [SerializeField] private List<PlacedObjectTypeSO> buildingTypesList;
    [SerializeField] private BuildingGhost buildingGhost;
    
    // Current state
    private Mode currentMode = Mode.Normal;
    private PlacedObjectTypeSO currentBuildingType;
    private PlacedObjectTypeSO.Dir buildingDirection = PlacedObjectTypeSO.Dir.Down;

    private void Awake()
    {
        // Start with the first building type selected
        if (buildingTypesList.Count > 0)
        {
            currentBuildingType = buildingTypesList[0];
        }
    }

    private void Update()
    {
        // Handle keyboard input
        HandleKeyboardInput();
        
        // Handle mouse input
        HandleMouseInput();
        
        // Update the ghost visual
        UpdateGhostVisual();
    }

    private void HandleKeyboardInput()
    {
        // Press B to toggle building mode on/off
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (currentMode == Mode.Normal)
            {
                EnterBuildingMode();
            }
            else
            {
                ExitBuildingMode();
            }
        }

        // Press Escape to exit building mode
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitBuildingMode();
        }

        // Press R to rotate the building (only works in building mode)
        // if (Input.GetKeyDown(KeyCode.R) && currentMode == Mode.Building)
        // {
        //     buildingDirection = PlacedObjectTypeSO.GetNextDir(buildingDirection);
        //     Debug.Log("Rotated building to: " + buildingDirection);
        // }

        // Number keys to select different building types
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectBuildingType(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectBuildingType(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectBuildingType(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectBuildingType(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectBuildingType(4);
    }

    private void HandleMouseInput()
    {
        // Left click to place building
        if (Input.GetMouseButtonDown(0) && currentMode == Mode.Building)
        {
            PlaceBuilding();
        }
        
        // Right click to destroy building
        if (Input.GetMouseButtonDown(1) && currentMode == Mode.Building)
        {
            DestroyBuilding();
        }
    }

    private void PlaceBuilding()
    {
        // Check if it's the player's turn
        if (TurnManager.Instance.currentState != TurnManager.TurnState.PlayerTurn)
        {
            Debug.Log("Can only build during your turn!");
            return;
        }

        // Check if player has enough resources
        if (currentBuildingType.HasBuildCosts())
        {
            if (!ResourceManager.Instance.CanAfford(currentBuildingType.buildCosts))
            {
                Debug.Log($"Not enough resources to build {currentBuildingType.nameString}! Need: {currentBuildingType.GetBuildCostText()}");
                return;
            }
        }

        // Get the grid position where the mouse is
        Vector3 mouseWorldPos = MousePosition2D.GetPosition();
        GridPosition gridPos = LevelGrid.Instance.GetGridPosition(mouseWorldPos);
        
        // Check if this is a valid position on the grid
        if (!LevelGrid.Instance.IsValidGridPosition(gridPos))
        {
            Debug.Log("Invalid position!");
            return;
        }

        // Get all the grid squares this building will occupy
        Vector2Int origin = new Vector2Int(gridPos.x, gridPos.y);
        List<Vector2Int> buildingSquares = currentBuildingType.GetGridPositionList(origin, buildingDirection);

        // Check if all squares are free to build on
        bool canBuild = true;
        foreach (Vector2Int square in buildingSquares)
        {
            GridObject gridObject = LevelGrid.Instance.GetGridObject(new Vector3(square.x, square.y));
            if (gridObject == null || !gridObject.CanBuild())
            {
                canBuild = false;
                break;
            }
        }

        // If we can't build, show message and stop
        if (!canBuild)
        {
            Debug.Log("Can't build here - space is occupied!");
            return;
        }

        // Spend the resources
        if (currentBuildingType.HasBuildCosts())
        {
            ResourceManager.Instance.TrySpendResources(currentBuildingType.buildCosts);
        }

        // Actually place the building
        Vector3 worldPos = LevelGrid.Instance.GetWorldPosition(gridPos);
        Vector2Int offset = currentBuildingType.GetRotationOffset(buildingDirection);
        Vector3 buildingWorldPos = worldPos + new Vector3(offset.x, offset.y, 0);

        PlacedObject newBuilding = PlacedObject.Create(
            buildingWorldPos,
            origin,
            buildingDirection,
            currentBuildingType
        );

        // Mark all the grid squares as occupied
        foreach (Vector2Int square in buildingSquares)
        {
            GridObject gridObject = LevelGrid.Instance.GetGridObject(new Vector3(square.x, square.y));
            gridObject.SetPlacedObject(newBuilding);
        }

        Debug.Log($"Building placed: {currentBuildingType.nameString}. {currentBuildingType.GetResourceGenerationText()}");
    }

    private void DestroyBuilding()
    {
        // Get what the mouse is pointing at
        Vector3 mouseWorldPos = MousePosition2D.GetPosition();
        GridObject gridObject = LevelGrid.Instance.GetGridObject(mouseWorldPos);
        
        if (gridObject == null)
            return;
            
        PlacedObject building = gridObject.GetPlacedObject();
        
        // Check if there's a building here
        if (building == null)
        {
            Debug.Log("No building here to destroy");
            return;
        }

        // Check if this building can be destroyed
        if (!building.GetPlacedObjectTypeSO().canBeDestroyed)
        {
            Debug.Log("This building cannot be destroyed!");
            return;
        }

        // Get all grid squares the building occupies
        List<Vector2Int> buildingSquares = building.GetGridPositionList();
        
        // Clear all the grid squares
        foreach (Vector2Int square in buildingSquares)
        {
            GridObject gridObj = LevelGrid.Instance.GetGridObject(new Vector3(square.x, square.y));
            gridObj.ClearPlacedObject();
        }
        
        // Destroy the actual building GameObject
        building.DestroySelf();
        Debug.Log("Building destroyed!");
    }

    private void UpdateGhostVisual()
    {
        // Show/hide the building ghost based on current mode
        if (buildingGhost != null)
        {
            buildingGhost.gameObject.SetActive(currentMode == Mode.Building);
        }
    }

    private void EnterBuildingMode()
    {
        currentMode = Mode.Building;
        Debug.Log("Entered building mode - Click to place buildings!");
    }

    private void ExitBuildingMode()
    {
        currentMode = Mode.Normal;
        Debug.Log("Exited building mode");
    }

    private void SelectBuildingType(int index)
    {
        // Make sure the index is valid
        if (index < 0 || index >= buildingTypesList.Count)
            return;
            
        currentBuildingType = buildingTypesList[index];
        Debug.Log("Selected building: " + currentBuildingType.nameString);
        
        // Automatically enter building mode when selecting a building
        if (currentMode == Mode.Normal)
        {
            EnterBuildingMode();
        }
    }

    // This method is used by the BuildingGhost script
    public PlacedObjectTypeSO GetPlacedObjectTypeSO()
    {
        return currentBuildingType;
    }
}