using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class HouseholdJsonModel
    {
        public FridgePantryJsonModel Fridge { get; set; } = new();
        public FridgePantryJsonModel Pantry { get; set; } = new();
        public List<PersonJsonModel> Persons { get; set; } = new();
        public List<ProductJsonModel> Products { get; set; } = new();
    }

    public class FridgePantryJsonModel
    {
        public decimal Capacity { get; set; }

        public List<int> ProductIds { get; set; } = new();
    }

    public class PersonJsonModel
    {
        public string Name { get; set; } = string.Empty;

        public List<int> FavoriteProductIds { get; set; } = new();

        public bool ResponsibleForPurchase { get; set; }
    }

    public class ProductJsonModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal CriticalLevel { get; set; }
        public DateTime BestBefore { get; set; }
        public bool StoreInFridge { get; set; }
    }
}
