using TMPro;
using UnityEngine;

namespace GoblinRTS.Economy
{
    public class EssenceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI essenceText;

        private void Start()
        {
            if (GameManager.Instance == null || GameManager.Instance.Essence == null)
                return;

            // Subscribe to wallet changes
            GameManager.Instance.Essence.OnChanged += UpdateText;

            // Initialize immediately
            UpdateText(GameManager.Instance.Essence.Current);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null || GameManager.Instance.Essence == null)
                return;

            GameManager.Instance.Essence.OnChanged -= UpdateText;
        }

        private void UpdateText(int value)
        {
            essenceText.text = $"Essence: {value}";
        }
    }
}