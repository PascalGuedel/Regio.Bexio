using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Regio.Bexio.Automapper;
using Regio.Bexio.Domain;
using Regio.Bexio.Infrastructure;
using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting up");
    var host = CreateHostBuilder(args).Build();

    using var serviceScope = host.Services.CreateScope();
    var services = serviceScope.ServiceProvider;

    var invoiceService = services.GetRequiredService<IImportInvoiceService>();
    var config = services.GetRequiredService<IConfiguration>();

    if (config.GetValue<bool>("Bexio:DeleteInvoicesAndContacts"))
    {
        await invoiceService.DeleteDataAsync();
    }

    await invoiceService.ImportInvoicesAsync();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

Console.ReadLine();
return;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<IInvoiceService, InvoiceService>()
                .AddSingleton<IContactService, ContactService>()
                .AddSingleton<IExcelService, ExcelService>()
                .AddSingleton<IImportInvoiceService, ImportInvoiceService>()
                .AddSingleton<IBexioHttpClient, BexioHttpClient>()
                .AddAutoMapper(typeof(MappingProfile))
                .AddHttpClient();
        });