using TMPro;
using UnityEngine;

public class MatchEndUI : MonoBehaviour
{
    public static MatchEndUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text resultText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowVictory()
    {
        if (panel != null)
            panel.SetActive(true);

        if (resultText != null)
            resultText.text = "VICTORY";
    }

    public void ShowDefeat()
    {
        if (panel != null)
            panel.SetActive(true);

        if (resultText != null)
            resultText.text = "DEFEAT";
    }
}