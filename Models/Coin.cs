using System;

namespace VendingMachineApp.Models
{
    public static class Coin
    {
        public static readonly int[] Denominations = [100, 50, 10, 5, 2, 1];

        public static bool IsSupported(int value)
        {
            foreach (var denomination in Denominations)
                if (denomination == value) return true;
            return false;
        }
    }
}