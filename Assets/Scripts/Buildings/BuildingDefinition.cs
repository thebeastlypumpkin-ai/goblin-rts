using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Building Definition", fileName = "NewBuildingDefinition")]
public class BuildingDefinition : ScriptableObject
{
    [Header("Identity")]
    public string buildingName;

    [Header("Prefabs")]
    public GameObject completedBuildingPrefab;

    [Header("Economy")]
    [Min(0)] public int cost = 0;
    [Min(0)] public float buildTimeSeconds = 0f;

    [Header("Stats")]
    [Min(1)] public int maxHealth = 100;

    [Header("Passive Repair")]
    public bool enablePassiveRepair = true;
    [Min(0f)] public float passiveRepairDelay = 5f;
    [Min(0f)] public float passiveRepairPerSecond = 5f;

    [Header("Income")]
    [Min(0)] public int incomePerTick = 0;

    [Header("Supply")]
    [Min(0)] public int supplyProvided = 0;

    [Header("Flags")]
    public bool isFortress = false;
    public bool isProductionBuilding = false;

    [Header("Fortress")]
    [Min(0)] public int fortressBaselineIncomePerTick = 0;

    [Header("Tiering")]
    public bool supportsTierUpgrades = true;
    [Min(1)] public int startingTier = 1;
    [Min(1)] public int maxTier = 3;

    [Header("Wall Placement")]
    public bool isWallSegment = false;
    [Min(0.1f)] public float wallSegmentLength = 5f;
    [Min(1f)] public float wallRotationStep = 90f;
}