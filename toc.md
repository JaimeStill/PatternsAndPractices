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
  * [CoreService](./13-services.md#coreservice)
  * [ObjectMapService](./13-services.md#objectmapservice)
  * [ThemeService](./13-services.md#themeservice)
  * [SnackerService](./13-services.md#snackerservice)
* [API Services](./13-services.md#api-services)
  * [ItemService](./13-services.md#itemservice)

### [Components](./14-components.md)

* [Overview](./14-components.md#overview)
* [Anatomy](./14-components.md#anatomy)
  * [Component Decorator](./14-components.md#component-decorator)
* [Interpolation](./14-components.md#interpolation)
* [Binding Syntax](./14-components.md#binding-syntax)
* [Directives](./14-components.md#directives)
  * [Attribute Directives](./14-components.md#attribute-directives)
  * [Structural Directives](./14-components.md#structural-directives)
* [Template Reference Variables](./14-components.md#template-reference-variables)
* [Input and Output Properties](./14-components.md#input-and-output-properties)
* [Template Expression Operators](./14-components.md#template-expression-operators)
  * [Safe Navigation](./14-components.md#safe-navigation)
  * [Pipes](./14-components.md#pipes)
    * [Numeric Pipes](./14-components.md#numeric-pipes)
    * [Casing Pipes](./14-components.md#casing-pipes)
    * [Json Pipe](./14-components.md#json-pipe)
    * [Date Pipe](./14-components.md#date-pipe)
    * [KeyValue Pipe](./14-components.md#keyvalue-pipe)
    * [Slice Pipe](./14-components.md#slice-pipe)
    * [Async Pipe](./14-components.md#async-pipe)
* [Lifecycle Hooks](./14-components.md#lifecycle-hooks)
  * [Use Cases](./14-components.md#use-cases)
  * [Best Practices](./14-components.md#best-practices)
* [ViewChild](./14-components.md#viewchild)
  * [RxJS fromEvent with ViewChild](./14-components.md#rxjs-fromevent-with-viewchild)
* [Flex Layout](./14-components.md#flex-layout)
* [Content Projection](./14-components.md#content-projection)

### [Root Component](./15-root-component.md)

* [Overview](./15-root-component.md#overview)
* [Walkthrough](./15-root-component.md#walkthrough)

### [Display Components](./16-display-components.md)

* [Overview](./16-display-components.md#overview)
* [BannerComponent](./16-display-components.md#bannercomponent)
* [FileUploadComponent](./16-display-components.md#fileuploadcomponent)
* [API Components](./16-display-components.md#api-components)
  * [ItemCardComponent](./16-display-components.md#itemcardcomponent)
  * [ItemListComponent](./16-display-components.md#itemlistcomponent)

### [Routes](./17-routes.md)

* [Angular Routing](./17-routes.md#angular-routing)
* [Route Components](./17-routes.md#route-components)
  * [ItemsComponent](./17-routes.md#itemscomponent)
  * [ItemComponent](./17-routes.md#itemcomponent)

### [Pipes](./18-pipes.md)

* [Overview](./18-pipes.md#overview)
* [Anatomy](./18-pipes.md#anatomy)
* [Custom Pipes](./18-pipes.md#custom-pipes)
  * [UrlEncodePipe](./18-pipes.md#urlencodepipe)
  * [TruncatePipe](./18-pipes.md#truncatepipe)

## Appendix

### [RxJS](./a1-rxjs.md)

* [Overview](./a1-rxjs.md#overview)
* [Subjects](./a1-rxjs.md#subjects)
  * [BehaviorSubject](./a1-rxjs.md#behaviorsubject)
  * [AsyncSubject](./a1-rxjs.md#asyncsubject)
  * [ReplaySubject](./a1-rxjs.md#replaysubject)
* [Operators](./a1-rxjs.md#operators)
  * [Randomizer](./a1-rxjs.md#randomizer)
  * [Slideshow](./a1-rxjs.md#slideshow)
  * [Photographers](./a1-rxjs.md#photographers)
  * [Show Search](./a1-rxjs.md#show-search)
* [Piping Operators](./a1-rxjs.md#piping-operators)

### [Advanced Routing](./a2-advanced-routing.md)

* [Overview](./a2-advanced-routing.md#overview)
  * [base-href](./a2-advanced-routing.md#base-href)
  * [router-outlet](./a2-advanced-routing.md#router-outlet)
  * [Router Links](./a2-advanced-routing.md#router-links)
  * [Active Router Links](./a2-advanced-routing.md#active-router-links)
  * [Route Parameters](./a2-advanced-routing.md#route-parameters)
  * [Activated Route](./a2-advanced-routing.md#activated-route)
  * [Routing Example](./a2-advanced-routing.md#routing-example)
* [Child Routes](./a2-advanced-routing.md#child-routes)
  * [Child Routes Example](./a2-advanced-routing.md#child-routes-example)
* [Secondary Routes](./a2-advanced-routing.md#secondary-routes)
  * [Secondary Routes Example](./a2-advanced-routing.md#secondary-routes-example)

### [Dialogs](./a3-dialogs.md)

* [Overview](./a3-dialogs.md#overview)
* [Dialog Basics](./a3-dialogs.md#dialog-basics)
  * [Dialog Options](./a3-dialogs.md#dialog-options)
  * [Data Injection](./a3-dialogs.md#data-injection)
  * [Layout](./a3-dialogs.md#layout)
  * [Basic Example](./a3-dialogs.md#basic-example)
* [Action Dialogs](./a3-dialogs.md#action-dialogs)
  * [Confirm Dialog](./a3-dialogs.md#confirm-dialog)
  * [Editor Dialog](./a3-dialogs.md#editor-dialog)
  * [Bin Dialog](./a3-dialogs.md#bin-dialog)

### [Uploads](./a4-uploads.md#)

* [Overview](./a4-uploads.md#overview)
* [.NET Core](./a4-uploads.md#net-core)
  * [Configuration](./a4-uploads.md#configuration)
  * [Data Layer](./a4-uploads.md#data-layer)
  * [Business Logic](./a4-uploads.md#business-logic)
  * [Controller](./a4-uploads.md#controller)
* [Angular](./a4-uploads.md#angular)
  * [Model](./a4-uploads.md#model)
  * [Service](./a4-uploads.md#service)
  * [Pipe](./a4-uploads.md#pipe)
  * [Components](./a4-uploads.md#components)
  * [Dialogs](./a4-uploads.md#dialogs)
  * [Routes](./a4-uploads.md#routes)
* [Related Data](./a4-uploads.md#related-data)
  * [Folders Back End](./a4-uploads.md#folders-back-end)
  * [Folders Front End](./a4-uploads.md#folders-front-end)
    * [Models](./a4-uploads.md#models)
    * [Services](./a4-uploads.md#services)
    * [Components](./a4-uploads.md#components)
    * [Dialogs](./a4-uploads.md#dialogs)
    * [Routes](./a4-uploads.md#routes)
    * [Updated App Component](./a4-uploads.md#updated-app-component)
* [Example App](./a4-uploads.md#example-app)

### [Security](./a5-security.md)

* [Overview](./a5-security.md#overview)
* [Authentication](./a5-security.md#authentication)
* [Authorization](./a5-security.md#authorization)
  * [API Guards](./a5-security.md#api-guards)
  * [Route Guards](./a5-security.md#route-guards)

### [SignalR](./a6-signalr.md)



### [Git Workflow](./a7-git-workflow.md)



## References

### [Links](./r1-links.md)

* [Software](./r1-links.md#software)
* [Visual Studio Code](./r1-links.md#visual-studio-code)
* [Visual Studio](./r1-links.md#visual-studio)
* [Awesome Web Apps](./r1-links.md#awesome-web-apps)
* [C#](./r1-links.md#c)
* [.NET Core](./r1-links.md#net-core)
  * [ASP.NET Core](./r1-links.md#aspnet-core)
  * [Entity Framework Core](./r1-links.md#entity-framework-core)
* [Web Technologies](./r1-links.md#web-technologies)
* [Angular](./r1-links.md#angular)
  * [Angular CLI](./r1-links.md#angular-cli)
  * [Angular Material](./r1-links.md#angular-material)
  * [Angular CDK](./r1-links.md#angular-cdk)
  * [Angular Flex Layout](./r1-links.md#angular-flex-layout)

### [Examples](./r2-examples.md)

* [Angular Material](./r2-examples.md#angular-material)
* [Components](./r2-examples.md#components)
  * [Display Components](./r2-examples.md#display-components)
  * [Route Components](./r2-examples.md#route-components)
* [Pipes](./r2-examples.md#pipes)
* [RxJS](./r2-examples.md#rxjs)
* [Advanced Routing](./r2-examples.md#advanced-routing)
* [Dialogs](./r2-exapmles.md#dialogs)
* [Uploads](./r2-examples.md#uploads)

[Back to Top](#table-of-contents)