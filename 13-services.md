# Services

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Service Scope](#service-scope)
* [Observables](#observables)
* [Core Services](#core-services)
    * [ObjectMapService](#objectmapservice)
    * [ThemeService](#themeservice)
    * [SnackerService](#snackerservice)
* [API Services](#api-services)
    * [ItemService](#itemservice)

## [Overview](#services)

The [Angular docs](https://angular.io/guide/architecture-services) say it best:

> *Service* is a broad category encompassing any value, function, or feature that an app needs. A service is typically a class with a narrow, well-defined purpose. It should do something specific and do it well.

As stated in the [Angular Overview](./08-angular-overview) article, Angular is a full front-end framework. One of the core tenants of this framework is the dependency injection system. A service is nothing more than a TypeScript class that is marked with the `@Injectable()` decorator. This decorator allows Angular to manage the lifetime of the class, and appropriately control access to the service within the component tree.

## [Service Scope](#services)

The `NgModule` and `Component` decorators contain a `providers` array. A TypeScript class marked with the `@Injectable()` decorator can be added to the `providers` array of an Angular Module or Component. This enables Angular to manage the following things:

* Access to the service
* When the service is created and destroyed

To access a service from the `providers` array, you simply inject it into the constructor of the component that should use it:

```ts
import { Component } from '@angular/core';
import { ExampleService } from '../../services';

@Component({
    selector: 'example',
    templateUrl: 'example.component.html',
    providers: [ ExampleService ]
})
export class ExampleComponent {
    constructor(
        public service: ExampleService
    ) { }
}
```

The dependency injection process can be explained looking at the following diagram:

[![dependency-injection](./images/13.01-dependency-injection.png)](./images/13.01-dependency-injection.png)

When a service is injected into the constructor of a component, it will perform the following steps:

* Check its own `providers` array for the requested service
    * If it is found, use that instance
* Traverse up the graph, searching the `providers` array for the requested service
    * The first instance it encounters is the instance that will be used
    * If no instance is available, an error is thrown

With this in mind, the following table expresses how the services in the diagram would be resolved for the components in the diagram:

Component | Service | Provider
----------|---------|---------
Component A | Service A | Component A
Component A | Service B | Module
Component A | Service C | Root Component
Component A | Service D | ***Error thrown!***
Component B | Service A | Module
Component B | Service B | Module
Component B | Service C | Root Component
Root Component | Service A | Module
Root Component | Service B | Module
Root Component | Service C | Root Component

With this in mind, it's important to understand that the state maintained by **Service A** in **Component B** and **Root Component** will be the same, while it will be different for **Component A**. 

* If **Service A** defines a `getItems()` function that populates an observable stream property, that property will be the same in **Component B** and **Root Component**: only one of the components would need to call `getItems()` for the stream to be populated for both components. 
* Conversely, **Component A** would need to call `getItems()` in order to populate the observable stream for its own instance of **Service A**.

## [Observables](#services)

> The explanation of RxJS Observables and Subjects in this section is intended to provide just enough information to understand how they work in the context of service state. RxJS will be covered in more detail in the [RxJS](./a2-rxjs.md) article.



## [Core Services](#services)

### [ObjectMapService](#services)

### [ThemeService](#services)

### [SnackerService](#services)

## [API Services](#services)

### [ItemService](#services)

[Back to Top](#services)