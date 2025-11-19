using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SpendingAnalyzer.Data;

public class SpendingAnalyzerDbContextFactory : IDesignTimeDbContextFactory<SpendingAnalyzerDbContext>
{
    public SpendingAnalyzerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SpendingAnalyzerDbContext>();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
                               ?? config["CONNECTIONSTRINGS__DEFAULTCONNECTION"]
                               ?? "Host=localhost;Port=5432;Database=spendingdb;Username=spending;Password=spendingpwd";

        optionsBuilder.UseNpgsql(connectionString);

        return new SpendingAnalyzerDbContext(optionsBuilder.Options);
    }
}
