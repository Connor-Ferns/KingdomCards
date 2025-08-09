using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, PlacedObjectTypeSO.Dir dir, PlacedObjectTypeSO PlacedObjectTypeSO)
    {
        Transform placedObjectTransform = Instantiate(PlacedObjectTypeSO.prefab, worldPosition, Quaternion.Euler(0, 0, PlacedObjectTypeSO.GetRotationAngle(dir)));

        PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();

        placedObject.placedObjectTypeSO = PlacedObjectTypeSO;
        placedObject.origin = origin;
        placedObject.dir = dir;

        return placedObject;
    }

    private PlacedObjectTypeSO placedObjectTypeSO;
    private Vector2Int origin;
    private PlacedObjectTypeSO.Dir dir;

    public List<Vector2Int> GetGridPositionList()
    {
        return placedObjectTypeSO.GetGridPositionList(origin, dir);
    }

    public PlacedObjectTypeSO GetPlacedObjectTypeSO()
    {
        return placedObjectTypeSO;
    }
    
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
