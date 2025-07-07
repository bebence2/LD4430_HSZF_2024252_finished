using LD4430_HSZF_2024252.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Persistence.MsSql
{
    public class ProductDataProvider : IProductDataProvider
    {
        private readonly AppDbContext _context;

        public ProductDataProvider(AppDbContext context)
        {
            _context = context;
        }

        //Create
        public void AddProduct(Product product)
        {
            try
            {
                Product product1 = new Product
                {
                    Name = product.Name,
                    Quantity = product.Quantity,
                    StoreInFridge = product.StoreInFridge,
                    BestBefore = product.BestBefore,
                    CriticalLevel = product.CriticalLevel
                };
                _context.Products.Add(product1);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hiba történt a termék mentésekor:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner: " + ex.InnerException.Message);
                }

                throw;
            }
        }
        //Read
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products.ToList();
        }

        public Product? GetProductById(int productId)
        {
            return _context.Products.FirstOrDefault(p => p.Id == productId);
        }

        public Product? GetProductByName(string productName)
        {
            return _context.Products.FirstOrDefault(p => p.Name == productName);
        }

        //Update
        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }
        //Delete
        public void DeleteProduct(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

        public void DeleteAll()
        {
            var allProducts = _context.Products.ToList();
            _context.Products.RemoveRange(allProducts);
            _context.SaveChanges();
        }




        public bool Exists(int id) => _context.Products.Any(p => p.Id == id);
        public bool Exists(string name) => _context.Products.Any(p => p.Name == name);
    }

}
