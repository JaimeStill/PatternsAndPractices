# Angular CLI

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Angular Workspace](#angular-workspace)

## [Overview](#angular-cli)

The Angular CLI is a command line tool that allows you to initialize, devleop, scaffold, and maintain Angular applications. <span>ASP.NET</span> Core uses the Angular CLI to host the Angular application when debugging in Development, and to compile the Angular application when publishing.

> If you want to know more about the Angular CLI, refer to the following documentation:
> * [Angular CLI Command Reference](https://angular.io/cli)
> * [Angular Workspace Configuration](https://angular.io/guide/workspace-config)

## [Angular Workspace](#angular-cli)

**{Project}.Web\\ClientApp\\angular.json** is a workspace configuration file for Angular that llows you to specify CLI command options and configurations to use when the CLI builds a project.  

This section will take a look at how important Angular infrastructure is specified in the **angular.json** file. Additionally, there are only three times when you would need to modify **angular.json**:

* You bring in a dependency on a new style or font to add to the `styles` array
* You bring in a dependency on a new 3rd party library that isn't related to Angular and need to add it to the `scripts` array
* You create a new global style file that needs to be added to the `styles` array
  * Seriously, don't do this. All global styles should be managed through the **{Project}.Web\\ClientApp\\src\\theme** directory, the root of which (**material-themes.scss**) has already been added as part of the template

**`angular.json`**

```json
{
  "$schema": "./node_modules/@angular/cli/lib/config/schema.json",
  "version": 1,
  "projects": {
    "web-app": {
      "root": "",
      "sourceRoot": "src",
      "projectType": "application",
      "prefix": "app",
      "schematics": {},
      "architect": {
        "build": {
          "builder": "@angular-devkit/build-angular:browser",
          "options": {
            "progress": true,
            "outputPath": "dist",
            "index": "src/index.html",
            "main": "src/main.ts",
            "polyfills": "src/polyfills.ts",
            "tsConfig": "src/tsconfig.app.json",
            "assets": [
              "src/assets",
              "src/favicon.ico"
            ],
            "styles": [
              "node_modules/material-icons/iconfont/material-icons.css",
              "node_modules/roboto-fontface/css/roboto/roboto-fontface.css",
              "src/theme/material-themes.scss"
            ],
            "scripts": [
              "node_modules/hammerjs/hammer.js"
            ]
          },
          "configurations": {
            "production": {
              "fileReplacements": [
                {
                  "replace": "src/environments/environment.ts",
                  "with": "src/environments/environment.prod.ts"
                }
              ],
              "optimization": true,
              "outputHashing": "all",
              "sourceMap": false,
              "extractCss": true,
              "namedChunks": false,
              "aot": true,
              "extractLicenses": true,
              "vendorChunk": false,
              "buildOptimizer": true
            }
          }
        },
        "serve": {
          "builder": "@angular-devkit/build-angular:dev-server",
          "options": {
            "browserTarget": "web-app:build"
          },
          "configurations": {
            "production": {
              "browserTarget": "web-app:build:production"
            }
          }
        },
        "extract-i18n": {
          "builder": "@angular-devkit/build-angular:extract-i18n",
          "options": {
            "browserTarget": "web-app:build"
          }
        },
        "lint": {
          "builder": "@angular-devkit/build-angular:tslint",
          "options": {
            "tsConfig": [
              "src/tsconfig.app.json"
            ],
            "exclude": [
              "**/node_modules/**"
            ]
          }
        }
      }
    }
  },
  "defaultProject": "web-app"
}
```  

`projects` represent all of the projects available in this workspace. In this case, there is only one project named `web-app`. At the bottom of the file, `web-app` is specified as the `defaultProject`.

> This section does not cover all of the properties specified in this configuration. It is listed for completeness, but will only cover some of the key infrastructure. If you want to understand more about something that is not covered, check out the [angular.json Schema](https://github.com/angular/angular-cli/wiki/angular-workspace).

> `index` and `main` point to the same files specified in [Angular Overview](./08-angular-overview.md). This is how they are configured for the Angular project.

`sourceRoot` is set to `src` and specifies that the `web-app` project's source is located at **{Project}.Web\\ClientApp\\src**. Because **angular.json** is located at **{Project}.Web\\ClientApp**, all URLs specified in this workspace are relative to that directory being the root.

`outputPath` is set to `dist` and specifies that the compiled Angular app will be output to **{Project}.Web\\ClientApp\\dist**. If you recall from the [Angular Overview](./08-angular-overview.md) article, this is the directory that is specified in the `AddSpaStaticFiles` service registration in the `Startup` class.

`index` is set to `src/index.html`, and specifies the index HTML file for the Angular app.

`main` is set to `src/main.ts` and specifies the entry point for the Angular app.

`polyfills` is set to `src/polyfills.ts` and specifies any additional javascript needed for Angular to function.

`tsConfig` is set to `src/tsconfig.app.json`, which extends `tsconfig.json`. For detailed information, see [tsconfig.json](https://www.typescriptlang.org/docs/handbook/tsconfig-json.html).

`assets` is an array that specifies the files and directories that are included in the build, but not used specifically by the app.

`styles` is an array that specifies any imported styles. In this case, the stylesheets for the **Material Icons** and **Roboto** fonts are specified, as well as the root stylesheet for the **{Project}.Web\\ClientApp\\src\\theme** directory.

> Theming will be covered in the [Material](./11-material.md) article.

`scripts` is an array of any third-party libraries that are used by the app, but not able to be imported via an Angular module. In this case, `hammer.js` is specified because Angular Material specifies it as an optional dependency to enable touch-based features.

`configurations` allows you to specify additional build configurations that differ from the base configuration.


[Back to Top](#angular-cli)