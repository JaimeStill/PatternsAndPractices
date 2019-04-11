# Business Logic  

* [Extension Methods](#extension-methods)
* [Asynchronous Operations](#asynchronous-operations)
* [Retrieving Data](#retrieving-data)
* [Managing Entities](#managing-entities)
* [Validation](#validation)
* [Example Extension Class](#example-extension-class)

## [Extension Methods](#business-logic)

Each primary entity type also has a corresponding static extension class that encapsulates the business logic for that type. These classes are defined in the **{Project}.Data\\Extensions** directory as **{Class}Extensions.cs**.  

> Extension methods enable you to *add* methods to existing types without creating a new derived type, recompiling, or otherwise modifying the original type.  

\- [Extension Methods - C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)  

This allows us to append functionality related to our entities to the `AppDbContext` type without bloating that class or the entity type classes. The entity definitions can focus on the shape of the data structures, and their logic can be expressed as transactions on the context object. `AppDbContext` and the `DbSet<T>` properties it contains already define a set of methods for performing CRUD operations and persisting their changes in the database. Defining extension methods that leverage these methods allow us to have fine-grained control over all of the facets of how these transactions occur.  

An extension method is defined given the following requirements:
* It must be defined as a static method in a static class
* The first parameter of the method must start with the `this` keyword and represents the type that the method is pinned to  

**Example Definition and Usage**  
```cs
public static class BasicExtensions
{
    public static string Mutate(this string text)
    {
        var mutate = string.Empty;

        for (var i = 0; i < text.Length; i++)
        {
            mutate += i % 2 == 0 ?
                Char.ToLower(text[i]) :
                Char.ToUpper(text[i]);
        }

        return mutate;
    }
}

public static void Main(string[] args)
{
    var test = "This is a standard string";

    // Notice how Mutate() is now a method on the string reference type
    Console.WriteLine(test.Mutate());
}
```  

The output of the above example:  

```
tHiS Is a sTaNdArD StRiNg
```  

Here's a [demo](https://repl.it/@JaimeStill/BrightHiddenWorkers) of the above example.

## [Asynchronous Operations](#business-logic)  

Modern CPUs contain multiple cores with multiple threads. Applications run on a single thread, but can delegate long running tasks to run asynchronously in a separate thread. Not only does this allow you to take advantage of the hardware that is running the application, it prevents you from locking up the main thread of execution. A detailed look at the asynchronous programming model is outside of the scope of this documentation, but you can find all of the details in the [Asynchronous Programming](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/) section of the documentation.  

What is important to know for this section is:
* How to write a `Task`
* How to call a `Task` using `async` / `await`
* How to write a `Task` that calls another `Task`  

When writing a `Task`, the method must return either a `Task` (synonymous to a `void` synchronous method), or a `Task<TResult>`, where `TResult` is the type returned by the task (synonymous to a synchronous method with a return value).  

**Example Task**
```cs
public Task<int> SumNumbers(int length)
{
    return Task.Run(() =>
    {
        var result = 0;

        for (var i = 0; i < length; i++)
        {
            result += i;
        }

        return result;
    });
}
```  

If a `Task` calls another `Task`, it can be written as an asynchronous method using `async` / `await`:

```cs
public async Task ComputeNumbers()
{
    Console.WriteLine("Computing a bunch of numbers");

    for (var i = 10; i < 100; i += 2)
    {
        var result = await SumNumber(i);
        Console.WriteLine($"SumNumbers({i}): {result}");
    }

    Console.WriteLine("Finished computing numbers");
}
```  

Luckily, we won't have to write many `Tasks` by hand because `DbContext` and `DbSet<T>` already define asynchronous methods for the long-running CRUD transactions. Here are a list of notable asynchronous operations that Entity Framework Core defines:  
* `ToListAsync()`
* `FirstOrDefaultAsync()`
* `AddAsync()` and `AddRangeAsync()`
* `SaveChangesAsync()`  

Because of these methods, when writing logic for entity types, we can simply use the `async` / `await` pattern for defining asynchronous tasks rather than writing the `Task` ourselves.

## [Retrieving Data](#business-logic)  

An important fact to take note of before diving into this section is that, when conditioning a LINQ expression in Entity Framework Core, nothing is executed against the database until the data is actually read. The initial LINQ operator performed on a `DbSet<T>` will generate an `IQueryable<T>` of the same type. The query will not actually be executed against the database until an operation occurs that would read the results into memory.  

This sentiment has a couple of very important connotations. First, and most important, is that you want to read your data into memory and return the results in any method that you execute. The query is conditioned in the context of a unit of execution based on the lifecycle of the `AppDbContext` object. In our app stack, the `AppDbContext` object is only alive for the duration of a single HTTP request. This prevents memory leaks and rogue database connections from occurring by allowing dependency injection to manage the lifecycle of this construct that needs to be disposed of after use. So if you try to read results from a dataset that hasn't been read to memory, but the context has been disposed, an exception will occur.  

To ensure that data is read to memory, always finish LINQ queries with either `ToListAsync()` (for a collection of objects), `FindAsync()` (when retrieving a single item by `Id` - the primary key of the record), or `FirstOrDefaultAsync()` (when retrieving a single item by a specific filter).

## [Managing Entities](#business-logic)

There are a few different scenarios to consider when manipulating entities in the database:
* Creating a new instance of an entity
* Updating an existing instance based on user-defined details
* Updating an existing instance based on system-provided details
* Permanently removing an existing instance from the database  

In the first scenario, an object of the entity type is passed to an `Add{Entity}` method (as the result of an HTTP POST operation), and after validation, the object is saved to the database.  

In the second scenario, an object of the entity type is passed to an `Update{Entity}` method (as the result of an HTTP POST operation), and after validation, the object is updated in the database.  

In the third scenario, an object of the entity type is passed to a method (as the result of an HTTP POST operation), and specific traits of the object are modified by the application, then the object is updated in the database.

In the fourth scenario, an object of the entity type is passed to a `Remove{Entity}` method (as the result of an HTTP POST operation), and the object is removed from the database.

**Examples of Manipulating Entities**  

```cs
// First scenario
public static async Task AddEntity(this AppDbContext db, Entity entity)
{
    if (await entity.Validate(db))
    {
        await db.Entities.AddAsync(entity);
        await db.SaveChangesAsync();
    }
}

// Second scenario
public static async Task UpdateEntity(this AppDbContext db, Entity entity)
{
    if (await entity.Validate(db))
    {
        // There is not an asynchronous Update
        db.Entities.Update(entity);
        await db.SaveChangesAsync();
    }
}

//Third scenario
public static async Task ToggleEntityDeleted(this AppDbContext db, Entity entity)
{
    // Attach the entity to the DbContext to enable change tracking
    db.Entities.Attach(entity);
    entity.IsDeleted = !entity.IsDeleted;
    await db.SaveChangesAsync();
}

//Fourth scenario
public static async Task RemoveEntity(this AppDbContext db, Entity entity)
{
    db.Entities.Remove(entity);
    await db.SaveChangesAsync();
}
```  

## [Validation](#business-logic)

Before adding or updating items that depend on user-defined values, you want to ensure that the data is valid. The convention here is to write a single `Validate()` extension method pinned to the entity class. This same method can be used for validation when adding or updating an object. There are two types of `Validate()` methods that can be written:  

* A method that needs to validate the entity against other objects stored in the database
    * This will be an asynchronous method that receives the `AppDbContext` as a second parameter and returns a `Task<bool>`.
* A method that only needs to validate the contents of the property
    * This will be a synchronous method that does not receive any additonal paramters and simply returns `bool`.  

**Example Validation Methods**  

```cs
public static bool Validate(this Entity entity)
{
    if (string.IsNullOrEmpty(entity.Name))
    {
        throw new Exception("Entity must have a name!");
    }

    return true;
}

public static async Task<bool> Validate(this Entity entity, AppDbContext db)
{
    if (string.IsNullOrEmpty(entity.Name))
    {
        throw new Exception("Entity must have a name");
    }

    var check = await db.Entities
        .FirstOrDefaultAsync(x =>
            x.Id != entity.Id &&
            x.Name.ToLower() == entity.Name.ToLower()
        );

    if (check != null)
    {
        throw new Exception("The provided Entity already exists");
    }

    return true;
}
```  

You'll notice that in both of these validation methods, it only ever returns `true`, and otherwise throws an exception. The way that the exception handling middleware is configured in the app stack, the message provided by the exception is forwarded to the client in the form of a snackbar notification. This way, we can provide feedback when a validation error occurs, and not cause the application to crash. More details of the middleware configuration can be found in the [Core Configuration](./04-core-configuration.md) and [Dependency Injection and Middleware](./05-di-and-middleware.md) sections.  

The asyncrhonous validation method can be used for both Add and Update because when adding a new `Entity`, the `Entity.Id` property will be `0`, and no existing item in the database will have an ID less than 1. This allows us to ensure that the name of the entity is unique, but if we're updating an existing `Entity` and the `Entity.Name` property hasn't changed, it can still be updated without throwing an exception.

## [Example Extension Class](#business-logic)  

The below example extensions are built on the `Item` entity that was defined in the [Data Access Layer](./02-data-access-layer.md) article. The documentation above is meant as a primer to the concepts expressed as code below, and as such, will be expressed as code alone.  

```cs
using Demo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Data.Extensions
{
    public static class ItemExtensions
    {
        public static async Task<List<Item>> GetItems(this AppDbContext db, bool isDeleted = false)
        {
            var model = await db.Items
                .Where(x => x.IsDeleted == isDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return model;
        }

        public static async Task<List<Item>> SearchItems(this AppDbContext db, string search, bool isDeleted = false)
        {
            search = search.ToLower();

            var model = await db.ItemTags
                .Include(x => x.Tag)
                .Include(x => x.Item)
                    .ThenInclude(x => x.Category)
                .Where(x => x.Item.IsDeleted == isDeleted)
                .Where(x =>
                    x.Item.Name.ToLower().Contains(search) ||
                    x.Item.Category.Label.ToLower().Contains(search) ||
                    x.Tag.Label.ToLower().Contains(search)
                )
                .Select(x => x.Item)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return model;
        }

        public static async Task<Item> GetItem(this AppDbContext db, int id)
        {
            var item = await db.Items.FindAsync(id);
            return item;
        }

        public static async Task AddItem(this AppDbContext db, Item item)
        {
            if (await item.Validate(db))
            {
                await db.Items.AddAsync(item);
                await db.SaveChangesAsync();
            }
        }

        public static async Task UpdateItem(this AppDbContext db, Item item)
        {
            if (await item.Validate(db))
            {
                db.Items.Update(item);
                await db.SaveChangesAsync();
            }
        }

        public static async Task ToggleItemDeleted(this AppDbContext db, Item item)
        {
            db.Items.Attach(item);
            item.IsDeleted = !item.IsDeleted;
            await db.SaveChangesAsync();
        }

        public static async Task RemoveItem(this AppDbContext db, Item item)
        {
            db.Items.Remove(item);
            await db.SaveChangesAsync();
        }

        public static async Task<bool> Validate(this Item item, AppDbContext db)
        {
            if (string.IsNullOrEmpty(item.Name))
            {
                throw new Exception("Item must have a name");
            }

            if (item.CategoryId < 1)
            {
                throw new Exception("Item must have a Category");
            }

            var check = await db.Items
                .FirstOrDefaultAsync(x =>
                    x.Id != item.Id &&
                    x.Name.ToLower() == item.Name.ToLower()
                );

            if (check != null)
            {
                throw new Exception("The provided Item already exists");
            }

            return true;
        }
    }
}
```