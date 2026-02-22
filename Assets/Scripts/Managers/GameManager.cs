using UnityEngine;

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

    private void Awake()
    {
        // Enforce singleton
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
        // Temporary debug hotkey: P toggles pause.
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

        // Minimal pause behavior
        Time.timeScale = (CurrentState == GameState.Paused) ? 0f : 1f;

        Debug.Log($"[GameManager] State: {oldState} -> {CurrentState}");
    }

    public void TogglePause()
    {
        SetState(CurrentState == GameState.Paused ? GameState.InGame : GameState.Paused);
    }
}