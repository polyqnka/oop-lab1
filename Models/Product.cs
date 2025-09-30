using System;
using System.Diagnostics.Contracts;

namespace VendingMachineApp.Models
{
    public enum ProductCategory
    {
        Drink,
        Snack,
        Other
    }

    public class Product
    {
        public int Id { get; internal set; }
        public string Name { get; }
        public ProductCategory Category { get; }
        public int Price { get; }
        public int Quantity { get; internal set; }

        public Product(string name, ProductCategory category, int price, int quantity)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Category = category;
            ArgumentOutOfRangeException.ThrowIfNegative(price);
            Price = price;
            ArgumentOutOfRangeException.ThrowIfNegative(quantity);
            Quantity = quantity;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} ({Category}) - {Price} руб. - В наличии: {Quantity}";
        }
    }
}
