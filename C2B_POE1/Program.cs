using C2B_POE1.Data;
using C2B_POE1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<C2B_POE1Context>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("C2B_POE1Context") ??
						 throw new InvalidOperationException("Connection string 'C2B_POE1Context' not found.")));

builder.Services.AddControllersWithViews()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = null;
	});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.LogoutPath = "/Account/Logout";
		options.AccessDeniedPath = "/Account/AccessDenied";
		options.ExpireTimeSpan = TimeSpan.FromHours(2);
		options.SlidingExpiration = true;
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

//password hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddHttpClient();

var functionBaseUrl = "https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table";

// Table services
builder.Services.AddSingleton(sp =>
	new AzureTableService<Category>(sp.GetRequiredService<HttpClient>(), "Category", functionBaseUrl));
builder.Services.AddSingleton(sp =>
	new AzureTableService<Product>(sp.GetRequiredService<HttpClient>(), "Product", functionBaseUrl));
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

var cultureInfo = new System.Globalization.CultureInfo("en-ZA");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();