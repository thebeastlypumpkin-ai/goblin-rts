using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask groundLayerMask;

    private List<Unit> selectedUnits = new List<Unit>();

    void Awake()
    {
        if (unitLayerMask.value == 0)
            unitLayerMask = LayerMask.GetMask("Unit");

        if (groundLayerMask.value == 0)
            groundLayerMask = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleSelectionClick();

        if (Input.GetMouseButtonDown(1))
            HandleMoveClick();
    }

    private Unit GetPrimarySelected()
    {
        return (selectedUnits.Count > 0) ? selectedUnits[0] : null;
    }

    private void ClearSelection()
    {
        foreach (var u in selectedUnits)
            if (u != null) u.SetSelected(false);

        selectedUnits.Clear();
    }

    void HandleSelectionClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayerMask))
        {
            Unit unit = hit.collider.GetComponent<Unit>();
            if (unit != null && !unit.IsDead)
            {
                bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (shiftHeld)
                    ToggleUnitSelection(unit);
                else
                    SelectUnit(unit);

                return;
            }
        }

        DeselectCurrent();
    }

    void ToggleUnitSelection(Unit unit)
    {
        if (selectedUnits.Contains(unit))
        {
            unit.SetSelected(false);
            selectedUnits.Remove(unit);
            Debug.Log("Removed from selection: " + unit.name);
        }
        else
        {
            selectedUnits.Add(unit);
            unit.SetSelected(true);
            Debug.Log("Added to selection: " + unit.name);
        }
    }

    void HandleMoveClick()
    {
        Unit primary = GetPrimarySelected();
        if (primary == null) return;

        if (primary.IsDead)
        {
            DeselectCurrent();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Right-click unit = attack
        if (Physics.Raycast(ray, out RaycastHit unitHit, 1000f, unitLayerMask))
        {
            Unit targetUnit = unitHit.collider.GetComponent<Unit>();
            if (targetUnit != null && !targetUnit.IsDead)
            {
                foreach (var u in selectedUnits)
                    if (u != null && u != targetUnit && !u.IsDead)
                        u.CommandAttack(targetUnit);

                Debug.Log("Attack Target: " + targetUnit.name);
                return;
            }
        }

        // Right-click ground = move
        if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayerMask))
        {
            foreach (var u in selectedUnits)
                if (u != null && !u.IsDead)
                    u.CommandMoveTo(groundHit.point);

            Debug.Log("Move Command to: " + groundHit.point);
        }
    }

    void SelectUnit(Unit unit)
    {
        ClearSelection();
        selectedUnits.Add(unit);
        unit.SetSelected(true);
        Debug.Log("Selected: " + unit.name);
    }

    void DeselectCurrent()
    {
        if (selectedUnits.Count > 0)
        {
            ClearSelection();
            Debug.Log("Deselected");
        }
    }
}