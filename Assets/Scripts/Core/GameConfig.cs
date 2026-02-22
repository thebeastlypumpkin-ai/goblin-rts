using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "RTS/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("World Scale")]
    public float worldUnitsPerMeter = 1f;

    [Header("Camera (Phase 6+)")]
    public float cameraPanSpeed = 15f;
    public float cameraZoomSpeed = 200f;
    public float cameraMinHeight = 10f;
    public float cameraMaxHeight = 60f;

    [Header("Units (Later Phases)")]
    public float defaultUnitMoveSpeed = 5f;
}