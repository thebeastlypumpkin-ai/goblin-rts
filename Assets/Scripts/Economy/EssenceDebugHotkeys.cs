using UnityEngine;

namespace GoblinRTS.Economy
{
    public class EssenceDebugHotkeys : MonoBehaviour
    {
        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Essence == null) return;

            // Press "=" to add (shift makes it "+", but KeyCode is still Equals)
            if (Input.GetKeyDown(KeyCode.Equals))
                GameManager.Instance.Essence.Add(10);

            // Press "-" to spend
            if (Input.GetKeyDown(KeyCode.Minus))
                GameManager.Instance.Essence.TrySpend(5);
        }
    }
}