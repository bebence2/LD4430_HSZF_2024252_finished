using LD4430_HSZF_2024252.Model;
using LD4430_HSZF_2024252.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class FavoriteProductService : IFavoriteProductService
    {
        private readonly IFavoriteProductDataProvider _dataProvider;
        private readonly IProductDataProvider _productDataProvider;
        private readonly IPersonDataProvider _personDataProvider;

        public FavoriteProductService(IFavoriteProductDataProvider dataProvider, IProductDataProvider productDataProvider, IPersonDataProvider personDataProvider)
        {
            _dataProvider = dataProvider;
            _productDataProvider = productDataProvider;
            _personDataProvider = personDataProvider;
        }

        public void AddFavorite(int personId, int productId)
        {
            if (_dataProvider.Exists(personId, productId))
                throw new InvalidOperationException("Ez a kedvenc már létezik."); //Prevents duplicate favorites

            _dataProvider.AddFavorite(personId, productId);
        }

        public void RemoveFavorite(int personId, int productId)
        {
            _dataProvider.RemoveFavorite(personId, productId);
        }
        public void RemoveAllFavorites()
        {
            _dataProvider.RemoveAllFavorites();
        }

        public bool Exists(int personId, int productId)
        {
            return _dataProvider.Exists(personId, productId);
        }

        //Returns all persong who have the specified product (productId) as favorite
        public IEnumerable<Person> GetPersonsByFavoriteProductId(int productId)
        {
            return _dataProvider.GetPersonsByFavoriteProductId(productId);
        }
    }
}
