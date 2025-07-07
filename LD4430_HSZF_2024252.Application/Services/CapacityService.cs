using LD4430_HSZF_2024252.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class CapacityService : ICapacityService
    {
        private readonly ICapacityDataProvider _capacityDataProvider;
        private readonly IProductDataProvider _productDataProvider;

        public CapacityService(ICapacityDataProvider capacityDataProvider, IProductDataProvider productDataProvider)
        {
            _capacityDataProvider = capacityDataProvider;
            _productDataProvider = productDataProvider;
        }

        public event CapacityFullEventHandler? OnCapacityFull;
        public event CapacityCriticalEventHandler? OnCapacityCritical;

        // Gets the current fridge capacity, calculates the total quantity of products stored in the fridge.
        public decimal GetFridgeCapacity()
        {
            TriggerCapacityEvents("Hűtő", _productDataProvider.GetAllProducts()
                .Where(p => p.StoreInFridge)
                .Sum(p => p.Quantity), _capacityDataProvider.GetCapacity("Fridge"));
            return _capacityDataProvider.GetCapacity("Fridge");
        }
        public void CheckCapacity(string storageUnit, decimal currentLoad)
        {
            var maxCapacity = _capacityDataProvider.GetCapacity(storageUnit);
            TriggerCapacityEvents(storageUnit, currentLoad, maxCapacity); // Checks if the current load exceeds the capacity and triggers events accordingly
        }

        // Gets the current pantry capacity, calculates the total quantity of products stored in the pantry.
        public decimal GetPantryCapacity()
        {
            TriggerCapacityEvents("Kamra", _productDataProvider.GetAllProducts()
                .Where(p => !p.StoreInFridge)
                .Sum(p => p.Quantity), _capacityDataProvider.GetCapacity("Pantry"));
            return _capacityDataProvider.GetCapacity("Pantry");
        }

        public void SetFridgeCapacity(decimal capacity)
        {
            _capacityDataProvider.SetCapacity("Fridge", capacity);
        }

        public void SetPantryCapacity(decimal capacity)
        {
            _capacityDataProvider.SetCapacity("Pantry", capacity);
        }

        // Triggers capacity events based on the current load and maximum capacity
        private void TriggerCapacityEvents(string storageUnitName, decimal currentLoad, decimal maxCapacity)
        {
            if (maxCapacity == 0)
            {
                return;
            }
            var loadRatio = currentLoad / maxCapacity;

            if (loadRatio >= 1.0m)
            {
                OnCapacityFull?.Invoke(this, new CapacityFullEventArgs
                {
                    StorageUnitName = storageUnitName
                });
            }
            else if (loadRatio >= 0.9m)
            {
                OnCapacityCritical?.Invoke(this, new CapacityEventArgs
                {
                    StorageUnitName = storageUnitName
                });
            }
        }

        //Checks if the new total exceeds the capacity, throws event if capacity becomes critifal or full 
        public void CheckAndRaiseCapacityEvents(bool isFridge, decimal newTotal)
        {
            string unitName = isFridge ? "Fridge" : "Pantry";
            string unitDisplayName = isFridge ? "hűtő" : "kamra";
            decimal capacity = _capacityDataProvider.GetCapacity(unitName);

            if (newTotal >= capacity)
            {
                OnCapacityFull?.Invoke(this, new CapacityFullEventArgs
                {
                    StorageUnitName = unitDisplayName
                });
            }
            else if (newTotal >= capacity * 0.9m)
            {
                OnCapacityCritical?.Invoke(this, new CapacityEventArgs
                {
                    StorageUnitName = unitDisplayName
                });
            }
        }

        public bool IsFridgeFull()
        {
            var currentLoad = _productDataProvider.GetAllProducts()
                .Where(p => p.StoreInFridge)
                .Sum(p => p.Quantity);

            return currentLoad >= _capacityDataProvider.GetCapacity("Fridge");
        }
        public bool IsPantryFull()
        {
            var currentLoad = _productDataProvider.GetAllProducts()
                .Where(p => !p.StoreInFridge)
                .Sum(p => p.Quantity);

            return currentLoad >= _capacityDataProvider.GetCapacity("Pantry");
        }
    }
}
