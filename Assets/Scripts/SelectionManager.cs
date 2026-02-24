using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask groundLayerMask;

    private Unit currentSelectedUnit;

    void Awake()
    {
        // Auto-fill masks if you forget to set them in Inspector.
        if (unitLayerMask.value == 0)
            unitLayerMask = LayerMask.GetMask("Unit");

        if (groundLayerMask.value == 0)
            groundLayerMask = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        // Left click = select
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelectionClick();
        }

        // Right click = move command
        if (Input.GetMouseButtonDown(1))
        {
            HandleMoveClick();
        }
    }

    void HandleSelectionClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayerMask))
        {
            Unit unit = hit.collider.GetComponent<Unit>();
            if (unit != null && !unit.IsDead)
            {
                SelectUnit(unit);
                return;
            }
        }

        // Clicked not-a-unit => deselect
        DeselectCurrent();
    }
    void HandleMoveClick()
    {
        if (currentSelectedUnit == null)
            return;

        // ✅ NEW: If selected unit died, deselect it
        if (currentSelectedUnit.IsDead)
        {
            DeselectCurrent();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 1) Priority: right-click a unit to attack it
        if (Physics.Raycast(ray, out RaycastHit unitHit, 1000f, unitLayerMask))
        {
            Unit targetUnit = unitHit.collider.GetComponent<Unit>();

            // Don't allow targeting yourself
            if (targetUnit != null && targetUnit != currentSelectedUnit)
            {
                currentSelectedUnit.SetTarget(targetUnit);
                Debug.Log("Attack Target: " + targetUnit.name);
                return;
            }
        }

        // 2) Otherwise: right-click ground to move
        if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayerMask))
        {
            currentSelectedUnit.ClearTarget();
            currentSelectedUnit.MoveTo(groundHit.point);
            Debug.Log("Move Command to: " + groundHit.point);
        }
    }
    void SelectUnit(Unit unit)
    {
        if (currentSelectedUnit != null)
            currentSelectedUnit.SetSelected(false);

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