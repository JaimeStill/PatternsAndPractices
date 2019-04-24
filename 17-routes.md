# Routes

[Table of Contents](./toc.md)

* [Angular Routing](#angular-routing)
* [Route Components](#route-components)
    * [ItemsComponents](#itemscomponent)
    * [ItemComponent](#itemcomponent)

## [Angular Routing](#routes)

In Angular, a **route** is resolved to a **Component** which will be rendered at a `<router-outlet>` within the app. To enable routing, the following are needed:

* A **Route Component** must be defined in the **routes** TypeScript module
    * Located at **{Project}.Web\\ClientApp\\src\\app\\routes**
* Routes must be defined in the `index.ts` of the **routes** module
* The `RouterModule` must be imported into the `AppModule`, and the **Routes** exported from the **routes** module must be provided to the `RouterModule.forRoot()` function.

The built-in `HomeComponent` route component is an empty shell that serves as a starting point for the app template. Here is its definition:

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
})
export class HomeComponent { }
```

**`home.component.html`**

```html
<mat-toolbar>Home</mat-toolbar>
```

It simply renders a `MatToolbar` component with a title of **Home**.

`index.ts` in the **routes** module differs from the **components** module in that it not only needs to define the `RouteComponents` to be declard in `AppModule`, but also the `Routes` to be registered with the Angular Router.

**`index.ts`**

```ts
import { Route } from '@angular/router';
import { HomeComponent } from './home/home.component';

export const RouteComponents = [
  HomeComponent
];

export const Routes: Route[] = [
  { path: 'home', component: HomeComponent },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: '**', redirectTo: 'home', pathMatch: 'full' }
];
```

Each object in the `Routes` array represents a potential route. Here are some of the important properties specified by the `Route` interface:

**`path?: string`**

The path to match against, a URL string that uses router matching notation. Can be a wild card (`**`) that matches any URL. Default is "/" (the root path).

**`pathMatch?: string`**

The path-matching strategy, one of `prefix` or `full`. Default is `prefix`.

By default, the router checks URL elements from the left to see if the URL matches a given path, and stops when there is a match. For example, `/team/11/user` matches `team/:id`.

The path-match strategy `full` matches against the entire URL. It is important to do this when redirecting empty-path routes. Otherwise, because an empty path is a prefix of any URL, the router would apply the redirect even when navigating to the redirect destination, creating an endless loop.

**`component?: Type<any>`**

The component to instantiate when the path matches. Can be empty if child routes specify components.

> Child routes will be covered in the [Child Routes](./a3-child-routes.md) article.

**`redirectTo?: string`**

A URL to which to redirect when a path matches. Absolute if the URL begins with a slash (`/`), otherwise relative to the path URL. When not present, router does not redirect.

**`outlet?: string`**

Name of a `RouterOutlet` object where the component can be placed when the path matches.

> See [Route](https://angular.io/api/router/Route) for detailed information.

Let's look at each of the defined routes specified above:

```ts
{ path: 'home', component: HomeComponent }
```

Route: `{appUrl}/home`

Renders the `HomeComponent` in the default `<router-outlet>` defined in the `AppComponent` template.

```ts
{ path: '', redirectTo: 'home', pathMatch: 'full' }
```

Route: `{appUrl}/`

Redirects to `{appUrl}/home`

```ts
{ path: '**', redirectTo: 'home', pathMatch: 'full' }
```

Route: `{appUrl}/{anything}`

Matches any route that is not previously defined in the `Routes` array. Redirects to `{appUrl}/home`.

Here is the relevant `AppModule` infrastructure necessary for enabling routing:

**`app.module.ts`**

```ts
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { Routes, RouteComponents } from './routes';

@NgModule({
  declarations: [
    AppComponent,
    [...RouteComponents]
  ],
  imports: [
    /* additional imports */
    RouterModule.forRoot(Routes)
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

The provided template does not define an alternative `RouterOutlet`, so all of the routes (that are not child routes) will render at the `<router-outlet>` defined in the `AppComponent` template:

```html
<!-- preceding template markup -->
<section class="app-body">
  <router-outlet></router-outlet>
</section>
<!-- proceding template markup -->
```

> See [RouterOutlet](https://angular.io/api/router/RouterOutlet) and [Routing and Navigation](https://angular.io/guide/router) for detailed information about routing.

## [Route Components](#routes)

As mentioned in the [Components](./14-components.md) article, **Route Components**:

* Can be resolved to a route via the Angular router
* Are able to interact with services and manage the overall state of the view they represent through that service interaction
* Orchestrate the layout of *Display Components*, providing them with data retrieved through services
* Handle events triggered through user interaction with a *Display Component*
* Are defined in the **{Project}.Web\\ClientApp\\src\\app\\routes** module

> This section will bridge the gap between the `Item` components defined at the end of the [Display Components](./16-display-components.md) article, and the `ItemService` defined at the end of the [Services](./13-services.md) article.

### [ItemsComponent](#routes)

The intent of `ItemsComponent` is to provide a read-only view of a collection of `ItemCardComponent` using `ItemListComponent`. Both `ItemCardComponent` and `ItemListComponent` have been optimized to take advantage of routing. These optimizations will be pointed out as each route component is defined.

**`items.component.ts`**

```ts
import {
  Component,
  OnInit
} from '@angular/core';

import { Router } from '@angular/router';

import { ItemService } from '../../services';
import { Item } from '../../models';

@Component({
  selector: 'items',
  templateUrl: 'items.component.html',
  providers: [ItemService]
})
export class ItemsComponent implements OnInit {
  constructor(
    public itemService: ItemService,
    private router: Router
  ) { }

  ngOnInit() {
    this.itemService.getItems();
  }

  viewItem = (item: Item) => this.router.navigate(['item', item.id]);
}
```

* The `Router` is injected into the component as `router`.
* In the `OnInit` lifecycle hook, the items are retrieved using `ItemService.getItems()`.
* `viewItem` responds to the `select` output event of `ItemCardComponent` that is passed through `ItemListComponent`.
  * The `navigate` function on the router accepts all route parameters in an array. In this case, we're navigating to `/item/:id`, where `:id` is the value of `item.id` for the `Item` that has been passed into the `viewItem` function.

**`items.component.html`**

```html
<mat-toolbar>Items</mat-toolbar>
<ng-template #loading>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
<ng-container *ngIf="itemService.items$ | async as items else loading">
  <item-list [items]="items"
             (select)="viewItem($event)"></item-list>
</ng-container>
```

* The `items$` stream is subscribed to using the `async as {prop} else {template}` workflow.
* The `items` property resolved from the `items$` stream is provided to the `items` input property of `ItemListComponent`.
* The `select` output event, which forwards the `select` output event from the intended `ItemCardComponent`, is registered to the `viewItem` function.

### [ItemComponent](#routes)

When the `select` output event is triggered by clicking the Edit action button an a `ItemCardComponent`, the app navigates to the route for that component. `ItemComponent` is a route component with the purpose of enabling an `Item` to be edited. It is responsible for:

* Providing a navigation button for returning to the `ItemsComponent` view
* Handling `save` output events for the `ItemCardComponent`
* Handling `remove` output events for the `ItemCardComponent`
* Rending the `ItemCardComponent` for an `Item` retrieved based on the value of the `/:id` parameter provided by the route

**`item.component.ts`**

```ts
import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  Router,
  ActivatedRoute,
  ParamMap
} from '@angular/router';

import { Subscription } from 'rxjs';

import { ItemService } from '../../services';
import { Item } from '../../models';

@Component({
  selector: 'item',
  templateUrl: 'item.component.html',
  providers: [ItemService]
})
export class ItemComponent implements OnInit, OnDestroy {
  private routeSub: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public itemService: ItemService
  ) { }

  ngOnInit() {
    this.routeSub = this.route.paramMap
      .subscribe(async (param: ParamMap) => {      
        if (param.has('id')) {
          const id = Number.parseInt(param.get('id'));
          const res = await this.itemService.getItem(id);

          if (!res) {
            this.navigate();
          }
        } else {
          this.navigate();
        }
      });
  }

  ngOnDestroy() {
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
  }

  private navigate = () => this.router.navigate(['items']);

  saveItem = async (item: Item) => {
    const res = await this.itemService.updateItem(item);
    res && this.itemService.getItem(item.id);
  }

  removeItem = async (item: Item) => {
    const res = await this.itemService.removeItem(item);
    res && this.navigate();
  }
}
```

* A `private routeSub: Subscription` property is defined, which keeps track of the `ActivatedRoute.paramMap` subscription
* The following services are injected into the component:
  * `route: ActivatedRoute` - allows attributes of the route to be retrieved
  * `router: Router` - an instance of the Angular router
  * `itemService: ItemService` - an instance of `ItemService` scoped to `ItemComponent`
* In the `OnInit` lifecycle hook, `routeSub` is assigned the value of the subscription to the `ActivatedRoute.paramMap` subscription
  * The route parameters are checked for the `id` parameter
    * As defined in the route definition: `/item/:id`
  * If the parameter has a value:
    * Parse the `id` to a `number` and call the `ItemService.getItem(id)` function.
    * If the result of `getItem` returns false, return to the `/items` view
      * This will happen if an `/:id` value is provided for an `Item` that does not exist on the API
  * If the parameter does not have a value, return to the `/items` view
* In the `OnDestroy` lifecycle hook, if `routeSub` has a value, its `unsubscribe()` function is called
* `private navigate()` provides a standardized means of navigating back to the `/items` view
* `async saveItem(item: Item)` is called whenever the `save` output event is triggered on the `ItemCardComponent`:
  * `item` is passed to `ItemService.updateItem(item)`. If it completes successfully, the `ItemService.getItem()` function is called and provided `item.id`.
* `async removeItem(item: Item)` is called whenever the `remove` output event is triggered on the `ItemCardComponent`:
  * `item` is passed to `ItemService.removeItem(item)`. If it completes successfully, return to the `/items` view, as the `Item` represented by this route no longer exists

Here is how the **routes** module is setup to handle these route components:

**`index.ts`**

```ts
import { Route } from '@angular/router';
import { ItemsComponent } from './item/items.component';
import { ItemComponent } from './item/item.component';

export const RouteComponents = [
  ItemsComponent,
  ItemComponent
];

export const Routes: Route[] = [
  { path: 'items', component: ItemsComponent },
  { path: 'item/:id', component: ItemComponent },
  { path: '', redirectTo: 'items', pathMatch: 'full' },
  { path: '**', redirectTo: 'items', pathMatch: 'full' }
];
```

[StackBlitz - Route Components](https://stackblitz.com/edit/docs-route-components?file=src%2Fapp%2Froutes%2Findex.ts)

[Back to Top](#routes)