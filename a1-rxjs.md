# RxJS

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Creating Streams](#creating-streams)
    * [Subjects](#subjects)
    * [Functions](#functions)
* [Operators](#operators)
* [Piping Operators](#piping-operators)  

> The examples in this article can be found on [StackBlitz - Docs RxJS](https://stackblitz.com/edit/docs-rxjs)

## [Overview](#rxjs)

Per the [RxJS documentation](https://rxjs.dev/guide/overview):  

> RxJS is a library for composing asynchronous and event-based programs using observable sequences.  

The essential concepts in RxJS which solve async event management are:

* [Observable](https://rxjs.dev/guide/observable) - represents the idea of an invokable collection of future values or events.
* [Observer](https://rxjs.dev/api/index/interface/Observer) - A collection of callbacks that know how to listen to values delivered by the Observable.
* [Subscription](https://rxjs.dev/guide/subscription) - Represents the execution of an Observable, is primarily useful for cancelling the execution.
* [Operators](https://rxjs.dev/guide/operators) - Pure functions that enable a functional programming style of dealing with collections with operations like `map`, `filter`, `concat`, `reduce`, etc.
* [Subject](https://rxjs.dev/guide/subject) - The equivalent of an EventEmitter, and the only way of multicasting a value or event to multiple Observers.
* [Schedulers](https://rxjs.dev/guide/scheduler) - Centralized dispatchers to control concurrency, allowing us to coordinate when computation happens on e.g. `setTimeout` or `requestAnimationFrame` or others.

> The following section will look at an example that uses all of the above except for **Schedulers**. Refer to the documentation for detailed information on Schedulers.  

Consider the following Angular component:

**`overview.component.ts`**  

```ts
import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  Observable,
  Subject,
  Subscription
} from 'rxjs';

@Component({
  selector: 'overview',
  templateUrl: 'overview.component.html'
})
export class OverviewComponent implements OnInit, OnDestroy {
  private subject = new Subject<number>();
  private subscription: Subscription;

  error: string;
  stream = new Array<number>();
  observable = this.subject.asObservable();
  closed = false;

  ngOnInit() {
    this.subscription = this.observable
      .subscribe(
        val => {
          this.stream.push(val);
          console.info(`stream received value: ${val}`);
        },
        err => {
          this.error = err;
          this.closed = true;
          console.error(`error thrown : ${err}`);
        },
        () => {
          this.closed = true;
          console.log('complete called!');
        }
      );
  }

  ngOnDestroy() {
    if (this.subscription && !this.subject.closed) {
      this.subscription.unsubscribe();
    }
  }

  triggerSubject = () => {
    this.stream.length > 9 ?
      this.subject.error('The array buffer has been exceeded!') :
      this.subject.next(this.getRandomNumber());
  }

  triggerComplete = () => {
    this.subject.complete();
  }

  private getRandomNumber = (): number => Math.floor(Math.random() * Math.floor(200));
}
```

* A `private subject: Subject<number>` is used to manage the values provided by the Observable stream.
* A `private subscription: Subscription` is defined to allow the stream to be unsubscribed in the `OnDestroy` lifecycle hook.
* `error: string` is used to capture any errors that are encountered in the Observer.
* `stream: Array<number>` keeps track of all of the values that are received by the stream.
* The `observable` property is a public, read-only instance of the stream managed by the `subject` property.
* `closed` is used to keep track of whether or not the stream can still be observed.

When the `OnInit` lifecycle hook is triggered, the `observable` property is subscribed to, and the three arrow functions defined inside of the `.subscribe()` function implement the **Observer** interface:

* A `next` function that is triggered when a new value is received by the stream
* An `error` function that is triggered whenever the stream encounters an error
* A `complete` function that is triggered whenever the stream is successfully closed without encountering any errors

When the `OnDestroy` lifecycle hook is triggered, if `subscription` has a value and `subject.closed` is `false` (it hasn't already been unsubscribed), then `subscription.unsubscribe()` is called to close the stream.

The `triggerSubject()` function checks to see if the `stream` array contains more than 9 entries. If so, an error is thrown on the **Subject** (in order to demonstrate the `error` function of the **Observer** defined in the `.subscribe()` function during `OnInit`). Otherwise, a new random number is provided to the stream.

The `triggerComplete()` function calls the `complete()` function on the stream, triggering the `complete` function specified in the **Observer** defined in the `.subscribe()` function during `OnInit`.

The `getRandomNumber()` private function returns a random value between 1 - 200.

For completeness, here's the template defined for the component:

**`overview.component.html`**

```html
<mat-toolbar>Overview</mat-toolbar>
<section>
  <button mat-stroked-button 
          color="accent"
          (click)="triggerSubject()"
          [style.margin.px]="8"
          [disabled]="closed">Trigger Value</button>
  <button mat-stroked-button
          color="warn"
          (click)="triggerComplete()"
          [style.margin.px]="8"
          [disabled]="closed">Complete</button>
</section>
<ng-container *ngIf="error?.length > 0">
  <mat-toolbar>Error</mat-toolbar>
  <h3 class="color warn"
      [style.margin.px]="12">{{error}}</h3>
</ng-container>
<ng-container *ngIf="closed">
  <mat-toolbar>Closed</mat-toolbar>
  <h3 [style.margin.px]="12">The stream is closed</h3>
</ng-container>
<ng-container *ngIf="stream.length > 0">
  <mat-toolbar>Stream</mat-toolbar>
  <pre class="background card" 
       [style.padding.px]="8"><code>{{stream | json}}</code></pre>
</ng-container>
<ng-container *ngIf="!(stream.length > 0)">
  <h3 [style.margin.px]="12">The stream is currently empty!</h3>
</ng-container>
```

You can see this example at [StackBlitz - RxJS Overview](https://docs-rxjs.stackblitz.io/overview).

## [Creating Streams](#rxjs)

### [Subjects](#rxjs)

### [Functions](#rxjs)

## [Operators](#rxjs)

## [Piping Operators](#rxjs)

[Back to Top](#rxjs)