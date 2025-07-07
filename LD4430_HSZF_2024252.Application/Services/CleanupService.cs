using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class CleanupService : ICleanupService
    {
        private readonly LD4430_HSZF_2024252.Persistence.MsSql.ICleanupService _dataProvider;

        public CleanupService(LD4430_HSZF_2024252.Persistence.MsSql.ICleanupService dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void DeleteAll() => _dataProvider.DeleteAll();
    }
}
