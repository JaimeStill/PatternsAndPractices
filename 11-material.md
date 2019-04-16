# Material

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Theming](#theming)
    * [Material Styles](#material-styles)
    * [Material Themes](#material-themes)
* [Components](#component-usage)
    * [Menu](#menu)
    * [Slider](#slider)

## [Overview](#material)

[Angular Material](https://material.angular.io) is a UI framework based on the [Material Design](https://material.io) design language. It is written in Angular and maintained by a team at Google.

There are two important aspects to using Angular Material:

* Styling and theming within the framework
* Using the components without your own components and layouts

These aspects are what this documentation will cover in detail.

> Although some of the Material components will be introduced here, they will be discussed in detail in the [Components](./14-components.md) through [Routes](./17-routes.md) articles.

## [Theming](#material)

Angular Material, in conjunction with the [SCSS](https://sass-lang.com/documentation/file.SCSS_FOR_SASS_USERS.html) flavor of [Sass](https://sass-lang.com/guide), enables flexible, powerful styling and theming of an Angular app. However, the theming guide on the Angular Material site doesn't explain a lot of the nuances of advanced theming, so this section will attempt to fill in the gaps.

One thing of note is that it's incredibly important to get familiar with the [core library](https://github.com/angular/material2/tree/master/src/lib/core) of Angular Material in order to understand how it is constructed. It is also helpful to take a look at the material component stylings, for example [MatToolbar](https://github.com/angular/material2/blob/master/src/lib/toolbar/_toolbar-theme.scss) to better understand how styling is used internally.

Some important aspects of the Angular Material source:

File | Importance
-----|-----------
[_theming.scss](https://github.com/angular/material2/blob/master/src/lib/core/theming/_theming.scss) | Defines SCSS helper functions for theming in Angular Material
[_palette.scss](https://github.com/angular/material2/blob/master/src/lib/core/theming/_palette.scss) | Defines the color palettes, including their various shades, contrast values, and theming functions
[_typography.scss](https://github.com/angular/material2/blob/master/src/lib/core/typography/_typography.scss) | Defines styles, functions, and mixins related to managing app typography
[_elevation.scss]() | Defines styles, functions, and mixins related to the elevation effects
[_variables.scss]() | Defines useful global variables used throughout the framework
[_ripple.scss](https://github.com/angular/material2/blob/master/src/lib/core/ripple/_ripple.scss) | Defines mixins for styling the ripple effect
[purple-green.scss](https://github.com/angular/material2/blob/master/src/lib/core/theming/prebuilt/purple-green.scss) | A very basic example prebuilt Angular Material theme

There are many more files I could have included, and it would be beneficial for you to look through them, but for the scope of understanding the way the app stack is styled and themed, the following files were crucial:

* `_theming.scss`
* `_palette.scss`
* `_elevation.scss`

There are two files related to the styling and theming of the app stack, located within **{Project}.Web\\ClientApp\\src\\theme**:

* `material-styles.scss`
* `material-themes.scss`

The next two sections will cover these files in detail.

### [Material Styles](#material)

To take advantage of the custom theming capabilities of Angular Material, one (and only **ONE**) file must include the following at the top:

```scss
@import() '~@angular/material/theming';
@include() mat-core();
```

Any other files that need these capabilities must import the `.scss` file that specify this.

In this app stack, `material-styles.scss` specifies this, and `material-themes.scss` imports `material-styles.scss`.

Apart from importing and including the Angular Material theming capabilities, `material-styles.scss` has two important responsibilities:
* Specify a Sass **mixin** that is called whenever a theme is created to dynamically apply styles based on that theme
* Define global styles that are relevant globally within the application

**`material-styles.scss`**

```scss
@import '~@angular/material/theming';
@include mat-core();

@mixin style-theme($theme) {
  $p: map-get($theme, primary);
  $a: map-get($theme, accent);
  $w: map-get($theme, warn);
  $b: map-get($theme, background);
  $f: map-get($theme, foreground);
  $primary: mat-color($p);
  $accent: mat-color($a);
  $warn: mat-color($w);
  $background: mat-color($b, background);
  $background-card: mat-color($b, card);
  $foreground: mat-color($f, text);
  $app-bar: mat-color($b, app-bar);
  $primary-contrast: mat-color($p, default-contrast);
  $accent-contrast: mat-color($a, default-contrast);
  $warn-contrast: mat-color($w, default-contrast);

  .color.primary {
    color: $primary;
  }

  .color.accent {
    color: $accent;
  }

  .color.warn {
    color: $warn;
  }

  .background.primary {
    background-color: $primary;
    color: $primary-contrast;
  }

  .background.accent {
    background-color: $accent;
    color: $accent-contrast;
  }

  .background.warn {
    background-color: $warn;
    color: $warn-contrast;
  }

  .background.card {
    background-color: $background-card;
  }

  .background.stacked {
    background-color: $background;
  }

  code.snippet,
  a.link {
    color: $primary;
  }

  code.snippet.alt,
  a.link:visited {
    color: $accent;
  }

  mat-paginator.mat-paginator {
    background-color: $background;
  }

  a.mat-list-item.active,
  a.mat-tab-link.active {
    color: $primary;
  }

  text {
    fill: $foreground;
  }
}

html, body, app, .app-panel {
    overflow: hidden;
    margin: 0;
    height: 100%;
}

.app-body {
    height: 100%;
    overflow: auto;
}

.mat-typography h1,
.mat-typography h2,
.mat-typography h3,
.mat-typography h4,
.mat-typography h5,
.mat-typography h6,
.mat-typography p,
mat-form-field.mat-form-field {
  margin: 8px;
}

mat-toolbar.mat-toolbar.app-toolbar {
  padding: 0 8px;

  button.mat-icon-button {
    margin-right: 8px;
  }
}

mat-toolbar.mat-toolbar {
  span {
    margin-right: 4px;
    margin-left: 4px;
  }
  button.mat-button {
    margin-left: 4px;
    margin-right: 4px;
  }
}

a.mat-tab-link {
  min-width: 80px;
}

.container {
  padding: 4px;
}

.card {
  margin: 4px;
}

.clickable {
  cursor: pointer;
}

img.thumbnail {
  height: 80px;
}

img.profile-image {
  height: 200px;
}

.static-elevation {
  @include mat-elevation(2);
}

.elevated {
  @include mat-elevation-transition;
  @include mat-elevation(2);

  &:hover {
    @include mat-elevation(6);
  }
}
```

The first thing the `style-theme` mixin does is declare variables for all of the palettes specified by the theme:

* primary - The primary theme palette
* accent - The theme's accent palette
* warn - The theme's warn palette
* background - The background palette of the theme
* foreground - The foreground palette of the theme

> The `map-get` function is a built-in Sass function for mapping a key to a value. See [map-get](https://sass-lang.com/documentation/Sass/Script/Functions.html#map_get-instance_method) for detailed information.

After these palette variables are defined, more variables are defined to extract specific colors from these palettes:

* `$primary` - The default shade of the primary color specified by the theme
* `$accent` - The default shade of the accent color specified by the theme
* `$warn` - The default shade of the warn color specified by the theme
* `$background` - The default shade of the background color specified by the theme
* `$background-card` - The background shade the theme uses to render a card against the background
* `$foreground` - The text shade of the foreground color specified by the theme
* `$app-bar` - The background shade the theme uses to render an app bar against the background
* `$primary-contrast` - The foreground shade used to render text against the primary theme color
* `$accent-contrast` - The foreground shade used to render text against the accent theme color
* `$warn-contrast` - The foreground shade used to render text against the warn theme color

> `mat-color` is a function defined in the `_theming.scss` file in the Angular Material source. See [_theming.scss](https://github.com/angular/material2/blob/master/src/lib/core/theming/_theming.scss#L47) for detailed information.

To help this make more sense, look at the [color palette definitions](https://github.com/angular/material2/blob/master/src/lib/core/theming/_palette.scss#L39) in the Angular Material source code. `$mat-red` represents a potential palette that is used for any of the colors (**primary**, **accent**, or **warn**). One of the shades (`50` - `A700`) is selected as the base color for the theme color specified, and each shade has a corresponding `contrast` value that determines the associated foreground color. For instance, `$mat-red.500` has a color value of `#f44336`, and text will be drawn in the color represented by the `$light-primary-text` (`white` in this case).

> `$dark-primary-text` and `$light-primary-text` are defined in `_palette.scss`. See [_palette.scss](https://github.com/angular/material2/blob/a6c6a58cc4bccddeb87ad71b33a849527e23c462/src/lib/core/theming/_palette.scss#L28) for detailed information.

> For a complete list of available theme variables, refer to the `$mat-light-theme-background`, `$mat-dark-theme-background`, `$mat-light-theme-foreground`, and `$mat-dark-theme-foreground` variables defined in `_palette.scss`. See [_palette.scss](https://github.com/angular/material2/blob/a6c6a58cc4bccddeb87ad71b33a849527e23c462/src/lib/core/theming/_palette.scss#L674) for detailed information.

Below the mixin, global styles are defined. There are two interesting pieces of the global style that are interesting:

* The height and overflow definitions for the root component (`AppComponent`) layout
* The `.static-elevation` and `.elevated` styles

In order to render the theme background appropriately, all of the structural elements that host the `<router-outlet>` need to render at `height: 100%`. Additionally, to prevent the main app toolbar from scrolling with the rest of the app, overflow is hidden in all but the `.app-body` element.

Whenever an element is styled with the `.static-elevation` class, it will be rendered with an elevation of `2`. This style makes use of the `mat-elevation` function, which is defined in [_elevation.scss](https://github.com/angular/material2/blob/master/src/lib/core/style/_elevation.scss#L147).

Whenever an element is styled with the `.elevated` class, it starts with a default elevation of `2`, using the same `mat-elevation` mixin. However, whenever hover onto / off of the element, it will transition between an elevation of `2` and an elevation of `6`, using the `mat-elevation-transition` mixin defined in [_elevation.scss](https://github.com/angular/material2/blob/master/src/lib/core/style/_elevation.scss#L206).

> See [StackBlitz - Angular Material Elevation Transitions](https://stackblitz.com/edit/angular-material-elevation-transitions) for an example of this behavior. Also, if you select a new theme from the option menu at the top right of the app bar, you'll see how the `style-theme` mixin dynamically adjusts the theme of the app.

### [Material Themes](#material)

The `material-themes.scss` file has four responsibilities:

* Import the other `.scss` files in the current theme, to include the file that imports and includes the Angular Material theming infrastructure.
* Define variables for all of the color palettes to be used for the themes
* Define a default theme
* Define styles for all of the available themes

**`material-themes.scss`**

``` scss
@import "./material-styles.scss";

$light-blue: mat-palette($mat-light-blue, A400);
$red: mat-palette($mat-red);
$deep-orange: mat-palette($mat-deep-orange, A200);
$pink: mat-palette($mat-red, A100);
$crimson: mat-palette($mat-red, A400);

/*-- Dark Theme Colors --*/
$green-dark: mat-palette($mat-green, A400);
$indigo-dark: mat-palette($mat-indigo, A200);
$purple-dark: mat-palette($mat-purple, A700);
$amber-dark: mat-palette($mat-amber, A400);
$teal-dark: mat-palette($mat-teal, A400);

/*-- Light Theme Colors --*/
$green-light: mat-palette($mat-green, A700);
$indigo-light: mat-palette($mat-indigo, A400);
$purple-light: mat-palette($mat-purple, 700);
$amber-light: mat-palette($mat-amber, A700);
$teal-light: mat-palette($mat-teal, A700);

/*-- Default Theme --*/
$default-theme: mat-dark-theme($green-dark, $light-blue);
@include style-theme($default-theme);
@include angular-material-theme($default-theme);

/*-- Dark Themes --*/
.dark-green {
  $default-theme: mat-dark-theme($green-dark, $light-blue);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-blue {
  $default-theme: mat-dark-theme($light-blue, $green-dark);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-red {
  $default-theme: mat-dark-theme($red, $light-blue, $pink);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-indigo {
  $default-theme: mat-dark-theme($indigo-dark, $deep-orange);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-orange {
  $default-theme: mat-dark-theme($deep-orange, $indigo-dark);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-purple {
  $default-theme: mat-dark-theme($purple-dark, $amber-dark);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-amber {
  $default-theme: mat-dark-theme($amber-dark, $purple-dark);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-crimson {
  $default-theme: mat-dark-theme($crimson, $teal-dark, $deep-orange);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.dark-teal {
  $default-theme: mat-dark-theme($teal-dark, $crimson, $deep-orange);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

/*-- Light Themes --*/
.light-green {
  $default-theme: mat-light-theme($green-light, $light-blue);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-blue {
  $default-theme: mat-light-theme($light-blue, $green-light);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-red {
  $default-theme: mat-light-theme($red, $light-blue, $pink);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-indigo {
  $default-theme: mat-light-theme($indigo-light, $deep-orange);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-orange {
  $default-theme: mat-light-theme($deep-orange, $indigo-light);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-purple {
  $default-theme: mat-light-theme($purple-light, $amber-light);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-amber {
  $default-theme: mat-light-theme($amber-light, $purple-light);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-crimson {
  $default-theme: mat-light-theme($crimson, $teal-light, $deep-orange);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}

.light-teal {
  $default-theme: mat-light-theme($teal-light, $crimson, $deep-orange);
  @include style-theme($default-theme);
  @include angular-material-theme($default-theme);
}
```

The `mat-palette` function is defined in the `_theming.scss` file. The second argument provided corresponds to the shade from the palette to be used. Calling `mat-palette` with only a palette defaults to the `$palette.500` value. You can specify up to four arguments, the last three being optional: 

```
mat-palette($base-palette, $default: 500, $lighter: 100, $darker: 700)
```

> See [_theming.scss](https://github.com/angular/material2/blob/master/src/lib/core/theming/_theming.scss#L17) for detailed information.

> The palettes specified in this file, for instance `$mat-light-blue`, are defined in `_palettes.scss`. See [_palette.scss](https://github.com/angular/material2/blob/master/src/lib/core/theming/_palette.scss#L39) for detailed information.

The `$default-theme` variable represents the active theme. To create a theme, you must do three things:

1. Set `$default-theme` by calling either the `mat-light-theme` or `mat-dark-theme` function, providing the palettes as required
2. Call the `style-theme` mixin defined in `material-styles.scss`, passing in the `$default-theme` variable
3. Call the `angular-material-theme` mixin, passing in the `$default-theme` variable

The `mat-light-theme` and `mat-dark-theme` functions are defined in `_theming.scss` and are defined as follows:

``` scss
// Creates a container object for a light theme to be given to individual component theme mixins.
@function mat-light-theme($primary, $accent, $warn: mat-palette($mat-red)) {
  @return (
    primary: $primary,
    accent: $accent,
    warn: $warn,
    is-dark: false,
    foreground: $mat-light-theme-foreground,
    background: $mat-light-theme-background,
  );
}


// Creates a container object for a dark theme to be given to individual component theme mixins.
@function mat-dark-theme($primary, $accent, $warn: mat-palette($mat-red)) {
  @return (
    primary: $primary,
    accent: $accent,
    warn: $warn,
    is-dark: true,
    foreground: $mat-dark-theme-foreground,
    background: $mat-dark-theme-background,
  );
}
```

The `$primary` and `$accent` palettes are required, and the `$warn` palette is optional.

The `angular-material-theme` mixin is defined in the `_all-theme.scss` file, located in **src/lib/core/theming** in the Angular Material source code. It receives the `$default-theme` variable and passes it through all of the necessary mixins to appropriately styel the theme.

> See [_all-theme.scss](https://github.com/angular/material2/blob/8050f633b56b6c048fc72dad2ab79304afdfad19/src/lib/core/theming/_all-theme.scss#L40) for detailed information.

Changing the theme is simply a matter of changing the style class on the root element in `AppComponent`. If the element contains the `.dark-green` class, it will be styled based on the theme defined in that class style.

> Setting and keeping track of the current theme is managed by a `ThemeService` and will be covered in the [Services](./13-services.md) article.

## [Components](#material)

There are a whole host of components available out of the box in Angular Material. You can find them in the [Components](https://material.angular.io/components/categories) section of the documentation.

Each component definition contains 3 sections:

* An overview of the component
* The API for the component
* Component examples

It is highly recommended that, before implementing a component, you read through its documentation.

The **API** section of a component's documentation will not only have the properties associated with that component, but any classes or interfaces that are directly related to that component. For instance, `MatSelect` also provides the details for the `MatSelectChange` interface that is provided whenever the `selectionChange` output property is called.

In the next two sections, simple examples of two components are demonstrated.

### [Menu](#material)

This example will make use of the [MatMenu](https://material.angular.io/components/menu/overview) component. We will render it along with an icon that toggles whenever the menu is opened or closed. It will simply contain a list of three inactive action buttons.

**`home.component.ts`**

```ts
import { Component } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  opened = false;
}
```

The only thing we need to keep track of is whether or not the menu is currently opened. So our component contains an `opened` property that defaults to `false`. We aren't responding to any events, so we don't need to import any Material infrastructure in the code of the component.

**`home.component.html`**

```html
<mat-toolbar>Menu Example</mat-toolbar>
<button mat-icon-button 
        [matMenuTriggerFor]="menu"
        (menuOpened)="opened = true"
        (menuClosed)="opened = false">
  <mat-icon *ngIf="opened">arrow_drop_down</mat-icon>
  <mat-icon *ngIf="!(opened)">arrow_right</mat-icon>
  <span>Menu</span>
</button>
<mat-menu #menu="matMenu">
  <button mat-menu-item>
    <mat-icon>edit</mat-icon>
    <span>Edit</span>
  </button>
  <button mat-menu-item>
    <mat-icon>attach_file</mat-icon>
    <span>Attach</span>
  </button>
  <button mat-menu-item>
    <mat-icon>delete</mat-icon>
    <span>Delete</span>
  </button>
</mat-menu>
```

The first button in the above example is the trigger for the actual `<mat-menu>` component. Clicking it will cause the menu to animate into existence. The `[matMenuTriggerFor]="menu"` attribute on this button specifies this relationship. `(menuOpened)` and `(menuClosed)` are **Output properties** for the `MatMenuTrigger` interface, and whenever they are triggered, they update the value of the `opened` property.

Depending on whether `opened` is `true` or `false`, a different `<mat-icon>` element is rendered (using the `*ngIf` structural directive to make this determination).

> For a full listing of the available icons in the Material Icons font face, see [Material Design Icons](https://material.io/tools/icons/). The value to place in between the `<mat-icon></mat-icon>` element is shown here. If some of the name is cut off, click on the icon and expand the **Selected Icon** bar at the bottom to see the full name.

The `<mat-menu #menu="matMenu">` element specifies that its ID is `#menu` and that it is a `matMenu`. It then contains a collection of `<button>` elements marked with the `mat-menu-item` attribute directive. They each contain a `<mat-icon>` and `<span>` to illustrate the contents of the button.

You can see this example at [StackBlitz - Docs Menu Example](https://stackblitz.com/edit/docs-menu-example?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.html).

### [Slider](#material)

This example will make use of the [MatSlider](https://material.angular.io/components/slider/overview) component and will be a bit more involved than the last example.

Before showing the implementation, I'll define the intent of the example, as well as the transactions that will need to happen to enable this intent.

We want to display an image with a slider above it that, when dragged, will adjust the size of the image. To accomplish this, we'll need the following:

* A `size` property that keeps track of the current size of the image
* A function that is called when the `<mat-slider>` element emits the `(change)` event.
    * The `(change)` output property emits an event property of type `MatSliderChange`, so we'll need to import this into the component definition.
    * `(change)` is an **Output property** and output properties emit events. Don't worry about understanding this concept yet. It will be covered in detail in the [Components](./14-components.md) article.
* An image to display

> This demonstration will also make use of the [Angular Flex Layout](https://github.com/angular/flex-layout/wiki) library, which will also be covered in detail in the [Components](./14-components.md) article.

**`home.component.ts`**

```ts
import { Component } from '@angular/core';
import { MatSliderChange } from '@angular/material';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  size = 500;

  updateSize = (event: MatSliderChange) => this.size = event.value;
}
```

Here, we import the `MatSliderChange` class for use with the argument provided by the event emitted by `<mat-slider (change)="updateSize($event)">`. This syntax of passing an event emitted variable into a function in the template is standard in Angular: `$event` simply means the object being provided by the event emitter when it is called.

The `size` property is set to a default of `500`.

The `updateSize` function receives the `MatSliderChange` event object and assigns its `value` property to our `size` property.

**`home.component.html`**

```html
<mat-toolbar>Slider Example</mat-toolbar>
<section fxLayout="column"
         fxLayoutAlign="start center">
  <section fxLayout="row"
           fxLayoutAlign="start center">
    <p>Image Size</p>
    <mat-slider [min]="400"
                [max]="600"
                [value]="size"
                (change)="updateSize($event)"
                [thumbLabel]="true"
                color="warn"
                step="10"
                tickInterval="1"
                [style.width.px]="420"></mat-slider>
  </section>
  <img src="https://picsum.photos/500?random"
       [height]="size" />
</section>
```

Before looking at the `<mat-slider>` element usage, take a look at the `<img>` tag. It pulls a random photo from [picsum.photos](https://picsum.photos/) with a default size of 500. The `height` attribute is bound to the value of the `size` property.

The `<mat-slider>` element specifies the following:

* A minimum value of 400
* A maximum value of 600
* A current value based on the `size` property
* Whenever the `change` output property executes, it calls our `updateSize` function, passing in the `$event` object.
* Whenever you click and drag on the slider, the thumb label will show with the current value.
* The color is set to the **warn** color from the current theme palette
    * You can change the theme of the app by clicking the paint bucket icon on the right of the top toolbar in the example linked below.
* The `step` value is set to **10**, meaning that the value represented by the slider can only be adjusted in increments / decrements of 10.
* The `tickInterval` is set to 1, but this will cause it to be displayed in increments of **10** because of the `step` value.
* The slider is rendered with a width of `420px`.

You can see this example at [StackBlitz - Docs Slider Example](https://stackblitz.com/edit/docs-slider-example?file=src%2Fapp%2Froutes%2Fhome%2Fhome.component.ts).

[Back to Top](#material)