# Display Components

* [Overview](#overview)
* [BannerComponent](#bannercomponent)
* [FileUploadComponent](#fileuploadcomponent)
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

> The [Attachments](./a3-attachments.md) article will detail the process of handling file uploads. This section is just concerned with showing how to build a robust component based around the `<input type="file">` element.
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

## [ItemCardComponent](#display-components)

## [ItemListComponent](#display-components)