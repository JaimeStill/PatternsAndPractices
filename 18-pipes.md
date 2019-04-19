# Pipes

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Anatomy](#anatomy)
* [Custom Pipes](#custom-pipes)
    * [UrlEncodePipe](#urlencodepipe)
    * [TruncatePipe](#truncatepipe)

## [Overview](#pipes)

The [Components - Pipes](./`4-components.md#pipes) article demonstrated the **Pipe** operator: `|` and a lot of Angular's built-in pipes. Now, it's time to get a better understanding of what a pipe is, and what it is used for.

## [Anatomy](#pipes)

Per the [Angular docs](https://angular.io/guide/pipes#using-pipes), a pipe takes in data as input and transforms it to a desired output. The examples of the built-in pipes demonstrated this by providing a value, and manipulating the way that value is rendered in the rendered output of the component.

Here are some additional rules concerning pipes:

* A pipe can have parameters, separated by a `:` and contained in quotes (either `'` or `"`)
    * Ex. - `{{now | date:'yyyy MMM dd'}}` renders the `now` date as `2019 Apr 19`
* Pipes can be chained together, separated by the pipe operator: `|`
    * Ex. - `{{now | date:'fullDate' | uppercase}}` renders the `now` date as `FRIDAY, APRIL 19, 2019`

## [Custom Pipes](#pipes)

To create your own pipe, you must:
* Create a class with the `@Pipe` decorator
* Implement the `PipeTransform` interface

> See [Pipe](https://angular.io/api/core/Pipe) and [PipeTransform](https://angular.io/api/core/PipeTransform) for detailed information

Here is the starting point for writing a custom pipe:

```ts
import {
  Pipe,
  PipeTransform
} from '@angular/core';

@Pipe({
  name: '{pipeName}' // whatever the pipe should be called when used
})
export class UrlEncodePipe implements PipeTransform {
  // transform accepts a value that can be constrained to any type
  transform(value: any) {
      // transform value and return the result
  }
}
```

Any custom pipe needs to be appropriately registered in the `index.ts` of the **pipes** TypeScript module, located at **{Project}.Web\\ClientApp\\src\\app\\pipes**:

```ts
import { CustomPipe } from './custom.pipe';

export const Pipes = [
  CustomPipe
]
```

### [UrlEncodePipe](#pipes)

The purpose of the `UrlEncodePipe` is to execute the `CoreService.urlEncode(value)` function on the value passed into the pipe. This is to show that, just as with **Components**, **Pipes** can also make use of Angular's built-in Dependency Injection framework.

**`url-encode.pipe.ts`**

```ts
import {
  Pipe,
  PipeTransform
} from '@angular/core';

import { CoreService } from '../services';

@Pipe({
  name: 'urlEncode'
})
export class UrlEncodePipe implements PipeTransform {
  constructor(
    private core: CoreService
  ) { }

  transform(value: string) {
    return this.core.urlEncode(value);
  }
}
```

* The pipe is name `urlEncode`
* The global `CoreService` is injected into the constructor of the pipe as `core`.
* The `transform(value: string)` function of the `PipeTransform` interface returns the value of calling `urlEncode(value)` on the `value` provided.

**Usage**:

```html
<p>{{value | urlEncode}}</p>
```

### [TruncatePipe](#pipes)

The purpose of `TruncatePipe` is to not only be able to truncate large blocks of text, but to specify the amount of characters to truncate after, and the value of the ellipses to show after the text has been truncated. For instance, a large block of text truncated after 10 characters with an ellipses of `...` would render like this:

```
This is lo...
```

> The character limit would tend to be larger, but this gives a good idea of the purpose.  

**`truncate.pipe.ts`**

```ts
import {
  Pipe,
  PipeTransform
} from '@angular/core';

@Pipe({
  name: 'truncate'
})
export class TruncatePipe implements PipeTransform {
  transform(value: string, limit = 50, ellipses = '...') {
    if (!value) { return ''; }
    return value.length <= limit ? value : `${value.substr(0, limit)}${ellipses}`;
  }
}
```

* The pipe is named `truncate`
* The `transform()` function receives the passed in `value`, an optional `limit` parameter that defaults to `50`, and an optional `ellipses` parameter that defaults to `...`
* If `value` is null or empty, return an empty string
* If `value.length` is less than or equal to the limit, simply return `value` as is. Otherwise, return the substring of `value` starting at `0` and ending at the `limit` with the `ellipses` value appended at the end.

**Usage**:

```html
<!-- renders 50 characters with the default ellipses -->
<p>{{paragraph | truncate}}</p>

<!-- renders 80 characters with the default ellipses -->
<p>{{paragraph | truncate:80}}</p>

<!-- renders 80 characters and uses *** as the ellipses -->
<p>{{paragraph | truncate:80:'***'}}</p>
```

[StackBlitz - Pipes](https://stackblitz.com/edit/docs-pipes?file=src%2Fapp%2Fpipes%2Ftruncate.pipe.ts)

[Back to Top](#pipes)