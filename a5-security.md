# Security

[Table of Contents](./toc.md)

* [Overview](#overview)
* [Authentication](#authentication)
* [Authorization](#authorization)
  * [API Guards](#api-guards)
  * [Route Guards](#route-guards)

## [Overview](#security)

Both ASP.NET Core and Angular are built in such a way that they automatically mitigate concerns surrounding common web app vulnerabilities. This article will primarily be concerned with security from the standpoint of authentication and authorization. Authentication involves determining who a user is, and authorization determines what they are able to access.

> For detailed information on built-in security measures, see:
> * [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/?view=aspnetcore-2.2)
> * [Angular Security](https://angular.io/guide/security)  

The **{Project}.Identity** and **{Project}.Identity.Mock** libraries were introduced in the following articles:

* [Dependency Injection and Middleware - Identity](./05-di-and-middleware.md#identity)
* [Dependency Injection and Middleware - Active Directory Provider](./05-di-and-middleware.md#active-directory-provider) 
* [Dependency Injection and Middleware - Mock Provider](./05-di-and-middleware.md#mock-provider)

If you haven't read through these yet, you should definitely do so as the following section expands on these libraries.

This article will cover the following topics:

* Authentication
  * Configuring Windows Authentication for development and production
  * Tying Identity to the Data Access Layer
  * Business Logic
  * API Controller
  * Angular Service
  * Synchronizing Authentication
* Authorization
  * Web API Controller and Route Guards
  * Angular Route Guards

## [Authentication](#security)



## [Authorization](#security)



### [API Guards](#security)



### [Route Guards](#security)



[Back to Top](#security)