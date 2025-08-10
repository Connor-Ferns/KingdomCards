using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu()]
public class PlacedObjectTypeSO : ScriptableObject
{
    public static Dir GetNextDir(Dir dir)
    {
        switch (dir)
        {
            default: return Dir.Down;
            case Dir.Down: return Dir.Left;
            case Dir.Left: return Dir.Up;
            case Dir.Up: return Dir.Right;
            case Dir.Right: return Dir.Down;
        }
    }
    public enum Dir
    {
        Down,
        Left,
        Up,
        Right
    }

    [Header("Basic Building Info")]
    public string nameString;
    public Transform prefab;
    public Sprite visual;
    public int width;
    public int height;
    public bool canBeDestroyed;
    public List<Vector2Int> Offsets;

    [Header("Resource System")]
    [Tooltip("Resources this building generates each turn")]
    public List<ResourceManager.ResourceAmount> resourceGeneration = new List<ResourceManager.ResourceAmount>();

    [Tooltip("Resources required to build this")]
    public List<ResourceManager.ResourceAmount> buildCosts = new List<ResourceManager.ResourceAmount>();

    public static int GetRotationAngle(Dir dir)
    {
        switch (dir)
        {
            default:
            case Dir.Down: return 0;
            case Dir.Left: return 90;
            case Dir.Up: return 180;
            case Dir.Right: return 270;
        }
    }

    public Vector2Int GetRotationOffset(Dir dir)
    {
        switch (dir)
        {
            default:
            case Dir.Down: return Offsets[0];
            case Dir.Left: return Offsets[1];
            case Dir.Up: return Offsets[2];
            case Dir.Right: return Offsets[3];
        }
    }

    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir)
    {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();

        switch (dir)
        {
            default:
            case Dir.Down:
            case Dir.Up:
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
            case Dir.Left:
            case Dir.Right:
                for (int x = 0; x < height; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
        }
        return gridPositionList;
    }

    public bool HasResourceGeneration()
    {
        return resourceGeneration != null && resourceGeneration.Count > 0;
    }

    public bool HasBuildCosts()
    {
        return buildCosts != null && buildCosts.Count > 0;
    }

    public string GetResourceGenerationText()
    {
        if (!HasResourceGeneration()) return "No resources generated";

        string text = "Generates: ";
        for (int i = 0; i < resourceGeneration.Count; i++)
        {
            text += $"{resourceGeneration[i].amount} {resourceGeneration[i].resourceType}";
            if (i < resourceGeneration.Count - 1) text += ", ";
        }
        return text;
    }
    
    public string GetBuildCostText()
    {
        if (!HasBuildCosts()) return "Free";

        string text = "Cost: ";
        for (int i = 0; i < buildCosts.Count; i++)
        {
            text += $"{buildCosts[i].amount} {buildCosts[i].resourceType}";
            if (i < buildCosts.Count - 1) text += ", ";
        }
        return text;
    }
}
