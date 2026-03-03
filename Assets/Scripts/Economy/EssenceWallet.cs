using System;

namespace GoblinRTS.Economy
{
    // Plain C# class (NOT a MonoBehaviour)
    public class EssenceWallet
    {
        public int Current { get; private set; }

        public event Action<int> OnChanged;

        public EssenceWallet(int startingAmount)
        {
            Current = Math.Max(0, startingAmount);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            Current += amount;
            OnChanged?.Invoke(Current);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (Current < amount) return false;

            Current -= amount;
            OnChanged?.Invoke(Current);
            return true;
        }

        public void ForceSet(int amount)
        {
            Current = Math.Max(0, amount);
            OnChanged?.Invoke(Current);
        }
    }
}