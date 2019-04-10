# .NET Core Overview  

* [Docs](#docs)
* [Overview](#overview)
* [Dependencies](#dependencies)

## [Docs](#net-core-overview)  

> It is highly recommended that you spend time getting familiar with the official documentation. It is very well written and will serve as the greatest point of reference when running into design questions or issues. See [References - Links](./r1-links.md)

## [Overview](#net-core-overview)  

The back end is written in C# using .NET Core 2.2 and is based on the back end configuration for the `dotnet new angular` template. It is split up between 6 different projects:  

Project | Type | Purpose
--------|------|--------
**dbseeder** | Console | Ensure database is created and data is seeded given an Environment Variable or Connection String.
**{Project}.Core** | Class Library | Defines global <span>ASP.NET</span> Core services and extension methods.
**{Project}.Data** | Class Library | Entity Framework project that defines entities, business logic, database migrations, and the database session interface object `AppDbContext`.
**{Project}.Identity** | Class Library | Provides a means of interfacing with Active Directory using an interface and middleware.
**{Project}.Identity.Mock** | Class Library | Extends `IUserProvider` from the Identity project to allow dev. machines not joined to an Active Directory domain to mock the Identity features.
**{Project}.Web** | Web App | <span>ASP.NET</span> Core SPA application with Angular  

## [Dependencies](#net-core-overview)  

> All of the package references in the app stack are official Microsoft libraries.  

There are two types of dependencies for a .NET Core app. Packages are 3rd party libraries hosted on NuGet. References are links to projects within your own solution. They can be installed from the command line as follows:  

```
{project-url}>dotnet add package {package}
{project-url}>dotnet add reference ..\{project}
```  

Also note that if you create a new project, it needs to be added to the root solution:  

```
{solution-url}>dotnet sln add .\{project}
```  

You can also inspect a project's dependencies by opening the `{project}.csproj` file. Here's an example of `{project}.Data.csproj`:  

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="2.2.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="2.2.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\Qxyz.Core\Qxyz.Core.csproj" />
  <ProjectReference Include="..\Qxyz.Identity\Qxyz.Identity.csproj" />
</ItemGroup>
```  

You can also open the `{Solution}.sln` file to determine which projects have been added:  

```
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 15
VisualStudioVersion = 15.0.26124.0
MinimumVisualStudioVersion = 15.0.26124.0
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Qxyz.Data", "Qxyz.Data\Qxyz.Data.csproj", "{0D866680-7D79-4BF4-A2CF-AFD1A78B0112}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Qxyz.Web", "Qxyz.Web\Qxyz.Web.csproj", "{4660440B-DB59-4EB6-9DD4-7364DA7C5AB6}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Qxyz.Core", "Qxyz.Core\Qxyz.Core.csproj", "{5CB001FB-30E4-44CA-982A-98CE1C040DCA}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Qxyz.Identity", "Qxyz.Identity\Qxyz.Identity.csproj", "{27F932CF-BC67-4C74-941A-779A6E6B6DDF}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "dbseeder", "dbseeder\dbseeder.csproj", "{817428A5-1A2D-4F84-AB52-4AD14A04E212}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Qxyz.Identity.Mock", "Qxyz.Identity.Mock\Qxyz.Identity.Mock.csproj", "{0A67C46E-1632-49EE-A970-61CC4284C423}"
EndProject
```  

### dbseeder  

Package | Version | Type
--------|---------|-----
**Microsoft.EntityFrameworkCore.Relational** | 2.2.3 | Package
**Microsoft.EntityFrameworkCore.SqlServer** | 2.2.3 | Package
**{Project}.Core** | N/A | Reference
**{Project}.Data** | N/A | Reference  

### {Project}.Core  

Package | Version | Type
--------|---------|-----
**Microsoft.AspNetCore.Diagnostics** | 2.2.0 | Package
**Microsoft.AspNetCore.Http** | 2.2.2 | Package
**Microsoft.AspNetCore.Http.Extensions** | 2.2.0 | Package  

### {Project}.Data  

Package | Version | Type
--------|---------|-----
**Microsoft.EntityFrameworkCore.SqlServer** | 2.2.0 | Package
**Microsoft.EntityFrameworkCore.Tools** | 2.2.0 | Package
**{Project}.Core** | N/A | Reference
**{Project}.Identity** | N/A | Reference  

### {Project}.Identity  

Package | Version | Type
--------|---------|-----
**Microsoft.AspNetCore.Http** | 2.2.2 | Package
**Microsoft.Extensions.Configuration.Abstractions** | 2.2.0 | Package
**Microsoft.Extensions.Configuration.Binder** | 2.2.0 | Package
**System.DirectoryServices** | 4.5.0 | Package
**System.DirectoryServices.AccountManagement** | 4.5.0 | Package
**{Project}.Core** | N/A | Reference  

### {Project}.Identity.Mock  

Package | Version | Type
--------|---------|-----
**Microsoft.AspNetCore.Authentication** | 2.2.0 | Package
**Microsoft.AspNetCore.Authentication.Cookies** | 2.2.0 | Package
**Microsoft.AspNetCore.Http** | 2.2.2 | Package
**{Project}.Identity** | N/A | Reference  

### {Project}.Web

Package | Version | Type
--------|---------|-----
**Microsoft.AspNetCore.App** | N/A | Package
**Microsoft.AspNetCore.Razor.Design** | 2.2.0 | Package
**System.DirectoryServices** | 4.5.0 | Package
**System.DirectoryServices.AccountManagement** | 4.5.0 | Package
**{Project}.Core** | N/A | Reference
**{Project}.Data** | N/A | Reference
**{Project}.Identity** | N/A | Reference
**{Project}.Identity.Mock** | N/A | Reference