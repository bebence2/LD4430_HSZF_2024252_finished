using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public interface IFavoriteProductService
    {
        void AddFavorite(int personId, int productId);
        void RemoveFavorite(int personId, int productId);
        public IEnumerable<Person> GetPersonsByFavoriteProductId(int productId); //Returns all persong who have the specified product (productId) as favorite
        void RemoveAllFavorites();
        bool Exists(int personId, int productId);
    }
}
