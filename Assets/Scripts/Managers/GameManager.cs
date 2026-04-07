using System.Collections.Generic;
using UnityEngine;
using GoblinRTS.Economy;

public enum GameState
{
    Boot,
    InGame,
    Paused
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: SerializeField]
    public GameState CurrentState { get; private set; } = GameState.Boot;

    [SerializeField] private GameConfig config;
    public GameConfig Config => config;

    [SerializeField] private int startingEssence = 0;

    private Dictionary<int, EssenceWallet> teamEssenceWallets = new Dictionary<int, EssenceWallet>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Duplicate detected -> destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (config == null)
        {
            Debug.LogWarning("[GameManager] No GameConfig assigned. Assign GameConfig_Default in the inspector.");
        }

        Debug.Log("[GameManager] Awake -> Singleton set, persists across scenes.");
    }

    private void Start()
    {
        SetState(GameState.InGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        var oldState = CurrentState;
        CurrentState = newState;

        Time.timeScale = (CurrentState == GameState.Paused) ? 0f : 1f;

        Debug.Log($"[GameManager] State: {oldState} -> {CurrentState}");
    }

    public void TogglePause()
    {
        SetState(CurrentState == GameState.Paused ? GameState.InGame : GameState.Paused);
    }

    public EssenceWallet GetTeamEssenceWallet(int teamId)
    {
        if (!teamEssenceWallets.ContainsKey(teamId))
        {
            EssenceWallet newWallet = new EssenceWallet(startingEssence);
            int capturedTeamId = teamId;

            newWallet.OnChanged += value =>
                Debug.Log($"[Economy] Team {capturedTeamId} Essence = {value}");

            teamEssenceWallets.Add(teamId, newWallet);

            Debug.Log($"[Economy] Created essence wallet for Team {teamId} with starting amount {startingEssence}");
        }

        return teamEssenceWallets[teamId];
    }

    public int GetTeamEssence(int teamId)
    {
        return GetTeamEssenceWallet(teamId).Current;
    }

    public void AddTeamEssence(int teamId, int amount)
    {
        GetTeamEssenceWallet(teamId).Add(amount);
    }

    public bool TrySpendTeamEssence(int teamId, int amount)
    {
        return GetTeamEssenceWallet(teamId).TrySpend(amount);
    }
}