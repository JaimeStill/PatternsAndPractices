# Angular

* [Overview](#overview)
* [Integration](#integration)
    * [Startup](#startup)
    * [MSBuild](#msbuild)
* [Bootstrapping](#bootstrapping)
    * [Index](#index)
    * [Main](#main)
    * [AppModule](#appmodule)
* [Managing Dependencies](#managing-dependencies)

## [Overview](#angular)

Angular is a full front-end framework written in TypeScript. It is more than just a view library that allows you to write web components. While it offers the ability to create components, it enables much more straight out of the box, to include but not limited to:
* Routing
* Services with Dependency Injection
* Modular design
* Ahead of Time compilation
* Hot Module Reloading

An understanding of the following languages will be crucial to success in Angular:

* [HTML](https://developer.mozilla.org/en-US/docs/Glossary/HTML)
* [CSS](https://developer.mozilla.org/en-US/docs/Glossary/CSS)
* [JavaScript](https://developer.mozilla.org/en-US/docs/Glossary/JavaScript)
* [TypeScript](https://www.typescriptlang.org/docs/home.html)

> It is highly recommended that you read through the Angular documentation. It's very well written, and covers crucial details that, if not understood, will make the Angular portion of this guide very confusing. There's a full section of recommended reading in [References - Links](./r1-links.md#angular).

## [Integration](#angular)  

Before jumping into the details of working with Angular, it's important to understand how it is integrated with the <span>ASP.NET</span> Core back-end, and how it is initialized.  

The entirety of the Angular application is contained in the **{Project}.Web\\ClientApp** directory. Often, when working in the app stack, I'll have two instances of visual studio code:

* An instance pointed at the root of the app stack for back-end development
* An instance pointed at the **{Project}.Web\\ClientApp** directory for front-end development.

Any changes made to the front-end instance of Visual Studio Code will automatically be detected by the back-end instnace. If you're running your application on the back-end instance and modify the Angular app in the front-end instance, hot module replacement will still be triggered and refresh the page when you save changes on the front-end instance.  

Integration with <span>ASP.NET</span> Core and bootstrapping of the Angular application occurs in the following files:

* **{Project}.Web\\Startup.cs**
* **{Project}.Web.csproj**

> This section will not go into too much detail about exactly HOW Angular is integrated into <span>ASP.NET</span> Core. It's just important that you understand WHERE it is integrated so you know why certain configuration is in place. If you care about any of the HOW details, check out the [Angular](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/angular?view=aspnetcore-2.2&tabs=visual-studio) and [JavaScriptServices](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa-services?view=aspnetcore-2.2) integration guides. The Angular template is built on top of the API developed in the **JavaScriptServices** guide.

### [Startup](#angular)

Angular is primarily integrated via the `Startup` class. Below is an example of `Startup` with everything but the pieces related to Angular integration removed:

**`Startup`**

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/dist";
        });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app.UseSpaStaticFiles();

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (env.IsDevelopment())
            {
                spa.UseAngularCliServer(npmScript: "start");
            }
        });
    }
}
```  

`services.AddSpaStaticFiles()` registers an `ISpaStaticFileProvider` service that can provide static files to be served for a Single Page Application. In this case, the `configuration.RootPath` specifies where the compiled application will live in production.

`app.UseSpaStaticFiles()` configures the application to serve static files for a Single Page Application. The files are located using the registered `ISpaStaticFileProvider` service (as configured in `AddSpaStaticFiles` for production, and `UseAngularCliServer` for development).

`app.UseSpa()` handles all requests from this point in the middleware chain by returning the default page for the Single Page Application. 

The `spa` lambda function represents an `ISpaBuilder` instance which is used for configuring the hosting of a Single Page Application and attaching middleware. 

`spa.Options.SourcePath` tells the middleware, relative to the working directory, where the SPA source files are contained.

`spa.UseAngularCliServer` is used only in development. It handles requests by passing them through to an instance of the Angular CLI server. You can always serve up-to-date CLI-built resources without having to run the Angular CLI server manually.

### [MSBuild](#angular)

`Startup` is responsible for setting up the services and middleware that integrate Angular, but the **{Project}.Web.csproj** file is responsible for integrating the build process. Below, you will find the Angular-specific details of the project file:

**`{Project}.Web.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netcoreapp2.2</TargetFramework>
    <TypeScriptCompileBlocked>true</TypeScriptCompileBlocked>
    <TypeScriptToolsVersion>Latest</TypeScriptToolsVersion>
    <IsPackable>false</IsPackable>
    <SpaRoot>ClientApp\</SpaRoot>
    <LangVersion>latest</LangVersion>
    <DefaultItemExcludes>$(DefaultItemExcludes);$(SpaRoot)node_modules\**</DefaultItemExcludes>

    <!-- Set this to true if you enable server-side prerendering -->
    <BuildServerSideRenderer>false</BuildServerSideRenderer>
  </PropertyGroup>

  <ItemGroup>
    <!-- Don't publish the SPA source files, but do show them in the project files list -->
    <Content Remove="$(SpaRoot)**" />
    <None Remove="$(SpaRoot)**" />
    <None Include="$(SpaRoot)**" Exclude="$(SpaRoot)node_modules\**" />
  </ItemGroup>

  <Target Name="DebugEnsureNodeEnv" BeforeTargets="Build" Condition=" '$(Configuration)' == 'Debug' And !Exists('$(SpaRoot)node_modules') ">
    <!-- Ensure Yarn is installed -->
    <Exec Command="yarn --version" ContinueOnError="true">
      <Output TaskParameter="ExitCode" PropertyName="ErrorCode" />
    </Exec>
    <Error Condition="'$(ErrorCode)' != '0'" Text="Yarn is required to build and run this project. To continue, please install Node.js from https://yarnpkg.com/, and then restart your command prompt or IDE." />
    <Message Importance="high" Text="Restoring dependencies using 'yarn'. This may take several minutes..." />
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn install" />
  </Target>

  <Target Name="PublishRunWebpack" AfterTargets="ComputeFilesToPublish">
    <!-- As part of publishing, ensure the JS resources are freshly built in production mode -->
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn install" />
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn build" />
    <Exec WorkingDirectory="$(SpaRoot)" Command="yarn build:ssr" Condition=" '$(BuildServerSideRenderer)' == 'true' " />

    <!-- Include the newly-built files in the publish output -->
    <ItemGroup>
      <DistFiles Include="$(SpaRoot)dist\**; $(SpaRoot)dist-server\**" />
      <DistFiles Include="$(SpaRoot)node_modules\**" Condition="'$(BuildServerSideRenderer)' == 'true'" />
      <ResolvedFileToPublish Include="@(DistFiles->'%(FullPath)')" Exclude="@(ResolvedFileToPublish)">
        <RelativePath>%(DistFiles.Identity)</RelativePath>
        <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
      </ResolvedFileToPublish>
    </ItemGroup>
  </Target>

</Project>
```

This is not anything you should ever have to modify, but I wanted you to be aware that it exists. There are two sections specifically that I feel should be discussed:

* `<Target Name="DebugEnsureNodeEnv" />`
* `<Target Name="PublishRunWebpack" />`

`DebugEnsureNodeEnv` is written so that when the project is built, if `node_modules` isn't present in the **ClientApp** folder, it will run `yarn install`.

`PublishRunWebpack` is written so that when you publish the web app, it will also appropriately compile the Angular dependencies to the **ClientApp\\dist** folder. This is the folder that is specified in the `AddSpaStaticFiles` service registration in `Startup`.

## [Bootstrapping](#angular)

In addition to understanding how Angular is integrated with <span>ASP.NET</span> Core, it's also important to consider how it is initialized.

Angular is bootstrapped through the following files:

* **{Project}.Web\\ClientApp\\src\\index.html**
* **{Project}.Web\\ClientApp\\src\\main.ts**
* **{Project}.Web\\ClientApp\\src\\app\\app.module.ts**

### [Index](#angular)

This is another file that you will most likely never touch, but is important to be aware of. There are two important characteristics of **{Project}.Web\\ClientApp\\src\\index.html**:

* `<base href="/">`
* `<app-root>Loading...</app-root>`

Here is the **index.html** file for reference:

**`index.html`**

```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>Web App</title>
  <base href="/">

  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="icon" type="image/x-icon" href="favicon.ico">
</head>
<body>
  <app-root>Loading...</app-root>
</body>
</html>
```

`<base href="/">` simply tells the Angular router how to compose navigation URLs. In this case, the `href="/"` value specifies that if we are navigating to the `/home` URL, the URL will be `http://{app-url}/home`. However, if there were a prefix (say you want to differentiate Angular routes from MVC routes, for instance) and the value is `href="/angular"`, that same route would be `http://{app-url}/angular/home`.

`<app-root>Loading...</app-root>` specifies where to render the bootstrapped Angular component for our application. While the Angular module is still compiling, it will display ***Loading...*** until the Angular component can be rendered.

The components that are bootstrapped and available to be loaded into this file are specified as:

* An `AppModule` is bootstrapped using `bootstrapModule` in `main.ts`
    * Any component that is in the `bootstrap` array of the `AppModule` metadata definition can be rendered in the **index.html** view (in this case, `AppComponent`)
        * Modules will be covered in the [Modules](./10-modules.md) section
        * Components (including `AppComponent`) will be covered in the [Components](./14-components.md) section

### [Main](#angular)

**{Project}.Web\\ClientApp\\src\\main.ts** is another file you will most likely never touch, but is important to be aware of. It serves as the main entry point for the Angular application. It compiles the application with the JIT compiler and bootstraps the application's root module (`AppModule`) to run in the browser. 

Here it is:

**`main.ts`**

```ts
import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));
```  

`getBaseUrl` is used to determine th value of the `<base href="/">` tag in **index.html**. This is then used to register the `BASE_URL` of the app.

If the application is running in production, `enableProdMode()` is called, which turns off assertions and other checks within the framework.

`platformBrowserDynamic(providers).bootstrapModule(AppModule)` passes in the `BASE_URL` configuration to the app and bootstraps the app, using the root component (specified as `AppComponent` in the `bootstrap` array) of `AppModule`.

### [AppModule](#angular)

`AppModule` is the root module for the Angular application. It performs the following functions:

* Specifies the Components and Dialogs that are available for use in the application
* Imports the modules that the application depends on
* Specifies the components that are available when the module is bootstrapped

> To prevent confusion, I have removed a lot of the implementation details of `AppModule` to focus on the areas that enable the app to be bootstrapped

**`app.module.ts`**

```ts
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

All of the dependent Angular modules that thet application depend on are imported via the `imports` array. The `AppComponent` is made available via the `declarations` array, and is specified as a root component via the `bootstrap` array.

> If you look at the `selector` specified in `AppComponent`, you will see that it is registered as `app-root`. This is why, in **index.html**, app component can be rendered with `<app-root></app-root>`.

## [Managing Dependencies](#angular)

The app stack uses **Yarn** to manage npm modules (3rd-party front-end dependencies). All of the `yarn` commands listed below should be run in a command prompt pointed at the **{Project}.Web\\ClientApp** directory.

Command | Description
--------|------------
`yarn -h` | Shows the available `yarn` commands
`yarn {command} -h` | Shows the help file for the specified command
`yarn add {package}` | Adds the latest release version of the package specified to `dependencies`.
`yarn add -D {package}` | Adds the latest release version of the package specified to `devDependencies`.
`yarn add -O {package}` | Adds the latest release version of the package specified to `optionalDependencies`
`yarn add {package}@{version}` | Adds the specified version of the package to `dependencies`
`yarn remove {package}` | Removes the package specified and rebuild the dependency graph
`yarn install` | Install any missing dependencies and rebuild the dependency graph
`npm view {package} versions --json` | View all avaiable versions of the package specified.  

All of the specified packages are listed in **{Project}.Web\\ClientApp\\package.json**:

```json
{
  "name": "web-app",
  "version": "0.0.0",
  "license": "MIT",
  "scripts": {
    "ng": "ng",
    "start": "ng serve",
    "build": "ng build",
    "build:ssr": "npm run build -- --app=ssr --output-hashing=media"
  },
  "private": true,
  "dependencies": {
    "@angular/animations": "^7.2.12",
    "@angular/cdk": "^7.3.7",
    "@angular/common": "^7.2.12",
    "@angular/compiler": "^7.2.12",
    "@angular/core": "^7.2.12",
    "@angular/flex-layout": "^7.0.0-beta.24",
    "@angular/forms": "^7.2.12",
    "@angular/material": "^7.3.7",
    "@angular/platform-browser": "^7.2.12",
    "@angular/platform-browser-dynamic": "^7.2.12",
    "@angular/router": "^7.2.12",
    "@aspnet/signalr": "^1.1.2",
    "aspnet-prerendering": "^3.0.1",
    "core-js": "2.6.5",
    "hammerjs": "^2.0.8",
    "rxjs": "^6.4.0",
    "zone.js": "0.8.29"
  },
  "devDependencies": {
    "@angular-devkit/build-angular": "^0.13.8",
    "@angular-devkit/core": "^7.3.8",
    "@angular/cli": "^7.3.8",
    "@angular/compiler-cli": "^7.2.12",
    "@angular/language-service": "^7.2.12",
    "@types/node": "^11.13.0",
    "codelyzer": "^5.0.0",
    "material-icons": "^0.3.1",
    "roboto-fontface": "^0.10.0",
    "ts-node": "^8.0.3",
    "tslint": "^5.15.0",
    "typescript": "3.1.6"
  },
  "optionalDependencies": {
    "node-sass": "^4.11.0"
  }
}
```