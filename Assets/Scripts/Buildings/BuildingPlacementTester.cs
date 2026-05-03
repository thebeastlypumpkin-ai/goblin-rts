using UnityEngine;

public class BuildingPlacementTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingPlacementSystem placementSystem;
    [SerializeField] private BuildingDefinition barracksDefinition;
    [SerializeField] private BuildingDefinition beastPenDefinition;
    [SerializeField] private BuildingDefinition researchBuildingDefinition;
    [SerializeField] private BuildingDefinition essenceWellDefinition;
    [SerializeField] private BuildingDefinition towerADefinition;
    [SerializeField] private BuildingDefinition towerBDefinition;
    [SerializeField] private BuildingDefinition wallDefinition;



    void Awake()
    {
        if (placementSystem == null)
        {
            placementSystem = FindFirstObjectByType<BuildingPlacementSystem>();
        }
    }

    void Update()
    {
        if (placementSystem == null || barracksDefinition == null) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            placementSystem.StartPlacing(barracksDefinition);
            Debug.Log("Barracks placement started.");
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            placementSystem.StartPlacing(beastPenDefinition);
            Debug.Log("Beast Pen placement started.");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            placementSystem.StartPlacing(researchBuildingDefinition);
            Debug.Log("Research Building placement started.");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            placementSystem.StartPlacing(essenceWellDefinition);
            Debug.Log("Essence Well placement started.");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            placementSystem.StartPlacing(towerADefinition);
            Debug.Log("Tower A placement started.");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            placementSystem.CancelPlacement();
            Debug.Log("Placement canceled.");
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            placementSystem.StartPlacing(towerBDefinition);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            placementSystem.StartPlacing(wallDefinition);
        }


    }
}
