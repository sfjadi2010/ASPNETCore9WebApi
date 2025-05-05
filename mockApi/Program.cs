using Bogus;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using mockApi.Data;
using mockApi.Models;
using mockApi.Models.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connection);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();

    // Seed the database with initial data if needed
    if (!context.Products.Any())
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker + 1) // Ensure unique Ids starting from 1
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Finance.Amount(50, 2000))
            .RuleFor(p => p.CategoryId, f => f.Random.Int(1, 5));

        var products = productFaker.Generate(10000);
        context.Products.AddRange(products);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.MapGet("/products", async (AppDbContext db) =>
{
    return await db.Products.OrderBy(p => p.Id).Take(25).ToListAsync();
});

app.MapGet("/categoryInfo", async (AppDbContext db) =>
{
    var products = await db.Products.AsNoTracking().ToListAsync();

    var productsByCategory = products.CountBy(p => p.CategoryId).OrderBy(x => x.Key);
    return productsByCategory.Select(categoryGroup => new CategoryDTO
    {
        CategoryId = categoryGroup.Key,
        ProductCount = categoryGroup.Value
    }).ToList();
});

app.Run();
