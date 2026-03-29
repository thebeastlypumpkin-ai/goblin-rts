using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(TeamMember))]
public class TeamColorApplier : MonoBehaviour
{
    private Renderer rend;
    private TeamMember teamMember;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        teamMember = GetComponent<TeamMember>();
    }

    private void Start()
    {
        ApplyColor();
    }

    public void ApplyColor()
    {
        if (TeamColorManager.Instance == null) return;
        if (SpectatorManager.Instance == null) return;

        int teamId = (int)teamMember.Team;
        int localTeamId = SpectatorManager.Instance.LocalTeamId;

        Color color = TeamColorManager.Instance.GetColor(teamId, localTeamId);

        rend.material.color = color;
    }
}