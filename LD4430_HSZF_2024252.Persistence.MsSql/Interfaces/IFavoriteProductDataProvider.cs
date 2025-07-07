using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public interface IFavoriteProductDataProvider
    {
        void AddFavorite(int personId, int productId);
        void AddFavorite(FavoriteProduct favorite);
        IEnumerable<FavoriteProduct> GetAllFavorites();
        public IEnumerable<Person> GetPersonsByFavoriteProductId(int productId);
        void RemoveFavorite(int personId, int productId);
        void RemoveAllFavorites();
        bool Exists(int personId, int productId);
    }
}
