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
  * [Lifecycle Hooks](#lifecycle-hooks)
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

[StackBlitz - Interpolation](https://stackblitz.com/edit/docs-interpolation?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html)

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

[Stackblitz - Binding Syntax Overview](https://stackblitz.com/edit/docs-binding-syntax?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html)

### [Directives](#components)

[Built-in Directives](https://angular.io/guide/template-syntax#built-in-directives) documentation

Components are directives that define template-oriented features. The `@Component` decoratore extends the `@Directive` decorator. Directives exist to simplify complex tasks. Angular ships with built-in directives, and you can define your own with the `@Directive` decorator.

> In my time working with Angular, I have yet to encounter a scenario where I've needed to write a directive. The directives provided by Angular, Angular Material, and Flex Layout have always provided a fit the problem at hand. That's not to say you many never end up needing to write your own Directive, it's just that the need is extremely rare. The following sections will purely focus on demonstrating the built-in directives.

#### [Attribute Directives](#components)

[Attribute Directives](https://angular.io/guide/attribute-directives) documentation

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

#### [Structural Directives](#components)

[Structural Directives](https://angular.io/guide/structural-directives) documentation

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

### [Template Reference Variables](#components)

[Template Reference Variables](https://angular.io/guide/template-syntax#template-reference-variables--var-) documentation

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

### [Input and Output Properties](#components)

[Input and Output Property](https://angular.io/guide/template-syntax#input-and-output-properties) documentation

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

### [Template Expression Operators](#components)

[Template Expression Operators](https://angular.io/guide/template-syntax#template-expression-operators) documentation

The template expression language (relevant to interpolation) employs a superset of JavaScript syntax supplemented with a few special operators for specific scenarios.

#### Pipe Operator: `|`

> Pipes are covered in depth in the [Pipes](./16-pipes.md) article. This section will discuss some of the built-in pipes.

The result of an expression might require some transformation before you're ready to use it in a binding. For example, you might display a number as a currency, force text to uppercase, or display an object in JSON format.

> [Built-in Pipes](https://angular.io/api?type=pipe)

**Numeric Pipes**

* [DecimalPipe](https://angular.io/api/common/DecimalPipe)
* [CurrencyPipe](https://angular.io/api/common/CurrencyPipe)
* [PercentPipe](https://angular.io/api/common/PercentPipe)

**Decimal Pipe**

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

**Casing Pipes**

* [LowerCasePipe](https://angular.io/api/common/LowerCasePipe)
* [UpperCasePipe](https://angular.io/api/common/UpperCasePipe)
* [TitleCasePipe](https://angular.io/api/common/TitleCasePipe)


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

**Json Pipe**

* [JsonPipe](https://angular.io/api/common/JsonPipe)

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

**Date Pipe**

* [DatePipe](https://angular.io/api/common/DatePipe)

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

**KeyValue Pipe**

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

**Slice Pipe**

* [SlicePipe](https://angular.io/api/common/SlicePipe)

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

**Async Pipe**

* [AsyncPipe](https://angular.io/api/common/AsyncPipe)

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

* [ng-template](https://angular.io/guide/structural-directives#the-ng-template)
* [ng-container](https://angular.io/guide/structural-directives#ng-container-to-the-rescue)

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

### [Lifecycle Hooks](#components)

### [ViewChild](#components)

### [Flex Layout](#components)

## [AppComponent](#components)

## [Display Components](#components)

### [BannerComponent](#components)

### [FileUploadComponent](#components)

### [ItemListComponent](#components)

### [ItemCardComponent](#components)

[Back to Top](#components)