using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;
    [Header("Layers")]
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask buildingLayerMask;
    [Header("Group Move Spacing")]
    [SerializeField] private float formationSpacing = 1.5f;   // distance between units
    [SerializeField] private int formationColumns = 4;        // grid width

    private List<Unit> selectedUnits = new List<Unit>();
    private Building selectedBuilding;
    public Building SelectedBuilding => selectedBuilding;

    private void PruneSelection()
    {
        selectedUnits.RemoveAll(u => u == null || u.IsDead);
    }

    void Awake()
    {
        Instance = this;

        if (unitLayerMask.value == 0)
            unitLayerMask = LayerMask.GetMask("Unit");

        if (groundLayerMask.value == 0)
            groundLayerMask = LayerMask.GetMask("Ground");

        if (buildingLayerMask.value == 0)
            buildingLayerMask = LayerMask.GetMask("Building");
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

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

        if (Physics.Raycast(ray, out RaycastHit buildingHit, 1000f, buildingLayerMask))
        {
            Building building = buildingHit.collider.GetComponentInParent<Building>();
            if (building != null)
            {
                ClearSelection();
                selectedBuilding = building;
                Debug.Log("Selected building: " + building.name);
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayerMask))
        {
            Unit unit = hit.collider.GetComponentInParent<Unit>();
            if (unit != null && !unit.IsDead)
            {
                selectedBuilding = null;

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
        if (selectedBuilding != null)
        {
            Ray buildingRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(buildingRay, out RaycastHit buildingGroundHit, 1000f, groundLayerMask))
            {
                selectedBuilding.SetRallyPoint(buildingGroundHit.point);
                Debug.Log("Rally point set for: " + selectedBuilding.name);
            }

            return;
        }

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
        selectedBuilding = null;
        ClearSelection();
        selectedUnits.Add(unit);
        unit.SetSelected(true);
        Debug.Log("Selected: " + unit.name);
    }

    void DeselectCurrent()
    {
        bool hadAnythingSelected = selectedUnits.Count > 0 || selectedBuilding != null;

        selectedBuilding = null;
        ClearSelection();

        if (hadAnythingSelected)
        {
            Debug.Log("Deselected");
        }
    }
}