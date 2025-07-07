using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public interface ICapacityService
    {

        event CapacityFullEventHandler? OnCapacityFull; //Event for full capacity level
        event CapacityCriticalEventHandler? OnCapacityCritical; //Event for critical capacity level
        void CheckCapacity(string storageUnit, decimal currentLoad);
        decimal GetFridgeCapacity();
        decimal GetPantryCapacity();
        void SetFridgeCapacity(decimal capacity);
        void SetPantryCapacity(decimal capacity);
        void CheckAndRaiseCapacityEvents(bool isFridge, decimal newTotal); //Checks if the new total exceeds the capacity, throws event if capacity becomes critifal or full  
    }
}
