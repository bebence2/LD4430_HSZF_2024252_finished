using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class ProductEventArgs : EventArgs
    {
        public string PersonName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal CriticalLevel { get; set; }
    }

    public class CapacityFullEventArgs : EventArgs
    {
        public string PersonName { get; set; } = "";
        public string StorageUnitName { get; set; } = "";
    }
    public class CapacityCriticalEventArgs : EventArgs
    {
        public string PersonName { get; set; } = "";
        public string StorageUnitName { get; set; } = "";
    }

}
