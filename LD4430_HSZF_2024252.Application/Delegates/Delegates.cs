using LD4430_HSZF_2024252.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public delegate void CapacityFullEventHandler(object sender, CapacityFullEventArgs e);
    public delegate void CapacityCriticalEventHandler(object sender, CapacityEventArgs e);
}
