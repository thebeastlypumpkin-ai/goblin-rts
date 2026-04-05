using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;

    [Header("Prefab")]
    public GameObject unitPrefab;

    [Header("Production")]
    public BuildingType producedBy;

    [Header("Costs")]
    public int essenceCost;

    [Header("Build Time")]
    public float buildTime;

    [Header("Supply")]
    public int supplyCost;

    [Header("UI")]
    public Sprite icon;

    [Header("Squad Settings")]
    public int visualSquadSize = 5;

    [Header("Tags")]
    public UnitTag primaryTag = UnitTag.None;
}