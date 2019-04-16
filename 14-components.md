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
* [Input and Output Properties](#input-and-output-properties)
* [Template Expression Operators](#template-expression-operators)
  * [Safe Navigation](#safe-navigation)
  * [Pipes](#pipes)
    * [Numeric Pipes](#numeric-pipes)
    * [Casing Pipes](#casing-pipes)
    * [Json Pipe](#json-pipe)
    * [Date Pipe](#date-pipe)
    * [KeyValue Pipe](#keyvalue-pipe)
    * [Slice Pipe](#slice-pipe)
    * [Async Pipe](#async-pipe)
* [Lifecycle Hooks](#lifecycle-hooks)
  * [Using Hooks](#using-hooks)
  * [Best Practices](#best-practices)
* [ViewChild](#viewchild)
  * [RxJS fromEvent with ViewChild](#rxjs-fromevent-with-viewchild)
* [Flex Layout](#flex-layout)

## [Overview](#components)

The [Angular docs](https://angular.io/guide/architecture-components), once again, say it best:

> A *component* controls a patch of screen called a *view*. You define a component's logic - what it does to support the view - inside a class. The class interacts with the view through an API of properties and methods.

In an Angular app, the concept of Components has been split into three different categories:

**Root Component**

This is the `AppComponent`, located at **{Project}.Web\\ClientApp\\src\\app**. It defines the root layout for the Angular app and manages the initialization of root application services.

**Route Components**

These components can be resolved to a route via the Angular router. They are able to interact with services and manage the overall state of the view through that service interaction. It orchestrates the layout of *Display Components*, providing them with data retrieved through services. Any events triggered through user interaction with a Display Component is managed by the parent Route Component. Route Components are defined in the **{Project}.Web\\ClientApp\\src\\app\\routes** module.

**Display Components**

Display Components can be thought of as extending the HTML specification. In the same way that a `<p>` or `<input>` tag has attributes, properties, and events, so too do Display Components. They do not interact with services; they exclusively rely on an external source for their properties to be set, and for their events to be responded to.

> This article is concerned with covering the structure and details of working with Components. Root Component, Route Components, and Display Components will be covered in the articles that follow.

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

## [Interpolation](#components)

> [Interpolation](https://angular.io/guide/template-syntax#interpolation) documentation

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

[StackBlitz - Interpolation](https://stackblitz.com/edit/docs-interpolation?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html)

## [Binding Syntax](#components)

> [Binding Syntax](https://angular.io/guide/template-syntax#binding-syntax-an-overview) documentation

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

[Stackblitz - Binding Syntax Overview](https://stackblitz.com/edit/docs-binding-syntax?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html)

## [Directives](#components)

> [Built-in Directives](https://angular.io/guide/template-syntax#built-in-directives) documentation

Components are directives that define template-oriented features. The `@Component` decoratore extends the `@Directive` decorator. Directives exist to simplify complex tasks. Angular ships with built-in directives, and you can define your own with the `@Directive` decorator.

> In my time working with Angular, I have yet to encounter a scenario where I've needed to write a directive. The directives provided by Angular, Angular Material, and Flex Layout have always provided a fit the problem at hand. That's not to say you many never end up needing to write your own Directive, it's just that the need is extremely rare. The following sections will purely focus on demonstrating the built-in directives.

### [Attribute Directives](#components)

> [Attribute Directives](https://angular.io/guide/attribute-directives) documentation

Attirbute directives listen to and modify the behavior of other HTML elements, attributes, properties, and components. They are usually applied to elemenst as if they were HTML attributes, hence the name.

> Many Angular Modules, such as the [RouterModule]() and the [FormsModule] define their own attribute directives. This section will discuss the most commonly used attribute directives.

**NgClass**

**`home.component.ts`**
```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  fontClasses: {};
  isSerif = false;
  isBold = false;

  setFontClasses() {
    this.fontClasses = {
      'serif': this.isSerif,
      'bold': this.isBold
    };
  }
}
```

**`home.component.css`**

```css
p.serif {
  font-family: Cambria, Cochin, Georgia, Times, 'Times New Roman', serif;
}

p.bold {
  font-weight: 700;
}
```

**`home.component.html`**
```html
<mat-toolbar>NgClass</mat-toolbar>
<mat-checkbox [(ngModel)]="isSerif" 
              (change)="setFontClasses()"
              [style.margin.px]="12">Serif</mat-checkbox>
<mat-checkbox [(ngModel)]="isBold" 
              (change)="setFontClasses()"
              [style.margin.px]="12">Bold</mat-checkbox>
<p [ngClass]="fontClasses" 
   [style.margin.px]="12">
	Style is dynamically determined using the NgClass attribute directive.
</p>
```

**NgStyle**

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html'
})
export class HomeComponent {
  styles: {};
  fontColor = "#333333";
  isUppercase = false;

  setStyles() {
    this.styles = {
      'color': this.fontColor,
      'text-transform': this.isUppercase ? 'uppercase' : 'none'
    };
  }
}
```

**`home.component.html`**

```html
<mat-toolbar>NgStyle</mat-toolbar>
<section [style.margin.px]="12">
  <mat-checkbox [(ngModel)]="isUppercase"
                (change)="setStyles()">Uppercase</mat-checkbox>
</section>
<section>
  <p [style.margin.px]="12">Font Color</p>
  <mat-radio-group [(ngModel)]="fontColor" (change)="setStyles()">
    <mat-radio-button value="#333333"
                      [style.margin.px]="12">Black</mat-radio-button>
    <mat-radio-button value="#00ff88"
                      [style.margin.px]="12">Teal</mat-radio-button>
  </mat-radio-group>
</section>
<p [ngStyle]="styles">
  Style is dynamically applied using the NgStyle attribute directive.
</p>
```

**NgModel**

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  colors = [
    'Red',
    'Orange',
    'Yellow',
    'Green',
    'Blue',
    'Indigo',
    'Violet'
  ];

  color = 'Green';
}
```

**`home.component.html`**

```html
<mat-toolbar>NgModel</mat-toolbar>
<mat-form-field [style.margin.px]="12">
  <mat-label>Color</mat-label>
  <mat-select [(ngModel)]="color">
    <mat-option *ngFor="let c of colors" [value]="c">{{c}}</mat-option>
  </mat-select>
</mat-form-field>
<p [style.margin.px]="12">Selected Color: <span [style.color]="color">{{color}}</span></p>
```

[StackBlitz - Attribute Directives](https://stackblitz.com/edit/docs-attribute-directives?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html)

### [Structural Directives](#components)

> [Structural Directives](https://angular.io/guide/structural-directives) documentation

Structural directives are responsible for HTML layout. They shape or reshape the DOM's *structure*, typically by adding, removing, and manipulating the host elements to which they are attached.

**NgIf**

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  showDetails = false;
}
```

**`home.component.html`**

```html
<mat-toolbar>NgIf</mat-toolbar>
<h2>Topic</h2>
<button (click)="showDetails = !showDetails"
        mat-button
        color="accent"
        [style.margin.px]="8">
  <span *ngIf="showDetails">Hide Details</span>
  <span *ngIf="!showDetails">Show Details</span>
</button>
<p *ngIf="showDetails">Details of the topic described</p>
```

**NgFor**

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  articles = [
    { title: 'Services', description: 'A deep-dive into Angular Services' },
    { title: 'Components', description: 'Demonstrate how to build Angular Components' },
    { title: 'Routing', description: 'Angular Routing in a nutshell' }
  ]
}
```

**`home.component.html`**

```html
<mat-toolbar>NgFor</mat-toolbar>
<section fxLayout="row | wrap"
         fxLayoutAlign="space-evenly start"
         class="container">
  <section *ngFor="let a of articles"
           class="background card elevated clickable"
           [style.width.px]="180">
    <h3>{{a.title}}</h3>
    <p>{{a.description}}</p>
  </section>           
</section>
```

**NgSwitch**

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  meal: string;

  meals = [
    'breakfast',
    'lunch'
  ];

  breakfasts = [
    { price: 4, name: 'Oatmeal' },
    { price: 6, name: 'Bacon & Eggs' },
    { price: 10, name: 'Chicken & Waffles' }
  ];

  lunches = [
    { price: 5, name: 'Reuben' },
    { price: 3, name: 'BLT' },
    { price: 8, name: 'Cuban' }
  ];
}
```

**`home.component.html`**

```html
<mat-toolbar>NgSwitch</mat-toolbar>
<mat-form-field [style.margin.px]="12">
  <mat-label>Meal</mat-label>
  <mat-select [(ngModel)]="meal">
    <mat-option *ngFor="let m of meals"
                [value]="m">{{m}}</mat-option>
  </mat-select>
</mat-form-field>
<div [ngSwitch]="meal" 
     [style.margin.px]="12">
  <div *ngSwitchCase="'breakfast'"
       fxLayout="row | wrap"
       fxLayoutAlign="space-evenly start"
       class="container">
    <section *ngFor="let b of breakfasts"
             class="background card elevated clickable"
             [style.width.px]="180"
             fxLayout="row"
             fxLayoutAlign="start center">
      <p fxFlex>{{b.name}}</p>
      <p>{{b.price}}</p>
    </section>
  </div>
  <div *ngSwitchCase="'lunch'"
       fxLayout="row | wrap"
       fxLayoutAlign="space-evenly start"
       class="container">
    <section *ngFor="let l of lunches"
             class="background card elevated clickable"
             [style.width.px]="180"
             fxLayout="row"
             fxLayoutAlign="start center">
      <p fxFlex>{{l.name}}</p>
      <p>{{l.price}}</p>
    </section>
  </div>
  <div *ngSwitchDefault>Select a meal</div>
</div>
```

[StackBlitz - Structural Directives](https://stackblitz.com/edit/docs-structural-directives?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.ts)

## [Template Reference Variables](#components)

> [Template Reference Variables](https://angular.io/guide/template-syntax#template-reference-variables--var-) documentation

Template reference variables allow you to reference a DOM element, Component, Directive, or Web Component.

Use the hash symbol, `#`, to declare a reference variable:

```html
<input #search placeholder="Search">
```

`#search` declares a `search` variable on an `<input>` element.

A template reference variable can be refered to *anywhere* in the template:

```html
<button (click)="searchItems(search.value)">Search</button>
```

[StackBlitz - Template Reference Variables](https://stackblitz.com/edit/docs-template-reference-variables?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html)

## [Input and Output Properties](#components)

> [Input and Output Property](https://angular.io/guide/template-syntax#input-and-output-properties) documentation

An *Input* property is a *settable* property annotated with an `@Input` decorator. Values flow *into* the property when it is data bound with a [property binding](https://angular.io/guide/template-syntax#property-binding).

An *Output* property is an *observable* property annotated with an `@Output` decorator. The property almost always return an Angular [EventEmitter](https://angular.io/api/core/EventEmitter). Values flow *out* of the component as events bound with an [event binding](https://angular.io/guide/template-syntax#event-binding).

The following example is a bit more involved than any of the previous examples, so I will take a moment to discuss how it uses Input and Output properties:

* A `CardComponent` is defined with:
  * an `@Input item` property of type `Item`.
  * an `@Output select` property of type `EventEmitter<Item>`.
    * This means that whenever `select.emit(item)` is called, the listener will receive an object of type `Item` from the event call.
  * In the `CardComponent`, when the root `<section>` element is clicked, it calls `select.emit(item)`, which passes the `item` input property to the caller through the `select` output event.
* The `CardService` is defined with:
  * a `cards` property that is an array of `Item` objects.
* `HomeComponent` registers `CardService` with its `providers` array.
* `HomeComponent` injects an instance of type `CardService` into its constructor as `cardService`.
* `HomeComponent` defines a `selectCard` function with an `Item` parameter. The function calls `window.alert` and provides a message indicating the item that was selected.
* The `HomeComponent` template iterates through each `cardService.cards` and, for each `Item` in the array, renders a `CardComponent`.
  * The `item` input property is set to the card in the iteration.
  * The `select` output event is linked to the `selectCard` function.
    * The `$event` argument in the template indicates the object received by the `EventEmitter` defined on the `CardComponent`.

> Remember that items defined in a TypeScript module in the app stack should be handled appropriately in the `index.ts` file.

**`item.ts`**

```ts
export class Item {
  name: string;
  description: string;
}
```

**`card.component.ts`**

```ts
import {
  Component,
  Input,
  Output,
  EventEmitter
} from '@angular/core';

import { Item } from '../models';

@Component({
  selector: 'card',
  templateUrl: 'card.component.html'
})
export class CardComponent {
  @Input() item: Item;
  @Output() select = new EventEmitter<Item>();
}
```

**`card.component.html`**

```html
<section (click)="select.emit(item)"
         fxLayout="column"
         fxLayoutAlign="start stretch"
         class="background card elevated clickable">
  <p class="background accent" 
     [style.padding.px]="8"
     [style.margin]="0">{{item.name}}</p>
  <p [style.padding.px]="4">{{item.description}}</p>
</section>
```

**`card.service.ts`**

```ts
import { Injectable } from '@angular/core';

import { Item } from '../models';

@Injectable()
export class CardService {
  cards = new Array<Item>(
    { name: 'Item A', description: 'A description of Item A' },
    { name: 'Item B', description: 'A description of Item B' },
    { name: 'Item C', description: 'A description of Item C' }
  );
}
```

**`home.component.ts`**

```ts
import { Component } from '@angular/core';
import { CardService } from '../../services';
import { Item } from '../../models';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  providers: [CardService]
})
export class HomeComponent {
  constructor(
    public cardService: CardService
  ) { }

  selectCard = (item: Item) => window.alert(`${item.name} selected!`);
}
```

**`home.component.html`**

```html
<mat-toolbar>Input and Output Properties</mat-toolbar>
<section class="container"
         fxLayout="row | wrap"
         fxLayoutAlign="start start">
  <card *ngFor="let c of cardService.cards" 
        [item]="c" 
        (select)="selectCard($event)"></card>
</section>
```

[StackBlitz - Input and Output Properties](https://stackblitz.com/edit/docs-input-output-properties?file=src%2Fapp%2Fcomponents%2Fcard.component.ts)

## [Template Expression Operators](#components)

> [Template Expression Operators](https://angular.io/guide/template-syntax#template-expression-operators) documentation

The template expression language (relevant to interpolation) employs a superset of JavaScript syntax supplemented with a few special operators for specific scenarios.

### [Safe Navigation](#components)

Safe Navigation Operator: `?.`

> [Safe Navigation Operator](https://angular.io/guide/template-syntax#the-safe-navigation-operator----and-null-property-paths) documentation

If you attempt to bind to the property of an object that is `null`, an error will be thrown. The null safe operator allows you to safely access properties on objects that may be null:

```html
<!-- error thrown! -->
<p>{{nullObject.name}}</p>

<!-- safe! -->
<p>{{nullObject?.name}}</p>
```

This is particularly useful when doing conditional checks with `*ngIf`.

Instead of having to do this:

```html
<ng-container *ngIf="nullObject && nullObject.name">
  <!-- template -->
</ng-container>
```

you can do this:

```html
<ng-container *ngIf="nullObject?.name">
  <!-- template -->
</ng-container>
```

[StackBlitz - Safe Navigation Operator](https://stackblitz.com/edit/docs-safe-navigation-operator?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html)

### [Pipes](#components)

Pipe Operator: `|`

> [Pipe Operator](https://angular.io/guide/template-syntax#the-pipe-operator---) documentation  
> [Built-in Pipes](https://angular.io/api?type=pipe) documentation

The result of an expression might require some transformation before you're ready to use it in a binding. For example, you might display a number as a currency, force text to uppercase, or display an object in JSON format.

> All of the following pipes are demonstrated in a [StackBlitz - Built-In Pipes](https://stackblitz.com/edit/docs-built-in-pipes?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.ts) example.

#### [Numeric Pipes](#components)

**Decimal Pipe**

> [DecimalPipe](https://angular.io/api/common/DecimalPipe) documentation  

Usage:

```
{{value | number:'minInteger.minDecimal-maxDecimal'}}
```

`minInteger` specifies the minimum number of digits to show before the decimal. `minDecimal` specifies the minimum number of digits to show after the decimal. `maxDecimal`, which is optional, specifies the maximum number of digits to show after the decimal.

Example:

```html
<!-- renders: 3.142 -->
<p>{{pi | number}}</p>

<!-- renders 3.1 -->
<p>{{pi | number:'1.1-1'}}</p>

<!-- renders 3.14159 -->
<p>{{pi | number:'1.5'}}</p>
```

**Currency Pipe**

> [CurrencyPipe](https://angular.io/api/common/CurrencyPipe) documentation  

Shows a number in currency format.

Usage:

```
{{value | currency}}
{{value | currency:'CUR'}}
```

Where `CUR` is the currency region. `GBP`, for example, will show the currency in Great Brittain Pounds and `EUR` will show the currency in Euros. The default is locale-based. I'm in the United States, so for me, the default would be `USD`.

Examples:

```html
<!-- Renders $1,500.00 in the US -->
<p>{{1500 | currency}}</p>

<!-- Renders £1,500.00 -->
<p>{{1500 | currency:'GBP'}}</p>
```

**Percent Pipe**

> [PercentPipe](https://angular.io/api/common/PercentPipe) documentation  

Shows a number in percentage format.

Usage:

```
{{value | percent}}
{{value | percent:'minInteger.minDecimal-maxDecimal'}}
```

`minInteger` specifies the minimum number of digits to show before the decimal. `minDecimal` specifies the minimum number of digits to show after the decimal. `maxDecimal`, which is optional, specifies the maximum number of digits to show after the decimal.

Examples:

```html
<!-- renders: 86% -->
<p>{{.86 | percent}}</p>

<!-- renders: 86.00% -->
<p>{{.86 | percent:'1.2'}}
```

#### [Casing Pipes](#components)

> [LowerCasePipe](https://angular.io/api/common/LowerCasePipe) documentation  
> [UpperCasePipe](https://angular.io/api/common/UpperCasePipe) documentation  
> [TitleCasePipe](https://angular.io/api/common/TitleCasePipe) documentation

These pipes are used to transform the casing of a string.

Usage:

```
{{value | lowercase}}
{{value | uppercase}}
{{value | titlecase}}
```

Example:

```html
<!-- renders: some kind of string -->
<p>{{'SOME KIND OF STRING' | lowercase}}</p>

<!-- renders: SOME KIND OF STRING -->
<p>{{'some kind of string' | uppercase}}</p>

<!-- renders: Some Kind Of String -->
<p>{{'some kind of string' | titlecase}}</p>
```

#### [Json Pipe](#components)

> [JsonPipe](https://angular.io/api/common/JsonPipe) documentation

Renders a JavaScript object in JSON format.

Usage:

```
{{object | json}}
```

Example:

```ts
data = {
  name: 'Jaime',
  position: 'Full Stack Engineer',
  contact: {
    cell: '555-555-0001',
    office: '555-555-0002'
  }
}
```

```
<pre><code>{{data | json}}</code></pre>
```

Output:

```json
{
  "name": "Jaime",
  "position": "Full Stack Engineer",
  "contact": {
    "cell": "555-555-0001",
    "office": "555-555-0002"
  }
}
```

#### [Date Pipe](#components)

> [DatePipe](https://angular.io/api/common/DatePipe) documentation

Renders the date in a particular format

Usage:

```
{{now | date}}
{{now | date:'format'}}
```

`format` can be any of the [pre-defined format options](https://angular.io/api/common/DatePipe#pre-defined-format-options) or a [custom format option](https://angular.io/api/common/DatePipe#custom-format-options).

Examples:

```
<!-- renders: Apr 15, 2019 (or equivalent date) -->
{{now | date}}

<!-- renders: 4:05 PM (or equivalent time) -->
{{now | date:'shortTime'}}

<!-- renders: 2019 Apr 15 16:05:36 (or equivalent datetime) -->
{{now | date:'yyyy MMM dd HH:mm:ss'}}
```

#### [KeyValue Pipe](#components)

> [KeyValuePipe]() documentation

Transforms an `Object` or a `Map` into an array of key value pairs.

Usage:

``` html
<p *ngFor="let d of data | keyvalue">{{d.key}}: {{d.value}}</p>
```

Where `data` is a JavaScript `Object` or a `Map`.

Example:

```ts
obj: {[key: number]: string} = { 2: 'foo', 1: 'bar' };
map = new Map([[2, 'foo'], [1, 'bar']]);
```

```html
<h2>Object Piped</h2>
<p *ngFor="let item of obj | keyvalue">{{item.key}}: {{item.value}}</p>
<h2>Map Piped</h2>
<p *ngFor="let item of map | keyvalue">{{item.key}}: {{item.value}}</p>
```

Output:

**Object Piped**  
1: bar  
2: foo

**Map Piped**  
1: bar  
2: foo

#### [Slice Pipe](#components)

> [SlicePipe](https://angular.io/api/common/SlicePipe) documentation  

Creates a new `Array` or `String` containing a subset (slice) of the elements.

Usage:

```
{{arr | slice:start:end}}
```

`start` - the starting index of the subset to return:
  * **a positive integer**: return the item at `start` index and all items after in the list or string expression.
  * **a negative integer**: return the item at `start` index from the end of all items after in the list or string expression.
  * **if positive and greater than the size of the expression**: return an empty list or string
  * **if negative and greater than the size of the expression**: return entire list or string.

`end` - the ending index of the subset to return:
  * **omitted**: return all items until the end.
  * **if positive**: return all items before `end` index of the list or string.
  * **if negative** return all items before `end` index from the end of the list or string.

Examples:

```html
<!-- str = slice this string to pieces -->
<!-- renders: slice -->
<p>{{str | slice:0:5}}</p>

<!-- renders: this string -->
<p>{{str | slice:6:17}}</p>

<!-- renders: to pieces -->
<p>{{str | slice:18}}</p>

<!-- renders: pieces -->
<p>{{str | slice:-6}}</p>

<!-- renders: pie -->
<p>{{str | slice:-6:-3}}</p>
```

#### [Async Pipe](#components)

> [AsyncPipe](https://angular.io/api/common/AsyncPipe) documentation  

Unwraps a value from an asynchronous primitive

The `async` pipe subscribes to an `Observable` or `Promise` and returns the latest value emitted. When a new value is emitted, the `async` pipe marks the component to be checked for changes. When the component gets destroyed, the `async` pipe unsubscribes automatically to avoid potential memory leaks.

Usage:

The use case for the `async` pipe is a bit different than the other pipes because it doesn't immediately resolve a value (due to the asynchronous nature of the value it is called against). For that reason, there's a pattern that I like to use when working with `async` to ensure that the view is appropriately conditioned:

```html
<ng-template #loading>
  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
</ng-template>
<ng-container *ngIf="stream$ | async as stream else loading">
  <!-- work with the unwrapped stream value here -->
</ng-container>
```

> [ng-template](https://angular.io/guide/structural-directives#the-ng-template) documentation  
> [ng-container](https://angular.io/guide/structural-directives#ng-container-to-the-rescue) documentation  

The `<ng-template>` is an Angular element that is never directly displayed. In this case, the `<mat-progress-bar>` will only be displayed whenever the `loading` case is encountered in the `*ngIf` statement. `loading` in `*ngIf` points to the **template reference variable** defined on the `<ng-template #loading>` element.

`<ng-container>` is a grouping element that doesn't interfere with styles or layout because Angular *doesn't put it in the DOM*. I tend to use them a lot in places where I just want to do a conditional check and not have the element that contains the conditional to be rendered. In the example above, I use it to check for when the resolved value of `stream$` is available. When it has a value, it will render the contents inside of `<ng-container>` as if `<ng-container>` never existed.

`stream$ | async as stream else loading` essentially says:
* Subscribe to the `stream$` Observable \ `await` the `stream$` promise asynchronously and place the wrapped value into the `stream` property.
* While there is not value available, render the `loading` template, specified by `<ng-template #loading>`.
* When resolved, render the contents inside of the `<ng-container>`.

> Note that you still have to call whatever function actually executes the asynchronous action that will occur. This should always be done in the `OnInit` component lifecycle hook. Lifecycle Hooks are covered in the next section.

Example:

**`home.component.ts`**

```ts
import { Component, OnInit } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  styleUrls: ['home.component.css']
})
export class HomeComponent implements OnInit {
  private stream = new BehaviorSubject<string>(null);

  stream$ = this.stream.asObservable();
  promise: Promise<string>|null = null;

  ngOnInit() {
    setTimeout(() => this.stream.next('Observable stream resolved'), 1000);
    this.promise = new Promise<string>((resolve) => {
      setTimeout(() => resolve('Promise resolved'), 2000);
    });
  }
}
```

**`home.component.html`**

```html
<mat-toolbar>Async Pipe</mat-toolbar>
<ng-template #loading>
  <mat-progress-bar mode="indeterminate" color="accent"></mat-progress-bar>
</ng-template>
<ng-container *ngIf="stream$ | async as s else loading">
  <p>{{s}}</p>
</ng-container>
<ng-container *ngIf="promise | async as p else loading">
  <p>{{p}}</p>
</ng-container>
```

## [Lifecycle Hooks](#components)

> [Lifecycle Hooks](https://angular.io/guide/lifecycle-hooks) documentation

A component's lifecycle is managed by Angular. Angular will:

* create
* render
* create and render children
* check data-bound properties for changes
* destroy

**`ngOnChanges()`**

Responds when Angular (re)sets data-bound input properties. The method receives a `SimpleChanges` object of current and previous property values.

Called before `ngOnInit()` and whenever one or more data-bound input properties change.

**`ngOnInit()`**

Initialize the directive / component after Angular first displays the data-bound properties and sets the directive / component's input properties.

Called *once*, after the *first* `ngOnChanges()`.

**`ngDoCheck`**

Detect and act upon changes that Angular can't or won't detect on its own.

Called during every change detection run, immediately after `ngOnChanges()` and `ngOnInit()`.

**`ngAfterContentInit()`**

Respond after Angular projects external content into the component's view / the view that a directive is in.

Called *once* after the first `ngDoCheck()`.

**`ngAfterContentChecked()`**

Responds after Angular checks the content projected into the directive / component.

Called afert the `ngAfterContentInit()` and every subsequent `ngDoCheck()`.

**`ngAfterViewInit()`**

Repond after Angular initializes the component's views and child views / the view that a directive is in.

Called *once* after the first `ngAfterContentChecked()`.

**`ngAfterViewChecked()`**

Respond after Angular checks the component's views and child views / the view that a directive is in.

Called after the `ngAfterViewInit()` and every subsequent `ngAfterContentChecked()`.

**`ngOnDestroy()`**

Cleanup just before Angular destroys the directive / component. Unsubscribe Observables and detach event handlers to avoid memory leaks.

Called *just before* Angular destroys the directive / component.

### [Using Hooks](#components)

To perform tasks inside of a lifecycle hook, you must do three things:

* Import the necessary hook
* Implement the hook's interface
* Call the hook's function

**`home.component.ts`**

```ts
import {
  Component,
  OnInit
} from '@angular/core'

import { AppService } from '../../services';

@Component({
  selector: 'home',
  templateUrl: 'home.component.ts',
  providers: [AppService]
})
export class HomeComponent implements OnInit {
  constructor(
    public app: AppService
  ) { }

  ngOnInit() {
    this.app.getData();
  }
}
```

* The `OnInit` interface is imported from `@angular/core`.
* `HomeComponent` specifies that it implements the `OnInit` interface.
* The `ngOnInit()` function is executed when the component is initialized, calling the `AppService.getData()` function.

### [Best Practices](#components)

This section discusses the best practices associated with commonly-used Lifecycle Hooks.

**`OnInit`**

Used for two main reasons:

1. To perform complex initializations shortly after construction
2. To set up the component after Angular sets the input properties

Used where you would normally perform initialization tasks inside of a `constructor`. As a rule of thumb:

* The `constructor` should only be used for dependency injection
* `OnInit` should be used to perform component startup tasks

> Angular team lead Misko Hevery explains why you should avoid copmlex constructor logic in this [article](http://misko.hevery.com/code-reviewers-guide/flaw-constructor-does-real-work/).

**`OnDestroy`**

Cleanup logic that *must* run before Angular destroys the component. This is where you'll want to free any resources that won't be garbage collected automatically:

* Unsubscribe from Observables
* Unsubscribe from DOM events
* Stop interval timers
* Unregister callbacks registered with global or application services

Neglecting to do so risks memory leaks.

> A huge benefit of using the `async` pipe to subscribe to Observables is that the pipe automatically handles unsubscribing, preventing you from having to call `OnDestroy`.

## [ViewChild](#components)

> [ViewChild](https://angular.io/api/core/ViewChild) documentation  

The `@ViewChild` decorator configures a view query. The change detector looks for the first element or the directive matching the selector in the view DOM. If the view DOM changes, and a new child matches the selector, the property is updated.

This allows you to access elements of a Component's view template inside of the class definition.

Usage:

```ts
@ViewChild(selector)
set child(c: Type) {
  // perform interactions with the element
}
```

* `selector` is the directive type or the name used for querying.
* `c: Type` is the instance of the element to interact with and the type of element it is.

Supported selectors include:
* any class with the `@Component` or `@Directive` decorator
* a template reference variable as a string
  * query `<my-component #cmp></my-component>` with `@ViewChild('cmp')
* any provider defined in the child component tree of the current component
  * `@ViewChild(SomeService) someService: SomeService;`
* any provider defined through a string token
  * `@ViewChild('someToken') someTokenVal: any`
* a `TemplateRef`
  * Query `<ng-template></ng-template>` with `@ViewChild(TemplateRef) template;`

### [RxJS fromEvent with ViewChild](#components)

> If you recall from the [Services - CoreService](./13-services.md#coreservice) article, I said that `generateInputObservable()` would be discussed in this article. This is where it will be demonstrated.
>
> Again, RxJS will be covered in depth in the [RxJS](./a2-rxjs.md) article. But it would be negligent to cover `@ViewChild` without showing the power it enables with RxJS. If the following section doesn't make sense, spend some time getting comfortable with RxJS first, then revisit it.

The RxJS function [fromEvent](https://rxjs.dev/api/index/function/fromEvent) allows you to pass in a DOM element and DOM event name in order to generate an **Observable**. In addition, you can pipe in additional RxJS operators to create some interesting features. The `CoreService.generateInputObservable()` function does just that. You provide an `<input>` DOM element, and it calls `fromEvent` to register the `keyup` event with several other RxJS operators to ensure that:

* The **Observable** isn't generated until the user has stopped typing for a period of **300** milliseconds
* Only the `event.target.value` string is wrapped inside of the Observable
* The Observable isn't triggered if the resulting value is the same as the value it held previously

The primary use case for this is to be able to to execute asynchronous operations that are based on user input.

Example:

**`post.ts`**

```ts
export interface Post {
  id: number;
  userId: number;
  title: string;
  body: string;  
}
```

`Post` defines an interface for objects retrieved from the public [JsonPlaceholder - Posts](https://jsonplaceholder.typicode.com/posts) API.

**`app.service.ts`**

```ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { SnackerService } from './snacker.service';
import { Post } from '../models';

@Injectable()
export class AppService {
  private apiUrl = 'https://jsonplaceholder.typicode.com/posts';
  private posts = new BehaviorSubject<Post[]>(null);

  posts$ = this.posts.asObservable();

  constructor(
    private http: HttpClient,
    private snacker: SnackerService
  ) { }

  getPosts = () => this.http.get<Post[]>(this.apiUrl)
    .subscribe(
      data => this.posts.next(data),
      err => this.snacker.sendErrorMessage(err)
    );

  searchPosts = (val: string) => this.http.get<Post[]>(this.apiUrl)
    .subscribe(
      data => this.posts.next(data.filter(this.filterPost(val))),
      err => this.snacker.sendErrorMessage(err)
    );

  private filterPost = (val: string) => (x: Post) =>
    x.title.toLowerCase().includes(val.toLowerCase()) ||
    x.body.toLowerCase().includes(val.toLowerCase());
}
```

* The `private BehaviorSubject<T>` exposed as a `public Observable<T>` pattern is used in this service.
* `getPosts()` retrieves all of the data from the public API endpoint
* `searchPosts(val: string)` retrieves all of the data from the public API endpoint, then filters out all of the posts that don't contain the search value in the `title` or `body` properties of the `Post[]` returned by the API call.

**`post-card.component.html`**

```html
<section class="background card elevated clickable"
         fxLayout="column"
         fxLayoutAlign="start stretch"
         [matTooltip]="post.body">
  <section [style.margin.px]="0"
           [style.padding.px]="8"
           class="background accent">
    {{post.title}}
  </section>
  <p>{{post.body | truncate}}</p>
</section>
```

A simple card interface for a `Post` object

**`post-card.component.ts`**

```ts
import {
  Component,
  Input
} from '@angular/core';

import { Post } from '../../models';

@Component({
  selector: 'post-card',
  templateUrl: 'post-card.component.html'
})
export class PostCardComponent {
  @Input() post: Post;
}
```

Receives a `Post` as an **Input** property

**`home.component.html`**

```html
<mat-toolbar>RxJS fromEvent with ViewChild</mat-toolbar>
<ng-template #loading>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
<section>
  <mat-form-field [style.width.%]="80">
    <mat-label>Search Posts</mat-label>
    <input matInput #search>
  </mat-form-field>  
</section>
<ng-container *ngIf="app.posts$ | async as posts else loading">
  <section *ngIf="posts.length > 0"
           fxLayout="row | wrap"
           fxLayoutAlign="start start"
           class="container">
    <post-card *ngFor="let p of posts"
               [post]="p" [style.width.px]="240"></post-card>
  </section>
  <h3 *ngIf="!(posts.length > 0)">No Posts Found</h3>
</ng-container>
```

`<input matInput #search>` will be used inside of the component to trigger the `AppService.searchPosts()` API call

**`home.component.ts`**

```ts
import {
  Component,
  OnInit,
  ViewChild,
  ElementRef
} from '@angular/core';

import {
  AppService,
  CoreService
} from '../../services';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  providers: [AppService]
})
export class HomeComponent implements OnInit {
  private postsInit = false;

  @ViewChild('search')
  set search(input: ElementRef) {
    if (input && !this.postsInit) {
      this.core.generateInputObservable(input)
        .subscribe((val: string) => {
          val && val.length > 1 ?
            this.app.searchPosts(val) :
            this.app.getPosts();
        });

      this.postsInit = true;
    }
  }

  constructor(
    public app: AppService,
    private core: CoreService
  ) { }

  ngOnInit() {
    this.app.getPosts();
  }
}
```

* A `private postsInit` value is captured to ensure that the `fromEvent` Observable is only generated once
* `@ViewChild('search')` is used to query for the `<input matInput #search>` element, using the **template reference variable**, and a `search` setter function is defined for processing the `ViewChild` result, providing the `ElementRef` the query refers to
  * If the `input` object has a value and the Observable hasn't been initialized, the `generateInputObservable` function (defined in [CoreService](./13-services.md#coreservice)) is called and `input` is passed in
  * The resulting Observable is subscribed to, and whenever it is triggered by typing into the `<input>` element, the value is returned in the subscription
    * If there is a value, the `searchPosts(val)` function is called
    * If there isn't a value, the posts are reset with `getPosts()`
  * `postsInit` is set to `true` the first (and only) time the Observable is generated
* The initial posts are retrieved in `ngOnInit`

[StackBlitz - ViewChild](https://stackblitz.com/edit/docs-viewchild?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.ts)

## [Flex Layout](#components)

> [Flex Layout](https://github.com/angular/flex-layout/wiki) documentation

Angular Flex Layout is a directive-based API for using Flexbox, CSS Grid, and mediaQuery.

You may have noticed in some of the Component template examples the following directives used:

```html
<section fxLayout="row | wrap"
         fxLayoutAlign="space-evenly start">
  <!-- more markup -->
<section>
```

These directives are part of the Flex Layout library and alter the way that the `<section>` arranges its child elements.

This library is used extensively when laying out views in the app stack. It's highly recommended that you take the time to familiarize yourself with the features that it is built around, as well as the library's API itself:

* [Flexbox](https://developer.mozilla.org/en-US/docs/Learn/CSS/CSS_layout/Flexbox)
* [CSS Grid](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Grid_Layout)
* [Media Queries](https://developer.mozilla.org/en-US/docs/Web/CSS/Media_Queries)
* [Flex Layout API](https://github.com/angular/flex-layout/wiki/Declarative-API-Overview)
* [Flex Layout Responsive API](https://github.com/angular/flex-layout/wiki/Responsive-API)
* [Flex Layout Demo](https://tburleson-layouts-demos.firebaseapp.com/#/docs)

[Back to Top](#components)