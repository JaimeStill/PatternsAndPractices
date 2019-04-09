# Core Configuration

* [Overview](#overview)
* [Extensions](#extensions)
* [Logging](#logging)

## [Overview](#core-configuration)

The purpose of the **{Project}.Core** project is to define core infrastructure and functionality for the back end of the app stack. This section will outline two key features that illustrate the intent of this project:

* Core Extensions that enable functionality throughout the app stack
* Logging as a demonstration of service configuration for use with Dependency Injection and Middleware`

## [Extensions](#core-configuration)

Extension methods that are defined in **{Project}.Core\\Extensions\\CoreExtensions.cs** do not directly relate to any feature area or entity type. They exist to provide a single location for reusable methods that are helpful throughout the app stack.  

To show what I mean, it would be best to show the contents of the class as it exists in the template, and walk through what the methods are used for.  

**CoreExtensions.cs**  

```cs
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Demo.Core.Extensions
{
    public static class CoreExtensions
    {
        private static readonly string urlPattern = "[^a-zA-Z0-9-.]";

        public static string UrlEncode(this string url)
        {
            var friendlyUrl = Regex.Replace(url, @"\s", "-").ToLower();
            friendlyUrl = Regex.Replace(friendlyUrl, urlPattern, string.Empty);
            return friendlyUrl;
        }

        public static string UrlEncode(this string url, string pattern, string replace = "")
        {
            var friendlyUrl = Regex.Replace(url, @"\s", "-").ToLower();
            friendlyUrl = Regex.Replace(friendlyUrl, pattern, replace);
            return friendlyUrl;
        }

        public static string GetExceptionChain(this Exception ex)
        {
            var message = new StringBuilder(ex.Message);

            if (ex.InnerException != null)
            {
                message.AppendLine();
                message.AppendLine(GetExceptionChain(ex.InnerException));
            }

            return message.ToString();
        }
    }
}
```  

Method | Description
-------|------------
`UrlEncode(this string url)` | Allows a string to be encoded for use as a URL fragment based on the RegEx pattern `[^a-zA-Z0-9-.]`. This specifies that the only characters allowed are **a-z**, **A-Z**, `0-9`, `-`, and the characters `.` and `-`. Any spaces in the provided string are converted to the `-` character. Any additional characters that aren't allowed are omitted.
`UrlEncode(this string url, string pattern, string replace = "")` | An override of the above `UrlEncode` method that allows you to specify a custom RexEx pattern, as well as what to replace invalid characters with.
`GetExceptionChain(this Exception ex)` | Recursively collects all of the exception messages in a from a root exception, and returns the entire stack message as a string.  

There are many places that you may want to use these methods throughout the application, and this extensions class provides a single place to expose their functionality.

## [Logging](#core-configuration)

The **Logging** feature is written to provide a means of capturing exceptions that occur in log files on the server, as well as providing the details of the exceptions that occurred to the calling client. An example of how being able to communicate these exceptions to the client is useful is demonstrated in the [Business Logic - Validation](./03-business-logic.md#validation) section.  

> The primary infrastructure of this feature will be outline here, then the configuration and registration will be discussed in the following [Dependency Injection and Middleware](./05-di-and-middleware.md) section.  

To facilitate the desired functionality of the `LogProvider` class, extension methods need to be written against the core objects that the provider needs to interact with. All of these extensions are written in the **{Project}.Core\\Extensions\\LogExtensions.cs** static class.  

A useful bit of information to provide to the log file would be the state of the `HttpContext` at the time the exception is thrown. This can be extracted using the following extension method:  

```cs
public static async Task<string> GetContextDetails(this HttpContext context)
{
    var message = new StringBuilder();
    message.AppendLine($"User: {context.User.Identity.Name}");
    message.AppendLine($"Local IP: {context.Connection.LocalIpAddress}");
    message.AppendLine($"Local Port: {context.Connection.LocalPort}");
    message.AppendLine($"Remote IP: {context.Connection.RemoteIpAddress}");
    message.AppendLine($"Remote Port: {context.Connection.RemotePort}");
    message.AppendLine($"Content Type: {context.Request.ContentType}");
    message.AppendLine($"URL: {context.Request.GetDisplayUrl()}");
            
    if (context.Request.Headers.Count > 0)
    {
        message.AppendLine();
        message.AppendLine("Headers");
                
        foreach (var h in context.Request.Headers)
        {
            message.AppendLine($"{h.Key} : {h.Value}");
        }
    }

    context.Request.EnableBuffering();

    if (context.Request.Body.Length > 0)
    {
        message.AppendLine();
        message.AppendLine("Body");
                
        var body = await context.Request.ReadFormAsync();
                
        foreach (var k in body.Keys)
        {
            StringValues values;
            body.TryGetValue(k.ToString(), out values);
                    
            if (values.Count > 0)
            {
                foreach (var v in values)
                {
                    message.AppendLine($"{k.ToString()} : {v.ToString()}");
                }
            }
        }
    }
            
    return message.ToString();
}
```  

Another necessary feature is being able to write the details of the log message to the file system of the web server. This can be accomplished with the following extension method:  

```cs
public static async Task WriteLog(this StringBuilder message, string path)
{
    using (var stream = new StreamWriter(path))
    {
        await stream.WriteAsync(message.ToString());
    }
}
```  

Finally, we want to be able to forward the recursive exception message to the calling client so that they are able to detect when an error occurs. This is written as middleware to be placed in the middleware pipeline (the implementation of which will be captured in the [Dependency Injection and Middleware](./05-di-and-middleware.md) section):  

```cs
public static void HandleError(this IApplicationBuilder app, LogProvider logger)
{
    app.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>();

     
        // Log the error to the server using the original context
        if (error != null)
        {
            var ex = error.Error;
            await logger.CreateLog(context, ex);
        }

        // Update the context to reflect the error
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        // Send the error response back to the client
        if (error != null)
        {
            var ex = error.Error;
            await context.Response.WriteAsync(ex.GetExceptionChain(), Encoding.UTF8);
        }
    });
}
```  

With these extensions in place, the `LogProvider` class can be written:  

```cs
using Microsoft.AspNetCore.Http;
using Demo.Core.Extensions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Core.Logging
{
    public class LogProvider
    {
        public string LogDirectory { get; set; }
        public string GetLogName() => $"log-{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}.txt";
        
        public async Task CreateLog(HttpContext context, Exception exception)
        {
            var builder = new StringBuilder();
            builder.AppendLine("ContextDetails");
            builder.AppendLine();
            builder.AppendLine(await context.GetContextDetails());
            builder.AppendLine("Exception Details");
            builder.AppendLine(exception.GetExceptionChain());
            
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
            
            await builder.WriteLog($@"{LogDirectory}\{GetLogName()}");
        }
    }
}
```  

Member | Description
-------|------------
`LogDirectory` | The directory the logs will be written to
`GetLogName()` | Generates a log name based on the current date time
`CreateLog()` | Generates the details of the log, and writes it to the directory  

There is a bit more going on in the **{Project}.Core** project, but this provides the details necessary to understand how the rest works.  

> With the exception of the **{Project}.Core\\Sockets** features, which will be discussed in the [SignalR](./a7-signalr.md) section.