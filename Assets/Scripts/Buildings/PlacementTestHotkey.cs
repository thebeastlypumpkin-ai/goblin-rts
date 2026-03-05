using UnityEngine;

public class PlacementTestHotkey : MonoBehaviour
{
    [SerializeField] private BuildingPlacementSystem placement;
    [SerializeField] private BuildingDefinition testDefinition;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            placement.StartPlacing(testDefinition);
        }
    }
}