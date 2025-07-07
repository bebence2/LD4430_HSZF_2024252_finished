using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public interface IProductService
    {
        public delegate void ProductCriticalEventHandler(object sender, ProductEventArgs e);
        public event ProductCriticalEventHandler? OnProductCritical;

        IEnumerable<Product> GetAllProducts();
        Product? GetProductById(int productId);
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(int productId);
        void DeleteAll();
        bool Exists(string productName);
        bool Exists(int productId);
        IEnumerable<Product> GetCriticalProducts(); //Returns products below critical level
        IEnumerable<Product> GetExpiringProducts(); //Returns products that are expiring within the next 7 days
    }
}
