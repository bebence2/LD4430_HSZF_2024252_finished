using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public class CapacityDataProvider : ICapacityDataProvider
    {
        private readonly AppDbContext _context;

        public CapacityDataProvider(AppDbContext context)
        {
            _context = context;

            EnsureStorageUnitExists("Fridge");
            EnsureStorageUnitExists("Pantry");

            _context.SaveChanges();
        }

        //Create
        private void EnsureStorageUnitExists(string name)
        {
            if (!_context.StorageUnits.Any(s => s.Name == name))
            {
                _context.StorageUnits.Add(new StorageUnit { Name = name, Capacity = 0 });
            }
        }

        //Read
        public decimal GetCapacity(string storageUnitName)
        {
            return _context.StorageUnits.FirstOrDefault(s => s.Name == storageUnitName)?.Capacity ?? 0;
        }

        //Update
        public void SetCapacity(string storageUnitName, decimal capacity)
        {
            var unit = _context.StorageUnits.FirstOrDefault(s => s.Name == storageUnitName);
            if (unit != null)
            {
                unit.Capacity = capacity;
            }
            else
            {
                _context.StorageUnits.Add(new StorageUnit { Name = storageUnitName, Capacity = capacity });
            }
            _context.SaveChanges();
        }
    }
}
