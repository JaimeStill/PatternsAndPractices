using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Qxyz.Data.Extensions
{
    public static class DbInitializer
    {
        public static async Task Initialize(this AppDbContext db)
        {
            Console.WriteLine("Initializing database");
            await db.Database.MigrateAsync();
            Console.WriteLine("Database initialized");
        }
    }
}