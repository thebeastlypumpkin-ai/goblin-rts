using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Building Definition", fileName = "NewBuildingDefinition")]
public class BuildingDefinition : ScriptableObject
{
    [Header("Identity")]
    public string buildingName;

    [Header("Economy")]
    [Min(0)] public int cost = 0;
    [Min(0)] public float buildTimeSeconds = 0f;

    [Header("Stats")]
    [Min(1)] public int maxHealth = 100;

    [Header("Income")]
    [Min(0)] public int incomePerTick = 0;

    [Header("Supply")]
    [Min(0)] public int supplyProvided = 0;

    [Header("Flags")]
    public bool isFortress = false;
    public bool isProductionBuilding = false;
}