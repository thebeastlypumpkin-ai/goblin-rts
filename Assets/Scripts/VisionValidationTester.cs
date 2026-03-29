using UnityEngine;

public class VisionValidationTester : MonoBehaviour
{
    [SerializeField] private VisionEmitter emitter;

    private void Reset()
    {
        emitter = GetComponent<VisionEmitter>();
    }

    private void OnDrawGizmosSelected()
    {
        if (emitter == null)
        {
            emitter = GetComponent<VisionEmitter>();
        }

        if (emitter == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, emitter.VisionRadius);
    }
}