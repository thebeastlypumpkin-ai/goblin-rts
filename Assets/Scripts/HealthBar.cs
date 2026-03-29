using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private Unit unit;
    private Camera cam;
    private TeamMember teamMember;

    public void Bind(Unit target)
    {
        unit = target;
        teamMember = unit.GetComponent<TeamMember>();

        if (teamMember == null)
        {
            teamMember = unit.GetComponentInParent<TeamMember>();
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

        if (teamMember != null && TeamColorManager.Instance != null)
        {
            int localTeamId = (int)Team.Team1;
            int unitTeamId = (int)teamMember.Team;
            fillImage.color = TeamColorManager.Instance.GetColor(unitTeamId, localTeamId);
        }

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