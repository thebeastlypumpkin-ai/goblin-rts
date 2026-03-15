using System.Collections.Generic;
using UnityEngine;

public class ProductionQueue : MonoBehaviour
{
    private Queue<UnitDefinition> unitQueue = new Queue<UnitDefinition>();

    private UnitDefinition currentUnit;

    private float productionTimer = 0f;

    [SerializeField] private UnitDefinition debugUnit;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (debugUnit != null)
            {
                EnqueueUnit(debugUnit);
            }
        }

        ProcessQueue();
    }

    public void EnqueueUnit(UnitDefinition unit)
    {
        unitQueue.Enqueue(unit);

        Debug.Log($"Unit added to queue: {unit.unitName}");
    }

    private void ProcessQueue()
    {
        if (currentUnit == null)
        {
            if (unitQueue.Count == 0)
                return;

            currentUnit = unitQueue.Dequeue();
            productionTimer = currentUnit.buildTime;

            Debug.Log($"Started training: {currentUnit.unitName}");
        }

        productionTimer -= Time.deltaTime;

        if (productionTimer <= 0f)
        {
            Debug.Log($"Finished training: {currentUnit.unitName}");

            currentUnit = null;
        }
    }
}