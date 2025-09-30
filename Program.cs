using System;
using VendingMachineApp.Models;
using VendingMachineApp.Services;
using VendingMachineApp.Utilities;

namespace VendingMachineApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var inventory = new InventoryManager();
            inventory.AddProduct(ProductFactory.CreateProduct("Coke", ProductCategory.Drink, 60, 5));
            inventory.AddProduct(ProductFactory.CreateProduct("Pepsi", ProductCategory.Drink, 55, 3));
            inventory.AddProduct(ProductFactory.CreateProduct("Snickers", ProductCategory.Snack, 45, 10));
            inventory.AddProduct(ProductFactory.CreateProduct("Water", ProductCategory.Drink, 30, 10));
            inventory.AddProduct(ProductFactory.CreateProduct("Chips", ProductCategory.Snack, 70, 4));

            var vending = new VendingMachine(inventory);
            vending.SeedCoinReserve(new System.Collections.Generic.Dictionary<int, int>
            {
                [100] = 2,
                [50] = 4,
                [10] = 10,
                [5] = 10,
                [2] = 10,
                [1] = 20
            });

            var admin = new AdminService(vending);

            Console.WriteLine("=== Вендинговый автомат — Лабораторная работа ===");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1) Показать товары");
                Console.WriteLine("2) Вставить монету/банкноту");
                Console.WriteLine("3) Выбрать товар");
                Console.WriteLine("4) Отмена / Вернуть монеты");
                Console.WriteLine("5) Баланс вставленных средств");
                Console.WriteLine("6) Админ режим");
                Console.WriteLine("0) Выйти");

                Console.Write("Ввод: ");
                var key = Console.ReadLine()?.Trim();

                switch (key)
                {
                    case "1":
                        vending.ShowProducts();
                        break;
                    case "2":
                        Console.WriteLine("Доступные номиналы: 100, 50, 10, 5, 2, 1 (рублей). Введите номинал:");
                        if (int.TryParse(Console.ReadLine(), out int denom))
                        {
                            if (vending.InsertCoin(denom))
                            {
                                Console.WriteLine($"Вставлено {MoneyHelper.FormatAmount(denom)}. Текущий баланс: {MoneyHelper.FormatAmount(vending.CurrentInsertedAmount)}");
                            }
                            else
                            {
                                Console.WriteLine("Неподдерживаемый номинал или произошла ошибка.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неверный ввод.");
                        }
                        break;
                    case "3":
                        vending.ShowProducts();
                        Console.Write("Введите ID товара для покупки: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            var result = vending.Purchase(id);
                            Console.WriteLine(result.Message);
                            if (result.Change != null && result.Change.Count > 0)
                            {
                                Console.WriteLine("Выдача сдачи:");
                                foreach (var kv in result.Change)
                                    Console.WriteLine($"  {MoneyHelper.FormatAmount(kv.Key)} x {kv.Value}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неверный ввод ID.");
                        }
                        break;
                    case "4":
                        var returned = vending.CancelAndReturn();
                        if (returned.Count == 0) Console.WriteLine("Нечего возвращать.");
                        else
                        {
                            Console.WriteLine("Возвращены монеты:");
                            foreach (var kv in returned) Console.WriteLine($"  {MoneyHelper.FormatAmount(kv.Key)} x {kv.Value}");
                        }
                        break;
                    case "5":
                        Console.WriteLine($"Вставлено: {MoneyHelper.FormatAmount(vending.CurrentInsertedAmount)}");
                        break;
                    case "6":
                        Console.Write("Введите админ-пароль: ");
                        var pwd = Console.ReadLine();
                        if (admin.EnterAdminMode(pwd))
                        {
                            Console.WriteLine("=== ADMIN MODE ===");
                            bool adminExit = false;
                            while (!adminExit)
                            {
                                Console.WriteLine();
                                Console.WriteLine("a) Посмотреть инвентарь и монеты");
                                Console.WriteLine("b) Пополнить товар (id, количество)");
                                Console.WriteLine("c) Добавить новый товар");
                                Console.WriteLine("d) Добавить монеты в резерв");
                                Console.WriteLine("e) Забрать собранные средства");
                                Console.WriteLine("x) Выйти из админки");
                                Console.Write("Выбор: ");
                                var a = Console.ReadLine()?.Trim();
                                switch (a)
                                {
                                    case "a":
                                        vending.ShowProducts();
                                        vending.ShowCoinReserve();
                                        Console.WriteLine($"Собрано средств (на вывод): {MoneyHelper.FormatAmount(vending.EarnedAmount)}");
                                        break;
                                    case "b":
                                        Console.Write("id товара: ");
                                        if (!int.TryParse(Console.ReadLine(), out int pid)) { Console.WriteLine("Неверно."); break; }
                                        Console.Write("количество для добавления: ");
                                        if (!int.TryParse(Console.ReadLine(), out int qty)) { Console.WriteLine("Неверно."); break; }
                                        admin.Restock(pid, qty);
                                        break;
                                    case "c":
                                        Console.Write("Название: ");
                                        var name = Console.ReadLine();
                                        ArgumentOutOfRangeException.ThrowIfNullOrEmpty(name);
                                        Console.Write("Категория (Drink/Snack/Other): ");
                                        var cat = Console.ReadLine();
                                        Console.Write("Цена (рубли): ");
                                        if (!int.TryParse(Console.ReadLine(), out int price)) { Console.WriteLine("Неверная цена."); break; }
                                        Console.Write("Количество: ");
                                        if (!int.TryParse(Console.ReadLine(), out int q2)) { Console.WriteLine("Неверно."); break; }
                                        var category = ProductCategory.Other;
                                        if (string.Equals(cat, "Drink", StringComparison.OrdinalIgnoreCase)) category = ProductCategory.Drink;
                                        if (string.Equals(cat, "Snack", StringComparison.OrdinalIgnoreCase)) category = ProductCategory.Snack;
                                        admin.AddProductThroughFactory(name, category, price, q2);
                                        break;
                                    case "d":
                                        Console.WriteLine("Введите номинал для добавления:");
                                        if (!int.TryParse(Console.ReadLine(), out int den)) { Console.WriteLine("Неверно."); break; }
                                        Console.WriteLine("Введите количество:");
                                        if (!int.TryParse(Console.ReadLine(), out int count)) { Console.WriteLine("Неверно."); break; }
                                        admin.AddCoinsToReserve(den, count);
                                        break;
                                    case "e":
                                        var taken = admin.CollectMoney();
                                        Console.WriteLine($"Вы собрались: {MoneyHelper.FormatAmount(taken)} (и сброшены с баланса автомата)");
                                        break;
                                    case "x":
                                        adminExit = true;
                                        break;
                                    default:
                                        Console.WriteLine("Неизвестная команда.");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неверный пароль.");
                        }
                        break;
                    case "0":
                        Console.WriteLine("Выход. До свидания!");
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
        }
    }
}
