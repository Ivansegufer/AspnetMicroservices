using Npgsql;

namespace Discount.API.Extensions;

public static class WebApplicationBuilderExtension
{
   public static WebApplicationBuilder MigrateDatabase<TContext>(this WebApplicationBuilder builder, int retry = 0)
   {
      var serviceProvider = builder.Services.BuildServiceProvider();
      
      using (var scope = serviceProvider.CreateScope())
      {
         var services = scope.ServiceProvider;
         var logger = services.GetRequiredService<ILogger<TContext>>();

         try
         {
            logger.LogInformation("Migrating postgresql starting...");
            using var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
            connection.Open();

            using var command = new NpgsqlCommand()
            {
               Connection = connection,
               CommandText = "DROP TABLE IF EXISTS Coupon"
            };
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE Coupon (Id SERIAL PRIMARY KEY,
                                        ProductName VARCHAR(24) NOT NULL,
                                        Description TEXT, Amount INT)";
            command.ExecuteNonQuery();

            command.CommandText =
               "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
            command.ExecuteNonQuery();

            command.CommandText =
               "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
            command.ExecuteNonQuery();

            logger.LogInformation("Migrated postresql database.");
         }
         catch (Exception ex)
         {
            logger.LogError(ex, "An error occurred while migrating the postgresql database");

            if (retry < 10)
            {
               retry++;
               Thread.Sleep(2000);
               MigrateDatabase<TContext>(builder, retry);
            }
            else
            {
               throw new Exception("Database migration failed.");
            }
         }

         return builder;
      }
   }
}