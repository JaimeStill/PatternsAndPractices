using Microsoft.EntityFrameworkCore;
using UploadDemo.Core.Extensions;
using UploadDemo.Data;
using UploadDemo.Data.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace dbseeder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Arguments must be provided to seed the database. Your options are as follows:");
                Console.WriteLine("[environmentVariable] - an environment variable that points to a connection string");
                Console.WriteLine("-c [connectionString] - Option -c with the connection string directly specified");
                Console.WriteLine();
                throw new Exception("No connection string provided");
            }

            var arg = args.FirstOrDefault();
            var connection = string.Empty;

            if (arg.ToLower() == "-c")
            {
                connection = args.Skip(1).FirstOrDefault();

                while (string.IsNullOrEmpty(connection))
                {
                    Console.WriteLine("Please provide a connection string:");
                    connection = Console.ReadLine();
                    Console.WriteLine();
                }
            }
            else
            {
                while (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(arg)))
                {
                    Console.WriteLine("Please provide an environment variable that points to a connection string:");
                    arg = Console.ReadLine();
                    Console.WriteLine();
                }

                connection = Environment.GetEnvironmentVariable(arg);
            }

            try
            {
                Console.WriteLine($"Connection: {connection}");

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connection);

                using (var db = new AppDbContext(builder.Options))
                {
                    Console.WriteLine("Verifying DB Connection");
                    await db.Database.CanConnectAsync();
                    Console.WriteLine("Connection Succeeded");
                    Console.WriteLine();
                    await db.Initialize();
                }

                Console.WriteLine();
                Console.WriteLine("Database seeding completed successfully!");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while seeding the database:");
                Console.WriteLine(ex.GetExceptionChain());
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
