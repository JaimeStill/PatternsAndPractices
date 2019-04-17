# Root Component

* [Overview](#overview)
* [Walkthrough](#walkthrough)

## [Overview](#root-component)

The root component in the app stack is responsible for the overall layout of the app, as well as app startup initialization.

Out of the box, the template is setup to render the following, from top to bottom:

* A sticky classification banner
* A sticky toolbar
* A scrollable container that holds the `<router-outlet>` that renders **Route Components**

> Route Components are discussed in the [Routes](./17-routes.md) article.

It also performs the following initialization tasks:

* Retrieve the banner configuration from the global `BannerService`
* Subscribe to the current app theme from the global `ThemeService`, and keep track of the subscription with the private `themeSub` Subscription
* Just before the component is destroyed, the `themeSub` Subscription calls the `unsubscribe()` function, freeing up the Subscription resources.

## [Walkthrough](#root-component)

**`app.component.html`**

```ts
import { Theme } from './models';
import { Subscription } from 'rxjs';

import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  BannerService,
  ThemeService
} from './services';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
})
export class AppComponent implements OnInit, OnDestroy {
  private themeSub: Subscription;

  themeClass = 'default';

  constructor(
    public banner: BannerService,
    public theme: ThemeService
  ) { }

  ngOnInit() {
    this.banner.getConfig();
    this.themeSub = this.theme.theme$.subscribe((t: Theme) => this.themeClass = t.name);
  }

  ngOnDestroy() {
    this.themeSub && this.themeSub.unsubscribe();
  }
}
```

* None of the services injected into this component are registered with the component's `providers` array. They are all pulled from the `AppModule.providers` scope.

* `BannerService.getConfig()` makes an API call to a `BannerController` in <span>ASP.NET</span> Core. This action method returns the `BannerConfig` that has been injected into the controller, which is registered in `Startup.ConfigureServices`.

* The `ThemeService.theme$` stream is detailed in the [Services - ThemeService](./13-services.md#themeservice) article. Anytime a new theme is selected, the `theme$` stream will provide the updated value here.

**`app.component.html`**

```html
<div class="mat-typography mat-app-background app-panel"
     fxLayout="column"
     [ngClass]="themeClass">
  <ng-container *ngIf="banner.config$ | async as config else loading">
    <banner [label]="config.label"
            [background]="config.background"
            [color]="config.color"></banner>
    <mat-toolbar color="primary">
      <span fxFlex>Title</span>
      <button mat-icon-button
              [matMenuTriggerFor]="menu">
        <mat-icon>format_color_fill</mat-icon>
      </button>
      <mat-menu #menu="matMenu">
        <button mat-menu-item
                *ngFor="let t of theme.themes$ | async"
                (click)="theme.setTheme(t)">{{t.display}}</button>
      </mat-menu>
    </mat-toolbar>
    <section class="app-body">
      <router-outlet></router-outlet>
    </section>
  </ng-container>
</div>
<ng-template #loading>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
```
* The `mat-typography` and `mat-app-background` classes on the root `<div>` element ensure that the Typography and Background theme values are applied to the application.

* The `[ngClass]="themeClass"` directive dynamically assigns the value of `themeClass` to the root `<div>` element, applying the theming to the app.

* While the `BannerService.config$` stream doesn't have a value, the `#loading` template is shown, using the `async as {prop} else {template}` pipe syntax.
    * Once `config` has a value, the application will render and the values of `config` can be used.

* The `<mat-toolbar>` component in Angular Material has a Flexbox layout by default, so using the `fxFlex` directive in the `<span fxFlex>Title</span>` element ensures that any elements that follow are pushed to the right.
    * It takes up all of the available space not used by the other elements in the layout container.

* The `<button>` and `<mat-menu>` elements inside of the `<mat-toolbar>` provide the infrastructure necessary for displaying all of the available themes as menu item buttons, and allowing the selected theme to be applied by calling `theme.setTheme(t)` when a button is clicked.
    * `theme` in this case is the instance of `ThemeService` injected into `AppComponent`.

* The remainder of the Angular app is rendered at the `<router-outlet>` element, which renders components based on the current route the app is resolved to.