using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private Unit unit;
    private Camera cam;

    public void Bind(Unit target)
    {
        unit = target;
        if (unit == null)
        {
            Destroy(gameObject);
            return;
        }

        cam = Camera.main;

        unit.OnHealthChanged += HandleHealthChanged;
        unit.OnDied += HandleDied;

        // Force initial draw
        HandleHealthChanged(unit.HealthNormalized);
    }

    private void HandleHealthChanged(float normalized)
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Mathf.Clamp01(normalized);

        // Always keep object active for now
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
    private void HandleDied(Unit deadUnit)
    {
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (unit == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = unit.transform.position + worldOffset;

        if (cam != null)
            transform.forward = cam.transform.forward;
    }

    private void OnDestroy()
    {
        if (unit != null)
        {
            unit.OnHealthChanged -= HandleHealthChanged;
            unit.OnDied -= HandleDied;
        }
    }
}