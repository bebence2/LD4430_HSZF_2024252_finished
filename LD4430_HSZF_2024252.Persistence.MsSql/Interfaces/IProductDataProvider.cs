using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public interface IProductDataProvider
    {
        IEnumerable<Product> GetAllProducts();
        Product? GetProductById(int productId);
        Product? GetProductByName(string productName);
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(int productId);
        void DeleteAll();
        bool Exists(int id);
        bool Exists(string Name);
    }
}
