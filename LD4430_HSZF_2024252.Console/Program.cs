

using LD4430_HSZF_2024252.Application;
using LD4430_HSZF_2024252.Persistence.MsSql;
using LD4430_HSZF_2024252.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ConsoleTools;
using Microsoft.EntityFrameworkCore.Design;
class Program
{
    static async Task Main(string[] args)
    {


        //DI
        var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            services.AddDbContext<AppDbContext>();

            services.AddSingleton<IProductDataProvider, ProductDataProvider>();
            services.AddSingleton<IProductService, ProductService>();

            services.AddSingleton<IPersonDataProvider, PersonDataProvider>();
            services.AddSingleton<IPersonService, PersonService>();

            services.AddSingleton<IFavoriteProductDataProvider, FavoriteProductDataProvider>();
            services.AddSingleton<IFavoriteProductService, FavoriteProductService>();

            services.AddSingleton<LD4430_HSZF_2024252.Persistence.MsSql.ICleanupService, LD4430_HSZF_2024252.Persistence.MsSql.CleanupService>();
            services.AddSingleton<LD4430_HSZF_2024252.Application.ICleanupService, LD4430_HSZF_2024252.Application.CleanupService>();

            services.AddSingleton<IImportExportService, ImportExportService>();

            services.AddSingleton<IContextManagementProvider, ContextManagementProvider>();

            services.AddSingleton<ICapacityDataProvider, CapacityDataProvider>();
            services.AddSingleton<ICapacityService, CapacityService>();

        })
        .Build();


        using IServiceScope serviceScope = host.Services.CreateScope();

        IServiceProvider serviceProvider = serviceScope.ServiceProvider;

        var productService = host.Services.GetRequiredService<IProductService>();
        var personService = host.Services.GetRequiredService<IPersonService>();
        var cleanupService = host.Services.GetRequiredService<LD4430_HSZF_2024252.Application.ICleanupService>();
        var favoriteProductService = host.Services.GetRequiredService<IFavoriteProductService>();
        var capacityService = host.Services.GetRequiredService<ICapacityService>();
        var importExportService = host.Services.GetRequiredService<IImportExportService>();
        var contextManagementProvider = host.Services.GetRequiredService<IContextManagementProvider>();



        MenuHandler menuHandler = new MenuHandler(productService, personService, cleanupService, favoriteProductService, capacityService, importExportService, contextManagementProvider);

        menuHandler.MainMenu();

    }
}