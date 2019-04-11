# Data Access Layer  

[Table of Contents](./toc.md)

* [Project Infrastructure](#project-infrastructure)
* [Building the Data Layer](#building-the-data-layer)
    * [One to Many](#one-to-many)
    * [Many to Many](#many-to-many)
    * [Multiple One to Many Using the Same Table](#multiple-one-to-many-using-the-same-table)
    * [DbContext](#dbcontext)
* [Registering Entity Framework with .NET Core](#registering-entity-framework-with-net-core)
* [Database Management Workflow](#database-management-workflow)  

## [Project Infrastructure](#data-access-layer)

File / Folder | Description
--------------|------------
**`AppDbContext.cs`** | Database session object that maps entities to database tables and provides database configuration.
**Entities** | Directory that stores all of the entity classes.
**Extensions** | Directory containing static classes that define extension methods with business logic for each core entity.
**Infrastructure** | Defines service layer objects used in Dependency Injection and any associated middleware implementations.
**Migrations** | Auto-generated directory that contains snapshots of the database at the point of each migration, as well as the overall database configuration.

## [Building the Data Layer](#data-access-layer)

Entity models are C# classes that correspond to SQL data tables. Below are examples of several entity models that define relationships with each other.

### [One to Many](#data-access-layer)  

In this example, each `Item` has a single `Category`, and each `Category` can be related to many `Items`.

The `int Id` property defines the primary key for each class.

Navigation properties are defined based on their relationship type. Singular navigation properties are defined as `public {Entity} {Entity}` (`Category Category` in the example below). Collective navigation properties are defined as `public List<{Entity}> {Entities}` (`List<Item> Items` in the example below).  

```cs
public class Item
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }

    public Category Category { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Label { get; set; }
    public bool IsDeleted { get; set; }

    public List<Item> Items { get; set; }
}
```

### [Many to Many](#data-access-layer)

To represent a **Many to Many** relationship, you need to incorporate a **join table**. For instance, if many `Item` entities can be related to many `Tag` entities, you can use an `ItemTag` entity to represent this relationship.  

```cs
public class Tag
{
    public int Id { get; set; }
    public string Label { get; set; }
    public bool IsDeleted { get; set; }

    public List<ItemTag> TagItems { get; set; }
}

public class Item
{
    // Props defined above
    public List<ItemTag> ItemTags { get; set; }
}

public class ItemTag
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int TagId { get; set; }

    public Item Item { get; set; }
    public Tag Tag { get; set; }
}
```  

The tags for an item can now be accessed in C# as follows:  

```cs
public static async Task<List<Tag>> GetItemTags(this AppDbContext db, int itemId)
{
    var model = await db.ItemTags
        .Include(x => x.Tag)
        .Where(x => x.ItemId == itemId)
        .Select(x => x.Tag)
        .ToListAsync();

    return model;
}
```  

Or directly from the `Item` class in Angular (assuming the API call includes the `ItemTag` and `Tag` navigation props in the query):  

``` html
<div *ngFor="let t of item.itemTags">
  <tag-card [tag]="t.tag"></tag-card>
</div>
```

### [Multiple One to Many Using the Same Table](#data-access-layer)  

By default, tables are related in Entity Framework with the following convention:  

The class that contains the foreign key ov another class defines this relationship with a property of `{ClassName}Id`. For instance, `CategoryId`.  

The navigation property is defined as `public {Class} {Class}`, for instance:

```cs
public Category Category { get; set; }
```  

The navigation property that defines a many relationship is defined as `public List<{Class}> {Classes}` where `{Classes}` is the plural form of the class name. For instance:  

```cs
public List<Item> Items { get; set; }
```  

What if you need a single reference to the same table twice in a **One to Many** relationship? For instance, if an item has an origin location and a current location? The definition of the navigation preoprties would not fit with the conventional relationship definitions, so you would need to use the [Fluent API](https://docs.microsoft.com/en-us/ef/core/modeling/relationships#fluent-api) to let Entity Framework know how these entities are related.  

```cs
public class Location
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }

    public List<Item> CurrentItems { get; set; }
    public List<Item> OriginItems { get; set; }
}

public class Item
{
    // Props defined above
    public int CurrentLocationId { get; set; }
    public int OriginLocationId { get; set; }

    public Location CurrentLocation { get; set; }
    public Location OriginLocation { get; set; }
}
```  

Use Fluent API in `DbContext.OnModelCreating` to express the details of both **One to Many** relationships:  

```cs
public class AppDbContext : DbContext
{
    public DbSet<Location> Locations { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Item>()
            .HasOne(x => x.CurrentLocation)
            .WithMany(x => x.CurrentItems)
            .HasForeignKey(x => x.CurrentLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Item>()
            .HasOne(x => x.OriginLocation)
            .WithMany(x => x.OriginItems)
            .HasForeignKey(x => x.OriginLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### [DbContext](#data-access-layer)

> A `DbContext` instance represents a session with the database and can be used to query and save instances of your entities. `DbContext` is a combination of the **Unit of Work** and **Repository** patterns.  

\- [**DbContext Class Defintion**](https://docs.microsoft.com/en-us/ef/core/api/microsoft.entityframeworkcore.dbcontext)  

A `DbContext` definition requires at minimum 2 pieces of configuration:
* Constructor definition
* Entity `DbSet` property definitions  

`DbContext` also defines an `OnModelCreating(ModelBuilder modelBuilder)` method that can be overridden to customize your entity schema. For detailed information, see [Creating and configuring a model](https://docs.microsoft.com/en-us/ef/core/modeling/).  

Here is an example `AppDbContext` definition that also performs some setup for the entities defined above (see the comments for details about what the configuration is doing):  

``` cs
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class AppDbContext : DbContext
{
    /*
        Get DbContextOptions from the Startup.cs configuration in {Project}.Web
    */
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /*
        Register entities as DbSet properties
    */
    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemTag> ItemTags { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /*
            Rename tables to their actual class name instead of the DbSet property name.
            This prevents pluralized table names in SQL.
        */
        modelBuilder
            .Model
            .GetEntityTypes()
            .ToList()
            .ForEach(x =>
            {
                modelBuilder
                    .Entity(x.Name)
                    .ToTable(x.Name.Split('.').Last());
            });

        /*
            Map the TagItems navigation property.
            By convention, EF will not understand TagItems because there is
            not a TagItem class.
        */
        modelBuilder
            .Entity<Tag>()
            .HasMany(x => x.TagItems)
            .WithOne(x => x.Tag)
            .HasForeignKey(x => x.TagId);

        /*
            Map the OriginLocation and OriginItems navigation properties.
            By convention, EF will not understand OriginLocation because
            there is not an OriginLocation class. Also, EF will not
            understand OriginItems because there is not an OriginItem class.
        */
        modelBuilder
            .Entity<Item>()
            .HasOne(x => x.OriginLocation)
            .WithMany(x => x.OriginItems)
            .HasForeignKey(x => x.OriginLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        /*
            Map the CurrentLocation and CurrentItems navigation properties.
            By convention, EF will not understand CurrentLocation because
            there is not a CurrentLocation class. Also, EF will not
            understand CurrentItems because there is not a CurrentItem class.
        */
        modelBuilder
            .Entity<Item>()
            .HasOne(x => x.CurrentLocation)
            .WithMany(x => x.CurrentItems)
            .HasForeignKey(x => x.CurrentLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

## [Registring Entity Framework with .NET Core](#data-access-layer)

In `appsettings.Development.json`, create a connection string to the database you will be working out of (`(localost)\\ProjectsV13` is typically the development database):  

``` json
{
    "ConnectionStrings": {
        "Dev": "Server=(localhost)\\ProjectsV13;Database={db-name};Trusted_Connection=True"
    }
}
```  

Register the `DbContext` in the `ConfigureServices()` method of `Startup.cs` in `{Project}.Web`:  

```cs
public class Startup
{
    Public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("Dev"))
        );
    }
}
```

## [Database Management Workflow](#data-access-layer)  

Whenever the entity schema is modified and you want the database to reflect the current state, a [migration](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/) is created and the database is updated. Given the updates to the schema made above, the following commands would be used to add a migration and update the database:  

```
{Project}.Data>dotnet ef migrations add "initial" -s ..\{Project}.Web
{Project}.Data>dotnet ef database update -s ..\{Project}.Web
```  

The `-s` flag indicates the startup project since `AppDbContext` is configured outside of the EF project. For a complete CLI reference, see the [Entity Framework Core tools reference](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet).

[Back to Top](#data-access-layer)