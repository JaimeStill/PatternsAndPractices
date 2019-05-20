using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UploadDemo.Data.Entities;

namespace UploadDemo.Data.Extensions
{
    public static class DbInitializer
    {
        public static async Task Initialize(this AppDbContext db)
        {
            Console.WriteLine("Initializing database");
            await db.Database.MigrateAsync();
            Console.WriteLine("Database initialized");

            if (! await db.Folders.AnyAsync())
            {
                Console.WriteLine("Seeding Folders...");
                var folders = new List<Folder>
                {
                    new Folder
                    {
                        Name = "Project",
                        Description = "Files necessary to execute some project"
                    },
                    new Folder
                    {
                        Name = "Personal",
                        Description = "Personal files for safekeeping"
                    },
                    new Folder
                    {
                        Name = "Time Capsule",
                        Description = "Store these for posterity. Do not open until 3020!"
                    }
                };

                await db.Folders.AddRangeAsync(folders);
                await db.SaveChangesAsync();
            }
        }
    }
}