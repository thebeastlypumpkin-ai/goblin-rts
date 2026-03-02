using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [Header("Group Move Spacing")]
    [SerializeField] private float formationSpacing = 1.5f;   // distance between units
    [SerializeField] private int formationColumns = 4;        // grid width

    private List<Unit> selectedUnits = new List<Unit>();

    private void PruneSelection()
    {
        selectedUnits.RemoveAll(u => u == null || u.IsDead);
    }

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
            Unit unit = hit.collider.GetComponentInParent<Unit>();
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
        PruneSelection();

        if (selectedUnits.Count == 0)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Right-click unit = attack
        if (Physics.Raycast(ray, out RaycastHit unitHit, 1000f, unitLayerMask))
        {
            Unit targetUnit = unitHit.collider.GetComponentInParent<Unit>();
            if (targetUnit != null && !targetUnit.IsDead)
            {
                foreach (var u in selectedUnits)
                    if (u != null && u != targetUnit && !u.IsDead)
                        u.CommandAttack(targetUnit);
                int acceptedCount = 0;

                foreach (var u in selectedUnits)
                {
                    if (u == null || u.IsDead || u == targetUnit) continue;
                    if (u.CommandAttack(targetUnit)) acceptedCount++;
                }

                Debug.Log($"Attack Target: {targetUnit.name} (accepted by {acceptedCount}/{selectedUnits.Count})");
            }
        }
        if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayerMask))
        {
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit u = selectedUnits[i];
                if (u == null || u.IsDead) continue;

                int row = i / formationColumns;
                int column = i % formationColumns;

                float formationWidth = formationColumns * formationSpacing;
                float formationHeight = Mathf.Ceil((float)selectedUnits.Count / formationColumns) * formationSpacing;

                float offsetX = column * formationSpacing - formationWidth / 2f + formationSpacing / 2f;
                float offsetZ = row * formationSpacing - formationHeight / 2f + formationSpacing / 2f;

                Vector3 offset = new Vector3(offsetX, 0, offsetZ);

                Vector3 targetPosition = groundHit.point + offset;

                u.CommandMoveTo(targetPosition);
            }

            Debug.Log("Formation Move Command issued.");
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