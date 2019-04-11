# Modules

[Table of Contents](./toc.md)

* [Overview](#overview)
* [TypeScript Modules](#typescript-modules)
    * [Components](#components)
    * [Dialogs](#dialogs)
    * [Models](#models)
    * [Routes](#routes)
    * [Services](#services)
    * [Pipes](#pipes)
* [Angular Modules](#angular-modules)
    * [App Stack Modules](#app-stack-modules)
    * [Material Module](#material-module)
    * [Services Module](#services-module)
    * [App Module](#app-module)

## [Overview](#modules)

The idea of modularity deals with breaking a complex system down into self-contained, reusable components and building the system based on the public API made avaiable by these components. Both TypeScript and Angular support the concept of modules, and the app stack takes advantage of both systems. This article will walk through how each module system is implemented.

> For detailed information on both module systems, check out the following documentation:
> * [TypeScript Modules](https://www.typescriptlang.org/docs/handbook/modules.html)
> * [Angular Modules](https://angular.io/guide/ngmodules)

## [Typescript Modules](#modules)

To explain why using TypeScript modules in the app stack is helpful, it's important to take a quick look at how unruly managing Angular imports can get without them. The following is a small example of a module that I wrote before I started implementing TypeScript modules:

**`AppModule`**

``` ts
import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { AppService } from './services/app.service';

import { AppComponent } from './app.component';
import { MessageComponent } from './message.component';
import { ConsoleComponent } from './console.component';

const routes: Routes = [
  { path: 'home', component: AppComponent },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', component: AppComponent }
]

@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    RouterModule.forRoot(routes)
  ],
  providers: [ AppService ],
  declarations: [ 
    AppComponent,
    MessageComponent,
    ConsoleComponent
  ],
  bootstrap: [ AppComponent ]
})
export class AppModule { }
```

This is a very small application, and at first glance, there might not seem like there's anything wrong with this. But when you consider adding a significant number of components, routes associated with those components, and globally-scoped services, this format can quickly become unruly.

> Angular modules will be covered directly after the TypeScript module section. There has to be overlap somewhere because they are directly related topics, and you have to start somewhere! Don't worry if the characteristics of the Angular module don't make sense yet. They'll be fully covered below. This section is just concerned with how TypeScript modules drastically improve the maintainability of Angular modules.

The Angular app in the app stack comes pre-populated with the following folders within **{Project}.Web\\ClientApp\\src\\app**:

* **components**
* **dialogs**
* **models**
* **pipes**
* **routes**
* **services**

> All of these features will be covered in depth in their own articles. For now, the focus is on how making these features modular makes life easier.

Each folder is its own TypeScript module and manages the features it contains. How do you create a TypeScript module? All you have to do is create an `index.ts` folder at the root of any directory that you want to contain your module, and export any classes, functions, and variables that you want to be able to use outside of the module. The following sections will demonstrate how these modules are created for all of the core feature areas of the Angular app in the app stack. Then, the **Angular Modules** section that will follow will bring everything together to show how this setup drastically simplifies managing Angular modules.

### [Components](#modules)  

The **components** module comes pre-loaded with the following structure:

* **banner**
* **file-upload**
* `index.ts`

**banner** contains a `BannerComponent` for rendering a classification banner along the top of the application.

**file-upload** contains a `FileUploadComponent` to enable file selection for uploads.  

In the example Angular module above, if you wanted to use these components, you would have to open up `AppModule` and explicitly add these components to the `declarations` array.  

How do TypeScript modules help this? Here are the contents of `index.ts`:

``` ts
import { BannerComponent } from './banner/banner.component';
import { FileUploadComponent } from './file-upload/file-upload.component';

export const Components = [
  BannerComponent,
  FileUploadComponent
];
```  

We still have to define some mechanism for getting the components into `AppModule`, but this method allows all work with Components to be self-contained in the **components** module. It also has the added benefit of making `AppModule` substantially more light-weight (and scalable for working with a large number of components).

Here is how `AppModule` declares components using this TypeScript module:

``` ts
import { NgModule } from '@angular/core';

import { Components } from './components';

@NgModule({
    declarations: [
        [...Components]
    ]
})
export class AppModule { }
```  

Using the spread operation (`...`), we can just expand the contents of the `Components` array into the `declarations` array. The TypeScript module is able to manage the Angular module registration and simplify our workflow.

### [Dialogs](#modules)  

A Dialog is similar to a Component with the exception that it is rendered outside of the scope of the root component in the Angular application. Its primary use is as modal dialogs (hence the Dialog nomenclature). They need to be registered with the `declarations` array like a Component, but they also need to be added to the `entryComponents` array as well.

Grouping Dialogs into their own module allows the exact same behavior as above. Additionally, Dialogs need to be exported and made available as well, because they are used inside of Components.

Here's the `index.ts` for the base **dialog** module:

```ts
import { ConfirmDialog } from './confirm.dialog';

export const Dialogs = [
  ConfirmDialog
];

export * from './confirm.dialog';
```

And how `AppModule` declares dialogs using this TypeScript module:

```ts
import { NgModule } from '@angular/core';

import { Dialogs } from './dialogs';

@NgModule({
    declarations: [
        [...Dialogs]
    ],
    entryComponents [
        [...Dialogs]
    ]
})
export class AppModule { }
```  

Notice how, in `index.ts`, along with the `Dialogs` array, the `ConfirmDialog` is also exported? Keep this in mind, because the helpfulness of this is discussed next.

### [Models](#modules)  

Models are interfaces and classes that map directly to the C# entities that the Angular app interacts with over Web API. None of these interfaces or classes need to be registered with `AppModule`. So why is a **models** TypeScript module created?

Suppose you have a service that uses several interfaces defined in the **models** folder. This is how the models have to be imported without a TypeScript module:

**`ExampleService`**

```ts
import { Injectable } from '@angular/core';

import { InterfaceA } from '../models/interface-a'
import { InterfaceB } from '../models/interface-b';
import { InterfaceC } from '../models/interface-c';
/* Import. More. Interfaces */

@Injectable()
export class ExampleService { }
```  

You have to explicitly import each interface directly, which is not only a lot more typing, but if any of the model paths change, you have to go through everywhere the model is imported and update the path.

Here is an `index.ts`, created inside of the **models** folder, for this example scenario:

```ts
export * from './interface-a';
export * from './interface-b';
export * from './interface-c';
/* Export more interfaces */
```  

And here is the updated `ExampleService` making use of the newly modularized **models** module:

```ts
import { Injectable } from '@angular/core';

import {
  InterfaceA,
  InterfaceB,
  InterfaceC,
  /* More interfaces */
} from '../models';
```  

That's a whole lot less import! Now there is a big caveat around this:

> `import` statements within the module cannot import through `index.ts`. This will cause a circular dependency error and the app will practially explode.

The following scenario will break your app:

``` ts
// DON'T DO THIS!!!
import {
  InterfaceB,
  InterfaceC
} from '../models';

export class InterfaceA {
    b: InterfaceB,
    c: InterfaceC
}
```  

When referencing items within the same module, you MUST explicitly import them. The following is okay:

```ts
import { InterfaceB } from '../interface-b';
import { InterfaceC } from '../interface-c';

export class InterfaceA {
    b: InterfaceB,
    c: InterfaceC
}
```

### [Routes](#modules)

Routes are Components, but they differ in the following ways:

* They resolve to an Angular route
* They can interact with Angular services

With this in mind, the `index.ts` for the **routes** module contains a `RouteComponents` array (that is functionally the same as the `Components` array in the **components** module), and a `Routes` array for defining Angular routes:

```ts
import { Route } from '@angular/router';
import { HomeComponent } from './home/home.component';

export const RouteComponents = [
  HomeComponent
];

export const Routes: Route[] = [
  { path: 'home', component: HomeComponent },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: '**', redirectTo: 'home', pathMatch: 'full' }
];
```

Here is how AppModule declares routes using this TypeScript module:

```ts
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import {
  Routes,
  RouteComponents
} from './routes';

@NgModule({
  declarations: [
    [...RouteComponents]
  ],
  imports: [
    RouterModule.forRoot(Routes)
  ]
})
```  

Now, instead of routes being defined directly inside of `AppModule`, they can just be imported as the `Routes` array from the **routes** module, and provided as an argument to `RouterModule.forRoot(Routes)`.

### [Services](#modules)  

Going into the details of Angular's dependency injection system is beyond the scope of this section, but it is important to understand that a service can be scoped to both Modules and Components (and where it is scoped determines the lifecycle of the instance of the service). With this in mind, the **services** module provides a means for registering services scoped to the module, and exports services to be registered with any Component that may need to use it. Here is the `index.ts` for the base **services** module:

```ts
import { BannerService } from './banner.service';
import { CoreService } from './core.service';
import { ObjectMapService } from './object-map.service';
import { SnackerService } from './snacker.service';
import { ThemeService } from './theme.service';

export const Services = [
  BannerService,
  CoreService,
  ObjectMapService,
  SnackerService,
  ThemeService
];

export * from './banner.service';
export * from './core.service';
export * from './object-map.service';
export * from './snacker.service';
export * from './theme.service';

export * from './sockets/group-socket.service';
```

In this example, all of the following services are globally registered with the Angular module:

* `BannerService`
* `CoreService`
* `ObjectMapService`
* `SnackerService`
* `ThemeService`

You'll notice, however, that `GroupSocketService` is exported at the bottom, but not included in the `Services` array. That is because we want to be able to import `GroupSocketService` from the **services** module, but not register it globally with the Angular module.

Here is how the Angular module registers the global services:

```ts
import { NgModule } from '@angular/core';

import { Services } from './services';

@NgModule({
  providers: [
    [...Services]
  ]
})
export class ServicesModule { }
```

If you picked up on the fact that the Angular module is named `ServicesModule` and not `AppModule`, don't worry. This will be detailed in the Angular Modules section below.

### [Pipes](#modules)

Pipes are an Angular mechanism that allows for interpolated data to be transformed. Similar to Components, they need to be registered with the `providers` array of the Angular module. Here is the `index.ts` for the **pipes** module:

```ts
import { TruncatePipe } from './truncate.pipe';

export const Pipes = [
  TruncatePipe
];
```

And the Angular module registration:

```ts
import { NgModule } from '@angular/core';

import { Pipes } from './pipes';

@NgModule({
  declarations: [
    [...Pipes]
  ],
  exports: [
    [...Pipes]
  ]
})
export class ServicesModule { }
```  

Again, the **pipes** module is registered with `ServicesModule` and not `AppModule`, and the details of this will be outlined in the following sections.

## [Angular Modules](#modules)  

Angular Modules are classes decorated with `@NgModule`.

> For detailed information on decorators, see [Decorators](https://www.typescriptlang.org/docs/handbook/decorators.html)

The [NgModule API](https://angular.io/guide/ngmodule-api) documentation provides the best description of Angular modules:

At a high level, NgModules are a way to organize Angular apps and they accomplish this through the metadata in the `@NgModule` decorator. The metadata falls into three categories:

* **Static**: Compiler configuration which tells the compiler about directive selectors and where in templates the directives should be applied through selector matching. This is configured via the `declarations` array.
* **Runtime**: Injector configuration via the `providers` array
* **Composability/Grouping**: Bringing NgModules together and making them available via the `imports` and `exports` arrays.

```ts
@NgModule({
  // Static, this is compiler configuration
  declarations: [], // Configure the selectors
  entryComponents: [], // Generate the host factory

  // Runtime, or injector configuration
  providers: [], // Runtime injector configuration

  // Composability / Grouping
  imports: [], // componsing NgModules together
  exports: [], // making NgModules available to other parts of the app
})
```

**`@NgModule` metadata**

`declarations`

A list of [declarable](https://angular.io/guide/ngmodule-faq#q-declarable) classes, (*components*, *directives*, and *pipes*) that *belong to this module*. 

1. When compiling a template, you need to determine a set of selectors which should be used for triggering their corresponding directives.
2. The template is compiled within the context of an NgModule - the NgModule within which the template's component is declared - which determines the set of selectors using the following rules: 
    * All selectors of directives listed in `declarations`. 
    * All selectors of directives exported from imported NgModules.
    
Components, directives, and pipes must belong to `exactly` one module. The compiler emits an error if you try to declare the same class in more than one module. Be careful not to re-declare a class that is imported directly or indirectly from another module.

`providers`

A list of dependency-injection providers. 

Angular registers these providers with the NgModule's injector. If it is the NgModule used for bootstrapping, then it is the root injector.

These services become available for injection into any component, directive, pipe, or service which is a child of this injector.

A lazy-loaded module has its own injector which is typically a child of the application root injector.

Lazy-loaded services are scoped to the lazy module's injector. If a lazy-loaded module also provides the `UserService`, any component created within that module's context (such as by router navigation) gets the local instance of the service, not the instance in the root application injector.

Components in external modules continue to receive the instance provided by their injectors.

> For more information on injector hierarchy and scoping, see [Providers](https://angular.io/guide/providers) and the [DI Guide](https://angular.io/guide/dependency-injection).

`imports`

A list of modules which should be folded into this module. Folded means it is as if all the imported NgModule's exported properties were declared here.

Specifically, it is as if the list of modules whose exported components, directives, or pipes are referenced by the component templates where declared in this module.

A component template can [reference](https://angular.io/guide/ngmodule-faq#q-template-reference) another component, directive, or pipe when the reference is declared in this module or if the imported module has exported it. For example, a component can use the `NgIf` and `NgFor` directives only if the module has imported the Angular `CommonModule` (perhaps indirectly by importing `BrowserModule`).

You can import many standard directives from the `CommonModule` but some familiar directives belong to other modules. For example, you can use `[(ngModel)]` only after importing the Angular `FormsModule`.

`exports`

A list of declarations - `component`, `directive`, and `pipe` classes - that an importing module can use.

Exported declarations are the modules *public API*. A component in another module can [use](https://angular.io/guide/ngmodule-faq#q-template-reference) *this* module's `UserComponent` if it imports this module and the module exports `UserComponent`.

Declarations are private by default. If this module does *not* export `UserComponent`, then only the components within *this* module can use `UserComponent`.

Importing a module does *not* automatically re-export the imported module's imports. Module 'B' can't use *ngIf* just because it imported module 'A' which imported `CommonModule`. Module 'B' must import `CommonModule` itself.

A module can list another module among its `exports`, in which case all of that module's public components, directives, and pipes are exported.

[Re-export](https://angular.io/guide/ngmodule-faq#q-reexport) makes module transitivity explicit. If Module 'A' re-exports `CommonModule` and Module 'B' imports Module 'A', Module 'B' components can use *ngIf* even though 'B' itself didn't import `CommonModule`.

`bootstrap`

A list of comopnents that are automatically bootstrapped.

Usually there's only one component in this list, the *root component* of the application.

Angular can launch with multilpe bootstrap components, each with its own location in the host web page.

A bootstrap component is autmatically added to `entryComponents`.

`entryComponents`

A list of components that can be dynamically loaded into the view.

By default, an Angular app always has at least one entry component, the root component, `AppComponent`. Its purpose is to serve as a point of entry into the app, that is, you bootstrap it to launch the app.

Routed components are also *entry components* because they need to be loaded dynamically. The router creates them and drops them into the DOM near a `<router-outlet>`.

While the bootstrapped and routed components are *entry components*, you don't have to add them to a module's `entryComponents` list, as they are added implicitly.

Angular automatically adds components in the module's `bootstrap` and route definitions into the `entryComponents` list.

That leaves only components bootstrapped using one of the imperative techniques, such as [`ViewComponentRef.createComponent()`](https://angular.io/api/core/ViewContainerRef#createComponent) as undiscoverable.

Dynamic component loading is not common in most apps beyond the router. If you need to dynamically load components, you must add these components to the `entryComponents` list yourself.

> For more information, see [Entry Components](https://angular.io/guide/entry-components).

### [App Stack Modules](#modules)

The app stack contains 3 modules:
* `MaterialModule`
* `ServicesModule`
* `AppModule`

The following sections outline their purpose and layout.

### [Material Module](#modules)

[Angular Material](https://material.angular.io) is the design framework used within the app stack. It is written in Angular and maintained by a team at Google. It will be covered in detail in the [Material](./11-material.md) article.

The sole purpose of the `MaterialModule` is to import and export the Angular modules contained in both Angular Material and [Angular Flex Layout](https://github.com/angular/flex-layout/wiki).

It's quite long, but all it's doing is importing modules, and exporting them through the `MaterialModule`. This way, `AppModule` isn't exploded with a massive array of imports.

**`MaterialModule`**

```ts
import { NgModule } from '@angular/core';
import {
  MatAutocompleteModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatCommonModule,
  MatDatepickerModule,
  MatDialogModule,
  MatExpansionModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatNativeDateModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatRadioModule,
  MatRippleModule,
  MatSelectModule,
  MatSidenavModule,
  MatSliderModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatStepperModule,
  MatTabsModule,
  MatToolbarModule,
  MatTooltipModule,
  MatTableModule,
  MatPaginatorModule,
  MatSortModule,
  MatOptionModule,
  MatBadgeModule,
  MatBottomSheetModule,
  MatDividerModule,
  MatFormFieldModule,
  MatLineModule,
  MatPseudoCheckboxModule,
  MatTreeModule
} from '@angular/material';

import {
  FlexLayoutModule
} from '@angular/flex-layout';

@NgModule({
  exports: [
    FlexLayoutModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatCommonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatNativeDateModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatStepperModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatOptionModule,
    MatBadgeModule,
    MatBottomSheetModule,
    MatDividerModule,
    MatFormFieldModule,
    MatLineModule,
    MatPseudoCheckboxModule,
    MatTreeModule
  ]
})
export class MaterialModule { }
```

### [Services Module](#modules)

The `ServicesModule` exists to encapsulate the functionality of the **services** and **pipes** modules into their own Angular module, and make them available to `AppModule`.

The idea is that pipes and services should be self-sustaining and should not depend on the items defined in the `AppModule`. The only addition to the `ServicesModule` beyond the **pipes** and **services** module registrations is the `import` / `export` of the `HttpClientModule`.

**`ServicesModule`**

```ts
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { Services } from './services';
import { Pipes } from './pipes';

@NgModule({
  providers: [
    [...Services]
  ],
  declarations: [
    [...Pipes]
  ],
  imports: [
    HttpClientModule
  ],
  exports: [
    [...Pipes],
    HttpClientModule
  ]
})
export class ServicesModule { }
```

### [App Module](#modules)

The `AppModule` brings all of these moving pieces together. To make use of the `MaterialModule` and the `ServicesModule`, those modules just need to be included in the `imports` array.

The `AppModule` is also responsible for the following:

* Registering the `declarations` defined by the Angular app in this module
* Registering the `entryComponents` for dynamic components defined by the Angular app in this module
* Importing all of the external modules that the Angular app depends on
* Bootstrapping the *root component*, `AppComponent`

**`AppModule`**

```ts
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { MaterialModule } from './material.module';
import { ServicesModule } from './services.module';

import { AppComponent } from './app.component';

import { Routes, RouteComponents } from './routes';
import { Components } from './components';
import { Dialogs } from './dialogs';

@NgModule({
  declarations: [
    AppComponent,
    [...RouteComponents],
    [...Dialogs],
    [...Components]
  ],
  entryComponents: [
    [...Dialogs]
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    MaterialModule,
    ServicesModule,
    RouterModule.forRoot(Routes)
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

Thanks to all of the plumbing that was put in place using TypeScript modules and separate Angular modules for self-contained functionality, the `AppModule` is now scalable and substantially smaller than it would have been if we tried to cram everything into it.

[Back to Top](#modules)