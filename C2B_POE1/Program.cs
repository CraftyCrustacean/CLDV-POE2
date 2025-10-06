using C2B_POE1.Data;
using C2B_POE1.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<C2B_POE1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("C2B_POE1Context") ??
                         throw new InvalidOperationException("Connection string 'C2B_POE1Context' not found.")));

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

var functionBaseUrl = "https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table";

// Table services
builder.Services.AddSingleton(sp =>
    new AzureTableService<Category>(sp.GetRequiredService<HttpClient>(), "Category", functionBaseUrl));
builder.Services.AddSingleton(sp =>
    new AzureTableService<Product>(sp.GetRequiredService<HttpClient>(), "Product", functionBaseUrl));
builder.Services.AddSingleton(sp =>
    new AzureTableService<Customer>(sp.GetRequiredService<HttpClient>(), "Customer", functionBaseUrl));
builder.Services.AddSingleton(sp =>
    new AzureTableService<Order>(sp.GetRequiredService<HttpClient>(), "Orders", functionBaseUrl));
builder.Services.AddSingleton(sp =>
    new AzureTableService<OrderLine>(sp.GetRequiredService<HttpClient>(), "OrderLines", functionBaseUrl));

builder.Services.AddSingleton<AzureBlobService>(sp =>
    new AzureBlobService(sp.GetRequiredService<HttpClient>()));

builder.Services.AddSingleton<AzureFileService>(sp =>
    new AzureFileService(sp.GetRequiredService<HttpClient>()));

builder.Services.AddSingleton<OrdersFunctionService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
