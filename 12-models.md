# Models

[Table of Contents](./toc.md)

* [Overview](#overview)
    * [Transpiled JavaScript](#transpiled-javascript)
    * [Mapping Classes](#mapping-classes)
* [Implementation](#implementation)

## [Overview](#models)

Models exist in the **{Project}.Web\\ClientApp\\src\\app\\models** TypeScript module. They can be created as an `interface` or a `class` in two situations:

* You want a TypeScript representation of a C# class that you interface with via Web API
* You want to maintain a reusable data structure within the Angular app

Whether the model should be a `class` or an `interface` is dependent on one simple question: Does it merely define the structure of the data, or does it have additional functionality?

If it's merely representing the structure of data, the model should be an `interface`.

If additional functionality is provided, for instance, a `filter` property that is auto-generated based on several existing properties, the model should be a `class`.

There are two important side effects of making a model a `class` rather than an `interface`:

1. The transpiled **JavaScript** of a `class` is substantially larger than that of an `interface`
2. When receiving data from an HTTP call, you have to explicitly `map` the JSON object to the `class` type in order to gain access to the properties and functions not explicitly available in the received JSON object.

### [Transpiled JavaScript](#models)

Suppose I have the following class and interface in TypeScript:

```ts
interface IGreeter {
    id: number;
    name: string;
}

class Greeter {
    id: number;
    name: string;
}
```

Here is the tranpsiled JavaScript:

```js
var Greeter = /** @class */ (function () {
    function Greeter() {
    }
    return Greeter;
}());
```

> You can check this yourself in the [TypeScript Playground](https://www.typescriptlang.org/play/index.html)

A big difference between an `interface` and a `class` in TypeScript is that an `interface` only exists at *compile-time*. Because of [type erasure](https://github.com/Microsoft/TypeScript/wiki/FAQ#what-is-type-erasure), there is no translation to JavaScript for an `interface`. Once TypeScript is transpiled, it is simply JavaScript. Even the TypeScript `class` just becomes an Immediately Invoked Function Expression ([IIFE](https://developer.mozilla.org/en-US/docs/Glossary/IIFE)) when translated to JavaScript.

The important take away here is that if you can use an `interface`, you don't end up with any code generated in the transpiled JavaScript, and still get the benefits of ensuring the appropriate data structures are used. TypeScript has gotten substantially better at translating a `class` into equivalent JavaScript (thanks to advances in the ECMAScript standard), so the equivalent JavaScript isn't nearly as heavy as it used to be. But it will still be there, and if you can avoid it, then you should.

### [Mapping Classes](#models)

> This section will contain demonstrations using arrow functions, as well as the `filter` and `map` array functions. If you are not familiar with these concepts, I highly encourage you to read through the following:
> * [Functions](https://www.typescriptlang.org/docs/handbook/functions.html)
> * [Arrow Functions](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Functions/Arrow_functions)
> * [Arrays](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array#)

The other consideration when using a TypeScript `class` is that when you receive data though an HTTP request, it's not enough to use generics to express the data interface.

Suppose you have the following class:

```ts
export class Example {
    id: number;
    name: string;
    label: string;
    category: string;

    get filter(): string {
        return `${this.name} - ${this.label} - ${this.category}`;
    }
}
```

You want to be able to filter a collection of `Example` objects by the `filter` property, which combines the values of `name`, `label`, and `category` as follows:

```ts
filterExamples = (examples: Example[], search: string): Example[] =>
    examples.filter(x => 
    {
        search = search.toLowerCase();
        const f = x.filter.toLowerCase();
        return f.indexOf(search) !== -1;
    });
```

This function takes an `Example[]` and a `string`, and returns an array of `Example` objects if the `filter` property contains the search value (case insensitive).

Suppose you want to filter these objects after receiving the collection from an HTTP call:

```ts
getExamples = () => this.http.get<Example[]>('/api/example/getExamples')
    .subscribe(
        data => this.examples.next(data),
        err => this.snacker.sendErrorMessage(err.error)
    );
```

> Observables will be covered in greater detail in the [Services](./13-services.md) and [RxJS](./a2-rxjs.md) articles.

If you try to access the `filter` property on any of the objects received by the `getExamples` function, an error will be thrown. This is because, even though we specify that the type of data we're receiving is `Example[]`, we're still only assigning an `Object[]` to the `this.examples` observable stream.

In order to access the `filter` property, you need to explicitly assign the objects in the incoming data stream as an `Example` instance:

```ts
getExamples() => this.http.get<Example[]>('/api/example/getExamples')
    .subscribe(
        data => this.examples.next(data.map(x => Object.assign(new Example(), x))),
        err => this.snacker.sendErrorMessage(err.error)
    )
```

> The app stack provides an `ObjectMapService` for decoupling this logic from your API functions, and it will be covered in the [Services](./13-services.md) article.

## [Implementation](#models)

Back in the [Data Access Layer](./02-data-access-layer.md) article, we created the following entities:

* `Category.cs`
* `Item.cs`
* `ItemTag.cs`
* `Location.cs`
* `Tag.cs`

In this section, we'll create an `interface` for all but the `Item` entity, which will be a `class` with an auto-generated read-only `filter` property. Once these have been created, we want to make sure to export them in the **models** TypeScript module, so they can be easily imported elsewhere in the application. They will all be created at **{Project}.Web\\ClientApp\\src\\app\\models**.

> Note that the naming convention used for TypeScript files is kebab-casing. So **ItemTag.cs** would be **item-tag.ts**.

**`category.ts`**

```ts
import { Item } from './item';

export interface Category {
  id: number;
  label: string;
  isDeleted: boolean;

  items: Item[];
}
```

**`item-tag.ts`**

```ts
import { Item } from './item';
import { Tag } from './tag';

export interface ItemTag {
  id: number;
  itemId: number;
  tagId: number;

  item: Item;
  tag: Tag;
}
```

**`item.ts`**

```ts
import { Category } from './category';
import { ItemTag } from './item-tag';
import { Location } from './location';

export class Item {
  id: number;
  categoryId: number;
  currentLocationId: number;
  originLocationId: number;
  name: string;
  isDeleted: boolean;

  category: Category;
  currentLocation: Location;
  originLocation: Location;

  itemTags: ItemTag[];

  get filter(): string {
    return this.category === null ?
      this.name :
      `${this.name} - ${this.category.label}`;
  }
}
```

**`location.ts`**

```ts
import { Item } from './item';

export interface Location {
  id: number;
  name: string;
  isDeleted: boolean;

  originItems: Item[];
  currentItems: Item[];
}
```

**`tag.ts`**

```ts
import { ItemTag } from './item-tag';

export interface Tag {
  id: number;
  label: string;
  isDeleted: boolean;

  tagItems: ItemTag[];
}
```

The Angular app now has interfaces for all of the C# entity types that it will interact with via Web API, and all but `Item` are defined as an `interface`. The `Item` class defines a read-only `filter` property that checks to see whether `category` has a value. If so, the filter contains both the `name` and `category.label` properties. Otherwise, it just returns the `name` property.

[Back to Top](#models)