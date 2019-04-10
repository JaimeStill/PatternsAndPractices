# Database Seeding

* [DbInitializer](#dbinitializer)
* [dbseeder](#dbseeder)
    * [Debug](#debug)
    * [Publish](#publish)
    * [Scripts](#scripts)

## [DbInitializer](#database-seeding)

The `DbIntializer` static class is written as an extension class at **{Project}.Data\\Extensions\\DbInitializer.cs** and fills two roles:

* Ensuring the database has been created with the latest migration
* Ensuring that seeded data exists in the database  

To illustrate how to appropriately accomplish this, it's best to look at an example that works with the entities defined in the [Data Access Layer](./07-database-seeding.md) section:  

**`DbInitializer`**

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Demo.Data.Extensions
{
    public static class DbInitializer
    {
        public static async Task Initialize(this AppDbContext db)
        {
            Console.WriteLine("Initializing database");
            await db.Database.MigrateAsync();

            List<Category> categories;

            if (!(await db.Categories.AnyAsync()))
            {
                Console.WriteLine("Seeding categories...");
                categories = new List<Category>
                {
                    new Category
                    {
                        Label = "Category A",
                        IsDeleted = false
                    },
                    new Category
                    {
                        Label = "Category B",
                        IsDeleted = false
                    }
                };

                await db.Categories.AddRangeAsync(categories);
                await db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Retrieving seed categories...");
                categories = await db.Categories
                    .Take(2)
                    .ToListAsync();
            }

            List<Location> locations;

            if (!(await db.Locations.AnyAsync()))
            {
                Console.WriteLine("Seeding locations...");
                locations = new List<Location>
                {
                    new Location
                    {
                        Name = "Location A",
                        IsDeleted = false
                    },
                    new Location
                    {
                        Name = "Location B",
                        IsDeleted = false
                    }
                };

                await db.Locations.AddRangeAsync(locations);
                await db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Retrieving seed locations...");
                locations = await db.Locations
                    .Take(2)
                    .ToListAsync();
            }

            List<Item> items;

            if (!(await db.Items.AnyAsync()))
            {
                Console.WriteLine("Seeding items...");
                items = new List<Item>
                {
                    new Item
                    {
                        CategoryId = categories[0].Id,
                        OriginLocationId = locations[0].Id,
                        CurrentLocationId = locations[1].Id,
                        Name = "Item A",
                        IsDeleted = false
                    },
                    new Item
                    {
                        CategoryId = categories[1].Id,
                        OriginLocationId = locations[1].Id,
                        CurrentLocationId = locations[1].Id,
                        Name = "Item B",
                        IsDeleted = false
                    },
                    new Item
                    {
                        CategoryId = categories[0].Id,
                        OriginLocationId = locations[1].Id,
                        CurrentLocationId = locations[0].Id,
                        Name = "Item C",
                        IsDeleted = false
                    },
                    new Item
                    {
                        CategoryId = categories[1].Id,
                        OriginLocationId = locations[0].Id,
                        CurrentLocationId = locations[0].Id,
                        Name = "Item D",
                        IsDeleted = false
                    }
                };

                await db.Items.AddRangeAsync(items);
                await db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Retrieving seeded items...");
                items = await db.Items
                    .Take(4)
                    .ToListAsync();
            }

            List<Tag> tags;

            if (!(await db.Tags.AnyAsync()))
            {
                Console.WriteLine("Seeding tags...");
                tags = new List<Tag>
                {
                    new Tag
                    {
                        Label = "Tag A",
                        IsDeleted = false
                    },
                    new Tag
                    {
                        Label = "Tag B",
                        IsDeleted = false
                    },
                    new Tag
                    {
                        Label = "Tag C",
                        IsDeleted = false
                    }
                };

                await db.Tags.AddRangeAsync(tags);
                await db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Retrieving seeded tags...");
                tags = await db.Tags
                    .Take(3)
                    .ToListAsync();
            }

            List<ItemTag> itemTags;
            if (!(await db.ItemTags.AnyAsync()))
            {
                Console.WriteLine("Seeding item tags...");
                itemTags = new List<ItemTag>
                {
                    new ItemTag
                    {
                        ItemId = items[0].Id,
                        TagId = tags[0].Id
                    },
                    new ItemTag
                    {
                        ItemId = items[0].Id,
                        TagId = tags[1].Id
                    },
                    new ItemTag
                    {
                        ItemId = items[1].Id,
                        TagId = tags[1].Id
                    },
                    new ItemTag
                    {
                        ItemId = items[1].Id,
                        TagId = tags[2].Id
                    },
                    new ItemTag
                    {
                        ItemId = items[2].Id,
                        TagId = tags[2].Id
                    },
                    new ItemTag
                    {
                        ItemId = items[2].Id,
                        TagId = tags[0].Id
                    },
                    new ItemTag
                    {
                        ItemId = items[3].Id,
                        TagId = tags[0].Id
                    },
                    new ItemTag
                    {
                        ItemId = items[3].Id,
                        TagId = tags[1].Id
                    }
                };

                await db.ItemTags.AddRangeAsync();
                await db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Retrieving seeded item tags...");
                itemTags = await db.ItemTags
                    .Take(8)
                    .ToListAsync();
            }
        }
    }
}
```  

`await db.Database.MigrateAsync()` ensures that the database has been created on the server targeted by the `AppdbContext` instance, and that all of the migrations have been appropriately applied.  

Each line following the migration command follows the following workflow:

* Declare a variable to hold the seed data
* Check to see if data already exists in the targeted table
    * If no data exists, assign seed data to the declared variable, and persist it to the database
    * If data does exist, retrieve only as much data as is needed to complete the seed operation and assign it to the declared variable

The sequence in which the tables are seeded depends on how the data is related. You can't seed an `Item` if there isn't any `Location` or `Category` data because `Item` requires foreign key properties for those tables in order to be defined. The same is true for `ItemTag` and its dependencies on `Item` and `Tag`.  

> It's very important to never explicitly set the `Id` property for a primary key or foreign key relationship. Entity Framework will automatically assign the primary key value, and foreign key values should be applied directly from the object it's related to. This is why the seed values are retained in a list and accessed when dependent rows are defined for assigning foreign key relationships.

## [dbseeder](#database-seeding)

There are many ways that you could access the `DbInitializer.Initialize()` extension method. You could build it into the startup pipeline for the web app, or make it exposed through a Web API action method. However, I strongly advise against directly coupling the application to the database seeding functionality. It doesn't make sense to have to check for data every time the app is started up, neither of these methods allows you to dynamically condition the database for any given connection string on a remote server.  

Enter the **dbseeder** console application. This application allows you to specify a connection string and is able to directly access the **{Project}.Data** project outside of the context of the app stack. Whenever you update the data access layer, all you have to do is rebuild the dbseeder app and execute it on a target connection string, and your database will not only be updated with the new schema, but the new seed data will be populated as well.  

Let's take a look at how it's written:  

**`dbseeder\\Program.cs`**  
```cs
using Microsoft.EntityFrameworkCore;
using Demo.Core.Extensions;
using Demo.Data;
using Demo.Data.Extensions;
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
```  

The application flow works as follows:
* Thanks to C# 7, we can run the `Main()` method as an `async Task`, so we can take advantage of `async` / `await` throughout the application
* If no arguments are provided, the available argument formats are printed and an exception is thrown
* The first argument in the `args` array is assigned and a `connection` variable is initialized as an empty string.
    * If the `arg` variable is `-c`, that means there should be a second argument provided that represents the connection string.
        * The `connection` variable is assigned the second argument. If it does not have a value, you will be prompted to provide one until it does.
    * Otherwise, we check to see whether `arg` is an Environment Variable.
        * If `arg` is not an Environment Variable, you will be prompted to provide a value until it is.
        * When `arg` matches an Environment Variable, the value of the Environment Variable is assigned to the `connection` variable.
* A `try` / `catch` block is setup and if an exception is thrown at any time during the seeding process, it will be captured and printed before the program is terminated.
* The resolved `connection` is printed to the console.
* `DbContextOptionsBuilder` is used to build `DbContextOptions` for `AppDbContext` specifying Microsoft SQL Server as the provider pointed at the connection string specified by the `connection` variable via `UseSqlServer(connection)`.
* A session to the database is started with an instance of `AppDbContext`.
* The app verifies that it can connect to the database session through `AppDbContext`.
* The `DbInitializer.Initialize` method is executed against the `AppDbContext` instance.
* After successfully completing, the app alerts you to the success and waits for a key to be input before terminating.  

### [Debug](#database-seeding)

The **dbseeder** project does not need to be executed in Visual Studio. To debug, all you need to do is execute the following sequence of commands pointed at the **dbseeder** project directory:  

```
dbseeder>dotnet build
dbseeder>dotnet run -- {environmentVariable}
** or **
dbseeder>dotnet run -- -c {connectionString}
```  

> When specifying a connection string on the command line, do not escape backslashes. For instance, `"Server=(localdb)\\ProjectsV13` will throw a timeout error, but `Server=(localdb)\ProjectsV13...` will work. You only need to escape backslashes in JSON configuration files.

The `--` after `run` specifies that anything that follows will be provided as arguments to the application being run.  

If you want to debug the application in Visual Studio Code and step through the sequence of events, perform the following actions:

* Use the <kbd>Ctrl + Shift + D</kbd> keyboard shortcut to open up the **Debug** pane
* At the top of the pane, click the **Settings** gear icon
* Click **.NET Core** from the list of environments that pops up
* Click **dbseeder** under the list of projects to launch

A **launch.json** file will be created in a **.vscode** directory at the root of the app stack directory. Add a connection string argument to this file:  

> Additional launch settings removed for convenience. The connection string that you specify may be different from the one demonstrated below. You can verify it in **appsettings.Development.json** for your local development connection.

```cs
"configurations": [
    "args": [
        "-c",
        "Server=(localdb)\\ProjectsV13;Database=Demo-dev;Trusted_Connection=True;"
    ]
]
```

Set breakpoints anywhere in **dbseeder\\Program.cs** or **{Project}.Data\\Extensions\\DbInitializer.cs**, open up the **Debug** pane in Visual Studio Code, and click the green **Play** button.

> For detailed information, see [Debugging in Visual Studio Code](https://code.visualstudio.com/docs/editor/debugging).

### [Publish](#database-seeding)

Once you have **dbseeder** and `DbInitializer` setup how you want them, you can publish the application so it can be run directly with the `dbseeder` command on the command line.  

From a command prompt pointed at the **dbseeder** project directory, execute the following:  

```cs
dbseeder>dotnet publish -o {output-directory} -r {runtime} --self-contained
```  

An example of this:

```
dbseeder>dotnet publish -o D:\Desktop\dbseeder -r win-x64 --self-contained
```  

> See [.NET Core RID Catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) for detailed information on compiling .NET Core applications to a target runtime and the runtime identifiers that are available.  

### [Script](#database-seeding)  

All of this can be automated once you have it setup the way you like. This section will cover how to accomplish this using Windows `.cmd` scripts, but the same can be done in PowerShell or Bash.  

There are two relevant scripts that can be created:  

* Compiling the `dbseeder` command line utility
* Seeding a particular database with the `dbseeder` command  

The second script is particularly helpful because you can create numerous instances that target different databases. For instance, I can seed my local dev database, an AWS test database, and a production database from three separate scripts.  

The examples below are relevant to the enviornment that I'm building in. Use them as a template and modify them to fit your environment. These are the items you'll particularly need to modify:

* The drive letter you are working in
    * All of my data is stored on my `D:` drive
* The location of the project that contains your **dbseeder** project
    * In this example, **D:\\Desktop\\Projects\\Demo\\dbseeder**
* Where you want to output the published `dbseeder` command line utility
    * In this example, **D:\\Desktop\\Staging\\dbseeder**
* The runtime identifier you use to publish the `dbseeder` command line utility
    * In this example, **win-x64**
* The diretory from where you are calling the commands
    * In this example, **D:\\Desktop\\Commands\\Demo**
* The connection string you use for the seeding script

**`publish-dbseeder.cmd`**  

```cmd
D:
cd D:\Desktop\Projects\Demo\dbseeder
dotnet publish -o D:\Desktop\Staging\dbseeder -r win-x64 --self-contained
cd D:\Desktop\Commands\Demo
```  

**publish-dbseeder** performs the following steps:

* Ensure that I'm using the `D:` drive
* Change into the **dbseeder** project directory
* Publish **dbseeder** to **D:\\Desktop\\Staging\\dbseeder** using the **Windows 64-bit** runtime identifier, and ensuring that it is a self-contained executable
    * Specifying `--self-contained` means that the necessary .NET Core `.dll` files are included with the published app, so the target machine running the executable does not need to have .NET Core installed.
* Change back into the directory where the commands are stored

**`seed-database.cmd`**  

```cmd
D:
cd D:\Desktop\Staging\dbseeder
dbseeder -c Server=(localdb)\\ProjectsV13;Database=Demo-dev;Trusted_Connection=True;
cd D:\Desktop\Commands\Demo
```

**seed-database** performs the following steps:

* Ensure that I'm using the `D:` drive
* Change into the directory that hosts the `dbseeder` command line utility
* Execute `dbseeder` with a connection string argument
* Change back into the directory where the commands are stored  

To run these specific examples, I'd open a command prompt pointed at the **D:\\Desktop\\Commands\\Demo** directory and execute the following:

```cmd
D:\Desktop\Commands\Demo>publish-dbseeder
D:\Desktop\Commands\Demo>seed-database
```