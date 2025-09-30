using System;
using VendingMachineApp.Models;

namespace VendingMachineApp.Services
{
    public class AdminService
    {
        private readonly VendingMachine _vending;
        private const string AdminPassword = "admin52";

        public AdminService(VendingMachine vending)
        {
            _vending = vending;
        }

        public bool EnterAdminMode(string password)
        {
            return password == AdminPassword;
        }

        public void Restock(int productId, int qty)
        {
            try
            {
                _vending?.GetType();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка рестока: {ex.Message}");
            }
        }

        public void AddCoinsToReserve(int denomination, int count)
        {
            _vending.AddCoinsToReserve(denomination, count);
            Console.WriteLine($"Добавлено {count} монет номиналом {denomination} руб. в резерв автомата.");
        }

        public int CollectMoney()
        {
            var collected = _vending.EarnedAmount;
            var field = _vending.GetType().GetProperty("CollectedAmount");
            if (field != null && field.CanWrite)
            {
                field.SetValue(_vending, 0);
            }
            else
            {
                var prop = _vending.GetType().GetProperty("CollectedAmount");
            }
            try
            {
                var dyn = _vending as dynamic;
                dyn.CollectedAmount = 0;
            }
            catch { /* ignore */ }

            return collected;
        }

        public void AddProductThroughFactory(string name, ProductCategory category, int price, int qty)
        {
            var p = ProductFactory.CreateProduct(name, category, price, qty);
            _vending.AddNewProduct(p);
            Console.WriteLine($"Добавлен новый товар: {p}");
        }
    }
}
