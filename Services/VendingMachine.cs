using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using VendingMachineApp.Models;
using VendingMachineApp.Utilities;

namespace VendingMachineApp.Services
{
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public Dictionary<int, int> Change { get; set; } = new Dictionary<int, int>();
    }

    public class VendingMachine
    {
        private readonly InventoryManager _inventory;

        private readonly Dictionary<int, int> _coinReserve = new Dictionary<int, int>();

        private readonly Dictionary<int, int> _insertedCoins = new Dictionary<int, int>();

        public int EarnedAmount { get; set; } = 0;

        public VendingMachine(InventoryManager inventory)
        {
            _inventory = inventory;
            foreach (var denomination in Models.Coin.Denominations)
            {
                _coinReserve[denomination] = 0;
                _insertedCoins[denomination] = 0;
            }
        }

        public int CurrentInsertedAmount => _insertedCoins.Sum(kv => kv.Key * kv.Value);

        public void SeedCoinReserve(Dictionary<int, int> seed)
        {
            foreach (var kv in seed)
            {
                if (!Models.Coin.IsSupported(kv.Key)) continue;
                _coinReserve[kv.Key] = _coinReserve.ContainsKey(kv.Key) ? _coinReserve[kv.Key] + kv.Value : kv.Value;
            }
        }

        public void ShowCoinReserve()
        {
            Console.WriteLine("Резерв монет в автомате:");
            foreach (var denomination in Models.Coin.Denominations)
            {
                Console.WriteLine($"\t{MoneyHelper.FormatAmount(denomination)} : {_coinReserve.GetValueOrDefault(denomination, 0)} шт.");
            }
        }

        public void ShowProducts()
        {
            Console.WriteLine("Список товаров:");
            _inventory.ShowAll();
        }

        public bool InsertCoin(int denomination)
        {
            if (!Models.Coin.IsSupported(denomination)) return false;
            _insertedCoins[denomination] = _insertedCoins.GetValueOrDefault(denomination) + 1;
            return true;
        }

        public PurchaseResult Purchase(int productId)
        {
            var product = _inventory.GetById(productId);
            if (product == null) return new PurchaseResult { Success = false, Message = "Товар не найден" };
                        if (product.Quantity <= 0) return new PurchaseResult { Success = false, Message = "Товар закончился." };

            var inserted = CurrentInsertedAmount;
            if (inserted < product.Price) return new PurchaseResult { Success = false, Message = $"Недостаточно средств. Цена: {MoneyHelper.FormatAmount(product.Price)}, вставлено: {MoneyHelper.FormatAmount(inserted)}" };

            int changeNeeded = inserted - product.Price;

            var tempReserve = new Dictionary<int, int>(_coinReserve);
            foreach (var kv in _insertedCoins)
            {
                tempReserve[kv.Key] = tempReserve.GetValueOrDefault(kv.Key, 0) + kv.Value;
            }

            var change = TryMakeChange(changeNeeded, tempReserve);
            if (change == null)
            {
                return new PurchaseResult { Success = false, Message = "Не удаётся выдать сдачу. Операция отменена. Верните монеты." };
            }

            bool decremented = _inventory.TryDecrementQuantity(productId);
            if (!decremented) return new PurchaseResult { Success = false, Message = "Не удалось снять товар (возможно закончился)." };

            foreach (var kv in _insertedCoins)
                _coinReserve[kv.Key] = _coinReserve.GetValueOrDefault(kv.Key, 0) + kv.Value;

            foreach (var kv in change)
            {
                _coinReserve[kv.Key] = _coinReserve.GetValueOrDefault(kv.Key, 0) - kv.Value;
            }

            EarnedAmount += product.Price;

            foreach (var d in Models.Coin.Denominations) _insertedCoins[d] = 0;

            return new PurchaseResult { Success = true, Message = $"Вы получили: {product.Name}. Спасибо!", Change = change };
        }

        private Dictionary<int, int>? TryMakeChange(int amount, Dictionary<int, int> coinReserve)
        {
            if (amount == 0) return new Dictionary<int, int>();

            var result = new Dictionary<int, int>();
            foreach (var denom in Models.Coin.Denominations)
            {
                if (amount <= 0) break;
                int canUse = Math.Min(amount / denom, coinReserve.GetValueOrDefault(denom, 0));
                if (canUse > 0)
                {
                    result[denom] = canUse;
                    amount -= denom * canUse;
                }
            }

            if (amount != 0) return null;
            return result;
        }

        public Dictionary<int, int> CancelAndReturn()
        {
            var ret = new Dictionary<int, int>();
            foreach (var kv in _insertedCoins)
            {
                if (kv.Value > 0) ret[kv.Key] = kv.Value;
            }

            foreach (var d in Models.Coin.Denominations) _insertedCoins[d] = 0;
            return ret;
        }

        public void AddCoinsToReserve(int denomination, int count)
        {
            if (!Models.Coin.IsSupported(denomination)) throw new InvalidOperationException("Неподдерживаемый номинал");
            _coinReserve[denomination] = _coinReserve.GetValueOrDefault(denomination, 0) + count;
        }

        public void AddNewProduct(Product p) => _inventory.AddProduct(p);
    }
}
