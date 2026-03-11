using UnityEngine;

public class BuildingPlacementTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingPlacementSystem placementSystem;
    [SerializeField] private BuildingDefinition barracksDefinition;
    [SerializeField] private BuildingDefinition beastPenDefinition;
    [SerializeField] private BuildingDefinition researchBuildingDefinition;

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

        // Press B to start placing Barracks
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

        // Press Escape to cancel placement
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            placementSystem.CancelPlacement();
            Debug.Log("Placement canceled.");
        }
    }
}