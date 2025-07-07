using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public class CapacityEventArgs : EventArgs
    {
        public string StorageUnitName { get; set; } = "";
        public decimal RemainingCapacity { get; set; }
    }
}
