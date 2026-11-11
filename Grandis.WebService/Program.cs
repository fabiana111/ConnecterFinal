using Grandis.Benchmarks;
using Grandis.Data;
using Grandis.Data.Db;
using Grandis.Data.Interfaces;
using Grandis.Data.Repositories.AdoNet;
using Grandis.Data.Repositories.Dapper;
using Grandis.Data.Repositories.EfCore;
using Grandis.Order;
using Grandis.Product;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("GrandisDb")
    ?? throw new InvalidOperationException("Connection string 'GrandisDb' not found.");

// AdoNet + Dapper közös connection factory
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));

// EF Core: factory-alapú context-létrehozás (műveletenként friss DbContext)., erre azért van szükség mert egy futás alkalmával nem mérne jól, mert a DbContext nem thread-safe, és a DI konténerben singletonként van regisztrálva, így minden hívásnál új példányt kell létrehozni.
builder.Services.AddDbContextFactory<GrandisDbContext>(options =>
    options.UseSqlServer(connectionString));

// Program.cs / DI regisztráció
builder.Services.AddKeyedScoped<IProductRepository, AdoNetProductRepository>(DataProviderType.AdoNet);
builder.Services.AddKeyedScoped<IProductRepository, DapperProductRepository>(DataProviderType.Dapper);
builder.Services.AddKeyedScoped<IProductRepository, EfCoreProductRepository>(DataProviderType.EfCore);

builder.Services.AddKeyedScoped<IOrderRepository, AdoNetOrderRepository>(DataProviderType.AdoNet);
builder.Services.AddKeyedScoped<IOrderRepository, DapperOrderRepository>(DataProviderType.Dapper);
builder.Services.AddKeyedScoped<IOrderRepository, EfCoreOrderRepository>(DataProviderType.EfCore);

builder.Services.AddKeyedScoped<IDataBaseRepository, AdoNetRepository>(DataProviderType.AdoNet);
builder.Services.AddKeyedScoped<IDataBaseRepository, DapperRepository>(DataProviderType.Dapper);
builder.Services.AddKeyedScoped<IDataBaseRepository, EfCoreRepository>(DataProviderType.EfCore);

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<BenchmarkService>();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
