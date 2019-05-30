# Dialogs

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Dialog Basics](#dialog-basics)
  * [Dialog Options](#dialog-options)
  * [Data Injection](#data-injection)
  * [Layout](#layout)
  * [Basic Example](#basic-example)
* [Action Dialogs](#action-dialogs)
  * [Confirm Dialog](#confirm-dialog)
  * [Editor Dialog](#editor-dialog)
  * [Bin Dialog](#bin-dialog)

## [Overview](#dialogs)

A dialog component makes use of the [Angular Material Dialog](https://material.angular.io/components/dialog/overview) to render content in a modal dialog.

It can be used to:
* Display content relevant to the current UI context
* Request an action from a user, and respond to that action
* Manage independent, self-contained functionality:
  * Add or edit data items
  * Manage a collection of items

Dialogs are defined in a separate context from **Display Components** and **Route Components** because they exist outside of the standard component tree, and require different registration from either construct.

In addition to be added to the `declarations` array in a `NgModule`, they must also be added to the `entryComponents` array. Also, the class that represents the dialog component must be exported within its defining module to make it available for consumption in route components. Because of this, the **dialogs** TypeScript module is defined using the following layout:

**`index.ts`**

```ts
// import statements
import { SomeDialog } from './some.dialog';
// additional imports

// exported Dialogs array
export const Dialogs = [
  SomeDialog,
  // additional dialogs
];

// export statements
export * from './some.dialog';
// additional exports
```

In `AppModule`, the **dialogs** module is registered as follows:

**`app.module.ts`**

```ts
import { Dialogs } from './dialogs';

@NgModule({
  declarations: [
    [...Dialogs]
  ],
  entryComponents: [
    [...Dialogs]
  ]
});
export class AppModule { }
```

## [Dialog Basics](#dialogs)

A **Dialog Component** is defined in much the same way as any other component. It has a TypeScript class, a corresponding HTML template, and optionally, CSS.

The key differences lie in:
* How the dialog component is initialized and rendered
* How data is passed to the dialog component
* The way the template is structured

Before showing an example, it's important to understand the nuances of these differences in order to be able to successfully work with dialogs for their intended purpose.

### [Dialog Options](#dialogs)

In order to show a dialog, the `MatDialog.open()` function must be called, with the **Type** of dialog to show, and optionally, the corresponding `MatDialogConfig`. The `MatDialogConfig` class represents the options that are used when creating the dialog.

> For full details, see [MatDialogConfig](https://material.angular.io/components/dialog/api#MatDialogConfig) in the Angular Material documentation.

Three important options that can be set using `MatDialogConfig`:

Option | Type | Description
-------|------|------------
**data** | `any` | The data to be injected into the dialog from the component that opens it.
**width** | `string` | Any valid CSS unit of measurement, i.e. - `300px`, `80%`, etc.
**disableClose** | `boolean` | Determines whether or not clicking outside of the modal closes it.
**autoFocus** | `boolean` | Determines whether or not to focus the first focusable element in the dialog when rendered.

`MatDialog` can be injected in a component in the same way any other service can be.

**`example.component.ts`**

```ts
import { Component } from '@angular/core';
import { MatDialog } from '@angular/material';
import { ExampleDialog } from '../../dialogs';

@Component({
  selector: 'example'
})
export class ExampleComponent {
  constructor(
    private dialog: MatDialog
  ) { }

  openDialog = () => this.dialog.open(ExampleDialog, {
    data: 'Some data for ExampleDialog',
    width: '420px',
    disableClose: true
  });
}
```

### [Data Injection](#dialogs)

In order for data to be injected into a constructor outside of the context of the Angular Dependency Injection mechanism, the data being injected must use the `@Inject()` decorator, which receives an argument of type `InjectionToken<T>`. Angular Material defines a constant token that you can use called `MAT_DIALOG_DATA`.

> See [Inject](https://angular.io/api/core/Inject) in the Angular docs, and [MAT_DIALOG_DATA](https://material.angular.io/components/dialog/api#MAT_DIALOG_DATA) in the Angular Material docs for more information.

Here's an example constructor demonstrating how to access the `data` dialog option configured above:

**`example.dialog.ts`**

```ts
import {
  Component,
  Inject
} from '@angular/core';

import { MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'example-dialog'
})
export class ExampleDialog {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: string
  ) { }
}
```

### [Layout](#dialogs)

There are four directives available for structuring dialog content:

Directive | Description
----------|------------
`mat-dialog-title` | The title of the dialog, used in conjunction with a text element.
`<mat-dialog-content>` | Primary scrollable content of the dialog.
`<mat-dialog-actions>` | Container for action buttons at the bottom of the dialog. Button alignment can be controlled via the `align` attribute which can be set to `end` and `center`.
`mat-dialog-close` | Attribute added to a `<button>` that causes the dialog to close, with an optional result provided as the value to return.

It's also important to wrap the dialog in a container that has the `.mat-typography` class applied. This is necessary to because the dialog is rendered outside of the component tree, and without this container, it will not receive the Angular Material typography styling associated with the layout.

**`example.dialog.html`**

```html
<div class="mat-typography">
  <h2 mat-dialog-title>Example Dialog</h2>
  <mat-dialog-content>
    <p>{{data}}</p>
  </mat-dialog-content>
  <mat-dialog-actions>
    <button mat-button
            mat-dialog-close>Close</button>
  </mat-dialog-actions>
</div>
```

### [Basic Example](#dialogs)

* [StackBlitz - Demo](https://docs-dialog.stackblitz.io)
* [StackBlitz - Source](https://stackblitz.com/edit/docs-dialog?file=src%2Fapp%2Fdialogs%2Fdemo.dialog.ts)

In this example, the intent is to be able to pass a `DialogContent` interface to a dialog component, and have the properties of the interface determine the content of the dialog.

**`dialog-content.ts`**
``` ts
export interface DialogContent {
  title: string;
  body: string;
  actionLabel: string;
  action: () => void;
}
```

The dialog will render a title, a body, and an action button. The label for the button will be indicated with the `actionLabel` property, and the action performed will be specified with the `action` property.

**`demo.dialog.ts`**

```ts
import {
  Component,
  Inject
} from '@angular/core';

import {
  MatDialogRef,
  MAT_DIALOG_DATA
} from '@angular/material';

import { DialogContent } from '../models';

@Component({
  selector: 'demo-dialog',
  templateUrl: 'demo.dialog.html'
})
export class DemoDialog {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: DialogContent
  ) { }
}
```

**`demo.dialog.html`**

```html
<div class="mat-typography">
  <h2 mat-dialog-title>{{data.title}}</h2>
  <mat-dialog-content>
    <p>{{data.body}}</p>
  </mat-dialog-content>
  <mat-dialog-actions>
    <button mat-button
            color="accent"
            (click)="data.action()">{{data.actionLabel}}</button>
    <button mat-button
            mat-dialog-close>Close</button>
  </mat-dialog-actions>
</div>
```

The `HomeComponent` will define a `DialogContent` instance that is passed to the `DemoDialog`, and provides an interface for modifying those values. Additionally, it allows you to specify the `width` option via a `MatSlider`, and the `disableClose` option via a `MatSlideToggle`. Clicking the **Show Dialog** button will call the component's `openDialog()` function. The `action` property will simply log the contents of the `body` property to the console.

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

import {
  MatDialog,
  MatDialogConfig
} from '@angular/material';

import { DialogContent } from '../../models';
import { DemoDialog } from '../../dialogs';

@Component({
  selector: 'home-route',
  templateUrl: 'home.component.html'
})
export class HomeComponent {
  disableClose = false;
  width = 420;

  content: DialogContent = {
    title: 'Dialog Title',
    body: 'Show some information relevant to the current UI context.',
    action: () => console.log('dialog', this.content.body),
    actionLabel: 'Log to Console'
  };

  constructor(
    private dialog: MatDialog
  ) { }

  openDialog = () => this.dialog.open(DemoDialog, {
    data: this.content,
    width: `${this.width}px`,
    disableClose: this.disableClose
  });
}
```

**`home.component.html`**

```html
<button mat-raised-button
        color="accent"
        [style.margin.px]="12"
        (click)="openDialog()">Show Dialog</button>
<mat-toolbar>Dialog Content</mat-toolbar>
<section fxLayout="column"
         fxLayoutAlign="start stretch">
  <mat-form-field>
    <mat-label>Title</mat-label>
    <input matInput [(ngModel)]="content.title">
  </mat-form-field>
  <mat-form-field>
    <mat-label>Body</mat-label>
    <input matInput [(ngModel)]="content.body">
  </mat-form-field>
  <mat-form-field>
    <mat-label>Action Label</mat-label>
    <input matInput [(ngModel)]="content.actionLabel">  
  </mat-form-field>
</section>
<mat-toolbar>Dialog Options</mat-toolbar>
<section fxLayout="column"
         fxLayoutAlign="start stretch">
  <mat-slide-toggle [(ngModel)]="disableClose">Disable Close</mat-slide-toggle>
  <p class="mat-title">Width</p>
  <mat-slider [max]="800"
              [min]="400"
              [step]="20"
              [tickInterval]="1"
              [thumbLabel]="true"
              [(ngModel)]="width"></mat-slider>
</section>
```

## [Action Dialogs](#dialogs)

Most dialogs that you create will require a degree of user interaction. In these cases, there are two types of action dialogs:

* A dialog is used to request a response from the user
* A dialog provides a self-contained feature that, when closed, requires the data to be refreshed

The examples that follow will highlight these features. But before we jump into them, there are a few dialog features to discuss that make these features possible.

The `MatDialog.open()` function returns a `MatDialogRef` object, which provides event functions you can react to based on user interaction with the dialog.

> See [MatDialogRef](https://material.angular.io/components/dialog/api#MatDialogRef) for a detailed look at all of the available events.

The important event here is `afterClosed()`, which returns an `Observable<T | undefined>`. A value can be returned when the dialog is closed, which we can then do something with. Here is what this looks like:

```ts
this.dialog.open(ConfirmDialog)
  .afterClosed()
  .subscribe(res => /* do something with the result */);
```

There are two ways that a value can be returned from a dialog:

* Bind a value to `mat-dialog-close`
* Inject a `MatDialogRef<T>` into the constructor of the dialog, where `T` is the type of the dialog
  * Pass the return value to the `close()` function of the `MatDialogRef<T>`.

**Binding a value to `mat-dialog-close`**

```html
<mat-dialog-actions>
  <button mat-button
          [mat-dialog-close]="true">Confirm</button>
  <button mat-button
          [mat-dialog-close]="false">Cancel</button>
</mat-dialog-actions>
```

**Passing a value to `MatDialogRef.close()`**

```ts
@Component({
  selector: 'demo-dialog',
  providers: [DemoService]
})
export class DemoDialog {
  item = new Item();

  constructor(
    public dialogRef: MatDialogRef<DemoDialog>,
    public service: DemoService
  ) { }

  saveItem = async () => {
    const res = this.service.saveItem(this.item);
    res && this.dialogRef.close(true);
  }
}
```

### [Confirm Dialog](#dialogs)

* [StackBlitz - Demo](https://docs-confirm-dialog.stackblitz.io/home)
* [StackBlitz - Source](https://stackblitz.com/edit/docs-confirm-dialog)

The intent of this example is to build a confirmation dialog where the following items can be customized:

* Dialog title
* Prompt to the user
* Label of the confirm button
* Label of the cancel button

When the `Confirm` button is clicked, a value of `true` is returned.

When the `Cancel` button is clicked, a value of `false` is returned.

First, an interface is needed to define these customizable properties:

**`confirm.ts`**
```ts
export interface Confirm {
  title: string;
  prompt: string;
  confirm: string;
  cancel: string;
}
```

Next, a pretty straightforward dialog is defined:

**`confirm.dialog.ts`**

```ts
import {
  Component,
  Inject
} from '@angular/core';

import { MAT_DIALOG_DATA } from '@angular/material';
import { Confirm } from '../models';

@Component({
  selector: 'confirm-dialog',
  templateUrl: 'confirm.dialog.html'
})
export class ConfirmDialog {
  confirm: Confirm;
  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Confirm
  ) {
    this.confirm = data ?
      data :
      {
        title: 'Confirm Action?',
        prompt: 'Are you sure you would like to perform this action?',
        confirm: 'Confirm',
        cancel: 'Cancel'
      };
  }
}
```

An object that implements the `Confirm` interface is injected into the dialog. If no object is received, a default object is created.

**`confirm.dialog.html`**

```html
<div class="mat-typography">
  <h2 mat-dialog-title>{{confirm.title}}</h2>
  <mat-dialog-content>
    <p>{{confirm.prompt}}</p>
  </mat-dialog-content>
  <mat-dialog-actions>
    <button mat-button
            [mat-dialog-close]="true">{{confirm.confirm}}</button>
    <button mat-button
            [mat-dialog-close]="false">{{confirm.cancel}}</button>
  </mat-dialog-actions>
</div>
```

The `title` property is rendered as the `mat-dialog-title`.

The `prompt` property is rendered inside fo the `mat-dialog-content`.

If the **Confirm** button is clicked, the dialog is closed returning a value of `true`. The label for the button is the `confirm` property.

If the **Cancel** button is clicked, the dialog is closed returning a value of `false`. The label for the button is the `cancel` property.

All that's left to do now is make use of the dialog.

**`home.component.ts`**

```ts
import {
  Component,
  OnDestroy
} from '@angular/core';

import { MatDialog } from '@angular/material';
import { Subscription } from 'rxjs';
import { Confirm } from '../../models';
import { ConfirmDialog } from '../../dialogs';

@Component({
  selector: 'home-route',
  templateUrl: 'home.component.html'
})
export class HomeComponent implements OnDestroy {
  private subs = new Array<Subscription>();
  result: string;

  confirm: Confirm = {
    title: 'Umm...',
    prompt: 'Did you really intend to press the big red button?',
    confirm: 'Yes',
    cancel: 'No'
  }

  constructor(
    private dialog: MatDialog
  ) { }

  ngOnDestroy() {
    this.subs.forEach(x => x && x.unsubscribe());
  }

  openConfirmationDialog = () => this.subs.push(this.dialog.open(ConfirmDialog, {
    data: this.confirm,
    autoFocus: false
  })
  .afterClosed()
  .subscribe(res => 
    res !== undefined && 
    (this.result = res ? 
      `I don't know how you're reading this. The universe is dead now.` : 
      'Whew. Almost blew up the universe.'
    )
  ));
}
```

A `confirm: Confirm` property is created to pass into `MatDialogConfig.data` when opening a `ConfirmDialog`.

`MatDialog` is injected into the constructor of the component.

The `openConfirmationDialog()` opens the `ConfirmDialog`, passing the `confirm` property as `data`. The `afterClosed()` function generates an Observable that is subscribed to, passing the result when the dialog closes. This subscription is captured in the `subs` array so it can be disposed of properly in the **OnDestroy** lifecycle hook.

If `res` is not `undefined`, meaning the user actually clicked an action button as opposed to clicking the backdrop to close the dialog, the `result` property is set based on the returned value.

**`home.component.html`**

```html
<mat-toolbar>Confirmation</mat-toolbar>
<button mat-raised-button
        color="warn"
        [style.margin.px]="12"
        (click)="openConfirmationDialog()">Big Red Button</button>
<section *ngIf="result"
         class="container">
  <p>{{result}}</p>
</section>
```

Clicking **Big Red Button** calls the `openConfirmationDialog()` function.

If `result` has a value, it is rendered below the button.

### [Editor Dialog](#dialogs)

### [Bin Dialog](#dialogs)

[Back to Top](#dialogs)