using System;

namespace VendingMachineApp.Utilities
{
    public static class MoneyHelper
    {
        public static string FormatAmount(int rubles)
        {
            return $"{rubles} руб.";
        }
    }
}
