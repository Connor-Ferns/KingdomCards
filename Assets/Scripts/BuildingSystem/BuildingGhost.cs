using System;
using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    [SerializeField] private BuildingSystem buildingSystem;

    private PlacedObjectTypeSO placedObjectTypeSO;
    private Transform buildingVisual;

    private void Update()
    {
        FollowCursor();
        RefreshVisual();
    }

    private void FollowCursor()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(mousePosition);
        Vector3 worldGridPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);

        transform.position = Vector3.Lerp(transform.position, worldGridPosition, Time.deltaTime * 15f);
    }

    private void RefreshVisual()
    {
        placedObjectTypeSO = buildingSystem.GetPlacedObjectTypeSO();
        if (buildingVisual == null)
        {
            buildingVisual = Instantiate(placedObjectTypeSO.prefab, this.transform.position, Quaternion.identity, this.transform);
        }
        else
        {
            Destroy(buildingVisual.gameObject);
            buildingVisual = Instantiate(placedObjectTypeSO.prefab, this.transform.position, Quaternion.identity, this.transform);
        }
        
    }
}
