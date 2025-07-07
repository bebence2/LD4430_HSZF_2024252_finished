using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public class ContextManagementProvider : IContextManagementProvider
    {
        private readonly AppDbContext _context;

        public ContextManagementProvider(AppDbContext context)
        {
            _context = context;
        }

        public void ClearChangeTracker()
        {
            _context.ChangeTracker.Clear();
        }
    }
}
