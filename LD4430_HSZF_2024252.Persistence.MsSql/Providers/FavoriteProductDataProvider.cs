using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public class FavoriteProductDataProvider : IFavoriteProductDataProvider
    {
        private readonly AppDbContext _context;

        public FavoriteProductDataProvider(AppDbContext context)
        {
            _context = context;
        }

        //Create
        public void AddFavorite(int personId, int productId)
        {
            var favorite = new FavoriteProduct
            {
                personId = personId,
                productId = productId
            };

            _context.FavoriteProducts.Add(favorite);
            _context.SaveChanges();
        }
        public void AddFavorite(FavoriteProduct favorite)
        {
            _context.FavoriteProducts.Add(favorite);
            _context.SaveChanges();
        }
        //Read
        public IEnumerable<FavoriteProduct> GetAllFavorites()
        {
            using var context = new AppDbContext();
            return context.FavoriteProducts.ToList();
        }
        public IEnumerable<Person> GetPersonsByFavoriteProductId(int productId)
        {
            return _context.FavoriteProducts
                .Where(fp => fp.productId == productId)
                .Join(_context.Person,
                      fav => fav.personId,
                      person => person.Id,
                      (fav, person) => person)
                .Distinct()
                .ToList();
        }

        //No update method needed based on business logic (noone changes their favorite products)

        //Delete
        public void RemoveFavorite(int personId, int productId)
        {
            var fav = _context.FavoriteProducts.FirstOrDefault(f => f.personId == personId && f.productId == productId);
            if (fav != null)
            {
                _context.FavoriteProducts.Remove(fav);
                _context.SaveChanges();
            }
        }
        public void RemoveAllFavorites()
        {
            _context.FavoriteProducts.RemoveRange(_context.FavoriteProducts);
            _context.SaveChanges();
        }

        public bool Exists(int personId, int productId)
        {
            return _context.FavoriteProducts.Any(f => f.personId == personId && f.productId == productId);
        }
    }
}
