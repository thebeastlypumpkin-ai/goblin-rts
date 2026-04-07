using UnityEngine;

namespace GoblinRTS.Economy
{
    public class EssenceDebugHotkeys : MonoBehaviour
    {
        private void Update()
        {
            if (GameManager.Instance == null) return;

            // Press "=" to add 10 Essence to Team 0
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                GameManager.Instance.AddTeamEssence(0, 10);
            }

            // Press "-" to spend 5 Essence from Team 0
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                GameManager.Instance.TrySpendTeamEssence(0, 5);
            }
        }
    }
}