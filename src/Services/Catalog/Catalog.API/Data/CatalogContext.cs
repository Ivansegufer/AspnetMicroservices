using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Data;

public class CatalogContext : ICatalogContext
{
   public IMongoCollection<Product> Products { get; }
   
   public CatalogContext(IConfiguration configuration)
   {
      var client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING"));
      var database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME"));

      Products = database.GetCollection<Product>(Environment.GetEnvironmentVariable("MONGO_COLLECTION_NAME"));
      CatalogContextSeed.SeedData(Products);
   }
}