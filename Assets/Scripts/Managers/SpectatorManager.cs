using UnityEngine;

public class SpectatorManager : MonoBehaviour
{
    public static SpectatorManager Instance { get; private set; }

    [Header("Spectator Settings")]
    [SerializeField] private bool isSpectator = false;
    [SerializeField] private int localTeamId = 0;

    public bool IsSpectator => isSpectator;
    public int LocalTeamId => localTeamId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetSpectator(bool value)
    {
        isSpectator = value;
    }

    public void SetLocalTeam(int teamId)
    {
        localTeamId = teamId;
    }
}