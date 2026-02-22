using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [SerializeField] private GameConfig config;

    private void Awake()
    {
        if (config == null && GameManager.Instance != null)
        {
            config = GameManager.Instance.Config;
        }

        if (config == null)
        {
            Debug.LogWarning("[RTSCameraController] No GameConfig assigned.");
        }
    }

    private void Update()
    {
        float speed = (config != null) ? config.cameraPanSpeed : 15f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0f, z).normalized * speed * Time.unscaledDeltaTime;

        transform.position += move;
    }
}