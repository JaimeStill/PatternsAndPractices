using Microsoft.EntityFrameworkCore;
using Qxyz.Core.Extensions;
using Qxyz.Data;
using Qxyz.Data.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace dbseeder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (string.IsNullOrEmpty(args.ToString()))
            {
                Console.WriteLine("Arguments must be provided to seed the database. Your options are as follows:");
                Console.WriteLine("[environmentVariable] - an environment variable that points to a connection string");
                Console.WriteLine("-c [connectionString] - Option -c with the connection string directly specified");
                args = Console.ReadLine().Split(' ');
                Console.WriteLine();
            }

            var arg = args.FirstOrDefault();
            var connection = string.Empty;

            if (arg.ToLower() == "-c")
            {
                connection = args.Skip(1).ToString();

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
                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connection);

                using (var db = new AppDbContext(builder.Options))
                {
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
