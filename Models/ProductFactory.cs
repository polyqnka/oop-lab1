using System;
using System.Runtime.InteropServices;

namespace VendingMachineApp.Models
{
    public static class ProductFactory
    {
        private static int _nextId = 0;

        public static Product CreateProduct(string name, ProductCategory category, int price, int quantity)
        {
            var product = new Product(name, category, price, quantity)
            {
                Id = _nextId++
            };
            return product;
        }
    }
}
