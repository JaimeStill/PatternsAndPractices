# RxJS

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Subjects](#subjects)
  * [BehaviorSubject](#behaviorsubject)
  * [AsyncSubject](#asyncsubject)
  * [ReplaySubject](#replaysubject)
* [Operators](#operators)
  * [Randomizer](#randomizer)
  * [Slideshow](#slideshow)
  * [Photographers](#photographers)
  * [Show Search](#show-search)

> The examples in this article can be found on [StackBlitz - Docs RxJS](https://stackblitz.com/edit/docs-rxjs)

## [Overview](#rxjs)

This article is intended to familiarize you with core RxJS concepts outside of the Observable features built into Angular. It will cover enough to get you familiar with using RxJS by itself, but it will not cover each operator in depth. To do so would be outside of the scope of this documentation, and there is already a wealth of really well written documentation / examples centered around these concepts:

* [ReactiveX](http://reactivex.io) - Rx is available on many platforms, and RxJS is just the JS implementation of Rx
* [RxJS Docs](https://rxjs.dev/guide/overview)
* [LearnRxJS](https://www.learnrxjs.io/)
* [RxJS Quick Start](https://angularfirebase.com/lessons/rxjs-quickstart-with-20-examples/)
* [RxJS Marbles](https://rxmarbles.com/) - Interactive tool for visualizing effects of operators over time.
* [reactive.how](https://reactive.how/)
* [RxJS Visualizer](https://rxviz.com/)

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
  subject$ = this.subject.asObservable();
  updateSubject = () => this.subject.next(this.getRandomNumber());
}
```  

* `getRandomNumber()` is just a convenience function for triggering a random number value.
* The `subject$` **Observable** stream is created based on the `private subject: Subject<number>` property.
  * Anyone who subscribes to `subject$` will receive the appropriate values generated by `subject`.
* `updateSubject()` allows you to update the current value of `subject`.

**`subject.component.ts`**

```ts
import { Component } from '@angular/core';
import { SubjectService } from '../../services';

@Component({
  selector: 'subjects',
  templateUrl: 'subjects.component.html',
  providers: [SubjectService]
})
export class SubjectsComponent {
  constructor(
    public service: SubjectService;
  ) { }
}
```

**`subjects.component.html`**

```html
<section class="container">
  <p>Multicast an Observable stream to multiple subscribers</p>
</section>
<section class="container">
  <button mat-button
          color="accent"
          (click)="service.updateSubject()">Update Subject</button>
</section>
<section fxLayout="row"
         fxLayoutAlign="space-between center"
         class="container">
  <section class="background card static-elevation"
           fxLayout="column"
           fxLayoutAlign="start stretch"
           fxFlex>
    <p class="mat-title">Observable A</p>
    <p *ngIf="service.subject$ | async as a else aEmpty">Current Value: {{a}}</p>
    <ng-template #aEmpty>
      <p>Empty</p>
    </ng-template>
  </section>
  <section class="background card static-elevation"
           fxLayout="column"
           fxLayoutAlign="start stretch"
           fxFlex>
    <p class="mat-title">Observable B</p>
    <p *ngIf="service.subject$ | async as b else bEmpty">Current Value: {{b}}</p>
    <ng-template #bEmpty>
      <p>Empty</p>
    </ng-template>
  </section>
</section>
```  

* A button is provided that allows you to call `service.updateSubject()`, triggering the `service.subject$` Observable to receive a new value
* `subject$` is subscribed to twice in the template, and each time the stream receives a new value, the value of both subscriptions is appropriately updated

* [StackBlitz - Subject Demo](https://docs-rxjs.stackblitz.io/subjects/subject)
* [StackBlitz - Subject Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Fsubjects%2Fchildren%2Fsubject.component.html)

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
  private getRandomNumber = (): number => Math.floor(Math.random() * Math.floor(200));
  private behaviorSubject = new BehaviorSubject<number>(this.getRandomNumber());
  behaviorSubject$ = this.behaviorSubject.asObservable();
  updateBehaviorSubject = () => this.behaviorSubject.next(this.getRandomNumber());
}
```

* This implementation is the same as the implementation for `Subject`, but now, `behaviorSubject` is initialized with an initial value

**`behavior-subject.component.ts`**

```ts
import { Component } from '@angular/core';
import { SubjectService } from '../../services';

@Component({
  selector: 'subjects',
  templateUrl: 'subjects.component.html',
  providers: [SubjectService]
})
export class SubjectsComponent {
  constructor(
    public service: SubjectService;
  ) { }
}
```

**`behavior-subject.component.html`**

```html
<section class="container">
  <p>A variant of Subject that requires an initial value and emits its current value whenever it is subscribed to.</p>
</section>
<section class="container">
  <button mat-button
          color="accent"
          (click)="service.updateBehaviorSubject()">Update BehaviorSubject</button>
</section>
<section fxLayout="row"
         fxLayoutAlign="space-between center"
         class="container">
  <section class="background card static-elevation"
           *ngIf="service.behaviorSubject$ | async as a"
           fxFlex>
    <p class="mat-title">Observable A</p>
    <p>Value: {{a}}</p>
  </section>
  <section class="background card static-elevation"
           *ngIf="service.behaviorSubject$ | async as b"
           fxFlex>
    <p class="mat-title">Observable B</p>
    <p>Value: {{b}}</p>
  </section>
</section>
```

This example works the same as the `Subject` example, with the following differences, except no templates are needed for the subscriptions because `behaviorSubject` is initialized with a value.

* [StackBlitz - BehaviorSubject Demo](https://docs-rxjs.stackblitz.io/subjects/behavior-subject)
* [StackBlitz - BehaviorSubject Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Fsubjects%2Fchildren%2Fbehavior-subject.component.html)

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

* `private progressSubject: Subject<string>` is used to asynchronously relay whenever the value of `asyncSubject` is updated
* The `updateAsyncSubject()` function generates a message for `progressSubject`, and updates the value of `asyncSubject`
* The `completeAsyncSubject()` function calls `.complete()` on `asyncSubject()`.

**`async-subject.component.ts`**

```ts
import {
  Component,
  OnInit
} from '@angular/core';

import { SubjectService } from '../../../services';

@Component({
  selector: 'async-subject-demo',
  templateUrl: 'async-subject.component.html',
  providers: [SubjectService]
})
export class AsyncSubjectComponent implements OnInit {
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

**`async-subject.component.html`**

```html
<section class="container">
  <p>Only emits a value when it completes.</p>
</section>
<ng-template #loading>
  <section class="container">
    <section class="container background card static-elevation"
             *ngIf="service.progressSubject$ | async as progress">
      <p class="mat-title">Progress Observable</p>
      <p>{{progress}}</p>
    </section>
  </section>
</ng-template>
<ng-container *ngIf="service.asyncSubject$ | async as a else loading">
  <section class="container">
    <section class="container background card static-elevation">
      <p class="mat-title">AsyncSubject</p>
      <p>Completed With: {{a}}</p>
    </section>
  </section>
</ng-container>
```

While the value of `service.asyncSubject$` is empty, a template is displayed that subscribes to the `progressSubject$` stream, indicating when the value of `asyncSubject$` has been updated. Whenever `asyncSubject.complete()` is called, `asyncSubject$` will receive the final value of the stream before it is closed, and the value will be rendered.

* [StackBlitz - AsyncSubject Demo](https://docs-rxjs.stackblitz.io/subjects/async-subject)
* [StackBlitz - AsyncSubject Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Fsubjects%2Fchildren%2Fasync-subject.component.ts)

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
  replaySubject$ = this.replaySubject.asObservable();
  updateReplaySubject = () => this.replaySubject.next(this.getRandomNumber());
}
```

* The `replaySubject$` is an Observable generated from `private replaySubject: ReplaySubject<number>`
* The `updateReplaySubject()` function updates the value of `replaySubject` with a random number generated from `getRandomNumber()`

**`replay-subject.component.ts`**

```ts
import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import { Subscription } from 'rxjs';
import { SubjectService } from '../../../services';

@Component({
  selector: 'replay-subject-demo',
  templateUrl: 'replay-subject.component.html',
  providers: [SubjectService]
})
export class ReplaySubjectComponent implements OnInit, OnDestroy {
  private subs = new Array<Subscription>();
  maps = new Map<number, number[]>();

  constructor(
    public service: SubjectService
  ) { }

  ngOnInit() {
    this.runReplay();
  }

  ngOnDestroy() {
    this.service.unsubscribeMany(this.subs);
  }

  runReplay = () => {
    let it: number = 0;
    const id = setInterval(() => {
      switch (it) {
        case 0:
        case 4:
        case 8:
          const key = it === 0 ? 1 : it === 4 ? 2 : 3;
          const stream = new Array<number>();
          this.maps.set(key, stream);
          this.subs.push(this.service.replaySubject$.subscribe(x => stream.push(x)));
          this.service.updateReplaySubject();
          break;
        case 12:
          clearInterval(id);
          break;
        default:
          this.service.updateReplaySubject();
      }
      it++;
    }, 1000);
  }
}
```

* `private subs: Array<Subscription>` is used to keep track of internal subscriptions, so they can be properly unsubscribed in the **OnDestroy** lifecycle hook, using the `unsubscribeMany(subs: Subscription[])` function defined in `SubjectService`.
* `maps: Map<number, number[]>` keeps track of all of the values emitted from the streams that subscribe to `replaySubject$`
* `runReplay()` defines an iterator property, `it`, and initializes an interval every **1000** milliseconds:
  * If the value of `it` is **0**, **4**, or **8**:
    * A `key: number` is generated based on the current value of `it`
    * A `stream: Array<number>` is generated to keep track of all of the values received from a stream
    * `key` and `stream` are added to `maps`
    * A `replaySubject$` subscription is created, and each value received is pushed into the `stream` array
    * `updateReplaySubject()` is called, causing the stream to be updated
  * If the value of `it` is **12**:
    * The interval is cleared, and the streams will not be updated anymore
  * The **default** execution for the interval is to simply call `updateReplaySubject()`
* At the end of the interval, the `it` property is incremented.

**`subject.component.html`**

```html
<section class="container">
  <p>Emits old values to new subscribers, and new values to each current subscriber</p>
</section>
<ng-container *ngIf="maps.size > 0">
  <mat-toolbar>Observables</mat-toolbar>
  <section class="container"
           fxLayout="column"
           fxLayoutAlign="start start">
    <section *ngFor="let m of maps | keyvalue"
             class="background container card static-elevation">
      <p class="mat-title">Observable {{m.key}}</p>
      <section class="container background accent">
        <code>{{m.value | json}}</code>
      </section>
    </section>
  </section>
</ng-container>
<p *ngIf="!(maps.size > 0)">Loading...</p>
```

If `maps` contains any values, then it is is iterated using **NgFor**, and each of the `value` properties represented by its data is rendered to show the current state of the stream. As new subscriptions are created, new items are added to the map, and a new card is rendered with all of the data it has received through the subscription.

* [StackBlitz - ReplaySubject Demo](https://docs-rxjs.stackblitz.io/subjects/replay-subject)
* [StackBlitz - ReplaySubject Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Fsubjects%2Fchildren%2Freplay-subject.component.ts)

## [Operators](#rxjs)

Documentation: [Operators](https://rxjs.dev/guide/operators)

> RxJS is mostly useful for its *operators*, even though the Observable is the foundation. Operators are the essential pieces that allow complex asynchronous code to be easily composed in a declarative manner.

Operators are **functions**. There are two kinds of operators:

* **Creation Operators** - can be called as standalone functions to create a new Observable with some common predefined behavior, or by joining other Observables.

* **Pipeable Operators** - can be piped to Observables using the syntax `observableInstance.pipe(operator())`. When called, they do not *change* the existing Observable instance. Instead, they return a *new* Observable, whose subscription logic is based on the first Observable. A Pipeable Operator is essentially a pure function which takes one Observable as input and generates another Observable as output. Subscribing to the output Observable will also subscribe to the input Observable.

**Creation Operator Example**

```ts
import { fromEvent } from 'rxjs';

generateInputObservable = (button: HTMLElement) => fromEvent(button, 'click');
```

**Pipeable Operator Example**

```ts
import { filter } from 'rxjs/operators';

/*
  where $stream is an Observable<number>
*/
$stream.pipe(
  filter(x => x % 2 === 0)
);
```

The sections that follow will use examples to demonstrate the power of RxJS operators.

### [Randomizer](#rxjs)

**Description**  

In this example, we want to be able to retrieve a random image from the [Picsum Photos](https://picsum.photos/) api whenever a button is clicked and render the data.

**Operators Used**

Operator | Type | Docs
---------|------|-----
`ajax` | Creation | [link](https://rxjs.dev/api/ajax/ajax)
`fromEvent` | Creation | [link](https://rxjs.dev/api/index/function/fromEvent)
`map` | Pipeable | [link](https://rxjs.dev/api/operators/map)
`switchMap` | Pipeable | [link](https://rxjs.dev/api/operators/switchMap)
`retry` | Pipeable | [link](https://rxjs.dev/api/operators/retry)
`catchError` | Pipeable | [link](https://rxjs.dev/api/operators/catchError)

**`picsum.ts`**

```ts
export interface Picsum {
  id: string;
  author: string;
  width: number;
  height: number;
  url: string;
  download_url: string;
}
```

An interface of the shape of the data returned from the API is created to make working with the received data easier.

**`operator.service.ts`**

```ts
import { Injectable } from '@angular/core';

import {
  fromEvent,
  Observable
} from 'rxjs';

import {
  catchError,
  map,
  retry,
  switchMap
} from 'rxjs/operators';

import { ajax } from 'rxjs/ajax';
import { SnackerService } from './snacker.service';
import { Picsum } from '../models';

@Injectable()
export class OperatorService {
  constructor(
    private snacker: SnackerService
  ) { }

  private formatPicsum = (x: Picsum, width: string, height: string): Picsum => {
    if (x && x.download_url) {
      x.download_url = `https://picsum.photos/id/${x.id}/${width}/${height}`;
    }

    return x;
  }

  private getRandomPage = () => Math.floor(Math.random() * Math.floor(300));
  private getImageUrl = () => `https://picsum.photos/v2/list?page=${this.getRandomPage()}&limit=1`;

  private getImage = (width: string, height: string) => ajax(this.getImageUrl()).pipe(
    map(x => this.formatPicsum(x.response[0], width, height)),
    retry(3),
    catchError((err) => {
      this.snacker.sendErrorMessage(err);
      throw `Error occurred: ${err}`;
    })
  );

  image$: Observable<Picsum>;

  setRandomImageStream = (element: HTMLElement) =>
    this.image$ = fromEvent(element, 'click').pipe(
      switchMap(() => this.getImage('640', '360'))
    );
}
```

The `formatPicsum()` function allows the `download_url` property of the received data to be modified so that an image of a specified height and width will be returned, as opposed to the generally large image that is provided with the data by default.

`getRandomPage()` is a function that generates a random number up to **300**, allowing up to 300 different images to be retrieved.

`getImageUrl()` is a function that generates a URL to the public picsum photos API endpoint with a limit of **1** data object, and at a random page determined by the `getRandomPage()` function.

The `getImage()` function receives `height` and `width` arguments and uses the `ajax` creation operator to generate an Observable that calls the URL generated by the `getImageUrl()` function. The `map` function is then piped in, and an `Observable<Picsum>` is generated by passing the first item in the `response` value from the `ajax` call to the `formatPicsum()` function, along with the `width` and `height` arguments. In the event of an error, the Observable call will be retried **3** times using the piped in `retry` operator. If it still fails, `catchError` will cause the error to be communicated to the user via the `SnackerService`, and an error is thrown with the details of the received error.

The `image$: Observable<Picsum>` property is used to communicate the current `Picsum` value held by the stream.

`setRandomImageStream()` receives an `HTMLElement` argument. An Observable is created from the **click** event of the provided `HTMLElement`, and `switchMap` is piped in to convert the stream to the Observable created by the `getImage()` function. `image$` is set to the value of this piped Observable.

**`randomizer.component.html`**

```html
<section class="container">
  <button #randomizer
          mat-button
          color="accent">Randomize Image</button>          
</section>
<ng-template #message>
  <h3>Click the button to load an image</h3>
</ng-template>
<section class="container"
         *ngIf="service.image$ | async as image else message">
  <a [href]="image?.download_url"
     target="_blank">
    <img [src]="image?.download_url"
         [alt]="image?.download_url"
         [style.margin.px]="8">
  </a>
  <p class="mat-title">{{image.author}}</p>
  <p>Native Size: {{image.width}} * {{image.height}}</p>
  <a [href]="image.url"
     [style.margin.px]="8"
     target="_blank"
     class="link">Source Image</a>
</section>
```

A **template reference variable** of `#randomizer` is assigned to the button that will be passed to the `setRandomImageStream()` function from the `OperatorService`.

While `OperatorService.image$` does not have a value, a message is displayed (via the `<ng-template #message>` Angular template) indicating to click the button to load an image.

When `OperatorService.image$` has a value, the following is rendered via the `Picsum` object it represents:
* A clickable image, represented by the `download_url` property
* The author
* The native size of the image, determined by the `width` and `height` properties
* A link to the source image, represented by the `url` property

**`randomizer.component.ts`**

```ts
import {
  Component,
  ViewChild
} from '@angular/core';

import { MatButton } from '@angular/material';
import { OperatorService } from '../../../services';

@Component({
  selector: 'randomizer',
  templateUrl: 'randomizer.component.html',
  providers: [OperatorService]
})
export class RandomizerComponent {
  constructor(
    public service: OperatorService
  ) { }

  @ViewChild('randomizer')
  set randomizer(button: MatButton) {
    button && button._elementRef && this.service.setRandomImageStream(button._elementRef.nativeElement);
  }
}
```

`OperatorService` is registered with the `providers` array of the `RandomizerComponent`.

The button represented by the `#randomizer` template reference variable is assigned a set function that passes the `nativeElement` of the `MatButton` received to the `OperatorService.setRandomImageStream()` function. It is here where all of the infrastructure provided by the service is wired together, and enables the intent of the app to be accomplished with RxJS.

* [StackBlitz - Randomizer Demo](https://docs-rxjs.stackblitz.io/operators/randomizer)
* [StackBlitz - Randomizer Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Foperators%2Fchildren%2Frandomizer.component.ts)

### [Slideshow](#rxjs)

**Description**

The ability to execute functionality over time is provided in JavaScript via the `setTimeout()` and `setInterval()` functions. RxJS provides a means of doing this asynchronously with the `timer` and `interval` operators. These operators are nearly the same with the following difference:

* `timer` takes two parameters: a `dueTime` of type `number | Date` and a `periodOrScheduler` of type `number | SchedulerLike`. If both arguments are provided, `timer` will emit incrementing numbers, starting at zero, beginning after the value of `dueTime` at an interval specified by `periodOrScheduler`. If only the `dueTime` argument is specified, `timer` will emit a value once after the value specified by `dueTime`.

* `interval` takes defines a parameter, a `period`, and emits sequential numbers at a rate specified by the `period`.

In this example, the intent is to retrieve random images from the picsum photos API based at a specified interval. It uses both the `timer` and `interval` operators to demonstrate their usage and generate two distinct slideshows.

**Operators Used**

Operator | Type | Docs
---------|------|-----
`ajax` | Creation | [link](https://rxjs.dev/api/ajax/ajax)
`timer` | Creation | [link](https://rxjs.dev/api/index/function/timer)
`interval` | Creation | [link](https://rxjs.dev/api/index/function/interval)
`map` | Pipeable | [link](https://rxjs.dev/api/operators/map)
`switchMap` | Pipeable | [link](https://rxjs.dev/api/operators/switchMap)
`retry` | Pipeable | [link](https://rxjs.dev/api/operators/retry)
`catchError` | Pipeable | [link](https://rxjs.dev/api/operators/catchError)

**`picsum.ts`**

```ts
export interface Picsum {
  id: string;
  author: string;
  width: number;
  height: number;
  url: string;
  download_url: string;
}
```

The same interface used in the above [Randomizer](#randomizer) example.

**`operator.service.ts`**

```ts
import { Injectable } from '@angular/core';

import {
  interval,
  timer
} from 'rxjs';

import {
  catchError,
  map,
  retry,
  switchMap
} from 'rxjs/operators';

import { ajax } from 'rxjs/ajax';
import { SnackerService } from './snacker.service';
import { Picsum } from '../models';

@Injectable()
export class OperatorService {
  constructor(
    private snacker: SnackerService
  ) { }

  private formatPicsum = (x: Picsum, width: string, height: string): Picsum => {
    if (x && x.download_url) {
      x.download_url = `https://picsum.photos/id/${x.id}/${width}/${height}`;
    }

    return x;
  }

  private getRandomPage = () => Math.floor(Math.random() * Math.floor(300));
  private getImageUrl = () => `https://picsum.photos/v2/list?page=${this.getRandomPage()}&limit=1`;

  private getImage = (width: string, height: string) => ajax(this.getImageUrl()).pipe(
    map(x => this.formatPicsum(x.response[0], width, height)),
    retry(3),
    catchError((err) => {
      this.snacker.sendErrorMessage(err);
      throw `Error occurred: ${err}`;
    })
  );

  timer$ = timer(1000, 3000).pipe(
    switchMap(() => this.getImage('480', '270'))
  );

  interval$ = interval(2000).pipe(
    switchMap(() => this.getImage('480', '270'))
  );
}
```

The `formatPicsum()`, `getRandomPage()`, `getImageUrl()`, and `getImage()` functions in the service are unchanged from the [Randomizer](#randomizer) example. This example merely modifies the Observable that is generated to be based on `timer` and `interval` as opposed to `fromEvent`.

The `timer$` observable is created by initializing a `timer` operator with an initial delay of **1000** milliseconds, and an interval of **3000** milliseconds, then piping in `switchMap` to return the Observable generated by the `getImage()` function.

The `interval$` observable is created by initializing an `interval` operator with an interval of **2000** milliseconds, then piping in `switchMap` to return the Observable generated by the `getImage()` function.

**`slideshow.component.ts`**

```ts
import { Component } from '@angular/core';
import { OperatorService } from '../../../services';

@Component({
  selector: 'slideshow',
  templateUrl: 'slideshow.component.html',
  providers: [OperatorService]
})
export class SlideshowComponent {
  constructor(
    public service: OperatorService
  ) { }
}
```

`SlideshowComponent` registers the `OperatorService` with its `providers` array.

**`slideshow.component.html`**

```html
<mat-toolbar>Timer</mat-toolbar>
<ng-template #timerLoading>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
<section *ngIf="service.timer$ | async as timerImage else timerLoading"
              class="container">
  <a [href]="timerImage.url"
     target="_blank">
    <img [src]="timerImage.download_url">
  </a>
</section>
<mat-toolbar>Interval</mat-toolbar>
<ng-template #intervalLoading>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
<section *ngIf="service.interval$ | async as intervalImage else intervalLoading"
         class="container">
  <a [href]="intervalImage.url"
     target="_blank">
    <img [src]="intervalImage.download_url">
  </a>
</section>
```

While the `timer$` and `interval$` streams are empty, a progress bar is shown. Whenever they have values, the image is rendered via the `download_url` property, and when clicked, opens the source image through the `url` property.

* [StackBlitz - Slideshow Demo](https://docs-rxjs.stackblitz.io/operators/slideshow)
* [StackBlitz - Slideshow Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Foperators%2Fchildren%2Fslideshow.component.html)

### [Photographers](#rxjs)

**Description**

Using RxJS, a list of artists can be retrieved from the picsum photos API. These artists can then be populated into a select element, and when selected, the photos relevant to that artist can be rendered.

**Operators Used**

Operator | Type | Docs
---------|------|-----
`ajax` | Creation | [link](https://rxjs.dev/api/ajax/ajax)
`map` | Pipeable | [link](https://rxjs.dev/api/operators/map)
`switchMap` | Pipeable | [link](https://rxjs.dev/api/operators/switchMap)
`retry` | Pipeable | [link](https://rxjs.dev/api/operators/retry)
`catchError` | Pipeable | [link](https://rxjs.dev/api/operators/catchError)

**`picsum.ts`**

```ts
export interface Picsum {
  id: string;
  author: string;
  width: number;
  height: number;
  url: string;
  download_url: string;
}
```

The same `Picsum` interface defined in the previous examples.

**`operator.service.ts`**

```ts
import { Injectable } from '@angular/core';

import {
  Observable,
  Subscription
} from 'rxjs';

import {
  catchError,
  map,
  retry
} from 'rxjs/operators';

import { ajax } from 'rxjs/ajax';
import { SnackerService } from './snacker.service';
import { Picsum } from '../models';

@Injectable()
export class OperatorService {
  constructor(
    private snacker: SnackerService
  ) { }

  private formatPicsum = (x: Picsum, width: string, height: string): Picsum => {
    if (x && x.download_url) {
      x.download_url = `https://picsum.photos/id/${x.id}/${width}/${height}`;
    }

    return x;
  }

  private onlyUnique = (value: string, index: number, self: Array<string>) => self.indexOf(value) === index;

  private sortStrings = (a: string, b: string) => a < b ? -1 : a > b ? 1 : 0;

  private photoUrl = `https://picsum.photos/v2/list?limit=500`;

  private getPhotographerPhotos = (p: Picsum) => p.author.toLowerCase().includes(this.photographer.toLowerCase());

  unsubscribe = (sub: Subscription) => sub && !sub.closed && sub.unsubscribe();

  photographer: string;
  photos$: Observable<Picsum[]>;

  photographers$ = ajax(this.photoUrl).pipe(
    map(x =>
      (x.response as [])
        .map((x: Picsum) => x.author.toLocaleUpperCase())
        .filter(this.onlyUnique)
        .sort(this.sortStrings)
    ),
    retry(3),
    catchError((err) => {
      this.snacker.sendErrorMessage(err);
      throw `Error occurred: ${err}`;
    })
  );

  getPhotos = () => 
    this.photos$ = ajax(this.photoUrl).pipe(
      map(x =>
        (x.response as Picsum[])
          .filter(this.getPhotographerPhotos)
          .map(x => this.formatPicsum(x, '640', '360'))
      ),
      retry(3),
      catchError((err) => {
        this.snacker.sendErrorMessage(err);
        throw `Error occurred: ${err}`;
      })
    );
}
```

The `formatPicsum()` function is the same as in previous examples.

The `onlyUnique()` function is a filtering function used to ensure that the values in an `Array<string>` do not contain any duplicates.

The `sortStrings()` function is a sorting function that ensures that all of the strings in an `Array<string>` are in alphabetical order.

`photoUrl: string` is a URL string that points to the picsum photos API with a limit of **500** data objects.

The `getPhotographerPhotos()` function is a filtering function used to ensure that the author of the provided `Picsum` object matches the currently selected `photographer` property.

The `unsubscribe` function is a convenience function defined to ensure that a Subscription gets is properly unsubscribed from if it is not null and is not closed.

`photographer: string` is used to keep track of the currently selected photographer.

`photos$: Observable<Picsum[]>` is an Observable that represents the `Picsum` data associated with a selected photographer.

`photographers$: Observable<string[]>` is an Observable that yields a distinct list of the photographers retrieved from the `photoUrl` endpoint. The `map` operator is piped in, and the `response` array retrieved from the `ajax` result, and the contained objects are constrained to the `toLocaleUpperCase()` variant of the `author` property. The array is then filtered using the `onlyUnique` function, then sorted using the `sortStrings` function. The `retry` and `catchError` operators are piped in, as in the previous examples, to provide a small degree of error resilience.

The `getPhotos()` function assigns `photos$` the value of using the `map` operator, piped into the `ajax` operator pointed at the `photoUrl`, to filter the received array using the `getPhotographerPhotos()` function. The array is then mapped using the `formatPicsum()` function. Again, the `retry` and `catchError` operators are piped in to provide a small degree of error resilience.

**`photographers.component.ts`**

```ts
import { Component } from '@angular/core';
import { OperatorService } from '../../../services';

@Component({
  selector: 'photographers',
  templateUrl: 'photographers.component.html',
  providers: [OperatorService]
})
export class PhotographersComponent {
  constructor(
    public service: OperatorService
  ) { }
}
```

`PhotographersComponent` simply registers the `OperatorService` with its `providers` array and injects it into its constructor, making it available to the component template.

**`photographers.component.html`**

```html
<ng-template #loading>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
<section *ngIf="service.photographers$ | async as photographers else loading"
         fxLayout="column"
         fxLayoutAlign="start stretch">
  <mat-form-field>
    <mat-select [(value)]="service.photographer"
                (selectionChange)="service.getPhotos()">
      <mat-option *ngFor="let p of photographers"
                  [value]="p">{{p | titlecase}}</mat-option>
    </mat-select>
  </mat-form-field>
  <ng-template #message>
    <mat-label>Photographers</mat-label>
    <section class="container">
      <p>Select a photographer to view their photos.</p>
    </section>
  </ng-template>
  <ng-container *ngIf="service.photos$ | async as photos else message">
    <section *ngIf="photos.length > 0"
             fxLayout="row | wrap"
             fxLayoutAlign="start start"
             class="container">
      <section *ngFor="let p of photos"
               fxLayout="column"
               fxLayoutAlign="start stretch"
               class="background card elevated">
        <a [href]="p.download_url"
           target="_blank">
          <img [src]="p.download_url"
               [alt]="p.download_url">
        </a>
        <a [href]="p.url"
           [style.margin.px]="8"
           target="_blank"
           class="link">Source Image</a>
      </section>
    </section>
    <h3 *ngIf="!(photos.length > 0)">Photographer {{service.photographer}} has no photos</h3>
  </ng-container>
</section>
```

While the `photographers$` Observable is empty, a progress bar is displayed.

Once the Observable populates, the resulting data is provided as options to an Angular Material `MatSelect`, and the value of `OperatorService.photographer` is bound to the value of the select. When the value changes, `OperatorService.getPhotos()` is called, updating the value of the `OperatorService.photos$` Observable.

While the `photos$` Observable is empty, a message is displayed indicating that you can select a photographer to view their photos.

Once the Observable populates, the resulting data is rendered as a list that shows:

* A clickable image, populated by the `download_url` property, that opens the image in a new tab.
* A link to the source image, determined by the `url` property.

In the event that no photos are available in the resulting data, a message is shown inicating that the selected photographer has no photos. This situation won't occur because the list of photographers is derived from the listing of photos, but it's still good to keep the UI resilient.

* [StackBlitz - Photographers Demo]()
* [StackBlitz - Photographers Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Foperators%2Fchildren%2Fphotographers.component.html)

### [Show Search](#rxjs)

**Description**

This example will depart from the picsum photos API and build around the [TV Maze](http://www.tvmaze.com/api) public API. The intent is to be able to type into an input that triggers an Observable to search the API using the text provided. The results will then be provided to the app via an Observable and rendered as a list, with each result rendered as a card.

**Operators Used**  

Operator | Type | Docs
---------|------|-----
`ajax` | Creation | [link](https://rxjs.dev/api/ajax/ajax)
`fromEvent` | Creation | [link](https://rxjs.dev/api/index/function/fromEvent)
`debounceTime` | Pipeable | [link](https://rxjs.dev/api/operators/debounceTime)
`distinctUntilChanged` | Pipeable | [link](https://rxjs.dev/api/operators/distinctUntilChanged)
`map` | Pipeable | [link](https://rxjs.dev/api/operators/map)
`retry` | Pipeable | [link](https://rxjs.dev/api/operators/retry)
`catchError` | Pipeable | [link](https://rxjs.dev/api/operators/catchError)

**`show-channel.ts`**

```ts
export class ShowChannel {
  name: string;
}
```

An interface for the `webChannel` property of the data returned by the API

**`show-image.ts`**

```ts
export class ShowImage {
  medium: string;
  original: string;
}
```

An interface for the `image` property of the data returned by the API

**`show.ts`**

```ts
import { ShowChannel } from './show-channel';
import { ShowImage } from './show-image';

export class Show {
  id: number;
  url: string;
  name: string;
  runtime: number;
  premiered: string;
  officialSite: string;
  summary: string;

  image: ShowImage;
  webChannel: ShowChannel;

  genres: string[];  
}
```

An interface that describes the shape of the data (that we'll be using) returned by the API

**`operator.service.ts`**

```ts
import {
  Injectable,
  ElementRef
} from '@angular/core';

import {
  fromEvent,
  Observable,
  Subscription
} from 'rxjs';

import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  retry
} from 'rxjs/operators';

import { ajax } from 'rxjs/ajax';
import { SnackerService } from './snacker.service';
import { Show } from '../models';

@Injectable()
export class OperatorService {
  constructor(
    private snacker: SnackerService
  ) { }

  private formatShow = (show: Show): Show => {
    if (show && show.image) {
      show.image.medium = show.image.medium && show.image.medium.replace('http', 'https');
      show.image.original = show.image.original && show.image.original.replace('http', 'https');
    }

    return show;
  }

  unsubscribe = (sub: Subscription) => sub && !sub.closed && sub.unsubscribe();

  private tvApi = (search: string) => `https://api.tvmaze.com/search/shows?q=${search}`;

  generateInputObservable = (input: ElementRef): Observable<string> =>
    fromEvent(input.nativeElement, 'keyup').pipe(
      debounceTime(500),
      map((event: any) => event.target.value),
      distinctUntilChanged()
    );

  shows$: Observable<Show[]>;

  createShowStream = (search: string) =>
    this.shows$ = ajax(this.tvApi(search)).pipe(
      map(x =>
        (x.response as any[])
          .map(x => this.formatShow((x.show as Show)))
      ),
      retry(3),
      catchError((err) => {
        this.snacker.sendErrorMessage(err);
        throw `Error occurred: ${err}`;
      })
    );

  clearShowStream = () => this.shows$ = null;
}
```

The `formatShow()` function allows the `Show.image.medium` and `Show.image.original` properties received by calls to the API to be converted from **http** to **https** links. This is necessary, because StackBlitz doesn't allow non-https sources to be retrieved.

`unsubscribe()` is a convenience function that allows you to pass in a Subscription and ensure that it is property disposed.

The `tvApi()` function defines a `search: string` parameter that allows you to generate the appropriate endpoint URL based on the current search value.

`generateInputObservable()` is a function that defines an Angular `ElementRef` parameter. It generates an Observable using `fromEvent` with the `keyup` event of the provided element. `debounceTime` is used to ensure that **500** milliseconds pass after the last keystroke before executing. `map` extracts the text value from the emitted event, then `distinctUntilChanged` ensures that the value is different from the last value that was received by the stream.

`shows$: Observable<Show[]>` is used to represent the Observable that retrieves the listing of shows from the API.

The `createShowStream()` function accepts a `search: string` argument and uses the `ajax` operator to retrieve data using a URL generated with the `search` argument passed to the `tvApi()` function. `map` extracts the response array and executes the `formatShow()` function on all of the data. The `retry` and `catchError` operators are included to provide a small degree of error resilience.

The `clearShowStream()` function clears the value of the `shows$` Observable.

**`show-card.component.ts`**

```ts
import {
  Component,
  Input
} from '@angular/core';

import { Show } from '../../models';

@Component({
  selector: 'show-card',
  templateUrl: 'show-card.component.html'
})
export class ShowCardComponent {
  @Input() show: Show;
  @Input() width = 420;
  summary = false;

  toggleSummary = () => this.summary = !this.summary;
}
```

The `ShowCardComponent` is used to render a `Show` object as a card.

The `show` input property is used to assign the `Show` that the card represents.

The `width` input property defaults to **420** and can be used to adjust the horizontal sizing of the card.

The `summary` property is used to determine whether or not the `Show.summary` region of the card is being shown.

Thet `toggleSummary()` function is used to toggle the value of the `summary` property.

**`show-card.component.html`**

```html
<section class="background card elevated"
         fxLayout="column"
         fxLayoutAlign="start stretch"
         [style.width.px]="width">
  <h3 class="mat-title">{{show?.name}}</h3>
  <section class="background container stacked"
           fxLayout="row"
           fxLayoutAlign="space-between center">
    <a mat-button
       color="accent"
       target="_blank"
       [href]="show?.officialSite">Official Site</a>
    <button mat-icon-button
            (click)="toggleSummary()">
      <mat-icon *ngIf="summary"
                matTooltip="Hide">keyboard_arrow_down</mat-icon>
      <mat-icon *ngIf="!(summary)"
                matTooltip="Show">keyboard_arrow_right</mat-icon>
    </button>
  </section>
  <section *ngIf="summary"
           class="container"
           [innerHTML]="show?.summary"></section>
  <section fxLayout="column"
           fxLayoutAlign="center center"
           class="container"
           [style.margin.px]="8">
    <a *ngIf="show?.image?.medium"
       target="_blank"
       [href]="show?.image?.original">
      <img [src]="show?.image?.medium"
           [alt]="show?.image?.original"
           [style.width.px]="280">
    </a>
    <section *ngIf="!(show?.image?.medium)"
             fxLayout="column"
             fxLayoutAlign="center center"
             class="background stacked"
             [style.width.px]="280"
             [style.height.px]="398.13">
      No Image Available
    </section>
  </section>
  <section fxLayout="column"
           fxLayoutAlign="start center"
           [style.margin.px]="8">
    <mat-chip-list *ngIf="show?.genres?.length > 0">
      <mat-chip *ngFor="let g of show?.genres"
                color="accent"
                selected>{{g}}</mat-chip>
    </mat-chip-list>
    <mat-chip-list *ngIf="!(show?.genres?.length > 0)">
      <mat-chip>No Genre</mat-chip>
    </mat-chip-list>
  </section>
  <p class="mat-body-2">Channel: {{show?.webChannel?.name}}</p>
  <p>Premiered: {{show?.premiered | date:'mediumDate'}}</p>
</section>
```

Renders all of the properties in a card format.

**`show-search.component.html`**

```html
<section fxLayout="column"
         fxLayoutAlign="start stretch">
  <mat-form-field>
    <mat-label>Search TV Shows</mat-label>
    <input matInput
           #showSearch>
  </mat-form-field>
  <ng-template #message>
    <section class="container">
      <p>Use the search bar to find TV shows</p>
    </section>
  </ng-template>
  <ng-container *ngIf="service.shows$ | async as shows else message">
    <section *ngIf="shows?.length > 0"
             class="container"
             fxLayout="row | wrap"
             fxLayoutAlign="start start">
      <show-card *ngFor="let s of shows"
                 [show]="s"></show-card>
    </section>
    <section *ngIf="!(shows?.length > 0)"
             class="container">
      <p>No shows match the provided search term(s)</p>
    </section>
  </ng-container>
</section>
```

A **template reference variable** of `#showSearch` is applied to an input element that will generate an Observable using the `OperatorService.generateInputObservable()` function.

Whenever the `OperatorService.shows$` Observable is empty, a message is displayed indicating to use the search bar to find TV shows.

When the Observable has data, it is rendered as a list, and each `Show` is rendered inside of a `ShowCardComponent`.

If the data received by the Observable is empty, a message is shown that indicates that no shows match the provided search terms.

**`show-search.component.ts`**

```ts
import {
  Component,
  ViewChild,
  ElementRef,
  OnDestroy
} from '@angular/core';

import { Subscription } from 'rxjs';
import { OperatorService } from '../../../services';

@Component({
  selector: 'show-search',
  templateUrl: 'show-search.component.html',
  providers: [OperatorService]
})
export class ShowSearchComponent implements OnDestroy {
  private sub: Subscription;

  constructor(
    public service: OperatorService
  ) { }

  @ViewChild('showSearch')
  set showSearch(input: ElementRef) {
    if (input) {
      this.sub = this.service.generateInputObservable(input)
        .subscribe((value: string) =>
          value && value.length > 1 ?
            this.service.createShowStream(value) :
            this.service.clearShowStream()
        );
    }
  }

  ngOnDestroy() {
    this.service.unsubscribe(this.sub);
  }
}
```

The `OperatorService` is registered with the `providers` array of `ShowSearchComponent`.

The `private sub: Subscription` property is used to keep track of the Observable generated by `OperatorService.generateInputObservable()` so that it can be property disposed whenever the **OnDestroy** lifecycle hook is called.

The `#showSearch` template reference variable is used with the `ViewChild` decorator to define a set function for the element. An Observable is generated by passing the `input: ElementRef` to `OperatorServcie.generateInputObservable()`, and it is subscribed to. If the `value: string` received has a length greater than **1**, then `value` is passed to `OperatorService.createShowStream()`. Otherwise, the `shows$` Observable is cleared with the `OperatorService.clearShowStream()` function.

* [StackBlitz - Show Search Demo](https://docs-rxjs.stackblitz.io/operators/show-search)
* [StackBlitz - Show Search Source](https://stackblitz.com/edit/docs-rxjs?file=src%2Fapp%2Froutes%2Foperators%2Fchildren%2Fshow-search.component.ts)

[Back to Top](#rxjs)
