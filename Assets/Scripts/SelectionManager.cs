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
            if (unit != null)
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
        // Only issue move if we have a selected unit
        if (currentSelectedUnit == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayerMask))
        {
            Vector3 destination = hit.point;

            // We'll implement MoveTo in Step 5
            currentSelectedUnit.MoveTo(destination);
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