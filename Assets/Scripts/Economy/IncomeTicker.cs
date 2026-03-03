using UnityEngine;

namespace GoblinRTS.Economy
{
    public class IncomeTicker : MonoBehaviour
    {
        [SerializeField] private int incomePerTick = 5;
        [SerializeField] private float tickIntervalSeconds = 2f;

        private float timer;

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.InGame) return;

            timer += Time.deltaTime;

            if (timer >= tickIntervalSeconds)
            {
                timer = 0f;
                GameManager.Instance.Essence.Add(incomePerTick);
            }
        }
    }
}