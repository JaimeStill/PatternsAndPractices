# RxJS

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Subjects](#subjects)
  * [BehaviorSubject](#behaviorsubject)
  * [AsyncSubject](#asyncsubject)
  * [ReplaySubject](#replaysubject)
* [Operators](#operators)
  * [Creation Operators](#creation-operators)
  * [Join Creation Operators](#join-creation-operators)
  * [Transformation Operators](#transformation-operators)
  * [Filtering Operators](#filtering-operators)
  * [Join Operators](#join-operators)
  * [Multicasting Operators](#multicasting-operators)
  * [Error Handling Operators](#error-handling-operators)
  * [Utility Operators](#utility-operators)
  * [Boolean Operators](#boolean-operators)
  * [Mathematical Operators](#mathematical-operators)
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

## [Subjects](#rxjs)

Documentation: [Subjects](https://rxjs.dev/guide/subject)

> An RxJS Subject is a special type of Observable that allows values to be multicasted to many Observers. While plain Observables are unicast (each subscribed Observer owns an independent execution of the Observable), Subjects are multicast.
>
> A Subject is like an Observable, but can multicast to many Observers. Subjects are like EventEmitters: they maintain a registry of many listeners.

The example provided in the [Overview](#overview) section above makes use of the base `Subject`, but lets take a look at an example that demonstrates the multicasting capabilities of `Subject`.

**`subject.service.ts`**

```ts
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable()
export class SubjectService {
  private getRandomNumber = (): number => Math.floor(Math.random() * Math.floor(200));

  private subject = new Subject<number>();
  subjectA$ = this.subject.asObservable();
  subjectB$ = this.subject.asObservable();

  updateSubject = () => this.subject.next(this.getRandomeNumber());
}
```  

* `getRandomNumber()` is just a convenience function for triggering a random number value.
* The `subjectA$` and `subjectB$` **Observable** streams are created based on a `private subject: Subject` property.
  * Anyone who subscribes to either `subjectA$` or `subjectB$` will receive the appropriate values generated by `subject`.
* `updateSubject()` allows you to update the current value of `subject`.

**`subjects.component.ts`**

```ts
import { Component } from '@angular/core';
import { SubjectService } from '../../services';

@Component({
  selector: 'subjects',
  templateUrl: 'subjects.component.html',
  providers: [ SubjectService ]
})
export class SubjectsComponent {
  constructor(
    public service: SubjectService;
  ) { }
}
```

**`subjects.component.html`**

```html
<mat-toolbar>Subject</mat-toolbar>
<section>
  <p>Multicast an Observable stream to multiple subscribers</p>
</section>
<button mat-stroked-button
        color="accent"
        (click)="service.updateSubject()"
        [style.margin.px]="8">Update Subject</button>
<section fxLayout="row"
         fxLayoutAlign="start start">
  <ng-template #saEmpty>
    <p fxFlex>Observable A, based on Subject, is currently empty</p>
  </ng-template>
  <ng-template #sbEmpty>
    <p fxFlex>Observable B, based on Subject, is currently empty</p>
  </ng-template>
  <ng-container *ngIf="service.subjectA$ | async as sa else saEmpty">
    <section class="background card static-elevation arrow"
             fxLayout="column"
             fxLayoutAlign="start stretch"
             fxFlex>
      <p class="mat-body-strong">Observable A</p>
      <p>Current Value: {{sa}}</p>
    </section>
  </ng-container>
  <ng-container *ngIf="service.subjectB$ | async as sb else sbEmpty">
    <section class="background card static-elevation arrow"
             fxLayout="column"
             fxLayoutAlign="start stretch"
             fxFlex>
      <p class="mat-body-strong">Observable B</p>
      <p>Current Value: {{sb}}</p>
    </section>
  </ng-container>
</section>
```  

* A button is provided that allows you to call `service.updateSubject()`, triggering `service.subjectA$` and `service.subjectB$` subscriptions to receive a new value
* If `service.subjectA$` has a value, it will render the value
  * Otherwise, it will render a template communicating that the stream is empty
* If `service.subjectB$` has a value, it will render the value
  * Otherwise, it will render a template communicating that the stream is empty

[StackBlitz - Subjects](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Fservices%2Fsubject.service.ts)

### [BehaviorSubject](#rxjs)

The only difference between a `Subject` and a `BehaviorSubject` is:
* `BehaviorSubject` can be initialized with a starting value
* The stream value contained in `BehaviorSubject` can be accessed with the `value` property

Here's a quick example of a BehaviorSubject:

**`subject.service.ts`**

```ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class SubjectService {
  privagte getRandomNumber = (): number => Math.floor(Math.random() * Math.floor(200));
  private behaviorSubject = new BehaviorSubject<number>(this.getRandomNumber());

  behaviorSubject$ = this.behaviorSubject.asObservable();

  updateBehaviorSubject = () => this.behaviorSubject.next(this.getRandomNumber());
}
```

* This implementation is the same as the implementation for `Subject`, but now, `behaviorSubject` is initialized with an initial value
  * Also, only a single stream, `behaviorSubject$`, is generated from the `BehaviorSubject`.

**`subject.component.ts`**

```ts
import { Component } from '@angular/core';
import { SubjectService } from '../../services';

@Component({
  selector: 'subjects',
  templateUrl: 'subjects.component.html',
  providers: [ SubjectService ]
})
export class SubjectsComponent {
  constructor(
    public service: SubjectService;
  ) { }
}
```

**`subject.component.html`**

```html
<mat-toolbar>BehaviorSubject</mat-toolbar>
<section>
  <p>Contains an initial value, and the wrapped stream value can be accessed directly by the BehaviorSubject</p>
</section>
<button mat-stroked-button
        color="accent"
        (click)="service.updateBehaviorSubject()"
        [style.margin.px]="8">Update BehaviorSubject</button>
<section fxLayout="row"
         fxLayoutAlign="start start">
  <section class="container background card static-elevation arrow"
           *ngIf="service.behaviorSubject$ | async as a"
           fxFlex>
    <p class="mat-subheading-2">Behavior A</p>
    <p>Value: {{a}}</p>
  </section>
  <section class="container background card static-elevation arrow"
           *ngIf="service.behaviorSubject$ | async as b"
           fxFlex>
    <p class="mat-subheading-2">Behavior B</p>
    <p>Value: {{b}}</p>
  </section>
</section>
```

This example works the same as the `Subject` example, with the following differences:

* No template is needed to determine if the stream has a value, because it is initialized with a value
* The same stream, `behaviorSubject$`, is used in both stream subscriptions, as opposed to generating two separate streams
  * This same concept applies to `Subject`, and a single `subject$` stream could have been used for **Subject A** and **Subject B**

### [AsyncSubject](#rxjs)

`AsyncSubject` is considerably different from `Subject` in that it only ever provides a value when it calls `complete()`. An `AsyncSubject` can be updated with many values, but observers will only see a value passed through `subscribe()` whenever the subject has completed.

To understand this, here's a practical example:

**`subject.service.ts`**

```ts
import { Injectable } from '@angular/core';

import {
  AsyncSubject,
  Subject
} from 'rxjs';

@Injectable()
export class SubjectService {
  private getRandomNumber = (): number => Math.floor(Math.random() * Math.floor(200));

  private asyncSubject = new AsyncSubject<number>();
  private progressSubject = new Subject<string>();

  asyncSubject$ = this.asyncSubject.asObservable();
  progressSubject$ = this.progressSubject.asObservable();

  updateAsyncSubject = () => {
    const rand = this.getRandomNumber();
    this.progressSubject.next(`AsyncSubject triggered next: ${rand}`);
    this.asyncSubject.next(rand);
  }

  completeAsyncSubject = () => this.asyncSubject.complete();
}
```

* `private progressSubject: Subject` is used to asynchronously relay whenever the value of `asyncSubject` is updated
* The `updateAsyncSubject()` function generates a message for `progressSubject`, and updates the value of `asyncSubject`
* The `completeAsyncSubject()` function calls `.complete()` on `asyncSubject()`.

**`subject.component.ts`**

```ts
import {
  Component,
  OnInit
} from '@angular/core';

import { SubjectService } from '../../services';

@Component({
  selector: 'subjects',
  templateUrl: 'subjects.component.html',
  providers: [ SubjectService ]
})
export class SubjectsComponent implements OnInit {
  constructor(
    public service: SubjectService
  ) { }

  ngOnInit() {
    this.runAsyncSubject();
  }

  runAsyncSubject = () => {
    let it = 0;
    const id = setInterval(() => {
      if (it < 9) {
        this.service.updateAsyncSubject();
        it++;
      } else {
        this.service.completeAsyncSubject();
        clearInterval(id);
      }
    }, 1000);
  }
}
```

* In the **OnInit** lifecycle hook, the `runAsyncSubject()` function is called
* `runAsyncSubject()` defines an iterator variable to capture how many times an interval has occured
  * `setInterval()` executes every **1000** milliseconds
  * If the value of the iterator variable, `it`, is less than **9**, `this.service.updateAsyncSubject()` is called, causing the `service.asyncSubject` and `service.progressSubject` to both be updated
  * Otherwise, `this.service.completeAsyncSubject()` is called, and the interval is cleared
    * This is when the observer of the `asyncSubject$` stream will actually receive a value through its subscription before it is closed

**`subject.component.html`**

```html
<mat-toolbar>AsyncSubject</mat-toolbar>
<section>
  <p>Only emits a value when it completes.</p>
</section>
<ng-template #asyncLoading>
  <section [style.padding.px]="8"
           *ngIf="service.progressSubject$ | async as progress">
    <p>{{progress}}</p>
  </section>
</ng-template>
<ng-container *ngIf="service.asyncSubject$ | async as a else asyncLoading">
  <section class="container background card static-elevation arrow">
    <p class="mat-subheading-2">AsyncSubject</p>
    <p>Value: {{a}}</p>
  </section>
</ng-container>
```

While the value of `service.asyncSubject$` is empty, a template is displayed that subscribes to the `progressSubject$` stream, indicating when the value of `asyncSubject$` has been updated. Whenever `asyncSubject.complete()` is called, `asyncSubject$` will receive the final value of the stream before it is closed, and the value will be rendered.

### [ReplaySubject](#rxjs)

A `ReplaySubject` differs from `Subject` in that whenever an observer subscribes, any value the subject has held will be sent to the new subscriber.

**`subject.service.ts`**

```ts
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';

@Injectable()
export class SubjectService {
  private getRandomNumber = (): number => Math.floor(Math.random() * Math.floor(200));

  private replaySubject = new ReplaySubject<number>();

  replaySubjectA$ = this.replaySubject.asObservable();
  replaySubjectB$ = this.replaySubject.asObservable();

  updateReplaySubject = () => this.replaySubject.next(this.getRandomNumber());
}
```

* The `replaySubjectA$` and `replaySubjectB$` streams are generated from `private replaySubject: ReplaySubject<number>`
* `updateReplaySubject()` function updates the value of `replaySubject` with a random number generated from `getRandomNumber()`

**`subject.component.ts`**

```ts
import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import { Subscription } from 'rxjs';
import { SubjectService } from '../../services';

@Component({
  selector: 'subjects',
  templateUrl: 'subjects.component.html',
  providers: [ SubjectService ]
})
export class SubjectsComponent implements OnInit, OnDestroy {
  private subs = new Array<Subscription>();
  replayA = new Array<number>();
  replayB = new Array<number>();

  private unsubscribe = () => this.subs.forEach(x => {
    if (x && !x.closed) {
      x.unsubscribe();
    }
  });

  constructor(
    public service: SubjectService
  ) { }

  ngOnInit() {
    this.runReplaySubject();
  }

  ngOnDestroy() {
    this.unsubscribe();
  }

  runReplaySubject = () => {
    let it: number = 0;
    const id = setInterval(() => {
      switch (it) {
        case 0:
          this.service.updateReplaySubject();
          this.subs.push(this.service.replaySubjectA$.subscribe(val => this.replayA.push(val)));
          break;
        case 2:
          this.service.updateReplaySubject();
          this.subs.push(this.service.replaySubjectB$.subscribe(val => this.replayB.push(val)));
          break;
        case 5:
          clearInterval(id);
          break;
        default:
          this.service.updateReplaySubject();
          break;
      }
      it++;
    }, 1000);
  }
}
```

* `private subs: Array<Subscription>` is used to keep track of internal subscriptions, so they can be properly unsubscribed in the **OnDestroy** lifecycle hook
* `replayA` and `replayB` are used to capture any values received by the streams generated from `replaySubject`.
* `private unsubscribe()` is a convenience method for iterating through all of the **Subscription** objects in the `subs` property, and appropriately unsubscribing from any open streams.
* `runAsyncSubject()` defines an iterator property, `it`, and initializes an interval every **1000** milliseconds:
  * If the value of `it` is **0**:
    * `updateReplaySubject()` is called, and `replaySubjectA$` is subscribed to
  * If the value of `it` is **2**:
    * `updateReplaySubject()` is called, and `replaySubjectB$` is subscribed to
  * If the value of `it` is **5**:
    * The interval is cleared, and the streams will not be updated anymore
  * The **default** execution for the interval is to simply call `updateReplaySubject()`
* At the end of the interval, the `it` property is incremented.

**`subject.component.html`**

```html
<mat-toolbar>ReplaySubject</mat-toolbar>
<section>
  <p>Emits old values to new subscribers, and new values to each current subscriber</p>
</section>
<section fxLayout="row"
         fxLayoutAlign="start start"
         fxLayoutGap="8">
  <section fxFlex>
    <h3>Replay A</h3>
    <ng-container *ngIf="replayA.length > 0">
      <pre><code>{{replayA | json}}</code></pre>
    </ng-container>
    <p *ngIf="!(replayA.length > 0)">No values received</p>
  </section>
  <section fxFlex>
    <h3>Replay B</h3>
    <ng-container *ngIf="replayB.length > 0">
      <pre><code>{{replayB | json}}</code></pre>
    </ng-container>
    <p *ngIf="!(replayB.length > 0)">No values received</p>
  </section>
</section>
```

* If `replayA` or `replayB` contains any values, it will render the **json** format of the array
  * Otherwise, it renders a label communicating that no values have been received

The examples in the above **Subjects** section can be found at: [Stackblitz - Subjects](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Fservices%2Fsubject.service.ts)

## [Operators](#rxjs)

### [Creation Operators](#rxjs)

### [Join Creation Operators](#rxjs)

### [Transformation Operators](#rxjs)

### [Filtering Operators](#rxjs)

### [Join Operators](#rxjs)

### [Multicasting Operators](#rxjs)

### [Error Handling Operators](#rxjs)

### [Utility Operators](#rxjs)

### [Boolean Operators](#rxjs)

### [Mathematical Operators](#rxjs)

## [Piping Operators](#rxjs)

[Back to Top](#rxjs)