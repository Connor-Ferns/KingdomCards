using UnityEngine;

[CreateAssetMenu(fileName = "ResourceType", menuName = "Grid/ResourceType")]
public class ResourceTypeSO : ScriptableObject
{
    public string resourceName;
    public Transform prefab;
    public int width;
    public int height;
    public bool canBeDestroyed;
}
