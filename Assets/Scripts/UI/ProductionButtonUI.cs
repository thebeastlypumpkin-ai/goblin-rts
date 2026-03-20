using UnityEngine;
using UnityEngine.UI;

public class ProductionButtonUI : MonoBehaviour
{
    private ProductionQueue productionQueue;
    private UnitDefinition unitToProduce;

    public void Setup(ProductionQueue queue, UnitDefinition unit)
    {
        productionQueue = queue;
        unitToProduce = unit;
    }

    public void OnClick()
    {
        if (productionQueue != null && unitToProduce != null)
        {
            productionQueue.EnqueueUnit(unitToProduce);
        }
    }
}