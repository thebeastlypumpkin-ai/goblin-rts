using TMPro;
using UnityEngine;

namespace GoblinRTS.Economy
{
    public class EssenceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI essenceText;

        private void Update()
        {
            if (essenceText == null)
                return;

            if (GameManager.Instance == null)
            {
                essenceText.text = "Essence: 0";
                return;
            }

            int currentEssence = GameManager.Instance.GetTeamEssence(0);
            essenceText.text = $"Essence: {currentEssence}";
        }
    }
}