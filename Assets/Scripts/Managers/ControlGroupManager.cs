using System.Collections.Generic;
using UnityEngine;

public class ControlGroupManager : MonoBehaviour
{
    public static ControlGroupManager Instance;

    private Dictionary<int, List<Unit>> controlGroups = new Dictionary<int, List<Unit>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AssignGroup(int groupNumber, List<Unit> units)
    {
        if (controlGroups.ContainsKey(groupNumber))
        {
            controlGroups[groupNumber] = new List<Unit>(units);
        }
        else
        {
            controlGroups.Add(groupNumber, new List<Unit>(units));
        }
    }

    public List<Unit> GetGroup(int groupNumber)
    {
        if (controlGroups.ContainsKey(groupNumber))
        {
            return controlGroups[groupNumber];
        }

        return null;
    }

    private void Update()
    {
        if (SelectionManager.Instance == null)
            return;

        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                List<Unit> selectedUnits = SelectionManager.Instance.GetSelectedUnits();

                if (selectedUnits != null && selectedUnits.Count > 0)
                {
                    AssignGroup(i, selectedUnits);
                    Debug.Log($"Assigned {selectedUnits.Count} unit(s) to group {i}");
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                List<Unit> groupUnits = GetGroup(i);

                if (groupUnits != null && groupUnits.Count > 0)
                {

                    SelectionManager.Instance.ClearSelection();

                    foreach (Unit unit in groupUnits)
                    {
                        if (unit != null && !unit.IsDead)
                        {
                            SelectionManager.Instance.AddUnitToSelection(unit);
                        }
                    }

                    Debug.Log($"Recalled group {i}");
                }
            }
        }
    }
}