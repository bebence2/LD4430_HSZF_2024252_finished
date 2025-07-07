using LD4430_HSZF_2024252.Model;
using LD4430_HSZF_2024252.Persistence.MsSql;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Application
{
    public class ImportExportService : IImportExportService
    {
        private readonly IProductService _productService;
        private readonly IPersonService _personService;
        private readonly ICapacityService _capacityService;
        private readonly IFavoriteProductDataProvider _favoriteProductDataProvider;
        private readonly IContextManagementProvider _contextManagementProvider;

        public ImportExportService(
            IProductService productService,
            IPersonService personService,
            ICapacityService capacityService,
            IFavoriteProductDataProvider favoriteProductDataProvider,
            IContextManagementProvider contextManagementProvider)
        {
            _productService = productService;
            _personService = personService;
            _capacityService = capacityService;
            _favoriteProductDataProvider = favoriteProductDataProvider;
            _contextManagementProvider = contextManagementProvider;
        }

        //Imports household data from a JSON file and updates the database accordingly.
        public void ImportJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            // Deserialize the JSON into the HouseholdJsonModel
            var data = JsonConvert.DeserializeObject<HouseholdJsonModel>(json);
            if (data == null) throw new Exception("Érvénytelen JSON."); // Handle invalid JSON

            //Creating dictionary out of the products (id to name).
            var originalIdToName = new Dictionary<int, string>();

            foreach (var prod in data.Products)
            {
                originalIdToName[prod.Id] = prod.Name;
                // Check if the product already exists in the database
                if (!_productService.Exists(prod.Name))
                {
                    _productService.AddProduct(new Product
                    {
                        Name = prod.Name,
                        Quantity = prod.Quantity,
                        CriticalLevel = prod.CriticalLevel,
                        BestBefore = prod.BestBefore,
                        StoreInFridge = prod.StoreInFridge
                    });
                }
            }

            // Creating dictionary out of the products in the database
            var dbProducts = _productService.GetAllProducts();
            var nameToDbProduct = dbProducts.ToDictionary(p => p.Name, p => p);

            decimal fridgeCapacity = data.Fridge.Capacity;
            decimal pantryCapacity = data.Pantry.Capacity;

            // Set the capacities in the service (according to the JSON data)
            _capacityService.SetFridgeCapacity(fridgeCapacity);
            _capacityService.SetPantryCapacity(pantryCapacity);

            // Update the fridge products in the database based on the JSON data
            foreach (int prodId in data.Fridge.ProductIds)
            {
                if (originalIdToName.TryGetValue(prodId, out var name) &&
                    nameToDbProduct.TryGetValue(name, out var product))
                {
                    product.StoreInFridge = true;
                }
            }
            //Clearing the change tracker of EF core, to avoid errors caused by tracking the same entities multiple times.
            _contextManagementProvider.ClearChangeTracker();
            // Update the pantry products in the database based on the JSON data
            foreach (int prodId in data.Pantry.ProductIds)
            {
                if (originalIdToName.TryGetValue(prodId, out var name) &&
                    nameToDbProduct.TryGetValue(name, out var product))
                {
                    product.StoreInFridge = false;
                }
            }
            //Update all products in the database
            foreach (var product in nameToDbProduct.Values)
            {
                _productService.UpdateProduct(product);
            }

            // Importing people from JSON
            foreach (var person in data.Persons)
            {
                // Check if the person already exists in the database
                if (!_personService.Exists(person.Name))
                {
                    var newPerson = new Person
                    {
                        Name = person.Name,
                        ResponsibleForPurchase = person.ResponsibleForPurchase
                    };

                    _personService.AddPerson(newPerson);
                }
                // Creating a dictionary to map person names to their IDs, to store their favorite products
                var addedPerson = _personService.GetAllPerson()
                    .FirstOrDefault(p => p.Name == person.Name);

                if (addedPerson != null)
                {
                    foreach (int favProdId in person.FavoriteProductIds)
                    {
                        if (originalIdToName.TryGetValue(favProdId, out var favProdName) &&
                            nameToDbProduct.TryGetValue(favProdName, out var favProduct))
                        {


                            _favoriteProductDataProvider.AddFavorite(new FavoriteProduct
                            {
                                personId = addedPerson.Id,
                                productId = favProduct.Id
                            });
                        }
                    }
                }
            }


        }

        //Exports the current household data to a JSON file.
        public void ExportJson(string filePath)
        {
            //Collecting all the data from the services
            var products = _productService.GetAllProducts();
            var persons = _personService.GetAllPerson();
            var favorites = _favoriteProductDataProvider.GetAllFavorites();
            var fridge = _capacityService.GetFridgeCapacity();
            var pantry = _capacityService.GetPantryCapacity();
            //Creating a list of products with the JSON model format
            var productJsonList = products.Select(p => new ProductJsonModel
            {
                Id = p.Id,
                Name = p.Name,
                Quantity = p.Quantity,
                CriticalLevel = p.CriticalLevel,
                BestBefore = p.BestBefore,
                StoreInFridge = p.StoreInFridge
            }).ToList();
            //Creating a list of people with the favorites, for the right JSON model format
            var personsWithFavorites = persons.Select(person => new PersonJsonModel
            {
                Name = person.Name,
                ResponsibleForPurchase = person.ResponsibleForPurchase,
                FavoriteProductIds = favorites
                    .Where(f => f.personId == person.Id)
                    .Select(f => f.productId)
                    .ToList()
            }).ToList();
            //Arranging the data into the JSON model format
            var exportData = new HouseholdJsonModel
            {
                Products = productJsonList,
                Persons = personsWithFavorites,
                Fridge = new FridgePantryJsonModel { Capacity = fridge },
                Pantry = new FridgePantryJsonModel { Capacity = pantry }
            };
            //Serializing the data to JSON and writing in to the specified file path
            string json = JsonConvert.SerializeObject(exportData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // Exports the current household data to a text file in a human-readable format.
        public void ExportTxt(string filePath)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== Termékek ===");
            foreach (var p in _productService.GetAllProducts())
            {
                sb.AppendLine($"- {p.Name} | Mennyiség: {p.Quantity} | Kritikus: {p.CriticalLevel} | Lejár: {p.BestBefore:yyyy-MM-dd} | Hűtőben: {(p.StoreInFridge ? "Igen" : "Nem")}");
            }

            sb.AppendLine();
            sb.AppendLine("=== Személyek és kedvenceik ===");

            var persons = _personService.GetAllPerson();
            var favorites = _favoriteProductDataProvider.GetAllFavorites();
            foreach (var person in persons)
            {
                sb.AppendLine($"- {person.Name} (Vásárlásért felelős: {(person.ResponsibleForPurchase ? "Igen" : "Nem")})");
                var favs = favorites
                    .Where(f => f.personId == person.Id)
                    .Select(f => f.productId);
                foreach (var favId in favs)
                {
                    var product = _productService.GetAllProducts().FirstOrDefault(p => p.Id == favId);
                    if (product != null)
                        sb.AppendLine($"   * Kedvenc: {product.Name}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("=== Kapacitás ===");
            sb.AppendLine($"Hűtő kapacitása: {_capacityService.GetFridgeCapacity()}");
            sb.AppendLine($"Kamra kapacitása: {_capacityService.GetPantryCapacity()}");

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
