using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public class CleanupService : ICleanupService
    {
        private readonly AppDbContext _context;

        public CleanupService(AppDbContext context)
        {
            _context = context;
        }

        public void DeleteAll()
        {
            var favorites = _context.FavoriteProducts.ToList();
            var persons = _context.Person.ToList();
            var products = _context.Products.ToList();

            _context.FavoriteProducts.RemoveRange(favorites);
            _context.Person.RemoveRange(persons);
            _context.Products.RemoveRange(products);

            _context.SaveChanges();
        }
    }
}
