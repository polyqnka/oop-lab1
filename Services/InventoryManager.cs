using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using VendingMachineApp.Models;

namespace VendingMachineApp.Services
{
    public class InventoryManager
    {
        private readonly List<Product> _products = new List<Product>();

        public IEnumerable<Product> GetAll() => _products.ToList();

        public void AddProduct(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);
            _products.Add(product);
        }

        public void AddProduct(string name, ProductCategory category, int price, int quantity)
        {
            var product = ProductFactory.CreateProduct(name, category, price, quantity);
            AddProduct(product);
        }

        public Product? GetById(int id) => _products.FirstOrDefault(x => x.Id == id);

        public bool TryDecrementQuantity(int id)
        {
            var product = GetById(id);
            if (product == null) return false;
            if (product.Quantity <= 0) return false;
            product.Quantity--;
            return true;
        }

        public void Restock(int id, int quantity)
        {

            var product = GetById(id) ?? throw new InvalidOperationException("Product not found");
            ArgumentOutOfRangeException.ThrowIfNegative(quantity);
            product.Quantity += quantity;
        }

        public void ShowAll()
        {
            foreach (var product in _products)
                Console.WriteLine(product.ToString());
        }
    }
}
