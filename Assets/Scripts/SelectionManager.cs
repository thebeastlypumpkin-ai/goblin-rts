using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private Unit currentSelectedUnit;

    void Update()
    {
        // Left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }

    void HandleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Only hit objects on the Unit layer
        if (Physics.Raycast(ray, out hit))
        {
            Unit unit = hit.collider.GetComponent<Unit>();

            if (unit != null)
            {
                SelectUnit(unit);
                return;
            }
        }

        // If we click anything else, deselect
        DeselectCurrent();
    }

    void SelectUnit(Unit unit)
    {
        if (currentSelectedUnit != null)
        {
            currentSelectedUnit.SetSelected(false);
        }

        currentSelectedUnit = unit;
        currentSelectedUnit.SetSelected(true);

        Debug.Log("Selected: " + unit.name);
    }

    void DeselectCurrent()
    {
        if (currentSelectedUnit != null)
        {
            currentSelectedUnit.SetSelected(false);
            currentSelectedUnit = null;
            Debug.Log("Deselected");
        }
    }
}