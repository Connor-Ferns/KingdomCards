using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Kingdom Cards/Unit Type")]
public class UnitSO : ScriptableObject
{
    [System.Serializable]
    public enum UnitType
    {
        Settler,
        Melee,
        Ranged,
        Scout
    }

    [Header("Basic Unit Info")]
    public string unitName;
    public UnitType unitType;
    public Transform prefab;
    public Sprite icon;

    [Header("Combat Stats")]
    public int maxHealth;
    public int attackDamage;
    public int attackRange;
    public int defence;

    [Header("Movement")]
    public int movementRange = 2;

    [Header("Resource Costs")]
    public List<ResourceManager.ResourceAmount> buildCosts  = new List<ResourceManager.ResourceAmount>();
}