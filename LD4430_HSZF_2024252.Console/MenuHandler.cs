using ConsoleTools;
using LD4430_HSZF_2024252.Application;
using LD4430_HSZF_2024252.Model;
using LD4430_HSZF_2024252.Persistence.MsSql;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;


public class MenuHandler
{
    public void ShowDatabaseMenu()
    {
        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.WriteItemAction = item => Console.Write("{0}", item.Name);
                config.WriteHeaderAction = () => Console.WriteLine("Válassz egy opciót:");
            })
            .Add("Import / Export", ShowImportExportMenu)
            .Add("Adatbázis ürítése", () => DatabaseCleanup())
            .Add("Hűtő/Kamra statisztika", () => ShowStorageStatistics())
            .Add("Hűtő/Kamra kapacitás beállítása", () => SetStorageCapacities())
            .Add("Vissza", ConsoleMenu.Close);

        menu.Show();
    }


    public void SetStorageCapacities()
    {
        Console.Clear();
        Console.WriteLine("=== Hűtő és Kamra Kapacitás Beállítása ===\n");

        Console.Write($"Új hűtő kapacitás: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal newFridge))
        {
            _capacityProvider.SetFridgeCapacity(newFridge);
            Console.WriteLine($"Hűtő kapacitás frissítve: {newFridge}");
        }
        else
        {
            Console.WriteLine("Érvénytelen érték, nem történt módosítás.");
        }

        Console.Write($"\nÚj kamra kapacitás:");
        if (decimal.TryParse(Console.ReadLine(), out decimal newPantry))
        {
            _capacityProvider.SetPantryCapacity(newPantry);
            Console.WriteLine($"Kamra kapacitás frissítve: {newPantry}");
        }
        else
        {
            Console.WriteLine("Érvénytelen érték, nem történt módosítás.");
        }

        Console.WriteLine("\nNyomj meg egy gombot a visszatéréshez...");
        Console.ReadKey();
    }

    // This method displays the current usage of fridge and pantry storage, along with their capacities.
    public void ShowStorageStatistics()
    {
        var products = _productService.GetAllProducts();
        var fridgeUsed = products
            .Where(p => p.StoreInFridge)
            .Sum(p => p.Quantity);

        var pantryUsed = products
            .Where(p => !p.StoreInFridge)
            .Sum(p => p.Quantity);

        var fridgeCapacity = _capacityProvider.GetFridgeCapacity();
        var pantryCapacity = _capacityProvider.GetPantryCapacity();

        Console.Clear();
        Console.WriteLine("=== Hűtő és Kamra Statisztika ===\n");

        Console.WriteLine($"Hűtő: {fridgeUsed} / {fridgeCapacity}");
        _capacityProvider.CheckCapacity("Fridge", fridgeUsed);
        Console.WriteLine($"Kamra: {pantryUsed} / {pantryCapacity}");
        _capacityProvider.CheckCapacity("Pantry", pantryUsed);

        Console.WriteLine("\nNyomj meg egy gombot a folytatáshoz...");

        Console.ReadKey();
    }

    //This method displays all the people (who has favorite products) and all their favorite products.
    private void ShowFavoritesMenu()
    {
        int pageSize = 10;
        int currentPage = 0;
        bool showMenu = true;

        while (showMenu)
        {
            var allPersons = _personService.GetAllPerson().OrderBy(p => p.Name).ToList();
            var allProducts = _productService.GetAllProducts().ToList();

            var favorites = new List<(int PersonId, string PersonName, int ProductId, string ProductName)>();

            foreach (var person in allPersons)
            {
                var favoriteProductIds = allProducts
                    .Where(p => _favoriteService.Exists(person.Id, p.Id))
                    .Select(p => p.Id)
                    .ToList();

                foreach (var productId in favoriteProductIds)
                {
                    var product = allProducts.FirstOrDefault(p => p.Id == productId);
                    if (product != null)
                    {
                        favorites.Add((person.Id, person.Name, product.Id, product.Name));
                    }
                }
            }

            int totalPages = (int)Math.Ceiling(favorites.Count / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (currentPage >= totalPages) currentPage = totalPages - 1;

            Console.Clear();
            Console.WriteLine($"Kedvencek – oldal {currentPage + 1}/{totalPages}");

            var menu = new ConsoleMenu()
                .Configure(config =>
                {
                    config.WriteItemAction = item => Console.Write("{0}", item.Name);
                    config.WriteHeaderAction = () => Console.WriteLine("Név – Kedvenc termék");
                    config.EnableFilter = true;
                });

            var pageItems = favorites
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var fav in pageItems)
            {
                menu.Add($"{fav.PersonName} – {fav.ProductName}", () =>
                {
                    ShowFavoriteProductDetailsMenu(_productService.GetProductById(fav.ProductId), _personService.GetPersonById(fav.PersonId));
                });

            }

            if (currentPage > 0)
                menu.Add("<< Előző oldal", () => currentPage--);

            if (currentPage < totalPages - 1)
                menu.Add("Következő oldal >>", () => currentPage++);

            menu.Add("Kedvenc felvétele", () => AddFavorite());

            menu.Add("Vissza", () => showMenu = false);

            menu.CloseMenu();
            menu.Show();
        }
    }


    //A submenu for adding new favorites to people
    private void AddFavorite()
    {
        Console.Clear();
        Console.WriteLine("Új kedvenc termék felvétele");

        Console.Write("Személy neve: ");
        string personName = Console.ReadLine()??"null".Trim();

        Console.Write("Termék neve: ");
        string productName = Console.ReadLine() ?? "null".Trim();

        var person = _personService.GetAllPerson()
            .FirstOrDefault(p => p.Name.Equals(personName, StringComparison.OrdinalIgnoreCase));

        if (person == null)
        {
            Console.WriteLine("Nincs ilyen nevű személy.");
            Console.ReadKey();
            return;
        }

        var product = _productService.GetAllProducts()
            .FirstOrDefault(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));

        if (product == null)
        {
            Console.WriteLine("Nincs ilyen nevű termék.");
            Console.ReadKey();
            return;
        }

        if (_favoriteService.Exists(person.Id, product.Id))
        {
            Console.WriteLine("Ez a kedvenc már létezik.");
        }
        else
        {
            _favoriteService.AddFavorite(person.Id, product.Id);
            Console.WriteLine("Kedvenc sikeresen felvéve.");
        }

        Console.WriteLine("Nyomj meg egy gombot a folytatáshoz...");
        Console.ReadKey();
    }
    //A submenu for showing critical products, which are below their critical level.
    private void ShowCriticalProductsMenu()
    {
        var criticalProducts = _productService.GetCriticalProducts();
        if (!criticalProducts.Any())
        {
            Console.WriteLine("Nincs kritikus szint alatti termék.");
            Console.ReadKey();
            return;
        }

        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.Selector = ">> ";
                config.EnableFilter = false;
                config.Title = "Kritikus szint alatti termékek";
            });

        foreach (var product in criticalProducts)
        {
            menu.Add($"{product.Name} - Mennyiség: {product.Quantity}/ Kritikus szint: {product.CriticalLevel}", () => ShowProductDetailsMenu(product));
        }

        menu.Add("Vissza", ConsoleMenu.Close);
        menu.Show();
    }


    //A filler method to pause the console and wait for user input.
    private void Pause()
    {
        Console.WriteLine("\nNyomj meg egy gombot a folytatáshoz...");
        Console.ReadKey();
    }

    



    






    private readonly IProductService _productService;
    private readonly IPersonService _personService;
    private readonly IFavoriteProductService _favoriteService;
    private readonly LD4430_HSZF_2024252.Application.ICleanupService _cleanupService;
    private readonly ICapacityService _capacityProvider;
    private readonly IImportExportService _importExportService;
    private readonly IContextManagementProvider _contextManagementProvider;
    public void MainMenu()
    {
        
        var menu = new ConsoleMenu()
                .Configure(config =>
                {
                    config.WriteItemAction = item => Console.Write("{0}", item.Name);
                    config.WriteHeaderAction = () => Console.WriteLine("Válassz egy opciót:");
                })
                .Add("Termékek kezelése", ShowProductMenu)
                .Add("Személyek kezelése", ShowPersonMenu)
                .Add("Kedvenc termékek kezelése", ShowFavoritesMenu)
                .Add("Adatbázis műveletek", ShowDatabaseMenu)
                .Add("Kilépés", ConsoleMenu.Close);

        menu.Show();
    }
    public MenuHandler(IProductService productService, 
        IPersonService personService, 
        LD4430_HSZF_2024252.Application.ICleanupService cleanupService, 
        IFavoriteProductService favoriteProductService, 
        ICapacityService capacityProvider,
        IImportExportService importExportService,
        IContextManagementProvider contextManagementProvider)
    {
        _productService = productService;
        _personService = personService;
        _cleanupService = cleanupService;
        _favoriteService = favoriteProductService;
        _capacityProvider = capacityProvider;
        _importExportService = importExportService;
        _contextManagementProvider = contextManagementProvider;
        _productService.OnProductCritical += (sender, e) =>
        {
            Console.WriteLine($"[{e.PersonName}] {e.ProductName} termék a kritikus szint ({e.CriticalLevel}) alá csökkent!");
            Console.WriteLine("Nyomj meg egy gombot a folytatáshoz...");
            Console.ReadKey();
        };
        _capacityProvider.OnCapacityFull += (sender, e) =>
        {
            Console.WriteLine($"{e.PersonName} A {e.StorageUnitName} megtelt.");
            Console.WriteLine("Nyomj meg egy gombot a folytatáshoz...");
            Console.ReadKey();
        };
        _capacityProvider.OnCapacityCritical += (sender, e) =>
        {
            Console.WriteLine($"A {e.StorageUnitName} hamarosan megtelik.");
        };
    }
    //Shows product menu, to add, list and manage products.
    public void ShowProductMenu()
    {
        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.WriteItemAction = item => Console.Write("{0}", item.Name);
                config.WriteHeaderAction = () => Console.WriteLine("Válassz egy opciót:");
            })
            .Add("Termék hozzáadása", () => ShowProductAddMenu())
            .Add("Termékek listázása", () => ShowPagedProductList())
            .Add("Kritikus mennyiségek", () => ShowCriticalProductsMenu())
            .Add("Közeli lejáratú termékek", () => ShowExpiringProducts())
            .Add("Vissza", ConsoleMenu.Close);

        menu.Show();
    }

    //Showing product that are about to expire
    public void ShowExpiringProducts()
    {
        var criticalProducts = _productService.GetExpiringProducts();
        if (!criticalProducts.Any())
        {
            Console.WriteLine("Nincs közeli lejáratú termék.");
            Console.ReadKey();
            return;
        }

        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.Selector = ">> ";
                config.EnableFilter = false;
                config.Title = "Közeli lejáratú termékek";
            });

        foreach (var product in criticalProducts)
        {
            menu.Add($"{product.Name} - Mennyiség: {product.Quantity}/ Lejárat: {product.BestBefore.Date}", () => ShowProductDetailsMenu(product));
        }

        menu.Add("Vissza", ConsoleMenu.Close);
        menu.Show();
    }

    //Shows all the people, with an option to add new ones.
    public void ShowPersonMenu()
    {
        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.WriteItemAction = item => Console.Write("{0}", item.Name);
                config.WriteHeaderAction = () => Console.WriteLine("Válassz egy opciót:");
            })
            .Add("Személyek listázása", () => ShowPagedPersonList())
            .Add("Személy hozzáadása", () => ShowPersonAddMenu())
            .Add("Vissza", ConsoleMenu.Close);

        menu.Show();
    }



    //Submenu for adding new products, checking for valid inputs.
    private void ShowProductAddMenu()
    {
        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.WriteItemAction = item => Console.Write("{0}", item.Name);
                config.WriteHeaderAction = () => Console.WriteLine("Válassz egy opciót:");
            });

        menu.Add("Új termék hozzáadása", () =>
        {
            Console.Clear();

            Console.Write("Terméknév: ");
            string name = Console.ReadLine()??"null";

            decimal? quantity = null;
            while (quantity == null)
            {
                Console.Write("Mennyiség: ");
                string input = Console.ReadLine()??"null";
                if (decimal.TryParse(input, out decimal amount) && amount >= 0)
                    quantity = amount;
                else
                    Console.WriteLine("Adj meg egy 0 vagy nagyobb számot!");
            }

            decimal? criticalLevel = null;
            while (criticalLevel == null)
            {
                Console.Write("Kritikus mennyiség: ");
                string input = Console.ReadLine() ?? "null";
                if (decimal.TryParse(input, out decimal level) && level >= 0)
                    criticalLevel = level;
                else
                    Console.WriteLine("Adj meg egy 0 vagy nagyobb számot!");
            }

            DateTime? bestBefore = null;
            while (bestBefore == null)
            {
                Console.Write("Lejárat dátuma (ÉÉÉÉ-HH-NN): ");
                string input = Console.ReadLine() ?? "null";
                if (DateTime.TryParse(input, out DateTime date))
                    bestBefore = date;
                else
                    Console.WriteLine("Hibás dátumformátum.");
            }

            bool? storeInFridge = null;
            while (storeInFridge == null)
            {
                Console.Write("Hűtőben tárolandó? (i/n): ");
                string input = Console.ReadLine() ?? "null".Trim().ToLower();
                if (input == "i")
                    storeInFridge = true;
                else if (input == "n")
                    storeInFridge = false;
                else
                    Console.WriteLine("Csak 'i' vagy 'n' válasz engedélyezett.");
            }

            var product = new Product
            {
                Name = name,
                Quantity = quantity.Value,
                CriticalLevel = criticalLevel.Value,
                BestBefore = bestBefore.Value,
                StoreInFridge = storeInFridge.Value
            };

            try
            {
                _productService.AddProduct(product);
                Console.WriteLine("Termék sikeresen hozzáadva.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt: {ex.Message}");
            }

            Console.WriteLine("Nyomj meg egy gombot a folytatáshoz...");
            Console.ReadKey();
        });

        menu.Add("Vissza", ConsoleMenu.Close);
        menu.Show();
    }

    //Shows a paged product list, with details submenu 
    private void ShowPagedProductList()
    {
        var products = _productService.GetAllProducts();
        int pageSize = 10;
        int totalPages = (int)Math.Ceiling(products.Count() / (double)pageSize);
        int currentPage = 0;
        bool needTheMenu = true;

        while (needTheMenu)
        {
            Console.Clear();
            Console.WriteLine($"Termékek – oldal {currentPage + 1}/{totalPages}");

            var menu = new ConsoleMenu()
                .Configure(config =>
                {
                    config.WriteItemAction = item => Console.Write("{0}", item.Name);
                    config.WriteHeaderAction = () => Console.WriteLine("Válassz egy terméket:");
                    config.EnableFilter = true;
                });

            var pageItems = products
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var product in pageItems)
            {
                needTheMenu = false;
                menu.Add($"{product.Name}", () => ShowProductDetailsMenu(product));
            }

            if (currentPage > 0)
                menu.Add("<< Előző oldal", () => currentPage--);

            if (currentPage < totalPages - 1)
                menu.Add("Következő oldal >>", () => currentPage++);

            menu.Add("Vissza", () => needTheMenu = false);
            menu.CloseMenu();
            menu.Show();
        }
    }
    //Details submenu for products where the user can edit or delete the product.
    private void ShowProductDetailsMenu(Product product)
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            Console.WriteLine("TERMÉK ADATAI:");
            Console.WriteLine($"ID: {product.Id}");
            Console.WriteLine($"Név: {product.Name}");
            Console.WriteLine($"Mennyiség: {product.Quantity}");
            Console.WriteLine($"Kritikus szint: {product.CriticalLevel}");
            Console.WriteLine($"Lejárat: {product.BestBefore:yyyy-MM-dd}");
            Console.WriteLine($"Tárolás: {(product.StoreInFridge ? "Hűtő" : "Kamra")}");
            Console.WriteLine();

            var menu = new ConsoleMenu()
                .Configure(config =>
                {
                    config.WriteItemAction = item => Console.Write("{0}", item.Name);
                    config.WriteHeaderAction = () => Console.WriteLine("" +
                        $"ID: [{product.Id}] " + $"Név: {product.Name}  " + $"Mennyiség: {product.Quantity}  " +
                        $"Kritikus szint: {product.CriticalLevel}   " + $"Lejárat: {product.BestBefore:yyyy-MM-dd}   " +
                        $"Tárolás: {(product.StoreInFridge ? "Hűtő" : "Kamra")}" +
                        "\nVálassz egy opciót:");
                });

            menu.Add("Módosítás", () =>
            {
                EditProduct(product);
                product = _productService.GetProductById(product.Id)!;
            });

            menu.Add("Törlés", () =>
            {
                _productService.DeleteProduct(product.Id);
                Console.WriteLine("Termék törölve.");
                Thread.Sleep(1000);
                back = true;
            });

            menu.Add("Vissza", () => back = true);
            menu.CloseMenu();
            menu.Show();
        }
    }


    //Submenu to edit the details of a favorite product
    private void ShowFavoriteProductDetailsMenu(Product product, Person person)
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            Console.WriteLine("Kedvenc (Személy, Termék)");
            Console.WriteLine();

            var menu = new ConsoleMenu()
                .Configure(config =>
                {
                    config.WriteItemAction = item => Console.Write("{0}", item.Name);
                    Console.WriteLine($"     {person.Name}, {product.Name}"    );
                });

            menu.Add("Törlés", () =>
            {
                _favoriteService.RemoveFavorite(person.Id, product.Id);
                Console.WriteLine("Termék törölve.");
                Thread.Sleep(1000);
                back = true;
            });

            menu.Add("Vissza", () => back = true);
            menu.CloseMenu();
            menu.Show();
        }
    }

    //Submenu to edit the details of the product
    private void EditProduct(Product product)
    {
        Console.Clear();
        Console.WriteLine($"Termék szerkesztése (ID: {product.Id})");

        Console.Write($"Név [{product.Name}]: ");
        string name = Console.ReadLine() ?? "null";
        if (string.IsNullOrWhiteSpace(name)) name = product.Name;

        Console.Write($"Mennyiség [{product.Quantity}]: ");
        decimal quantity;
        if (!decimal.TryParse(Console.ReadLine(), out quantity)) quantity = product.Quantity;

        Console.Write($"Kritikus szint [{product.CriticalLevel}]: ");
        decimal criticalLevel;
        if (!decimal.TryParse(Console.ReadLine(), out criticalLevel)) criticalLevel = product.CriticalLevel;

        Console.Write($"Minőségmegőrzési dátum (YYYY-MM-DD) [{product.BestBefore:yyyy-MM-dd}]: ");
        DateTime bestBefore;
        if (!DateTime.TryParse(Console.ReadLine(), out bestBefore)) bestBefore = product.BestBefore;

        Console.Write($"Hűtőben tárolandó? (i/n) [{(product.StoreInFridge ? "i" : "n")}]: ");
        string fridgeInput = Console.ReadLine() ?? "null".Trim().ToLower();
        bool storeInFridge = fridgeInput switch
        {
            "i" => true,
            "n" => false,
            _ => product.StoreInFridge
        };

        product.Name = name;
        product.Quantity = quantity;
        product.CriticalLevel = criticalLevel;
        product.BestBefore = bestBefore;
        product.StoreInFridge = storeInFridge;


        _productService.UpdateProduct(product);
        Console.WriteLine("Termék sikeresen frissítve.");
        Console.WriteLine("Nyomj meg egy gombot a visszatéréshez...");
        Console.ReadKey();
    }
    private void DatabaseCleanup()
    {
        var menu = new ConsoleMenu().Configure(config =>
        {
            config.WriteItemAction = item => Console.Write("{0}", item.Name);
            config.WriteHeaderAction = () => Console.WriteLine("Válassz egy opciót:");
        })
            .Add("Adatbázis törlése", () =>
            {
                Console.Clear();
                Console.WriteLine("FIGYELEM! Ez a művelet az összes adatot törli az adatbázisból.");
                Console.Write("Biztosan folytatod? (i/n): ");
                var confirm = Console.ReadLine();

                if (confirm?.Trim().ToLower() == "i")
                {
                    try
                    {
                        _cleanupService.DeleteAll();
                        Console.WriteLine("\nAz adatbázis sikeresen törölve lett.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nHiba történt: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("\nMűvelet megszakítva.");
                }

                Console.WriteLine("\nNyomj meg egy gombot a visszatéréshez...");
                Console.ReadKey();
            })
            .Add("Vissza", ConsoleMenu.Close);

        menu.Show();
    }



    //Shows the details of a person, with options to edit or delete the person.
    private void ShowPersonDetailsMenu(Person person)
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            Console.WriteLine("SZEMÉLY ADATAI:");
            Console.WriteLine($"ID: {person.Id}");
            Console.WriteLine($"Név: {person.Name}");
            Console.WriteLine($"Felelős vásárló: {(person.ResponsibleForPurchase ? "Igen" : "Nem")}");
            Console.WriteLine();

            var menu = new ConsoleMenu()
                .Configure(config =>
                {
                    config.WriteItemAction = item => Console.Write("{0}", item.Name);
                    config.WriteHeaderAction = () =>
                        Console.WriteLine($"ID: [{person.Id}]  Név: {person.Name}  Vásárló: {(person.ResponsibleForPurchase ? "Igen" : "Nem")}\nVálassz egy opciót:");
                });

            menu.Add("Módosítás", () =>
            {
                EditPerson(person);
                person = _personService.GetPersonById(person.Id)!;
            });

            menu.Add("Törlés", () =>
            {
                _personService.DeletePerson(person.Id);
                Console.WriteLine("Személy törölve.");
                Thread.Sleep(1000);
                back = true;
            });

            menu.Add("Vissza", () => back = true);
            menu.CloseMenu();
            menu.Show();
        }
    }
    //shows a paged list of people, with an option to select a person and view their details.
    private void ShowPagedPersonList()
    {
        var people = _personService.GetAllPerson().ToList();
        int pageSize = 10;
        int totalPages = (int)Math.Ceiling(people.Count / (double)pageSize);
        int currentPage = 0;
        bool needTheMenu = true;

        while (needTheMenu)
        {
            Console.Clear();
            Console.WriteLine($"Személyek – oldal {currentPage + 1}/{totalPages}");

            var menu = new ConsoleMenu()
                .Configure(config =>
                {
                    config.WriteItemAction = item => Console.Write("{0}", item.Name);
                    config.WriteHeaderAction = () => Console.WriteLine("Válassz egy személyt:");
                    config.EnableFilter = true;
                });

            var pageItems = people
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var person in pageItems)
            {
                needTheMenu = false;
                menu.Add($"{person.Name}", () => ShowPersonDetailsMenu(person));
            }

            if (currentPage > 0)
                menu.Add("<< Előző oldal", () => currentPage--);

            if (currentPage < totalPages - 1)
                menu.Add("Következő oldal >>", () => currentPage++);

            menu.Add("Vissza", () => needTheMenu = false);
            menu.CloseMenu();
            menu.Show();
        }
    }
    //Shows a submenu to add a new person, checking for valid inputs.
    private void ShowPersonAddMenu()
    {
        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.WriteItemAction = item => Console.Write("{0}", item.Name);
                config.WriteHeaderAction = () => Console.WriteLine("Válassz egy opciót:");
            });

        menu.Add("Új személy hozzáadása", () =>
        {
            Console.Clear();

            Console.Write("Név: ");
            string name = Console.ReadLine() ?? "null";

            bool? isBuyer = null;
            while (isBuyer == null)
            {
                Console.Write("Felelős a vásárlásért? (i/n): ");
                string input = Console.ReadLine() ?? "null".Trim().ToLower();
                if (input == "i")
                    isBuyer = true;
                else if (input == "n")
                    isBuyer = false;
                else
                    Console.WriteLine("Csak 'i' vagy 'n' válasz engedélyezett.");
            }

            var person = new Person
            {
                Name = name,
                ResponsibleForPurchase = isBuyer.Value
            };

            try
            {
                _personService.AddPerson(person);
                Console.WriteLine("Személy hozzáadva.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba: {ex.Message}");
            }

            Console.WriteLine("Nyomj meg egy gombot a folytatáshoz...");
            Console.ReadKey();
        });

        menu.Add("Vissza", ConsoleMenu.Close);
        menu.Show();
    }
    // Submenu to edit the details of a person, checking for valid inputs.
    private void EditPerson(Person person)
    {
        Console.Clear();
        Console.WriteLine($"Személy szerkesztése (ID: {person.Id})");

        Console.Write($"Név [{person.Name}]: ");
        string name = Console.ReadLine() ?? "null";
        if (string.IsNullOrWhiteSpace(name)) name = person.Name;

        Console.Write($"Felelős vásárló? (i/n) [{(person.ResponsibleForPurchase ? "i" : "n")}]: ");
        string input = Console.ReadLine() ?? "null".Trim().ToLower();
        bool isBuyer = input switch
        {
            "i" => true,
            "n" => false,
            _ => person.ResponsibleForPurchase
        };

        person.Name = name;
        person.ResponsibleForPurchase = isBuyer;

        _personService.UpdatePerson(person);
        Console.WriteLine("Személy frissítve.");
        Console.WriteLine("Nyomj meg egy gombot a visszatéréshez...");
        Console.ReadKey();
    }





    //Shows the import/export menu, with options to import test data, import from a file, and export data in JSON or TXT format.
    private void ShowImportExportMenu()
    {
        
        var menu = new ConsoleMenu()
            .Configure(config =>
            {
                config.WriteItemAction = item => Console.Write("{0}", item.Name);
                config.WriteHeaderAction = () => Console.WriteLine("Import / Export menü:");
            });

        menu.Add("Teszt JSON betöltése", () =>
        {
            try
            {
                string filePath = Path.Combine(AppContext.BaseDirectory, "test.json");
                //LoadJsonData(filePath);
                _importExportService.ImportJson(filePath);
                Console.WriteLine("Tesztadat sikeresen betöltve.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba: {ex.Message}");
            }
            Pause();
        });

        menu.Add("JSON betöltése elérési útról", () =>
        {
            Console.Write("Add meg a JSON fájl elérési útját: ");
            string path = Console.ReadLine() ?? "null";

            try
            {
                _importExportService.ImportJson(path);
                //LoadJsonData(path);
                Console.WriteLine("Adatok sikeresen betöltve.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba: {ex.Message}");
            }
            Pause();
        });

        menu.Add("Adatbázis exportálása (Json)", () =>
        {
            try
            {
                string fileName = $"HouseholdRegisterExport_{DateTime.Now:HHmmss}.json";
                string folder = Path.Combine(AppContext.BaseDirectory, DateTime.Now.ToString("ddMMyyyy"));
                string fullPath = Path.Combine(folder, fileName);
                _importExportService.ExportJson(fullPath);


                Console.WriteLine($"Sikeres export: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba: {ex.Message}");
            }
            Pause();
        });

        menu.Add("Adatbázis exportálása (TXT)", () =>
        {
            try
            {
                string fileName = $"HouseholdRegisterExport_{DateTime.Now:HHmmss}.txt";
                string folder = Path.Combine(AppContext.BaseDirectory, DateTime.Now.ToString("ddMMyyyy"));
                string fullPath = Path.Combine(folder, fileName);
                _importExportService.ExportTxt(fullPath);
                

                Console.WriteLine($"Sikeres export: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba: {ex.Message}");
            }
            Pause();
        });

        menu.Add("Vissza", ConsoleMenu.Close);
        menu.Show();
    }
}

