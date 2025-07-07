using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class StorageUnitFullException : Exception
    {
        public StorageUnitFullException(string message) : base(message) { }
    }
}
