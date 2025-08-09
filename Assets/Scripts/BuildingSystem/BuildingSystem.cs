using System.Collections.Generic;
using CodeMonkey.Toolkit.TMousePosition;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [SerializeField] private List<PlacedObjectTypeSO> PlacedObjectTypeSOList;
    private PlacedObjectTypeSO placedObjectTypeSO;

    private PlacedObjectTypeSO.Dir dir = PlacedObjectTypeSO.Dir.Down;

    private bool isBuilding = false;
    [SerializeField] private BuildingGhost buildingGhost;

    private void Awake()
    {
        placedObjectTypeSO = PlacedObjectTypeSOList[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuilding = !isBuilding;
        }

        if (isBuilding == true)
        {
            buildingGhost.gameObject.SetActive(true);
        }
        else
        {
            buildingGhost.gameObject.SetActive(false);
        }

        if (isBuilding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GridPosition gridWorldPosition = LevelGrid.Instance.GetGridPosition(MousePosition2D.GetPosition());
                if (LevelGrid.Instance.IsValidGridPosition(gridWorldPosition))
                {
                    Vector3 gridPos = LevelGrid.Instance.GetWorldPosition(gridWorldPosition);

                    List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(new Vector2Int(gridWorldPosition.x, gridWorldPosition.y), dir);

                    // Test can build
                    bool canBuild = true;
                    foreach (Vector2Int gridPosition in gridPositionList)
                    {
                        if (!LevelGrid.Instance.GetGridObject(new Vector3(gridPosition.x, gridPosition.y)).CanBuild())
                        {
                            // Can't build here
                            canBuild = false;
                            break;
                        }
                    }

                    if (canBuild)
                    {
                        Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(dir);
                        Vector3 placedObjectWorldPosition = gridPos + new Vector3(rotationOffset.x, rotationOffset.y, 0);

                        PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, new Vector2Int(gridWorldPosition.x, gridWorldPosition.y), dir, placedObjectTypeSO);

                        foreach (Vector2Int gridPosition in gridPositionList)
                        {
                            LevelGrid.Instance.GetGridObject(new Vector3(gridPosition.x, gridPosition.y)).SetPlacedObject(placedObject);
                        }
                    }
                    else
                    {
                        Debug.Log("Cant Build Here!");
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            GridObject gridObject = LevelGrid.Instance.GetGridObject(MousePosition2D.GetPosition());
            PlacedObject placedObject = gridObject.GetPlacedObject();

            if (placedObject != null)
            {
                if (placedObject.GetPlacedObjectTypeSO().canBeDestroyed)
                {
                    placedObject.DestroySelf();

                    List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

                    foreach (Vector2Int gridPosition in gridPositionList)
                    {
                        LevelGrid.Instance.GetGridObject(new Vector3(gridPosition.x, gridPosition.y)).ClearPlacedObject();
                    }
                }
                else
                {
                    Debug.Log("Cant Destroy Resources");
                }
            }
            
        }

        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     dir = PlacedObjectTypeSO.GetNextDir(dir);
        // }

        if (Input.GetKeyDown(KeyCode.Alpha1)) { placedObjectTypeSO = PlacedObjectTypeSOList[0]; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { placedObjectTypeSO = PlacedObjectTypeSOList[1]; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { placedObjectTypeSO = PlacedObjectTypeSOList[2]; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { placedObjectTypeSO = PlacedObjectTypeSOList[3]; }

    }

    public PlacedObjectTypeSO GetPlacedObjectTypeSO()
    {
        return placedObjectTypeSO;
    }

    public void SetPlacedObjectTypeSO()
    {
        
    }
}
