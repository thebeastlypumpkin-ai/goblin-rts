using UnityEngine;

public class TeamColorManager : MonoBehaviour
{
    public static TeamColorManager Instance { get; private set; }

    [Header("Team Colors")]
    [SerializeField] private Color friendlyColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color neutralColor = Color.yellow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Color GetColor(int teamId, int localTeamId)
    {
        if (teamId == (int)Team.Neutral)
            return neutralColor;

        if (SpectatorManager.Instance != null && SpectatorManager.Instance.IsSpectator)
        {
            // In spectator mode: show all teams as neutral for now
            return neutralColor;
        }

        if (teamId == localTeamId)
            return friendlyColor;

        return enemyColor;
    }
}