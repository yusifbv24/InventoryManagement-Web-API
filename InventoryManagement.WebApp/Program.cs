// InventoryManagement.WebApp/Program.cs
using InventoryManagement.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register HTTP clients for microservices
builder.Services.AddHttpClient<ProductsService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MicroserviceUrls:Products"] ?? "http://localhost:5000/api/");
});

builder.Services.AddHttpClient<InventoryService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MicroserviceUrls:Inventory"] ?? "http://localhost:5200/api/");
});

builder.Services.AddHttpClient<OrdersService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MicroserviceUrls:Orders"] ?? "http://localhost:5400/api/");
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();