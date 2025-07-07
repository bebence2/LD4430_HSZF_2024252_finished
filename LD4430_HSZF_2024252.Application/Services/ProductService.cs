using LD4430_HSZF_2024252.Model;
using LD4430_HSZF_2024252.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class ProductService : IProductService
    {
        public event CapacityCriticalEventHandler? OnCapacityCritical;
        public event CapacityFullEventHandler? OnCapacityFull;
        public event IProductService.ProductCriticalEventHandler? OnProductCritical;

        private readonly IProductDataProvider _dataProvider;
        private readonly IFavoriteProductDataProvider _favoriteProductDataProvider;
        private readonly ICapacityService _capacityService;

        public ProductService(
            IProductDataProvider dataProvider,
            IFavoriteProductDataProvider favoriteProductDataProvider,
            ICapacityService capacityService)
        {
            _dataProvider = dataProvider;
            _favoriteProductDataProvider = favoriteProductDataProvider;
            _capacityService = capacityService;

            _capacityService.OnCapacityCritical += (s, e) => OnCapacityCritical?.Invoke(this, e);
            _capacityService.OnCapacityFull += (s, e) => OnCapacityFull?.Invoke(this, e);
        }

        public void AddProduct(Product product)
        {
            decimal usedCapacity = _dataProvider
                .GetAllProducts()
                .Where(p => p.StoreInFridge == product.StoreInFridge)
                .Sum(p => p.Quantity);

            decimal capacity = product.StoreInFridge
                ? _capacityService.GetFridgeCapacity()
                : _capacityService.GetPantryCapacity();

            if (usedCapacity + product.Quantity > capacity)
            {
                throw new StorageUnitFullException($"Nem lehet hozzáadni a terméket: nincs elég hely a {(product.StoreInFridge ? "hűtőben" : "kamrában")}.");
            }

            _dataProvider.AddProduct(product);

            _capacityService.CheckAndRaiseCapacityEvents(product.StoreInFridge, usedCapacity + product.Quantity);
        }

        public void UpdateProduct(Product product)
        {
            if (!_dataProvider.Exists(product.Id))
                throw new InvalidOperationException($"Product with ID {product.Id} not found.");

            _dataProvider.UpdateProduct(product);

            if (product.Quantity <= product.CriticalLevel)
            {
                var persons = _favoriteProductDataProvider.GetPersonsByFavoriteProductId(product.Id);

                foreach (var person in persons)
                {
                    OnProductCritical?.Invoke(this, new ProductEventArgs
                    {
                        PersonName = person.Name,
                        ProductName = product.Name,
                        CriticalLevel = product.CriticalLevel
                    });
                }
            }
        }

        public void DeleteProduct(int productId)
        {
            if (!_dataProvider.Exists(productId))
                throw new InvalidOperationException($"Product with ID {productId} not found.");

            _dataProvider.DeleteProduct(productId);
        }

        public void DeleteAll() => _dataProvider.DeleteAll();

        public IEnumerable<Product> GetAllProducts() => _dataProvider.GetAllProducts();

        public IEnumerable<Product> GetCriticalProducts()
        {
            return _dataProvider
                .GetAllProducts()
                .Where(p => p.Quantity <= p.CriticalLevel);
        }

        public IEnumerable<Product> GetExpiringProducts()
        {
            return _dataProvider.GetAllProducts()
                .Where(p => p.BestBefore < DateTime.Now.Date.AddDays(7));
        }

        public Product? GetProductById(int productId) => _dataProvider.GetProductById(productId);

        public bool Exists(int id) => _dataProvider.Exists(id);
        public bool Exists(string name) => _dataProvider.Exists(name);
    }

}
