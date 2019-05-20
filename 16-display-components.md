# Display Components

* [Overview](#overview)
* [BannerComponent](#bannercomponent)
* [FileUploadComponent](#fileuploadcomponent)
* [API Components](#api-components)
    * [ItemCardComponent](#itemcardcomponent)
    * [ItemListComponent](#itemlistcomponent)

## [Overview](#display-components)

As stated in the [Components](./14-components.md) article:

> Display Components can be thought of as extending the HTML specification. In the same way that a `<p>` or `<input>` tag has attributes, properties, and events, so too do Display Components. They do not interact with services; they exclusively rely on an external source for their properties to be set, and for their events to be responded to.

This means that they can define up to three things:

* A template for organizing display
* Input properties that can be set
* Output events that can be registered

A simple display component would look something like this:

**`simple.ts`**

```ts
export interface Simple {
  title: string;
  description: string;
}
```

**`simple.component.ts`**

```ts
import {
  Component,
  Input,
  Output,
  EventEmitter
} from '@angular/core';

import { Simple } from '../../models';

@Component({
  selector: 'simple',
  templateUrl: 'simple.component.html'
})
export class SimpleComponent {
  @Input() simple: Simple;
  @Output() select = new EventEmitter<Simple>();
}
```

**`simple.component.html`**

```html
<section fxLayout="column"
         fxLayoutAlign="start stretch"
         (click)="select.emit(simple)"
         class="background card elevated clickable">
  <p class="mat-title">{{simple.title}}</p>
  <p>{{simple.description}}</p>
</section>
```

This simple component accepts a single `simple: Simple` input property, and defines a `select: EventEmitter<Simple>` event handler. It is displayed as a card and renders `simple.title` at the top, using the `mat-title` style, and the `simple.description` at the bottom in regular font.

> For a detailed listing of the available Angular Material typography styles, see the [Typography](https://material.angular.io/guide/typography) guide.

[StackBlitz - Simple Display Component](https://stackblitz.com/edit/docs-simple-display-component?file=src%2Fapp%2Fcomponents%2Fsimple%2Fsimple.component.html)

## [BannerComponent](#display-components)

The provided app stack template defines a `BannerComponent` out of the box, and is used in `AppComponent` to render a classification banner along the top of the app.

In terms of Display Components, it doesn't get more simple than this:

**`banner.component.ts`**

```ts
import {
  Component,
  Input
} from '@angular/core';

@Component({
  selector: 'banner',
  templateUrl: 'banner.component.html'
})
export class BannerComponent {
  @Input() label: string;
  @Input() background: string;
  @Input() color: string;
}
```

* `label` determines the classification that will be rendered on the banner
* `background` determines the background color the banner will be rendered with
* `color` determines the color the banner text will be rendered in

The component template is just as simple:

**`banner.component.html`**

```html
<section fxLayout="row"
         fxLayoutAlign="center center"
         [style.background-color]="background"
         [style.color]="color"
         class="mat-typography">
  {{label}}
</section>
```

* `[style.background-color]` is set to the value of the `background` input property
* `[style.color]` is set to the value of the `color` input property
* `label` is rendered within the body of the component

## [FileUploadComponent](#display-components)

The `FileUploadComponent` is also defined in the base app stack template provided. It is considerably more complicated than the `BannerComponent` shown above.

> The [Uploads](./a4-uploads.md) article will detail the process of handling file uploads. This section is just concerned with showing how to build a robust component based around the `<input type="file">` element.
>
> If you're unfamiliar with the HTML File API, see the [File API]() documentation for detailed information.

The first thing to consider is the style of the component:

**`file-upload.component.css`**

```css
input[type=file] {
  display: none;
}
```

There is a `<input type="file">` defined in the template for the component, but it is never displayed. Instead, the interactions that the file input element is responsible for are handled by a `MatButton` component. This is possible thanks to **template reference variables** and the **ViewChild** decorator, as you will see shortly.

**`file-upload.component.html`**

```html
<input type="file"
       (change)="fileChange($event)"
       #fileInput
       [accept]="accept"
       [multiple]="multiple">
<button mat-button
        [color]="color"
        (click)="fileInput.click()">{{label}}</button>
```

> `MatButton` in the following descriptions refers to the `<button mat-button>` element. This is intentionally to differentiate the element as an Angular Material component, as opposed to the HTML `<button>` element.

* A reference to `<input type="file">` is obtained via the `#fileInput` template reference variable.
* The `(change)` event is registered to the `fileChange($event)` function defined by the component.
* `accept` determines which file types can be selected and is set to the value of the `accept` input property, defined by the component.
* `multilple` determines whether multiple files can be selected and is set to the value of the `multiple` input property, defined by the component.
* The `color` of the `MatButton` component is set with the `color` input property, defined by the component.
* Clicking the `MatButton` component calls the `click()` event handler on the `<input type="file">` element using the defined template reference variable.

**`file-upload.component.ts`**

```ts
import {
  Component,
  EventEmitter,
  Input,
  Output,
  ViewChild,
  ElementRef
} from '@angular/core';

@Component({
  selector: 'file-upload',
  templateUrl: 'file-upload.component.html',
  styleUrls: ['file-upload.component.css']
})
export class FileUploadComponent {
  @ViewChild('fileInput') fileInput: ElementRef;
  @Input() accept = '*/*';
  @Input() color = 'primary';
  @Input() label = 'Browse...';
  @Input() multiple = true;
  @Output() selected = new EventEmitter<[File[], FormData]>();

  fileChange = (event: any) => {
    const files: FileList = event.target.files;
    const fileList = new Array<File>();
    const formData = new FormData();

    for (let i = 0; i < files.length; i++) {
      formData.append(files.item(i).name, files.item(i));
      fileList.push(files.item(i));
    }

    this.selected.emit([fileList, formData]);
    this.fileInput.nativeElement.value = null;
  }
}
```

> See the [Basic Types - Tuple](https://www.typescriptlang.org/docs/handbook/basic-types.html#tuple) documentation if you are unfamiliar with TypeScript tuples.

**Properties**

* `fileInput` is an `ElementRef` instance of the `<input type="file">` and is obtained using a query via the `@ViewChild('fileInput')` decorator.
* `accept` is an input property with a default value of **\*/\***, which specifies that any file type can be selected.
* `color` is an input property with a default value of **primary**, which specifies the color of the `MatButton` component defined in the template.
* `label` is an input property with a default value of **Browse...**, which specifies the text that will be rendered on the `MatButton` component.
* `multiple` is an input property with a default value of `true`, which specifies whether or not multiple files can be selected inside of the file selector linked to the file input element.
* `selected` is an output property that returns a tuple of type `[File[], FormData]`. This is automatically emitted after the `File[]` and `FormData` properties have been populated as a result of the file input's `change` event triggering the `fileChange` function in this component.
    * Remember, `(change)="fileChange($event)"` is specified on `<input type="file">` in the template definition.

**Functions**

`fileChange(event: any)` receives the event from the file input's `change` event when it is triggered. 

* A `files: FileList` constant is defined as the value of `event.target.files`.
* A `fileList` constant is defined as a new `Array<File>` array.
* A `formData` constant is defined as a new `FormData` object.
* The `files` constant is iterated through in a `for` loop, and each file is added to both the `fileList` and `formData` objects.
* After the loop is complete, `selected.emit` triggers the `EventEmitter` and passes the values of `fileList` and `formData` in a Tuple.
* The value of the `<input type="file">` element is cleared after the `selected` event is emitted.

## [API Components](#display-components)

The above components are generically written to be used in any Angular application. The following section picks back up where the [Services](./13-services.md) article left off with demonstrating how the application flows from back to front. Before covering **Route Components**, it's important to understand how **Display Components** work so that you can understand the intent of a route component.

Two of the most common types of components that you will write are:

* Displaying a list of objects
* Rendering an object as a card

This section will define, from scratch, display components that render these capabilities with data retrieved from the `ItemService`.

### [ItemCardComponent](#display-components)

Before defining what a card for an `Item` should look like, it helps to see what the shape of the data is:

```json
{
  id: 1,
  categoryId: 1,
  originLocationId: 1,
  currentLocationId: 2,
  name: "Item A",
  isDeleted: false,
  category: {
    id: 1,
    name: "Category A",
    isDeleted: false,
    items: null
  },
  originLocation: {
    id: 1,
    name: "Location A",
    isDeleted: false
  },
  currentLocation: {
    id: 2,
    name: "Location B",
    isDeleted: false
  },
  itemTags: [
    {
      id: 1,
      itemId: 1,
      tagId: 1,
      tag: {
        id: 1,
        label: "Tag A",
        isDeleted: false
      },
      item: null
    },
    {
      id: 2,
      itemId: 1,
      tagId: 2,
      tag: {
        id: 2,
        label: "Tag B",
        isDeleted: false
      },
      item: null
    }
  ]
}
```

The following properties will be displayed in the `ItemCardComponent`:

* `name`
* `category.name`
* `originLocation.name`
* `currentLocation.name`
* `itemTags.tag.label`

The component will allow the following functionality:

* Determine whether the item is editable
  * The name of the item can be updated
  * The item can be deleted

Card implementation details:
* The same card will be able to toggle between viewing and editing. 
* If you begin editing and cancel, the changes that you made will not be lost. Going back to editing should allow you to resume editing where you left off.

With these details in mind, here is the `ItemCardComponent`:

**`item-card.component.ts`**

```ts
import {
  Component,
  Input,
  Output,
  EventEmitter
} from '@angular/core';

import { Item } from '../../models';

@Component({
  selector: 'item-card',
  templateUrl: 'item-card.component.html',
  styleUrls: ['item-card.component.css']
})
export class ItemCardComponent {
  itemState: Item = null;
  editing = false;

  @Input() item: Item = null;
  @Input() editable = true;
  @Output() save = new EventEmitter<Item>();
  @Output() remove = new EventEmitter<Item>();

  editItem = () => {
    if (this.editable) {
      if (this.itemState == null)
        this.itemState = Object.assign(new Item, this.item);

      this.editing = true;
    }
  }

  cancelEdit = () => this.editing = false;

  saveItem = () => {
    if (this.editable) {
      this.item = this.itemState;
      this.save.emit(this.item);
    }
  }

  removeItem = () => this.editable && this.remove.emit(this.item);
}
```

**Properties**

* `itemState: Item` is used to manage the state of the item when it is being edited.
* `editing` is used to track whether or not the card is currently in view or edit mode.
* `item: Item` is an input property for the item the card represents
* `editable` determines whether or not the current item can even be edited.
* `save` is an output event that is triggered whenever you want to save changes to the item when in edit mode.
* `remove` is an output event that is triggered whenever you want to delete the item in when in edit mode.

**Functions**

* `editItem()` is tied to the edit button, which can only be clicked (and will only be visible) when `editable` is `true`. It is only shown when `editing` is `false`.
  * If `itemState` is null, it is assigned the value of a new `Item` instance with the values of the `item` property.
  * `editing` is set to `true`.
* `cancelEdit()` is tied to the cancel button and simply sets `editing` back to `false`.
* `saveItem` is tied to the save button, which can only be clicked (and will only be visible) when `editable` is `true`. It is only shown when `editing` is `true`.
  * The value of `item` is set to the value of `itemState`, then is emitted through the `save` output event.
* `removeItem` is tied to the delete button, which can only be clicked (and will only be visible) when `editable` is `true`. It is only shown when `editing` is `true`.

**`item-card.component.html`**

```html
<section class="background card static-elevation arrow"
         fxLayout="column"
         fxLayoutAlign="start stretch">
  <section fxLayout="row"
           fxLayoutAlign="start center">
    <p *ngIf="!(editing)"
       class="mat-subheading-2"
       fxFlex>{{item.name}}</p>
    <mat-form-field *ngIf="editing"
                    fxFlex>
      <input matInput
             [(ngModel)]="itemState.name"
             fxFlex>
    </mat-form-field>
    <button *ngIf="!(editing)"
            mat-icon-button
            (click)="editItem()">
      <mat-icon>edit</mat-icon>
    </button>
    <button *ngIf="editing"
            mat-icon-button
            (click)="cancelEdit()">
      <mat-icon>cancel</mat-icon>
    </button>
    <button *ngIf="editing"
            mat-icon-button
            (click)="saveItem()">
      <mat-icon>save</mat-icon>
    </button>
    <button *ngIf="editing"
            mat-icon-button
            color="warn"
            (click)="removeItem()">
      <mat-icon>delete</mat-icon>
    </button>
  </section>
  <mat-chip-list class="background stacked">
    <mat-chip *ngFor="let t of item.itemTags"
              color="accent"
              selected>{{t.tag.label}}</mat-chip>
  </mat-chip-list>
  <section fxLayout="column"
           fxLayoutAlign="start stretch"
           class="container">
    <p>Category: {{item.category.name}}</p>
    <p>Current Location: {{item.currentLocation.name}}</p>
    <p>Origin Location: {{item.originLocation.name}}</p>
  </section>
</section>
```

The component template for `ItemCardComponent` can be broken down into three sections:

* Header:
  * Renders `item.name` inside of a `<p>` element when `editing` is `false`.
  * Binds `item.name` with `[(ngModel)]` to an `<input matInput>` element when `editing` is `true`.
  * Displays an **Edit** button whenever `editing` is `false`.
  * Displays **Cancel**, **Save**, and **Delete** buttons whenever `editing` is `true`.
* Tag list:
  * Displays each `Tag` in `item.itemTags` as a `<mat-chip>` inside of a `<mat-chip-list>`.
* Body
  * Displays `item.category.name`, `item.currentLocation.name`, and `item.originLocation.name`

**`item-card.component.css`**

```css
mat-form-field.mat-form-field {
  margin: 0 8px;
}

mat-chip-list.mat-chip-list {
  padding: 8px;
}
```

Minor layout styling for elements defined in the card template.

### [ItemListComponent](#display-components)

Being able to repeat the process of rendering an `Item` into a card is helpful, but it can also be helpful to be able to repeat the process of rendering multiple `Item` cards as a list.

A very important thing to keep in mind is that, because this is an intermediate component, you need to be able to relay any events raised by `ItemCardComponent` back to the **Route Component** responsible for handling those events. In this case, the following output events need to be provided:

* `save: EventEmitter<Item>`
* `remove: EventEmitter<Item>`

**`item-list.component.ts`**

```ts
import {
  Component,
  Input,
  Output,
  EventEmitter
} from '@angular/core';

import { Item } from '../../models';

@Component({
  selector: 'item-list',
  templateUrl: 'item-list.component.html'
})
export class ItemListComponent {  
  @Input() layout: string = "row | wrap";
  @Input() align: string = "space-evenly start";
  @Input() cardWidth: number = 420;

  @Input() items: Item[];
  @Output() save = new EventEmitter<Item>();
  @Output() remove = new EventEmitter<Item>();
}
```

* `layout` is used to determine the Flexbox layout of the list container
* `align` is used to determine how the items in the Flexbox container are aligned
* `cardWidth` specifies how wide the cards will be rendered inside of the list container
* `items` is the collection of items to render inside of the list container
* `save: EventEmitter<Item>` forwards any `save` output events triggered by an `ItemCardComponent`
* `remove: EventEmitter<Item>` forwards any `remove` output events triggered by an `ItemCardComponent`

**`item-list.component.html`**

```html
<section class="container"
         [fxLayout]="layout"
         [fxLayoutAlign]="align">
  <item-card *ngFor="let i of items"
             [item]="i"
             (save)="save.emit($event)"
             (remove)="remove.emit($event)"
             [style.width.px]="cardWidth"></item-card>
</section>
```

* The `layout` input property is bound to `fxLayout` on the root `<section>` element
* The `align` input property is bound to `fxLayoutAlign` on the root `<section>` element
* The `items` input property is iterated, and each `Item` is passed into an `<item-card>` component. The `save` and `remove` output events are registered to simply emit the corresponding output event defined on the list component.
* The `cardWidth` input property is specified as the `style.width.px` value for each `<item-card>` component rendered.

[StackBlitz - Item Components](https://stackblitz.com/edit/docs-item-components?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.ts)