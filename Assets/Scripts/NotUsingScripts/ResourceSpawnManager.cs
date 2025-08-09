using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawnManager : MonoBehaviour
{
    [SerializeField] private List<ResourceTypeSO> resourceTypes;
    [SerializeField] private int totalResourceCount = 50;

    [SerializeField] private GameObject SpawnedResourceContainer;

    private void Start()
    {
        SpawnResources();
    }

    private void SpawnResources()
    {
        for (int i = 0; i < totalResourceCount; i++)
        {
            ResourceTypeSO randomResource = resourceTypes[Random.Range(0, resourceTypes.Count)];

            GridPosition randomPos = GetRandomEmptyGridPosition();

            if (randomPos != null)
            {
                Vector3 worldPos = LevelGrid.Instance.GetWorldPosition(randomPos);
                Transform resourceGO = Instantiate(randomResource.prefab, worldPos, Quaternion.identity, SpawnedResourceContainer.transform);

                GridObject gridObject = LevelGrid.Instance.GetGridObject(randomPos);
                gridObject.SetPlacedObject(resourceGO.GetComponent<PlacedObject>());
            }
        }
    }

    private GridPosition GetRandomEmptyGridPosition()
    {
        for (int tries = 0; tries < 100; tries++)
        {
            int x = Random.Range(0, LevelGrid.Instance.GetWidth());
            int y = Random.Range(0, LevelGrid.Instance.GetHeight());
            GridPosition pos = new GridPosition(x, y);

            if (LevelGrid.Instance.IsValidGridPosition(pos))
            {
                GridObject gridObject = LevelGrid.Instance.GetGridObject(pos);
                if (gridObject.CanBuild())
                {
                    return pos;
                }
            }
        }
        return default;
    }
}
