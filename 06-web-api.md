# Web API  

[Table of Contents](./toc.md)

* [Controller Signature](#controller-signature)
    * [HTTP Conventions](#http-conventions)
    * [Getting Data](#getting-data)
    * [Posting Data](#posting-data)
* [Item Controller](#item-controller)

The previous two sections took a deep dive on configuration, services, and middleware in <span>ASP.NET</span> Core. This article picks back up where [Business Logic]() left off. That detour needed to happen so that you can understand how constructor injection works and where the `AppDbContext` instance is coming from (in addition to needing a place to put the other wealth of information those sections provide).

## [Controller Signature](#web-api)

This section isn't particularly long, and for good reason. Setting up all of the business logic in extension methods allows us to encapsulate all of the functionality of the application, and keep the controllers lightweight. Their express purpose is simply to define HTTP endpoints that the client app can call to reach the business logic presented. If you want a deeper understanding of controllers outside of the context of this documentation, start with [Controllers](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-2.2) in the documentation.  

A controller without any action methods has this basic type signature:  

```cs
[Route("api/[controller]")]
public class DataController : Controller
{

}
```  

The `RouteAttribute` defined at the top of the controller class definition specifies that this endpoint can be reached at **{app-url}/api/data**. By convention, specifying `[controller]` will set the endpoint route as the controller name, minus the **Controller** postfix.  

Suppose all of the business logic defined for the `Data` entity type was defined as extension methods pinned to an `AppDbContext` instance. `AppDbContext` would need to be injected into the controller:  

```cs
[Route("api/[controller]")]
public class DataController : Controller
{
    private AppDbContext db;

    public DataController(AppDbContext db)
    {
        this.db = db;
    }
}
```  

### [HTTP Conventions](#web-api)

There are a plethora of HTTP request types available in <span>ASP.NET</span> Core, however, it's only necessary to work with two:

* `HttpGet` - Used whenever data is retrieved and any data provided only needs to be specified in the route of the request
* `HttpPost` - Used whenever data needs to be sent in the body of the HTTP request

Any data that is sent to the controller via **POST** must be serializable to the type that the controller specifies as the parameter type in the action method being called.  

### [Getting Data](#web-api)

To send data via the route of a request, the `HttpGet` attribute can be specified as follows:  

```cs
[HttpGet("[action]/{value}")]
public async Task<List<Data>> GetData([FromRoute]string value) => await GetData(value);
```  

In this example, `[action]` corresponds to `GetData`. Similar to how `[controller]` is used in the controller class definition, `[action]` specifies the endpoint the action method can be reached at in relation to the base controller endpoint.

`{value}` corresponds to some value that is passed in via the route URL. The value provided must be serializable to the type specified as the parameter for the action method (in this case, a string).

In order to retrieve the value of `{value}`, the arguments for `GetData` must specify the `[FromRoute]` attribute. This tells the action method where to look for the argument when the method is called.  

With all of this in mind, this action method can be reached by executing a **GET** request to **{app-url}/api/data/getData/something** given that it is defined in the above `DataController`.  

### [Posting Data](#web-api)

To post data via the body of a request, the `HttpPost` attribute can be specified as follows:

```cs
[HttpPost("[action]")]
public async Task PostData([FromBody]Data data) => await PostData(data);
```  

Again, the `[action]` argument to `HttpPost` specifies that the endpoint can be reached as the name of the action method, **PostData**.  

An object of type `Data` is serialized from the body of the HTTP request because the `[FromBody]` attribute is specified in the parameter list and tells the action method where to find the argument when it is called.  

This action method can be reached by executing a **POST** request to **{app-url}/api/data/postData** given that it is defined in the above `DataController` and the JSON object provided in the body of the request can be serialized to an object of type `Data`.

You can combine `[FromRoute]` and `[FromBody]` in the same action method:  

```cs
[HttpPost("[action]/{id}")]
public async Task PostAssociatedData([FromRoute]int id, [FromBody]Data data) => await PostAssociatedData(id, data);
```  

This works exactly as expected. The `id` route parameter must be able to be serialized as an `int`, and the JSON object posted in the HTTP request must be able to be serialized to an object of type `Data`. Assuming the action method is defined in `DataController`, it can be reached by executing a **POST** request to **{app-url}/api/data/postAssociatedData/1**.  

## [Item Controller](#web-api)  

Here is a full example of an `ItemController` that is created based on the business logic defined for the `Item` entity type:  

**`ItemController`**  

```cs
using Demo.Data;
using Demo.Data.Entities;
using Demo.Data.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.Web.Controllers
{
    [Route("api/[controller]")]
    public class ItemController : Controller
    {
        private AppDbContext db;

        public ItemController(AppDbContext db)
        {
            this.db = db;
        }

        [HttpGet("[action]")]
        public async Task<List<Item>> GetItems() => await db.GetItems();

        [HttpGet("[action]")]
        public async Task<List<Item>> GetDeletedItems() => await db.GetItems(true);

        [HttpGet("[action]/{search}")]
        public async Task<List<Item>> SearchItems([FromRoute]string search) => await db.SearchItems(search);

        [HttpGet("[action]/{search}")]
        public async Task<List<Item>> SearchDeletedItems([FromRoute]string search) => await db.SearchItems(search, true);

        [HttpGet("[action]/{id}")]
        public async Task<Item> GetItem([FromRoute]int id) => await db.GetItem(id);

        [HttpPost("[action]")]
        public async Task AddItem([FromBody]Item item) => await db.AddItem(item);

        [HttpPost("[action]")]
        public async Task UpdateItem([FromBody]Item item) => await db.UpdateItem(item);

        [HttpPost("[action]")]
        public async Task ToggleItemDeleted([FromBody]Item item) => await db.ToggleItemDeleted(item);

        [HttpPost("[action]")]
        public async Task RemoveItem([FromBody]Item item) => await db.RemoveItem(item);
    }
}
```  

Things to note:  
* The `GetItems()` and `GetDeletedItems()` action methods call the same extension method, but `GetDeletedItems()` passes `true` as an argument to `GetItems(true)` to change the option parameter of `isDeleted` to `true`. This is great because it allows us to define one method for two different scenarios.
* The same is accomplished with the `SearchItems()` extension method called by the `SearchItems()` and `SearchDeletedItems()` action methods.
* All of the action methods are executed as asynchronous tasks. Multiple clients calling multiple action methods simultaneously will not lock up the main thread of execution and will allow for efficient distributed method execution.
* The signature of the controller is super light because all of the business logic is offloaded to extension methods. The controller simply maps HTTP endpoints to the extension methods they are associated with. Each controller should correspond to an extension class that contains the logic for the endpoints it exposes.

[Back to Top](#web-api)