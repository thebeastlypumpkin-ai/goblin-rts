using UnityEngine;

public class ProductionUIManager : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private ProductionButtonUI button;

    private void Start()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (SelectionManager.Instance == null)
            return;

        Building selectedBuilding = SelectionManager.Instance.SelectedBuilding;

        if (selectedBuilding == null)
        {
            panel.SetActive(false);
            return;
        }

        ProductionQueue queue = selectedBuilding.GetComponent<ProductionQueue>();

        if (queue == null || queue.AvailableUnitsCount() == 0)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);

        UnitDefinition unit = queue.GetAvailableUnit(0);
        button.Setup(queue, unit);
    }
}