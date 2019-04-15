# Table of Contents

## [README](./readme.md)

* [Format](./readme.md#format)
* [Prerequisites](./readme.md#prerequisites)
* [Overview](./readme.md#overview)
* [Installing the App Stack Template](./readme.md#installing-the-app-stack-template)
* [Creating an App Stack Project](./readme.md#creating-an-app-stack-project)
* [Building and Running](./readme.md#building-and-running)
* [Visual Studio Code Extensions](./readme.md#visual-studio-code-extensions)
* [Visual Studio](./readme.md#visual-studio)

## .NET Core

### [.NET Core Overview](./01-overview.md)

* [Docs](./01-overview.md#docs)
* [Overview](./01-overview.md#overview)
* [Dependencies](./01-overview.md#dependencies)

### [Data Access Layer](./02-data-access-layer.md)

* [Project Infrastructure](./02-data-access-layer.md#project-infrastructure)
* [Building the Data Layer](./02-data-access-layer.md#building-the-data-layer)
    * [One to Many](./02-data-access-layer.md#one-to-many)
    * [Many to Many](./02-data-access-layer.md#many-to-many)
    * [Multiple One to Many Using the Same Table](./02-data-access-layer.md#multiple-one-to-many-using-the-same-table)
    * [DbContext](./02-data-access-layer.md#dbcontext)
* [Registring Entity Framework with .NET Core](./02-data-access-layer.md#registering-entity-framework-with-net-core)
* [Database Management Workflow](./02-data-access-layer.md#database-management-workflow)

### [Business Logic](./03-business-logic.md)

* [Extension Methods](./03-business-logic.md#extension-methods)
* [Asynchronous Operations](./03-business-logic.md#asynchronous-operations)
* [Retrieving Data](./03-business-logic.md#retrieving-data)
* [Managing Entities](./03-business-logic.md#managing-entities)
* [Validation](./03-business-logic.md#validation)
* [Example Extension Class](./03-business-logic.md#example-extension-class)

### [Core Configuration](./04-core-configuration.md)

* [Overview](./04-core-configuration.md#overview)
* [Extensions](./04-core-configuration.md#extensions)
* [Logging](./04-core-configuration.md#logging)

### [Dependency Injection and Middleware](./05-di-and-middleware.md)

* [Overview](./05-di-and-middleware.md#overview)
* [Dependency Injection](./05-di-and-middleware.md#dependency-injection)
    * [Dependency Lifetime](./05-di-and-middleware.md#dependency-lifetime)
    * [Dependency Registration](./05-di-and-middleware.md#dependency-registration)
    * [Using Registered Services](./05-di-and-middleware.md#using-registered-services)
* [Middleware](./05-di-and-middleware.md#middleware)
    * [Middleware Registration](./05-di-and-middleware.md#middleware-registration)
* [Custom Services and Middleware](./05-di-and-middleware.md#custom-services-and-middleware)
    * [Logging](./05-di-and-middleware.md#logging)
    * [Identity](./05-di-and-middleware.md#identity)
        * [Active Directory Provider](./05-di-and-middleware.md#active-directory-provider)
        * [Mock Provider](./05-di-and-middleware.md#mock-provider)

### [Web API](./06-web-api.md)

* [Controller Signature](./06-web-api.md#controller-signature)
    * [HTTP Conventions](./06-web-api.md#http-conventions)
    * [Getting Data](./06-web-api.md#getting-data)
    * [Posting Data](./06-web-api.md#posting-data)
* [Item Controller](./06-web-api.md#item-controller)

### [Database Seeding](./07-database-seeding.md)

* [DbInitializer](./07-database-seeding.md#dbinitializer)
* [dbseeder](./07-database-seeding.md#dbseeder)
    * [Debug](./07-database-seeding.md#debug)
    * [Publish](./07-database-seeding.md#publish)
    * [Scripts](./07-database-seeding.md#scripts)

## Angular


### [Angular Overview](./08-angular-overview.md)

* [Overview](./08-angular-overview.md#overview)
* [Integration](./08-angular-overview.md#integration)
    * [Startup](./08-angular-overview.md#startup)
    * [MSBuild](./08-angular-overview.md#msbuild)
* [Bootstrapping](./08-angular-overview.md#bootstrapping)
    * [Index](./08-angular-overview.md#index)
    * [Main](./08-angular-overview.md#main)
    * [AppModule](./08-angular-overview.md#appmodule)
* [Managing Dependencies](./08-angular-overview.md#managing-dependencies)

### [Angular CLI](./09-angular-cli.md)

* [Overview](./09-angular-cli.md#overview)
* [Angular Workspace](./09-angular-cli.md#angular-workspace)

### [Modules](./10-modules.md)

* [Overview](./10-modules.md#overview)
* [TypeScript Modules](./10-modules.md#typescript-modules)
    * [Components](./10-modules.md#components)
    * [Dialogs](./10-modules.md#dialogs)
    * [Models](./10-modules.md#models)
    * [Routes](./10-modules.md#routes)
    * [Services](./10-modules.md#services)
    * [Pipes](./10-modules.md#pipes)
* [Angular Modules](./10-modules.md#angular-modules)
    * [App Stack Modules](./10-modules.md#app-stack-modules)
    * [Material Module](./10-modules.md#material-module)
    * [Services Module](./10-modules.md#services-module)
    * [App Module](./10-modules.md#app-module)

### [Material](./11-material.md)

* [Overview](./11-material.md#overview)
* [Theming](./11-material.md#theming)
    * [Material Styles](./11-material.md#material-styles)
    * [Material Themes](./11-material.md#material-themes)
* [Components](./11-material.md#components)
    * [Menu](./11-material.md#menu)
    * [Slider](./11-material.md#slider)

### [Models](./12-models.md)

* [Overview](./12-models.md#overview)
    * [Transpiled JavaScript](./12-models.md#traspiled-javascript)
    * [Mapping Classes](./12-models.md#mapping-classes)
* [Implementation](./12-models.md#implementation)

### [Services](./13-services.md)

* [Overview](./13-services.md#overview)
* [Service Scope](./13-services.md#service-scope)
* [Observables](./13-services.md#observables)
* [Core Services](./13-services.md#core-services)
    * [ObjectMapService](./13-services.md#object-map-service)
    * [ThemeService](./13-services.md#theme-service)
    * [SnackerService](./13-services.md#snacker-service)
* [API Services](./13-services.md#api-services)
    * [ItemService](./13-services.md#item-service)

### [Components](./14-components.md)

* [Overview](./14-components.md#overview)
* [Anatomy](./14-components.md#anatomy)
* [Interpolation](./14-components.md#interpolation)
* [Binding Syntax](./14-components.md#binding-syntax)
* [Directives](./14-components.md#directives)
    * [Attribute Directives](./14-components.md#attribute-directives)
    * [Structural Directives](./14-components.md#structural-directives)
* [Template Reference Variables](./14-components.md#template-reference-variables)
* [Input and Output Properties](./14-components.md#input-and-output-properties)
* [Template Expression Operators](./14-components.md#template-expression-operators)
* [ViewChild](./14-components.md#viewchild)
* [Flex Layout](./14-components.md#flex-layout)
* [AppComponent](./14-components.md#appcomponent)
* [Display Components](./14-components.md#display-components)
    * [BannerComponent](./14-components.md#banner-component)
    * [FileUploadComponent](./14-components.md#fileuploadcomponent)
    * [ItemListComponent](./14-components.md#itemlistcomponent)
    * [ItemCardComponent](./14-components.md#itemcardcomponent)

### [Routes](./15-routes.md)



### [Pipes](./16-pipes.md)


## Appendix


### [Appendix Overview](./a1-appendix-overview.md)



### [RxJS](./a2-rxjs.md)



### [Attachments](./a3-attachments.md)



### [Child Routes](./a4-child-routes.md)



### [Dialogs](./a5-dialogs.md)



### [Authorization](./a6-authorization.md)



### [SignalR](./a7-signalr.md)



## References

### [Links](./r1-links.md)

[Back to Top](#table-of-contents)