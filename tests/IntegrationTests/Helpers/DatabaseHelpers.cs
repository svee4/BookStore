using BookStore.Database;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Helpers;

public static class DatabaseHelpers
{
    
    /// <summary>
    /// Ensures database migrations are up to date
    /// </summary>
    /// <param name="services"></param>
    // There has to be a better way to do this but i havent found out about it yet
    public static async Task EnsureMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<BookStoreDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>
    /// Removes all book entities from database
    /// </summary>
    /// <param name="services"></param>
    public static async Task ClearBooks(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<BookStoreDbContext>();
        await db.Books.AsQueryable().ExecuteDeleteAsync();
    }
}