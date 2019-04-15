# Components

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Anatomy](#anatomy)
    * [Component Decorator](#component-decorator)
* [Usage](#usage)
    * [Interpolation](#interpolation)
    * [Binding Syntax](#binding-syntax)
    * [Directives](#directives)
        * [Attribute Directives](#attribute-directives)
        * [Structural Directives](#structural-directives)
    * [Template Reference Variables](#template-reference-variables)
    * [Template Expression Operators](#template-expression-operators)
    * [Input and Output Properties](#input-and-output-properties)
    * [ViewChild](#viewchild)
    * [Flex Layout](#flex-layout)
* [AppComponent](#appcomponent)
* [Display Components](#display-components)
    * [BannerComponent](#bannercomponent)
    * [FileUploadComponent](#fileuploadcomponent)
    * [ItemListComponent](#itemlistcomponent)
    * [ItemCardComponent](#itemcardcomponent)

## [Overview](#components)

The [Angular docs](https://angular.io/guide/architecture-components), once again, say it best:

> A *component* controls a patch of screen called a *view*. You define a component's logic - what it does to support the view - inside a class. The class interacts with the view through an API of properties and methods.

In an Angular app, the concept of Components has been split into three different categories:

**Root Component**

This is the `AppComponent`, located at **{Project}.Web\\ClientApp\\src\\app**. It defines the root layout for the Angular app and manages the initialization of root application services.

**Route Components**

These components can be resolved to a route via the Angular router. They are able to interact with services and manage the overall state of the view through that service interaction. It orchestrates the layout of *Display Components*, providing them with data retrieved through services. Any events triggered through user interaction with a Display Component is managed by the parent Route Component. Route Components are defined in the **{Project}.Web\\ClientApp\\src\\app\\routes** module.

> Route Components will be covered in detail in the [Routes](./15-routes.md) article.

**Display Components**

Display Components can be thought of as extending the HTML specification. In the same way that a `<p>` or `<input>` tag has attributes, properties, and events, so too do Display Components. They do not interact with services; they exclusively rely on an external source for their properties to be set, and for their events to be responded to.

This article is concerned with detailing the following topics:

* Structure and details of working with Components
* The root `AppComponent`
* Display Component concepts and examples

## [Anatomy](#components)

The simplest component would only have a class definition and its template would be specified inline:

```ts
import { Component } from '@angular/core';

@Component({
    selector: 'example',
    template: `
    <p>example variable</p>
    `
})
export class ExampleComponent { }
```

As specified in the [Modules](./10-modules.md) article, you have to register the component in `index.ts` inside of the **components** TypeScript module for it to be accessible to the rest of the application. Once registered, you can use it in another Component template as follows:

```html
<example></example>
```

All it does is render `<p>example variable</p>`, which isn't very helpful, but this illustrates how a component is written.

A component can consist of up to three types of files:

* A single class file that is marked with the `@Component` decorator
* A single `.html` template file
* `.css` or `.scss` style files

### [Component Decorator](#components)

The shell of a component is written as follows:

```ts
import { Component } from '@angular/core';

@Component({
    // decorator properties
})
export class ExampleComponent { }
```

This section will cover the important properties associated with the `@Component` decorator.

> For detailed information, see the [Component Decorator](https://angular.io/api/core/Component) API documentation.

**Decorator Properties**

Property | Description
---------|------------
`selector` | The CSS selector that identifies this directive in a template and triggers instantiation of the directive.
`providers` | Configures the [injector](https://angular.io/guide/glossary#injector) of this directive or component with a [token](https://angular.io/guide/glossary#di-token) that maps to a [provider](https://angular.io/guide/glossary#provider) of a dependency.
`templateUrl` | The relative path or absoluate URL of a template file for an Angular component. If provided, do not supply an inline template using `template`.
`template` | An inline template for an Angular component. If provided, do not supply a template file using `templateUrl`.
`styleUrls` | One or more relative paths or absolute URLs for files containing CSS stylesheets to use in this component.
`styles` | One or more inline CSS stylesheets to use in this component.
`animations` | One or more `trigger()` calls, containing `state()` and `transition()` definition. See the [Animations guide](https://angular.io/guide/animations) and animations API documentation.

## [Usage](#components)

The majority of the features described in the following sections is directly derived from the [Template Syntax](https://angular.io/guide/template-syntax) guide in the Angular docs. As such, each section will provide the following layout:

* a documentation link
* a brief description
* an example
* a link to a [StackBlitz](https://stackblitz.com/) demo.

### [Interpolation](#components)

[Interpolation](https://angular.io/guide/template-syntax#interpolation) documentation

Interpolation allows you to embed expressions into the markup of a Component template.

Angular evaluates all expressions, called **template expressions**, in double curly braces, `{{expression}}`. Angular executes the expression and assigns it to a property of a binding target, which could be any of the following:

* An HTML element property or attribute
* A component property
* A directive property

Template expressions are similar to JavaScript. Many JavaScirpt expressions are legal template expressions, with a few expressions:

* You can't use JavaScript expressions that have or promote side effects
    * Assignments (`=`, `+=`, `-=`, ...)
    * Operators such as `new`, `typeof`, `instanceOf`, etc.
    * Chaining expressions with `;` or `,`
    * The increment and decrement operators `++` and `--`
    * Some of the ES2015+ operators
* No support for the bitwise operators such as `|` and `&`
* New template expression operators, such as `|`, `?.` and `!`

**Basic Usage**

**`example.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
    selector: 'example',
    templateUrl: 'example.component.html'
})
export class ExampleComponent {
    value = "This is a property of ExampleComponent";
}
```

**`example.component.html`**

```html
<mat-toolbar>Interpolation Example</mat-toolbar>
<p>Interpolated property: {{value}}</p>
```

[StackBlitz - Interpolation](https://stackblitz.com/edit/docs-interpolation)

### [Binding Syntax](#components)

[Binding Syntax](https://angular.io/guide/template-syntax#binding-syntax-an-overview) documentation

Data binding is a mechanism for coordinating what users see, with application data values. While you could push values to and pulle values from HTML, the application is easier to write, read, and maintain if you turn these chores over to a binding framework. You simply declare bindings between binding sources and target HTML elements and let the framework do the work.

Binding can be grouped into three categories, distinguished by the direction of data flow: from the *source-to-view*, from the *view-to-source*, and in the two-way sequence: *view-to-source-to-view*.

**One-way from data source to view target**

Syntax:

```
{{expression}}
[target]="expression"
bind-target="expression"
```

Type:
* Interpolation
* Property
* Attribute
* Class
* Style

**One-way from view target to data source**

Syntax:

```
(target)="statement"
on-target="statement"
```

Type:
* Event

**Two-way**

Syntax:

```
[(target)]="expression"
bindon-target="expression"
```

Type:
* Two-way

**Binding Targets**

The **target** of a **data binding** is something in the DOM. Depending on the binding type, the target can be an (element | component | directive) property, and (element | component | directive) event, or (rarely) an attribute name.

**Property**

Target:

* Element property
* Component property
* Directive property

Examples:

```html
<img [src]="heroImageUrl">
<app-hero-detail [hero]="currentHero"></app-hero-detail>
<div [ngClass]="{'special': isSpecial}"></div>
```

**Event**

Target:
* Element event
* Component event
* Directive event

Examples:

``` html
<button (click)="onSave()">Save</button>
<app-hero-detail (deleteRequest)="deleteHero()"></app-hero-detail>
<div (myClick)="clicked=$event" clickable>click me</div>
```

**Two-way**

Target:
* Event and property

Example:

```html
<input [(ngModel)]="name">
```

**Attribute**

Target:
* Attribute

Example:

```html
<button [attr.aria-label]="help">help</button>
```

**Class**

Target:
* `class` property

Example:

```html
<div [class.isSpecial]="isSpecial">Special</div>
```

**Style**

Target:
* `style` property

```html
<button [style.color]="isSpecial ? 'red' : 'green'">
```

The following example demonstrates examples of each type of binding.

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
})
export class HomeComponent {  
  interpolation = "interpolated value";
  imgUrl = "https://picsum.photos/g/200?random";
  eventBinding = () => window.alert('event binding');
  twoWay = "Bound with [(ngModel)]";
  altAttr = "Broken link!";
  isHovered = false;
  margin = 12;
}
```

**`home.component.html`**

```html
<mat-toolbar>Interpolation</mat-toolbar>
<p>Value: {{interpolation}}</p>
<mat-toolbar>Property Binding</mat-toolbar>
<img [src]="imgUrl">
<mat-toolbar>Event Binding</mat-toolbar>
<button mat-raised-button 
        color="warn" 
        (click)="eventBinding()">Click Me!</button>
<mat-toolbar>Two-way Binding</mat-toolbar>
<div class="background card">
  <mat-form-field>
  <input matInput
         [(ngModel)]="twoWay">
  </mat-form-field>
  <p>Two Way: {{twoWay}}</p>
</div>
<mat-toolbar>Attribute Binding</mat-toolbar>
<img src=""
     [attr.alt]="altAttr">
<mat-toolbar>Class Binding</mat-toolbar>
<div class="background" 
     [class.primary]="isHovered"
     (mouseenter)="isHovered = true"
     (mouseleave)="isHovered = false">
  <p>Hover to change background</p>
</div>
<mat-toolbar>Style Binding</mat-toolbar>
<div [style.margin.px]="margin">
  <p>Styled with [style.margin.px]="margin"</p>
</div>
```

[Stackblitz - Binding Syntax Overview]()

### [Directives](#components)

#### [Attribute Directives](#components)

#### [Structural Directives](#components)

### [Template Reference Variables](#components)

### [Template Expression Operators](#components)

### [Input and Output Properties](#components)

### [ViewChild](#components)

### [Flex Layout](#components)

## [AppComponent](#components)

## [Display Components](#components)

### [BannerComponent](#components)

### [FileUploadComponent](#components)

### [ItemListComponent](#components)

### [ItemCardComponent](#components)

[Back to Top](#components)