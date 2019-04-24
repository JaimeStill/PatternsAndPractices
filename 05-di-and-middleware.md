# Dependency Injection and Middleware

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Dependency Injection](#dependency-injection)
    * [Dependency Lifetime](#dependency-lifetime)
    * [Dependency Registration](#dependency-registration)
    * [Using Registered Services](#using-registered-services)
* [Middleware](#middleware)
    * [Middleware Registration](#middleware-registration)
* [Custom Services and Middleware](#custom-services-and-middleware)
    * [Logging](#logging)
    * [Identity](#identity)
        * [Active Directory Provider](#active-directory-provider)
        * [Mock Provider](#mock-provider)

## [Overview](#dependency-injection-and-middleware)

The **{Project}.Web\\Startup.cs** class contains two methods:

* `ConfigureServices(IServiceCollection services)` - Configures application services via dependency injection
* `Configure(IApplicationBuilder app)` - Configure's the app's request processing (middleware) pipeline  

This article will cover the concepts of both Dependency Injection and Middleware in the context of <span>ASP.NET</span> Core.

## [Dependency Injection](#dependency-injection-and-middleware)

> For a detailed look at dependency injection, see [Dependency Injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2)  

In order to take advantage of Dependency Injection:
* A dependency must be registered in a service container. Service are registered in the `ConfigureService(IServiceCollection services)` method of the `Startup` class.
* The service can then be injected into the constructor of a class where it is used. The framework takes on the responsibility of creating an instance of the dependency and disposing of it when it's no longer needed.

> There are a number of [Framework-provided services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2#framework-provided-services).

### [Dependency Lifetime](#dependency-injection-and-middleware)

The lifetime of a service is specified when the dependency is registered. It can be registered with 3 distinct lifetimes:

Lifetime | Description
---------|------------
Transient | Created each time they're requested from the container. Useful for lightweight, stateless services.
Scoped | Created once per client request (connection).
Singleton | Created the first time they're requested. Each subsequent request uses the same instance.

When using a scoped service in a middleware, inject the service into the `Invoke` or `InvokeAsync` method. Don't inject vai constructor injection because it forces the service to behave like a singleton.

It's dangerous to resolve a scoped service from a singleton. It may cause the service to have incorrect state when processing subsequent requests.

### [Dependency Registration](#dependency-injection-and-middleware)

Outside of explicity specifying the above lifetimes, there are service registrations that don't explicity specify a service lifetime. For instance, when registering a `DbContext`, Entity Framework provides an `AddDbContext<TContext>()` method. The lifetime for this service will be scoped, but it is not explicitly stated in the service registration. It's important to be aware of this detail because mixing service lifetimes is very dangerous.  

With this said, here is the service registration as defined by the app stack template:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
        .AddJsonOptions(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });

    services.AddDbContext<AppDbContext>(options =>
    {
        if (Environment.IsDevelopment())
        {
            options.UseSqlServer(Configuration.GetConnectionString("Dev"));
            options.EnableSensitiveDataLogging();
        }
        else if (Environment.IsStaging())
        {
            options.UseSqlServer(Configuration.GetConnectionString("Test"));
            options.EnableSensitiveDataLogging();
        }
        else
        {
            options.UseSqlServer(Configuration.GetConnectionString("Project"));
        }
    });

    services.AddSpaStaticFiles(configuration =>
    {
        configuration.RootPath = "ClientApp/dist";
    });
}
```  

`services.AddMvc()` configures the <span>ASP.NET</span> Core service with the application. It then specifies the .NET Core compatibility version, then configures two important JSON serialization options using the `AddJsonOptions()` method:
* `ReferenceLoopHandling`
* `ContractResolver`  

`ReferenceLoopHandling.Ignore` means that when serializing an object to / from JSON, if a sub-property refers to the root property, it will simply not include it in the serialized object. This prevents infinite loop errors that resolve from serializing an infinite object graph.  

An example of this would be retrieving a `Category` and including all of the `Items` that the category contains. If the `Category` of the item is then included in the `Items` collection defined on the root `Category`, an infinite graph would continue to be drawn unless the category in the `Items` collection is ignored. To illustrate this:  

``` js
{
    id: 1,
    name: 'Category A',
    items: [
        {
            id: 1,
            name: 'Item A',
            category: {
                id: 1,
                name: 'Category A',
                items: [
                    {
                        id: 1,
                        name: 'Item A',
                        category: {
                            // you get the idea
                        }
                    }
                ]
            }
        }
    ]
}
```  

Using `ReferenceLoopHandling.Ignore`, the above example would become:

``` js
{
    id: 1,
    name: 'Category A',
    items: [
        {
            id: 1,
            name: 'Item A',
            category: null
        }
    ]
}
```  

`CamelCasePropertyNamesContractResolver()` is a bit of a mouthful, but it allows you to respect the typing convention of the languages that are being used on both the back end and front end. In C#, object properties are written in **PascalCase**, whereas in TypeScript, object properties are written in **camelCase**. When you set this as the `ContractResolver` in the serialization settings, the appropriate casing is used when serializing between object types.

This C# object:  

```cs
var item = new Item
{
    Name = "Item A",
    IsDeleted = false
}
```

will serialize to this:  

```ts
{
    name: "Item A",
    isDeleted: false
}
```  

As discussed above, `services.AddDbContext<AppDbContext>()` registers `AppDbContext` as a scoped service. One thing to note is that the service can be configured differently depending on the current environment. `Environment` is an instance of the framework-provided service `IHostingEnvironment`, and allows you to check for the current environment.  

`options.UseSqlServer(Configuration.GetConnectionString("Dev"))` specifies that the Microsoft SQL Server provider should be used for the context, and that the connection string should be retrieved from **ConnectionStrings.Dev** in the `Configuration` object. `Configuration` is an instance of the framework-provided service `IConfiguration`. Configuration can come from a variety of locations, but the three sources you should be familiar with are as follows:

* appsettings.json
* appsettings.{Environment}.json
* Environment Variables  

appsettings.{Environment}.json will only be loaded into configuration when the current environment matches the environment specified in **{Environment}**. For instance, **appsettings.Development.json** will only be included in the configuration when running in the Development environment.  

appsettings.json will be included regardless of environment.

Environment Variables are included regardless of environment, but because the target of execution varies by environment, they are still unique to the environment.  

> To map a nested configuration to an environment variable, for instance **ConnectionStrings.Test**, it should be specified as **ConnectionStrings_Test** as the environment variable key.

Finally, the `services.AddSpaStaticFiles()` registers an `ISpaStaticFileProvider` service that can provide static files to be served for a Single Page Application. See [AddSpaStaticFiles](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.spastaticfilesextensions.addspastaticfiles?view=aspnetcore-2.2).

### [Using Registered Services](#dependency-injection-and-middleware)  

There are two ways that you can inject a service with <span>ASP.NET</span> Core's depenency injection system:

* Constructor injection into classes
* `Invoke` and `InvokeAsync` injection into middleware  

**Example of Constructor Injection**  

```cs
public class UserController : Controller
{
    private AppDbContext db;
    private IUserProvider userProvider;

    public UserController(AppDbContext db, IUserProvider userProvider)
    {
        this.db = db;
        this.userProvider = userProvider;
    }

    // remaining class definition
}
```  

In this example, both the `AppDbContext` and `IUserProvider` instance are provided by the dependency injection container to the `UserController` class. If we're in the Development environment, `IUserProvider` will represent an instance of the `MockProvider` class. Otherwise, it will represent an instance of the `AdUserProvider` class.

**Example of Middleware Injection**  

```cs
public class AdUserMiddleare
{
    private readonly RequestDelegate next;

    public AdUserMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context, IUserProvider userProvider, IConfiguration config)
    {
        // Invoke implementation
    }
}
```  

In the above case, `IUserProvider` and `IConfiguration` are injected after the default `HttpContext` parameter.

## [Middleware](#dependency-injection-and-middleware)

Middleware is software that's assembled into an application pipeline to handle requests and responses. Each component:  
* Chooses whether to pass the request to the next component in the pipeline.  
* Can perform work before and after the next component in the pipeline is invoked.

Request delegates are used to build the request pipeline. The request delegates handle each HTTP request.

> Middleware is a very advanced topic, and covering it in depth in this documentation is beyond the scope of its purpose. The official documentation does a great job of expressing its use. [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.2).

### [Middleware Registration](#dependency-injection-and-middleware)

The middleware demonstrated in this section are examples of [Built-in middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.2#built-in-middleware). In the sections that follow, I'll outline how to build custom middleware.  

Here is an example middleware pipeline composed entirely of built-in middleware:  

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseDeveloperExceptionPage();

    app.UseStaticFiles();
    app.UseSpaStaticFiles();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller}/{action=Index}/{id?}"
        );
    });

    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";

        if (env.IsDevelopment())
        {
            spa.UseAngularCliServer(npmScript: "start");
        }
    });
}
```  

`app.UseDeveloperExceptionPage()` allows you to show detailed information about request exceptions. For detailed information, see [Handle errors in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-2.2).

The parameterless `app.UseStaticFiles()` marks the files in web root (**{Project}.Web\\wwwroot**) as servable. For detailed information, see [Static files in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-2.2).

`app.UseSpaStaticFiles()` configures the application to serve static files for a Single Page Application. The files will be located using the registered `ISpaStaticFileProvider` service (shown above at the bottom of the [Dependency Registration](#dependency-registration) section).

`app.UseMvc()` adds Routing Middleware to the request pipeline and configures MVC as the default handler. For detailed information, see [App startup in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-2.2) and [Routing in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2).

`app.UseSpa()` handles all requests from this point in the middleware chain by returning the default page for the Single Page Application. It specifies the path to the front end application (**{Project}.Web\\ClientApp**), and if in the development environment, configures the startup script for Angular CLI using the `spa.UseAngularCliServer()` method. For detailed information, see [UseSpa](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.spaapplicationbuilderextensions.usespa?view=aspnetcore-2.2) and [UseAngularCliServer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.spaservices.angularcli.angularclimiddlewareextensions.useangularcliserver?view=aspnetcore-2.2).

## [Custom Services and Middleware](#dependency-injection-and-middleware)

Middleware in <span>ASP.NET</span> Core has a specific signature it needs to follow:  

```cs
public class CustomMiddleware
{
    private readonly RequestDelegate next;

    public CustomMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Actions to perform up the middleware pipeline

        await next(context);

        // Action to perform down the middleware pipeline
    }
}
```  

Rules for middleware:

1. The class must contain a `private readonly RequestDelegate` field named `next`.
2. The constructor for the class must receive a `RequestDelegate` that is assigned to the `next` field.
3. Must contain an `async Task Invoke` method that receives an `HttpContext` as its first parameter.

In addition to these rules, the following features are available:
* Services can be injected in the `Invoke` method following the `HttpContext` parameter
* Any action that should occur as execution traverses *up* the pipeline should happen before the call to `await next(context)`.
* Any actions that should occur as execution traverses back *down* the pipeline should be executed after the call to `await next(context)`.  

Once Middleware has been defined, you can create a helper extension method for adding the middleware to the pipeline. The convention for this is to define a `MiddlewareExtensions` static class in the `Microsoft.AspNetCore.Builder` namespace:

```cs
using // namespace where middleware is defined

namespace Microsoft.AspNetCore.Builder
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<CustomMiddleware>();
}
```  

Then, inside of the `Configure()` method of `Startup.cs`, the middleware can be added to the pipeline as follows:

```cs
public void Configure(IApplicationBuilder app)
{
    // Middleware called before CustomMiddleware

    app.UseCustomMiddleware();

    // Middleware called after CustomMiddleware
}
```

### [Logging](#dependency-injection-and-middleware)

> Before tackling this section, I highly recommend familiarizing yourself with [lambda expressions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions), the [Action<T>](https://docs.microsoft.com/en-us/dotnet/api/system.action-1?view=netframework-4.7.2) delegate, and the [Func<T, TResult>](https://docs.microsoft.com/en-us/dotnet/api/system.func-2?view=netframework-4.7.2) delegate.

If you'll recall from [Core Configuration - Logging](./04-core-configuration.md#logging), a `LogProvider` class was created that formatted how exceptions are communicated to the client, as well as write a log file to the server. This section will cover configuring the provider in `Startup` and registering it with the middlware pipeline.  

First, an instance of `LogProvider` needs to be accessible from `Startup`:  

```cs
public class Startup
{
    public IConfiguration Configuration { get; }
    public IHostingEnvironment Environment { get; }
    public LogProvider Logger { get; }

    public Startup(IConfiguration config, IHostingEnvironment env)
    {
        Configuration = config;
        Environment = env;
        Logger = new LogProvider
        {
            LogDirectory = Configuration.GetValue<string>("LogDirectory") ??
                $@"{Environment.WebRootPath}\logs"
        };
    }
}
```  

The `LogProvider` instance doesn't need to be made available to the rest of the application, so it is not registered with the dependency injection container. Instead, it is made a read only public property on the `Startup` class. In the `Startup` constructor, the `LogProvider.LogDirectory` property (which defines where log files will be saved) is configured as either the value of the `LogDirectory` configuration variable, or the **{Project}.Web\\wwwroot\\logs** directory if there is not variable available.  

.NET Core has a framework-provided middleware called [UseExceptionHandler](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-2.2#configure-custom-exception-handling-code). The overload used for `UseExceptionHandler` receives an `Action<IApplicationBuilder>`. Writing a lambda expression for this argument allows us to use the `HandleError` extension method written in **{Project}.Core\\Extensions\\LogExtensions.cs** and provide our `LogProvider` instance to the method.  

Here's the `HandleError` extension method:

```cs
public static void HandleError(this IApplicationBuilder app, LogProvider logger)
{
    app.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>();
                
        if (error != null)
        {
            var ex = error.Error;
            await logger.CreateLog(context, ex);
        }
                
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
                
        if (error != null)
        {
            var ex = error.Error;
            await context.Response.WriteAsync(ex.GetExceptionChain(), Encoding.UTF8);
        }
    });
}
```

The `app.Run(Action<HttpContext>)` lambda function above allows you to write inline middleware (as opposed to writing a class as outlined above).  

Here is the middleware registration in `Startup`:

```cs
public void Configure(IApplicationBuilder app)
{
    // Middleware called before UseExceptionHandler

    app.UseExceptionHandler(err => err.HandleError(Logger));

    // Middleware called after UseExceptionHandler
}
```  

### [Identity](#dependency-injection-and-middleware)

The Logging example above provides a good introduction to middleware, but it does not cover the full scope of requiring services in Dependency Injection and does not use a custom middleware class. Identity, on the other hand, requires two entire projects to encapsulate its functionality. This section will walk through each of the pieces required for building the identity libraries, then show how all of the parts are registered.  

First, I will start with the infrastructure that is common between both libraries. **{Project}.Identity.Mock** references **{Project}.Identity** because it relies on both of the constructs.  

The `AdUser` class is an abstraction class that replicates properties of the `System.DirectoryServices.AccountManagement.UserPrincipal` class for represinting and [Active Directory User Principal](https://docs.microsoft.com/en-us/dotnet/api/system.directoryservices.accountmanagement.userprincipal?view=netframework-4.7.2).  

**`AdUser.cs`**

```cs
using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Demo.Core.Extensions;

namespace Demo.Identity
{
    public class AdUser
    {
        public DateTime? AccountExpirationDate { get; set; }
        public DateTime? AccountLockoutTime { get; set; }
        public int BadLogonCount { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }
        public string Domain { get; set; }
        public string EmailAddress { get; set; }
        public string EmployeeId { get; set; }
        public bool? Enabled { get; set; }
        public string GivenName { get; set; }
        public Guid? Guid { get; set; }
        public string HomeDirectory { get; set; }
        public string HomeDrive { get; set; }
        public DateTime? LastBadPasswordAttempt { get; set; }
        public DateTime? LastLogon { get; set; }
        public DateTime? LastPasswordSet { get; set; }
        public string MiddleName { get; set; }
        public string Name { get; set; }
        public bool PasswordNeverExpires { get; set; }
        public bool PasswordNotRequired { get; set; }
        public string SamAccountName { get; set; }
        public string ScriptPath { get; set; }
        public SecurityIdentifier Sid { get; set; }
        public string Surname { get; set; }
        public bool UserCannotChangePassword { get; set; }
        public string UserPrincipalName { get; set; }
        public string VoiceTelephoneNumber { get; set; }
        
        public static AdUser CastToAdUser(UserPrincipal user)
        {
            return new AdUser
            {
                AccountExpirationDate = user.AccountExpirationDate,
                AccountLockoutTime = user.AccountLockoutTime,
                BadLogonCount = user.BadLogonCount,
                Description = user.Description,
                DisplayName = user.DisplayName,
                DistinguishedName = user.DistinguishedName,
                EmailAddress = user.EmailAddress,
                EmployeeId = user.EmployeeId,
                Enabled = user.Enabled,
                GivenName = user.GivenName,
                Guid = user.Guid,
                HomeDirectory = user.HomeDirectory,
                HomeDrive = user.HomeDrive,
                LastBadPasswordAttempt = user.LastBadPasswordAttempt,
                LastLogon = user.LastLogon,
                LastPasswordSet = user.LastPasswordSet,
                MiddleName = user.MiddleName,
                Name = user.Name,
                PasswordNeverExpires = user.PasswordNeverExpires,
                PasswordNotRequired = user.PasswordNotRequired,
                SamAccountName = user.SamAccountName,
                ScriptPath = user.ScriptPath,
                Sid = user.Sid,
                Surname = user.Surname,
                UserCannotChangePassword = user.UserCannotChangePassword,
                UserPrincipalName = user.UserPrincipalName,
                VoiceTelephoneNumber = user.VoiceTelephoneNumber
            };
        }
        
        public string GetDomainPrefix() => DistinguishedName
            .Split(',')
            .FirstOrDefault(x => x.ToLower().Contains("dc"))
            .Split('=')
            .LastOrDefault()
            .ToUpper();
    }
}
```  

This class provides a convenience method for populating its properties from a `UserPrincipal` instance. The `GetDomainPrefix()` method is used to be able to send direct messages to users using SignalR.  

> SignalR will be reviewed at length in the [SignalR](./a6-signalr.md) article.

The `IUserProvider` interface exists so that multiple providers can be written. In this case, the true `AdUserProvider` that works in an Active Directory domain, and the `MockProvider` that implements the details of this interface in a way that allows us to work with these concepts outside of an Active Directory environment.  

**`IUserProvider`**  

```cs
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Demo.Identity
{
    public interface IUserProvider
    {
        AdUser CurrentUser { get; set; }
        bool Initialized { get; set; }
        Task Create(HttpContext context, IConfiguration config);
        Task Create(string samAccountName);
        Task AddIdentity(HttpContext context);
        Task<AdUser> GetAdUser(IIdentity identity);
        Task<AdUser> GetAdUser(string samAccountName);
        Task<AdUser> GetAdUser(Guid guid);
        Task<List<AdUser>> GetDomainUsers();
        Task<List<AdUser>> FindDomainUser(string search);
    }
}
```  

### [Active Directory Provider](#dependency-injection-and-middleware)

The remainder of the contents of **{Project}.Identity** are relevant to the genuine Active Directory provider used for integrating with an Active Directory domain.  

#### IdentityExtensions

There are two extension methods that are defined that simplify the functionality of the `GetDomainUsers()` and `FindDomainUser()` implementation methods of `IUserProvider`. They are defined in **{Project}.Identity\\Extensions\\IdentityExtensions.cs**.  

**`IdentityExtensions`**  

```cs
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace Demo.Identity.Extensions
{
    public static class IdentityExtensions
    {
        public static IQueryable<UserPrincipal> FilterUsers(this IQueryable<UserPrincipal> principals) =>
            principals.Where(x => x.Guid.HasValue);
            
        public static IQueryable<AdUser> SelectAdUsers(this IQueryable<UserPrincipal> principals) =>
            principals.Select(x => AdUser.CastToAdUser(x));
    }
}
```

`FilterUsers` is used to retrieve only `UserPrincipal` objects where `Guid.HasValue` is true.  

`SelectAdUsers` is used to effectively cast the `UserPrincipal` objects in the queried collection to `AdUser` objects.  

#### AdUserProvider Implementation

`AdUserProvider` provides the implementation details for the `IUserProvider` interface. It leverages the necessary features of the `System.DirectoryServices.AccountManagement` library for working with Active Directory. For detailed information, see the [System.DirectoryServices.AccountManagement](https://docs.microsoft.com/en-us/dotnet/api/system.directoryservices.accountmanagement?view=netframework-4.7.2) namespace documentation.  

**`AdUserProvider`**  

```cs
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Demo.Core.Extensions;
using Demo.Identity.Extensions;

namespace Demo.Identity
{
    public class AdUserProvider : IUserProvider
    {
        public AdUser CurrentUser { get; set; }
        public bool Initialized { get; set; }
        
        public async Task Create(HttpContext context, IConfiguration config)
        {
            CurrentUser = await GetAdUser(context.User.Identity);
            Initialized = true;
        }

        public Task AddIdentity(HttpContext context) => throw new NotImplementedException("Only used for MockIdentity implementation");

        public Task Create(string samAccountName) => throw new NotImplementedException("Use Create(HttpContext context) for UserProvider");
        
        public Task<AdUser> GetAdUser(IIdentity identity)
        {
            return Task.Run(() =>
            {
                try
                {
                    PrincipalContext context = new PrincipalContext(ContextType.Domain);
                    UserPrincipal principal = new UserPrincipal(context);
                    
                    if (context != null)
                    {
                        principal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, identity.Name);
                    }
                    
                    return AdUser.CastToAdUser(principal);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.GetExceptionChain());
                }
            });
        }
        
        public Task<AdUser> GetAdUser(string samAccountName)
        {
            return Task.Run(() =>
            {
                try
                {
                    PrincipalContext context = new PrincipalContext(ContextType.Domain);
                    UserPrincipal principal = new UserPrincipal(context);
                    
                    if (context != null)
                    {
                        principal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
                    }
                    
                    return AdUser.CastToAdUser(principal);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.GetExceptionChain());
                }
            });
        }
        
        public Task<AdUser> GetAdUser(Guid guid)
        {
            return Task.Run(() =>
            {
                try
                {
                    PrincipalContext context = new PrincipalContext(ContextType.Domain);
                    UserPrincipal principal = new UserPrincipal(context);
                    
                    if (context != null)
                    {
                        principal = UserPrincipal.FindByIdentity(context, IdentityType.Guid, guid.ToString());
                    }
                    
                    return AdUser.CastToAdUser(principal);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.GetExceptionChain());
                }
            });
        }
        
        public Task<List<AdUser>> GetDomainUsers()
        {
            return Task.Run(() =>
            {
                PrincipalContext context = new PrincipalContext(ContextType.Domain);
                UserPrincipal principal = new UserPrincipal(context);
                principal.UserPrincipalName = "*@*";
                principal.Enabled = true;
                PrincipalSearcher searcher = new PrincipalSearcher(principal);
                
                var users = searcher
                    .FindAll()
                    .AsQueryable()
                    .Cast<UserPrincipal>()
                    .FilterUsers()
                    .SelectAdUsers()
                    .OrderBy(x => x.Surname)
                    .ToList();
                    
                return users;
            });
        }
        
        public Task<List<AdUser>> FindDomainUser(string search)
        {
            return Task.Run(() =>
            {
                PrincipalContext context = new PrincipalContext(ContextType.Domain);
                UserPrincipal principal = new UserPrincipal(context);
                principal.SamAccountName = $"*{search}*";
                principal.Enabled = true;
                PrincipalSearcher searcher = new PrincipalSearcher(principal);
                
                var users = searcher
                    .FindAll()
                    .AsQueryable()
                    .Cast<UserPrincipal>()
                    .FilterUsers()
                    .SelectAdUsers()
                    .OrderBy(x => x.Surname)
                    .ToList();
                    
                return users;
            });
        }
    }
}
```  

The `Create` method is used to initialize the `AdUserProvider` service during execution in the middleware pipeline. It receives the current `HttpContext`, which contains the `IIdentity` of the current user in `HttpContext.User.Identity`. This property is then passed to the `GetAdUser()` method that this interface implementation defines, which sets the value of the `AdUser CurrentUser` property.

The `AddIdentity()` and `Create(string samAccountName)` methods are not implemented in this class because they are specific to the `MockProvider` class defined in the following section.  

`GetAdUser(IIdentity identity)` retrieves the `UserPrincipal` associated with the object using the `IIdentity.Name` property, and returns it as an `AdUser` instance.  

`GetAdUser(Guid guid)` functions the same as the above function, but retrieves the user by `Guid` rather than `IIdentity`.  

`GetDomainUsers()` retrieves all `UserPrincipal` objects in the domain where the object is enabled, contains an `@` in the `UserPrincipal.UserPrincipalName`, and `UserPrincipal.Guid.HasValue` is true. The results are then returned as a `List<AdUser>`.  

`FindDomainUser(string search)` retrieves all `UserPrincipal` objects in the domain where the object is enabled, the `UserPrincipal.SamAccountName` contains the value of `search`, and `UserPrincipal.Guid.HasValue` is true. The results are then returned as a `List<AdUser>`.  

#### AdUserMiddleware Implementation

With the provider written, a middleware class can now be written to create the `IUserProvider` instance. **{Project}.Identity\\AdUserMiddleware.cs** contains the definition of the middleware implementation.  

**`AdUserMiddleware`**  

```cs
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Demo.Identity
{
    public class AdUserMiddleware
    {
        private readonly RequestDelegate next;
        
        public AdUserMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        
        public async Task Invoke(HttpContext context, IUserProvider userProvider, IConfiguration config)
        {
            if (!(userProvider.Initialized))
            {
                await userProvider.Create(context, config);
            }
            
            await next(context);
        }
    }
}
```

When the `Invoke` method is called by the middleware pipeline, the `Create` method of the injected `IUserProvider` instance is called.  

#### AdUserMiddleware Convenience Method

**{Project}.Identity\\Extensions\\MiddlewareExtensions.cs** contains the convenience method for registering `AdUserMiddleware` to the middleware pipeline.  

**`MiddlewareExtensions`**  

```cs
using Demo.Identity;

namespace Microsoft.AspNetCore.Builder
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseAdMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<AdUserMiddleware>();
    }
}
```  

#### Startup Configuration

Now, the service and middleware can be registered in `Startup`.  

**`Startup`**  

> Unrelated details have been omitted for simplicity

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Services registered before IUserProvider

    services.AddScoped<IUserProvider, AdUserProvider>();

    // Services registered after IUserProvider
}

public void Configure(IApplicationBuilder app)
{
    // Middleware added before AdUserMiddleware

    app.UseAdMiddleware();

    // Middleware added after AdUserMiddleware
}
```  

`IUserProvider` is injected into the `Invoke` method of `AdUserMiddleware`, so it needs to be registered with the dependency injection container. Because each user session will have a unique identity, the service needs to be registered with the **Scoped** lifetime.  

`app.UseAdMiddleware()` is simply called in `Configure()`.

### [Mock Provider](#dependency-injection-and-middleware)

The following classes are defined in the `{Project}.Identity.Mock` library, and make use of the `AdUser` class and `IUserProvider` interface defined above.  

#### MockProvider Implementation

Because we took the care of building an `IUserProvider` interface, we aren't locked into working with `IUserProvider` in an Active Directory domain. This implementation defines all of the interactions necessary for setting up Cookie Authentication and building the implementation of `IUserProvider` around mocking some Active Directory User Principal objects.

**`MockProvider`**  

```cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Demo.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Demo.Identity.Mock
{
    public class MockProvider : IUserProvider
    {
        public AdUser CurrentUser { get; set; }
        public bool Initialized { get; set; }
        public Task Create(HttpContext context, IConfiguration config) => throw new NotImplementedException("Use Create(string samAccountName) for MockProvider");

        public async Task Create(string samAccountName)
        {
            CurrentUser = await GetAdUser(samAccountName);
            Initialized = true;
        }

        public async Task AddIdentity(HttpContext context)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, CurrentUser.SamAccountName),
                new Claim(ClaimTypes.Email, CurrentUser.UserPrincipalName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props
            );
        }

        public Task<AdUser> GetAdUser(IIdentity identity) => throw new NotImplementedException("Use GetAdUser(string samAccountName) for MockProvider");

        public Task<AdUser> GetAdUser(string samAccountName) =>
            Task.Run(() =>
                AdUsers.FirstOrDefault(x =>
                    x.SamAccountName.ToLower().Equals(samAccountName.ToLower())
                )
            );

        public Task<AdUser> GetAdUser(Guid guid) =>
            Task.Run(() =>
                AdUsers.FirstOrDefault(x => guid.Equals(x.Guid.Value))
            );

        public Task<List<AdUser>> GetDomainUsers() =>
            Task.Run(() =>
                AdUsers.ToList()
            );

        public Task<List<AdUser>> FindDomainUser(string search)
        {
            return Task.Run(() =>
            {
                search = search.ToLower();

                var users = AdUsers
                    .Where(
                        x => x.SamAccountName.ToLower().Contains(search) ||
                        x.UserPrincipalName.ToLower().Contains(search) ||
                        x.DisplayName.ToLower().Contains(search)
                    )
                    .OrderBy(x => x.Surname)
                    .ToList();

                return users;
            });
        }

        private static string baseDn = "CN=Users,DC=Mock,DC=Net";

        private static IQueryable<AdUser> AdUsers = new List<AdUser>()
        {
            new AdUser
            {
                DisplayName = "Graham, Leanne",
                DistinguishedName = $"CN=lgraham,{baseDn}",
                EmailAddress = "lgraham@mock.net",
                Enabled = true,
                GivenName = "Leanne",
                Guid = Guid.Parse("c40bcced-28cd-406e-84c0-2d1d446b9a63"),
                SamAccountName = "lgraham",
                Surname = "Graham",
                UserPrincipalName = "lgraham@mock.net",
                VoiceTelephoneNumber = "555.555.0001"
            },
            new AdUser
            {
                DisplayName = "Howell, Ervin",
                DistinguishedName = $"CN=ehowell,{baseDn}",
                EmailAddress = "ehowell@mock.net",
                Enabled = true,
                GivenName = "Ervin",
                Guid = Guid.Parse("f16f6b21-c2d9-4dcf-a8d2-96906ca49872"),
                SamAccountName = "ehowell",
                Surname = "Howell",
                UserPrincipalName = "ehowell@mock.net",
                VoiceTelephoneNumber = "555.555.0002"
            },
            // Additional users removed for brevity
        }.AsQueryable();
    }
}
```  

The `Create(HttpContext context, IConfiguration config)` method overload is not implemented as it is specific to the `AdUserProvider` implementation.  

The `Create(string samAccountName)` method overload receives a string representing the `SamAccountName` of an account. It executes the `GetAdUser(string samAccountName)` method overload and assigns the result to the `MockProvider.CurrentUser` property.  

The `AddIdentity` method configures the `ClaimsIdentity` for the mock user based on the currently selected account, and activates cookie authentication based on that identity. For detailed information, see [Use cookie authentication without ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-2.2).  

The `GetUser(IIdentity identity)` method overload is not implemented as it is specific to the `AdUserProvider` implementation.  

The `GetAdUser(string samAccountName)` and `GetAdUser(Guid guid)` method overloads simply use LINQ with their provided arguments against the built-in collection of user objects to return a user.  

`GetDomainUsers()` simply returns the built-in collection of user objects as a `List<AdUser>`.  

`FindDomainUser(string search)` uses LINQ to compare the provided `search` argument against the `SamAccountName`, `UserPrincipalName`, and `DisplayName` of the built-in collection of user objects, and returns the matching results, ordered by `Surname`, as a `List<AdUser>`.  

#### MockMiddleware Implementation

With the provider defined, `MockMiddleware` can now be created to manage the mock authentication pipeline.  

**`MockMiddleware`**  

```cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Demo.Identity.Mock
{
    public class MockMiddleware
    {
        private readonly RequestDelegate next;

        public MockMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IUserProvider provider, IConfiguration config)
        {
            if (!(provider.Initialized))
            {
                await provider.Create(config.GetValue<string>("CurrentUser"));

                if (!(context.User.Identity.IsAuthenticated))
                {
                    await provider.AddIdentity(context);
                }
            }

            await next(context);
        }
    }
}
```  

The `Invoke` method of the middleware class retrieves the `CurrentUser` variable out of the `IConfiguration` container that is injected. This is set in **appsettings.Development.json**. Then, if the identity on the `HttpContext` instance isn't authenticated, the cookie authentication is executed using the `AddIdentity` method.  

#### UseMockMiddleware Convenience Method

Just as a convenience method for `AdUserMiddleware` was created for registering the middleware with the pipeline, a method is created for `MockMiddleware` in **{Project}.Identity.Mock\\MiddlewareExtensions.cs**.  

**`MiddlewareExtensions`**  

```cs
using Demo.Identity.Mock;

namespace Microsoft.AspNetCore.Builder
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMockMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<MockMiddleware>();
    }
}
```  

#### Startup Configuration

With the library now complete, it's time to hook everything up in `Startup`. We only want to use the `MockProvider` when we're in the Development environment, and `AdUserProvider` otherwise. The changes shown in the service registration and middleware configuration below demonstrate how to accomplish this.  

**`Startup`**  

> Unrelated details have been omitted for simplicity

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // services registered before IUserProvider

        if (Environment.IsDevelopment())
        {
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddScoped<IUserProvider, MockProvider>();
        }
        else
        {
            services.AddScoped<IUserProvider, AdUserProvider>();
        }

        // services registered after IUserProvider
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        // middleware configured before identity

        if (env.IsDevelopment())
        {
            app.UseAuthentication();
            app.UseMockMiddleware();
        }
        else
        {
            app.UseAdMiddleware();
        }

        // middleware configured after identity
    }
}
```

If we're in the Development environment, we add the cookie authentication services in service registration using the default cookie authentication scheme. Then, we add a scoped instance of `IUserProvider` resolved as `MockProvider`. Otherwise, we just use the `AdUserProvider` implementation.  

In the middleware pipeline, if we're in the Development environment, we setup the authentication middleware, then `MockMiddleware`. Cookie authentication needs to be configured first in the pipeline, or when `MockMiddleware` attempts to add the cookie authentication, it will throw an exception. If we're not in the Development environment, `AdUserMiddleware` is registered in the pipeline.

[Back to Top](#dependency-injection-and-middleware)