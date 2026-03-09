using UnityEngine;

namespace GoblinRTS.Economy
{
    public class SupplyManager : MonoBehaviour
    {
        public static SupplyManager Instance { get; private set; }

        [Header("Runtime Supply")]
        [SerializeField] private int currentSupplyUsed = 0;
        [SerializeField] private int maxSupply = 0;

        public int CurrentSupplyUsed => currentSupplyUsed;
        public int MaxSupply => maxSupply;
        public int FreeSupply => Mathf.Max(0, maxSupply - currentSupplyUsed);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[Supply] Duplicate SupplyManager detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetMaxSupply(int newMaxSupply)
        {
            maxSupply = Mathf.Max(0, newMaxSupply);
        }

        public bool HasEnoughSupply(int amountNeeded)
        {
            if (amountNeeded <= 0)
                return true;

            return currentSupplyUsed + amountNeeded <= maxSupply;
        }

        public bool TryConsumeSupply(int amount)
        {
            if (amount <= 0)
                return true;

            if (!HasEnoughSupply(amount))
                return false;

            currentSupplyUsed += amount;
            return true;
        }

        public void ReleaseSupply(int amount)
        {
            if (amount <= 0)
                return;

            currentSupplyUsed -= amount;

            if (currentSupplyUsed < 0)
            {
                currentSupplyUsed = 0;
            }
        }

        public void ResetSupplyState()
        {
            currentSupplyUsed = 0;
            maxSupply = 0;
        }
    }
}