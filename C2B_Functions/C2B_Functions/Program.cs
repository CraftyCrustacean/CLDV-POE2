using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNameCaseInsensitive = true;
        });
    })
    .ConfigureServices((context, services) =>
    {
        var conn = context.Configuration.GetConnectionString("AzureStorage");

        services.AddSingleton(new TableClient(conn, "Orders"));
        services.AddSingleton(new TableClient(conn, "OrderLines"));

        services.AddSingleton(new TableClient(conn, "Product"));
        services.AddSingleton(new TableClient(conn, "Customer"));
        services.AddSingleton(new TableClient(conn, "Category"));

        services.AddSingleton(new BlobServiceClient(conn));
    })
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .Build();

host.Run();
