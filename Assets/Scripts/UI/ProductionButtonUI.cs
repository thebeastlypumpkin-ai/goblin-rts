using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductionButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;

    private ProductionQueue productionQueue;
    private UnitDefinition unitToProduce;

    public void Setup(ProductionQueue queue, UnitDefinition unit)
    {
        productionQueue = queue;
        unitToProduce = unit;

        if (labelText != null)
        {
            labelText.text = unitToProduce != null ? unitToProduce.unitName : "Missing";
        }
    }

    public void OnClick()
    {
        if (productionQueue != null && unitToProduce != null)
        {
            productionQueue.EnqueueUnit(unitToProduce);
        }
    }
}