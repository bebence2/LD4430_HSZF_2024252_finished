using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public interface ICapacityDataProvider
    {
        decimal GetCapacity(string storageUnitName);
        void SetCapacity(string storageUnitName, decimal capacity);
    }
}
