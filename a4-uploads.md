# Uploads

[Table of Contents](./toc.md)

* [Overview](#overview)
* [.NET Core](#net-core)
  * [Configuration](#configuration)
  * [Data Layer](#data-layer)
  * [Business Logic](#business-logic)
  * [Controller](#controller)
* [Angular](#angular)
  * [Model](#model)
  * [Service](#service)
  * [Pipe](#pipe)
  * [Components](#components)
  * [Dialogs](#dialogs)
  * [Routes](#routes)
* [Related Data](#related-data)
  * [Folders Back End](#folders-back-end)
  * [Folders Front End](#folders-front-end)

## [Overview](#uploads)

The [File API](https://developer.mozilla.org/en-US/docs/Web/API/File/Using_files_from_web_applications) defines a mechanism for allowing users to upload binary file data. This article will discuss setting up .NET Core to manage these uploads, building a workflow in Angular, and tying uploads to related data via Entity Framework.

> A full example of the content shown in this article can be found in [/examples/a4-uploads](./examples/a4-uploads)

## [.NET Core](#uploads)

### [Configuration](#uploads)

Before building out entities or API endpoints, it's important to understand how uploaded files will be stored and accessed. Rather than storing files as binary data in the database, they will be stored at a location that can be referenced by a SQL record. This will keep the database from growing unnecessarily huge, and reduce the amount of time needed to retrieve files.

To make the location of the file storage configurable, an `UploadConfig` class is created that specifies a `DirectoryBasePath` as well as a `UrlBasePath`. The `DirectoryBasePath` refers to the physical location where files will be stored, and the `UrlBasePath` refers to the root URL the files can be reference from in the app.

**`UploadConfig.cs`**

```cs
namespace {Project}.Core.Upload
{
    public class UploadConfig
    {
        public string DirectoryBasePath { get; set; }
        public string UrlBasePath { get; set; }
    }
}
```

During development, files will be stored in the `{Project}.Web/wwwroot/files/` directory. In any other environment, the directory and URL paths will be specified via `AppDirectoryBasePath` and `AppUrlBasePath` environment variables. Here is the relevant `Startup.cs` configuration for registering an `UploadConfig` instance with the dependency injection container:

**`Startup.cs`**

```cs
public class Startup
{
    private void SetupDevelopmentDirectories(IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            if (!Directory.Exists($@"{env.WebRootPath}/files"))
            {
                Directory.CreateDirectory($@"{env.WebRootPath}/files");
            }
        }
    }

    public Startup(IConfiguration configuration, IHostingEnvironment environment)
    {
        SetupDevelopmentDirectories(environment);
        // additional constructor logic
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // preceding service registrations

        if (Environment.IsDevelopment())
        {
            services.AddSingleton(new UploadConfig
            {
                DirectoryBasePath = $@"{Environment.WebRootPath}/files/",
                UrlBasePath = "/files/"
            });
        }
        else
        {
            services.AddSingleton(new UploadConfig
            {
                DirectoryBasePath = Configuration.GetValue<string>("AppDirectoryBasePath"),
                UrlBasePath = Configuration.GetValue<string>("AppUrlBasePath")
            });
        }

        // additional service registrations
    }
}
```  

The `SetupDevelopmentDirectories()` method that is defined, then called in the `Startup` constructor, ensures that the path specified when registering `UploadConfig` exists before it is registered.  

`UploadConfig` is then registered as a **Singleton** in `ConfigureServices`. If in the **Development** environment, the path will default to `{Project}.Web/wwwroot/files/`. Otherwise, the path will be retrieved from the `AppDirectoryBasePath` and `AppUrlBasePath` environment variables.

### [Data Layer](#uploads)

File uploads will be tracked in the database as an instance of an `Upload` entity, which is defined as follows:

**`Upload.cs`**

```cs
namespace UploadDemo.Data.Entities
{
    public class Upload
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public string File { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public long Size { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
```

Property | Description
---------|------------
`Url` | Specifies the URL the file can be served from in the application
`Path` | Specifies the fully qualified, physical location the file is located at
`File` | The original file name, including the file extension
`Name` | The URL-encoded name, including the file extension
`FileType` | The [MIME type](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types)
`Size` | The size, in bytes
`UploadDate` | Date the file is uploaded
`IsDeleted` | A flag indicating whether or not an upload has been soft deleted

Add `Upload` to an `AppDbContext` class:

**`AppDbContext.cs`**

``` cs
namespace UploadDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<Upload> Uploads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Model
                .GetEntityTypes()
                .ToList()
                .ForEach(x =>
                {
                    modelBuilder
                        .Entity(x.Name)
                        .ToTable(x.Name.Split('.').Last());
                });
        }
    }
}
```

Register `AppDbContext` with the `Startup` class:

**`Startup.cs`**

``` cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default"));
}
```

> Make sure to generate a migration and update the database as outlined in the [Data Access Layer - Database Management Workflow](./02-data-access-layer.md#database-management-workflow) article

### [Business Logic](#uploads)  

Getting uploads works the same as any other retrieval methods that have been shown at this point. For completeness, they will be shown with the rest of the methods. It is the create and delete methods that require some extra work.

**`UploadExtensions.cs`**

```cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UploadDemo.Core.Extensions;
using UploadDemo.Data.Entities;

namespace UploadDemo.Data.Extensions
{
    public static class UploadExtensions
    {
        public static async Task<List<Upload>> GetUploads(this AppDbContext db, bool isDeleted = false)
        {
            var uploads = await db.Uploads
                .Where(x => x.IsDeleted == isDeleted)
                .OrderByDescending(x => x.UploadDate)
                .ToListAsync();

            return uploads;
        }

        public static async Task<List<Upload>> SearchUploads(this AppDbContext db, string search, bool isDeleted = false)
        {
            search = search.ToLower();
            var uploads = await db.Uploads
                .Where(x => x.IsDeleted == isDeleted)
                .Where(x => x.File.ToLower().Contains(search))
                .OrderByDescending(x => x.UploadDate)
                .ToListAsync();

            return uploads;
        }

        public static async Task<Upload> GetUpload(this AppDbContext db, int uploadId) => 
            await db.Uploads
                .SetUploadIncludes()
                .FirstOrDefaultAsync(x => x.Id == uploadId);

        public static async Task<Upload> GetUploadByName(this AppDbContext db, string file) => 
            await db.Uploads
                .SetUploadIncludes()
                .FirstOrDefaultAsync(x => x.File.ToLower() == file.ToLower());

        public static async Task<List<Upload>> UploadFiles(this AppDbContext db, IFormFileCollection files, string path, string url)
        {
            if (files.Count < 1)
            {
                throw new Exception("No files provided for upload");
            }

            List<Upload> uploads = new List<Upload>();

            foreach (var file in files)
            {
                uploads.Add(await db.AddUpload(file, path, url));
            }

            return uploads;
        }

        public static async Task ToggleUploadDeleted(this AppDbContext db, Upload upload)
        {
            db.Uploads.Attach(upload);
            upload.IsDeleted = !upload.IsDeleted;
            await db.SaveChangesAsync();
        }

        public static async Task RemoveUpload(this AppDbContext db, Upload upload)
        {
            await upload.DeleteFile();
            db.Uploads.Remove(upload);
            await db.SaveChangesAsync();
        }

        static async Task<Upload> AddUpload(this AppDbContext db, IFormFile file, string path, string url)
        {
            var upload = await file.WriteFile(path, url);
            upload.UploadDate = DateTime.Now;
            await db.Uploads.AddAsync(upload);
            await db.SaveChangesAsync();
            return upload;
        }

        static async Task<Upload> WriteFile(this IFormFile file, string path, string url)
        {
            if (!(Directory.Exists(path)))
            {
                Directory.CreateDirectory(path);
            }

            var upload = await file.CreateUpload(path, url);

            using (var stream = new FileStream(upload.Path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return upload;
        }

        static Task<Upload> CreateUpload(this IFormFile file, string path, string url) => Task.Run(() =>
        {
            var f = file.CreateSafeName(path);

            var upload = new Upload
            {
                File = f,
                Name = file.Name,
                Path = $"{path}{f}",
                Url = $"{url}{f}",
                FileType = file.ContentType,
                Size = file.Length
            };

            return upload;
        });

        static string CreateSafeName(this IFormFile file, string path)
        {
            var increment = 0;
            var fileName = file.FileName.UrlEncode();
            var newName = fileName;

            while (File.Exists(path + newName))
            {
                var extension = fileName.Split('.').Last();
                newName = $"{fileName.Replace($".{extension}", "")}_{++increment}.{extension}";
            }

            return newName;
        }

        static Task DeleteFile(this Upload upload) => Task.Run(() =>
        {
            try
            {
                if (File.Exists(upload.Path))
                {
                    File.Delete(upload.Path);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetExceptionChain());
            }
        });
    }
}
```  

> The following will only describe the `UploadFiles` and `RemoveUpload` methods, as they pertain to the additional functionality inherent in file uploads. If the remaining methods are unfamiliar, refer to the [Business Logic](./03-business-logic.md) article.

We'll start with the `RemoveUpload` method, as it is substantially less complicated. 

* Call the private `DeleteFile` extension method
    * If the file specified at `Upload.Path` exists, delete the physical file
* Remove the associated `Upload` record from the database
* Save changes to the database

The `UploadFiles` extension method triggers a chain of defined method calls, each with a specific purpose. The following section will start with the deepest method, and work back up to `UploadFiles` so that you can see how the entire interaction occurs.

Method | Description
-------|------------
`CreateSafeName` | Extends on `IFormFile file` and accepts a `string path` argument. If the name of the provided file already exists at the specified path, a safe name is generated by adding an incremented number to the end of the name until a matching file is not found
`CreateUpload` | Extends on `IFormFile file` and accepts `string path` and `string url` arguments. Generates a safe name by calling the above `CreateSafeName` method. Generates an instance of the `Upload` class based on the safe name and properties of `file`, as well as the provided `path` and `url` arguments. The `Upload` instance is then returned.
`WriteFile` | Extends on `IFormFile file` and accepts `string path` and `string url` arguments. Ensures that the directory specified by the `path` argument exists. Then calls the above `CreateUpload` method to generate an `Upload` instance. The file is then written to the `path` specified using the `CopyToAsync` method of `IFormFile`, and the `Upload` instance is returned.
`AddUpload` | Extends on `AppDbContext db` and accepts `IFormFile file`, `string path`, and `string url` arguments. Calls the above `WriteFile` method to generate an instance of `Upload` and write the file contained by `file` to the specified `path`. The `UploadDate` property is set to `DateTime.Now`, and the `Upload` instance is added to the database. The changes are saved, and the `Upload` instance is returned.

The public `UploadFiles` method extends on `AppDbContext db` and accepts `IFormFileCollection files`, `string path`, and `string url` as arguments. If the amount of files provided to the method is less than one, an exception is thrown indicating that no files were provided.

Each file in the `IFormFileCollection` is iterated through and added to a `List<Upload> uploads` collection by calling the `AddUpload` method specified above. The resulting list of uploads is then returned to the caller.

### [Controller](#uploads)

Here is the controller that maps to the public methods specified for the `Upload` entity:

**`UploadController.cs`**

```cs
namespace UploadDemo.Web.Controllers
{
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        private AppDbContext db;
        private UploadConfig config;

        public UploadController(AppDbContext db, UploadConfig config)
        {
            this.db = db;
            this.config = config;
        }

        [HttpGet("[action]")]
        public async Task<List<Upload>> GetUploads() => await db.GetUploads();

        [HttpGet("[action]")]
        public async Task<List<Upload>> GetDeletedUploads() => await db.GetUploads(true);

        [HttpGet("[action]/{search}")]
        public async Task<List<Upload>> SearchUploads([FromRoute]string search) => await db.SearchUploads(search);

        [HttpGet("[action]/{search}")]
        public async Task<List<Upload>> SearchDeletedUploads([FromRoute]string search) => await db.SearchUploads(search, true);

        [HttpGet("[action]/{id}")]
        public async Task<Upload> GetUpload([FromRoute]int id) => await db.GetUpload(id);

        [HttpGet("[action]/{file}")]
        public async Task<Upload> GetUploadByName([FromRoute]string file) => await db.GetUploadByName(file);

        [HttpPost("[action]")]
        [DisableRequestSizeLimit]
        public async Task<List<Upload>> UploadFiles() =>
            await db.UploadFiles(
                Request.Form.Files,
                config.DirectoryBasePath,
                config.UrlBasePath
            );

        [HttpPost("[action]")]
        public async Task ToggleUploadDeleted([FromBody]Upload upload) => await db.ToggleUploadDeleted(upload);

        [HttpPost("[action]")]
        public async Task RemoveUpload([FromBody]Upload upload) => await db.RemoveUpload(upload);
    }
}
```

> With the exception of `UploadFiles`, this should all look familiar. If not, refer to the [Web API](./06-web-api.md) article.

By default, <span>ASP.NET</span> Core enforces a [**30MB** request size limit](https://github.com/aspnet/Announcements/issues/267) for HTTP requests. You can modify this default by either specifying a value using the [RequestSizeLimit](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.requestsizelimitattribute?view=aspnetcore-2.2) attribute on an endpoint, or bypassing limits completely by specifying the [DisableRequestSizeLimit](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.disablerequestsizelimitattribute?view=aspnetcore-2.2) attribute on an endpoint. In this case, `UploadFiles` bypasses the limit completely by specifying `DisableRequestSize`.

The `UploadFiles` API endpoint maps to the `UploadFiles` method defined above. `IFormFileCollection` is extracted from the body of the request at `Request.Form.Files` as the first argument. The `DirectoryBasePath` and `UrlBasePath` properties of the injected `UploadConfig` object are passed as the second and third arguments respectively.

## [Angular](#uploads)

Now that .NET Core is setup to handle file uploads on the back-end, it's time to see how to bring it all together by actually provided file uploads from Angular.

### [Model](#uploads)  

An `Upload` TypeScript class is added that defines the shape of the `Upload` data defined by the back-end.

**`Upload.ts`**

```ts
export class Upload {
  id: number;
  url: string;
  path: string;
  file: string;
  name: string;
  fileType: string;
  size: number;
  uploadDate: Date;
  isDeleted: boolean;
}
```

Make sure to register the `Upload` class with the `models` TypeScript module:

**`index.ts`**

```ts
export * from './banner-config';
export * from './theme';
export * from './upload';
```

### [Service](#uploads)

Before defining the `UploadService`, it's important to note that the `CoreService` defined by the template contains a `getUploadOptions()` function:

**`core.service.ts` - `getUploadOptions()`**  

```ts
getUploadOptions = (): HttpHeaders => {
  const headers = new HttpHeaders();
  headers.set('Accept', 'application/json');
  headers.delete('Content-Type');
  return headers;
}
```  

This appropriately conditions the headers for a `POST` request to send `FormData` to an API endpoint in .NET Core.

Using the standard convention of mapping API endpoints to an Angular service, an `UploadService` is defined:

**`upload.service.ts`**

```ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { CoreService } from './core.service';
import { SnackerService } from './snacker.service';
import { Upload } from '../models';


@Injectable()
export class UploadService {
  private uploads = new Subject<Upload[]>();
  private upload = new Subject<Upload>();

  uploads$ = this.uploads.asObservable();
  upload$ = this.upload.asObservable();

  constructor(
    private core: CoreService,
    private http: HttpClient,
    private snacker: SnackerService
  ) { }

  getUploads = () => this.http.get<Upload[]>('/api/upload/getUploads')
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  getDeletedUploads = () => this.http.get<Upload[]>('/api/upload/getDeletedUploads')
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  searchUploads = (search: string) => this.http.get<Upload[]>(`/api/upload/searchUploads/${search}`)
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  searchDeletedUploads = (search: string) => this.http.get<Upload[]>(`/api/upload/getDeletedUploads/${search}`)
    .subscribe(
      data => this.uploads.next(data),
      err => this.snacker.sendErrorMessage(err.error)
    );

  getUpload = (id: number): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.get<Upload>(`/api/upload/getUpload/${id}`)
      .subscribe(
        data => {
          this.upload.next(data);
          resolve(true);
        },
        err => {
          this.snacker.sendErrorMessage(err.error);
          resolve(false);
        }
      )
    });

  getUploadByName = (file: string): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.get<Upload>(`/api/upload/getUploadByName/${file}`)
        .subscribe(
        data => {
          this.upload.next(data);
          resolve(true);
        },
        err => {
          this.snacker.sendErrorMessage(err.error);
          resolve(false);
        }
      )
    });

  uploadFiles = (formData: FormData): Promise<Upload[]> =>
    new Promise((resolve) => {
      this.http.post<Upload[]>('/api/upload/uploadFiles', formData, { headers: this.core.getUploadOptions() })
        .subscribe(
          res => {
            this.snacker.sendSuccessMessage('Uploads successfully processed');
            resolve(res);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(null);
          }
        )
    });

  toggleUploadDeleted = (upload: Upload): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/upload/toggleUploadDeleted', upload)
        .subscribe(
          () => {
            const message = upload.isDeleted ?
              `${upload.file} successfully restored` :
              `${upload.file} successfully deleted`;

            this.snacker.sendSuccessMessage(message);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });

  removeUpload = (upload: Upload): Promise<boolean> =>
    new Promise((resolve) => {
      this.http.post('/api/upload/removeUpload', upload)
        .subscribe(
          () => {
            this.snacker.sendSuccessMessage(`${upload.file} permanently deleted`);
            resolve(true);
          },
          err => {
            this.snacker.sendErrorMessage(err.error);
            resolve(false);
          }
        )
    });
}
```  

> If you're unfamiliar with Angular Services, see the [Services](./13-services.md) article.  

The `uploadFiles` function receives a `formData: FormData` argument and returns a `Promise<Upload[]>`. An `HttpClient.post` function is called, providing `formData` in the body of the request, and setting the request headers by calling `CoreService.getUploadOptions()`. When the result returns, the user is alerted, and the results are resolved in the `Promise<Upload[]>` returned by the function. If an error occurs, the error is alerted to the user and `null` is resolved by the returned Promise.

Make sure to register the `UploadService` with the `services` TypeScript module:

**`index.ts`**

```ts
import { BannerService } from './banner.service';
import { CoreService } from './core.service';
import { ObjectMapService } from './object-map.service';
import { SnackerService } from './snacker.service';
import { ThemeService } from './theme.service';

export const Services = [
  BannerService,
  CoreService,
  ObjectMapService,
  SnackerService,
  ThemeService
];

export * from './banner.service';
export * from './core.service';
export * from './object-map.service';
export * from './snacker.service';
export * from './theme.service';
export * from './upload.service';

```

### [Pipe](#uploads)

The `size` property of the `Upload` class is provided in bytes. In order to provide better labeling for file sizes, a `BytesPipe` is created:

**`bytes.pipe.ts`**

```ts
import {
  Pipe,
  PipeTransform
} from '@angular/core';

@Pipe({
  name: 'bytes'
})
export class BytesPipe implements PipeTransform {
  transform(value: number, precision = 2) {
    if (!value || value === 0) return '0 Bytes';
    const k = 1024,
          dm = precision <= 0 ? 0 : precision || 2,
          sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'],
          i = Math.floor(Math.log(value) / Math.log(k));

    return parseFloat((value / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }
}
```

The `transform` function takes the provided value, and uses the floor of the logarithmic relationship between `k` (**1024**) and `value` to determine its logarithmic base. This exponential relationship of `k` and the logarithmic base is then divided by `value` to determine the natural size, along with an identifier for the size of the value represented, to the precision specified by the `precision` argument (defaults to **2**), and is returned.

> Math documentation:
> * [Math.floor](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math/floor)
> * [Math.log](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math/log)
> * [Math.pow](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math/pow)

Here's an example that demonstrates each step in pure JavaScript:

```js
const value = 23949380;
const k = 1024;
const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
const i = Math.floor(Math.log(k) / Math.log(value));
console.log('base', i); // "base" 2

const result = parseFloat((value / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
console.log('result', result); // "result" "22.84 MB"
```

Make sure to register the `BytesPipe` with the `pipes` TypeScript module:

**`index.ts`**  

```ts
import { BytesPipe } from './bytes.pipe';
import { TruncatePipe } from './truncate.pipe';

export const Pipes = [
  BytesPipe,
  TruncatePipe
];
```

### [Components](#uploads)

> To re-iterate the usefulness of the `FileUploadComponent` demonstrated in the [Display Components - FileUploadComponent](./16-display-components.md#fileuploadcomponent) article, it is defined here again so you can see how it is used in the context of file uploads. Refer to that section of the article for a detailed description of the component.

**`file-upload.component.css`**

```css
input[type=file] {
  display: none;
}
```

**`file-upload.component.html`**

```html
<input type="file"
       (change)="fileChange($event)"
       #fileInput
       [accept]="accept"
       [multiple]="multiple">
<button mat-button
        [color]="color"
        (click)="fileInput.click()">{{label}}</button>
```

**`file-upload.component.ts`**

```ts
import {
  Component,
  EventEmitter,
  Input,
  Output,
  ViewChild,
  ElementRef
} from '@angular/core';

@Component({
  selector: 'file-upload',
  templateUrl: 'file-upload.component.html',
  styleUrls: ['file-upload.component.css']
})
export class FileUploadComponent {
  @ViewChild('fileInput') fileInput: ElementRef;
  @Input() accept = '*/*';
  @Input() color = 'primary';
  @Input() label = 'Browse...';
  @Input() multiple = true;
  @Output() selected = new EventEmitter<[File[], FormData]>();

  fileChange = (event: any) => {
    const files: FileList = event.target.files;
    const fileList = new Array<File>();
    const formData = new FormData();

    for (let i = 0; i < files.length; i++) {
      formData.append(files.item(i).name, files.item(i));
      fileList.push(files.item(i));
    }

    this.selected.emit([fileList, formData]);
    this.fileInput.nativeElement.value = null;
  }
}
```  

The most important part of the `FileUploadComponent` in this context is that it allows users to select files, and returns these selected files as both an `Array<File>` for display purposes, and `FormData` with the files appended to it for passing to the `UploadService.uploadFiles` function.

A `FileListComponent` is created for being able to consistently render an `Array<File>` once files have been selected.

**`file-list.component.ts`**

```ts
import {
  Component,
  Input
} from '@angular/core';

@Component({
  selector: 'file-list',
  templateUrl: 'file-list.component.html'
})
export class FileListComponent {
  @Input() files: File[];
  @Input() layout = "row | wrap";
  @Input() align = "start start"
  @Input() elevated = true;
}
```  

In addition to the `File[]` input property, the `layout`, `align`, and `elevated` input properties can be provided to determine how the file list will be rendered.

**`file-list.component.html`**

```html
<p class="mat-title">Pending Uploads</p>
<section class="container"
         [fxLayout]="layout"
         [fxLayoutAlign]="align">
  <section *ngFor="let f of files"
           class="background card container"
           [class.static-elevation]="elevated">
    <p class="mat-subheading-2">{{f.name}}</p>
    <p>{{f.lastModified | date:'dd MMM yyyy HH:mm'}}</p>
    <p>{{f.type}}</p>
  </section>
</section>
```

To prevent from having to continually define search functionality in route components, a `SearchbarComponent` is created to setup the RxJS-based search pattern demonstrated in [Components - RxJS fromEvent with ViewChild](./14-components.md#rxjs-fromevent-with-viewchild).

**`searchbar.component.html`**

```html
<section fxLayout="column"
         fxLayoutAlign="start stretch">
  <mat-form-field>
    <mat-label>{{label}}</mat-label>
    <input #searchbar
           matInput>
  </mat-form-field>
</section>
```

**`searchbar.component.ts`**

```ts
import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  ElementRef,
  OnDestroy,
  ViewEncapsulation
} from '@angular/core';

import { Subscription } from 'rxjs';
import { CoreService } from '../../services';

@Component({
  selector: 'searchbar',
  templateUrl: 'searchbar.component.html'
})
export class SearchbarComponent implements OnDestroy {
  sub: Subscription;

  @Input() label = "Search";
  @Input() minimum: number = 2;
  @Output() search = new EventEmitter<string>();
  @Output() clear = new EventEmitter();

  constructor(
    private core: CoreService
  ) { }

  @ViewChild('searchbar')
  set searchbar (input: ElementRef) {
    if (input) {
      this.sub = this.core.generateInputObservable(input)
        .subscribe((val: string) => {
          val && val.length >= this.minimum ?
            this.search.emit(val) :
            this.clear.emit();
        });
    }
  }

  ngOnDestroy() {
    this.core.unsubscribe(this.sub);
  }
}
```

Rather than executing a service function as a result of input being typed into the `searchbar` input, it either emits a `search` or `clear` event, based the length of the value received through the subscrition compared to the value specified by the `minimum` input property (which defaults to **2**).

Because we have access to the `Upload.fileType`, the card that is used to render an `Upload` can be dynamically configured to render the content of the `Upload` for certain file types. This way, we can render all `Upload` items using a single card component, but have them render according to their content type.

**`upload-card.component.ts`**

```ts
import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit
} from '@angular/core';

import { Upload } from '../../models';

@Component({
  selector: 'upload-card',
  templateUrl: 'upload-card.component.html'
})
export class UploadCardComponent implements OnInit {
  expandable: boolean;
  filetype: string;
  @Input() expanded = false;
  @Input() clickable = true;
  @Input() upload: Upload;
  @Input() size = 600;
  @Output() select = new EventEmitter<Upload>();
  @Output() delete = new EventEmitter<Upload>();

  toggleExpanded = () => this.expanded = !this.expanded;

  ngOnInit() {
    this.filetype = this.upload.fileType.split('/')[0];

    switch (this.filetype) {
      case 'image':
      case 'audio':
      case 'video':
        this.expandable = true;
        break;
      default:
        this.expandable = false;
        this.expanded = false;
    }
  }
}
```  

Here is a description of the properties for this component:

Property | Type | Scope | Description
---------|------|-------|------------
`expandable` | `boolean` | local | Determines whether or not the card's content can be expanded for display
`filetype` | `string` | local | Represents the sub-type of the MIME type specified by `upload.fileType`
`expanded` | `boolean` | input | Whether or not the card's default state is expanded
`clickable` | `boolean` | input | Whether or not the title of the card should have the `.clickable` class applied
`upload` | `Upload` | input | The `Upload` instance the card represents
`size` | `number` | input | The width the card should be rendered at
`select` | `EventEmitter<Upload>` | output | Provides the `upload` property to indicate that it has been selected
`delete` | `EventEmitter<Upload>` | output | Provides the `upload` property to indicate that it has been deleted

The `toggleExpanded()` function simply toggles the state of the `expanded` property.

In the **OnInit** lifecycle hook, the `filetype` property is set based on the value of `upload.fileType`. Then, based on the content type represented by the value of `filetype`, the `expandable` property is set.

**`upload-card.component.html`**

```html
<section class="background card elevated"
         fxLayout="column"
         fxLayoutAlign="start stretch"
         [style.width.px]="size">
  <section class="container"
           fxLayout="row"
           fxLayoutAlign="start center">
    <p class="mat-title"
       [class.clickable]="clickable"
       fxFlex
       (click)="clickable && select.emit(upload)">{{upload.file}}</p>
    <button mat-icon-button
            color="warn"
            matTooltip="Delete"
            (click)="delete.emit(upload)">
      <mat-icon>delete</mat-icon>
    </button>
    <a mat-icon-button
       target="_blank"
       matTooltip="Download"
       [href]="upload.url">
      <mat-icon>save_alt</mat-icon>
    </a>
    <button *ngIf="expandable"
            mat-icon-button
            [matTooltip]="expanded ? 'Collapse' : 'Expand'"
            (click)="toggleExpanded()">
      <mat-icon *ngIf="expanded">keyboard_arrow_down</mat-icon>
      <mat-icon *ngIf="!(expanded)">keyboard_arrow_right</mat-icon>
    </button>
  </section>
  <section *ngIf="expanded">
    <ng-container [ngSwitch]="upload.fileType.split('/')[0]">
      <img *ngSwitchCase="'image'"
           [src]="upload.url"
           [alt]="upload.file"
           [width]="size">
      <section *ngSwitchCase="'audio'"
               class="container"
               fxLayout="column"
               fxLayoutAlign="center center">
        <audio controls
               [src]="upload.url"
               [style.width.px]="size - 20"></audio>
      </section>
      <video *ngSwitchCase="'video'"
             controls
             [width]="size">
        <source [src]="upload.url"
                [type]="upload.fileType">
        Sorry, this format isn't supported in your browser
      </video>
    </ng-container>
  </section>
  <mat-divider *ngIf="!(expanded)"></mat-divider>
  <section fxLayout="column"
           fxLayoutAlign="start center"
           class="container"
           [style.margin.px]="8">
    <mat-chip-list selectable="false">
      <mat-chip [matTooltip]="upload.fileType">{{upload.fileType | truncate:'15'}}</mat-chip>
      <mat-chip>{{upload.size | bytes}}</mat-chip>
      <mat-chip>{{upload.uploadDate | date:'dd MMM yyyy HH:mm'}}</mat-chip>
    </mat-chip-list>
  </section>
</section>
```  

The template for the card is represented by four sections:

* The base card
* The title bar
* The content region
* The upload metadata

**Base Card**  

This section of the template is represented by the root `<section>` element. The width of the card is set based on the `size` property.

**Title Bar**

This section of the template is represented by the first `<section>` element inside of the root `<section>`. It uses a row layout to render its content. The first item rendered is the title, represented by `upload.file`. If the `clickable` property is true, clicking the title will call `select.emit(upload)`, and apply the `.clickable` class to the title. The second item rendered is a delete button. If clicked, `delete.emit(upload)` is called. The third item rendered is a link button that resolves to `upload.url` in a new tab. If `expandable` is true, a toggle button is rendered which calls `toggleExpanded()` when clicked.

**Content Region**

This section of the template is represented by the second `<section>` element inside of the root `<section>`. If the `filetype` property is any of the following, the card is `expandable` and renders the content specified:

* **image** - An `<img>` element is rendered with the `src` pointed to `upload.url`. The width of the image is determined by the `size` property, and the `alt` text for the image is specified as `upload.file`.
* **audio** - An `<audio>` element is rendered with controls, and the `src` points to `upload.url`. The width of the audio controls are determined by the `size` property minus **20px**.
* **video** - A `<video>` element is rendered with controls, and the width is determined by the `size` property. Inside, a `<source>` element is defined specifying `src` as `upload.url`, and the `type` as `upload.fileType`. If the browser doesn't support this format, a message will be displayed in place of the video.

[NgSwitch](https://angular.io/guide/structural-directives#inside-ngswitch-directives) is used to determine what to render inside of the content region.

If the `expanded` property is false, a `MatDivider` is rendered in place of the content region.

**Upload Metadata**  

This section of the template is represented by the third `<section>` element inside of the root `<section>`. It uses a `MatChipList` to render the additional properties of the `Upload` the card represents. Not that the `upload.size` chip uses the `bytes` pipe to appropriately display the file size.

Make sure to register each component with the `components` TypeScript module:

**`index.ts`**

```ts
import { BannerComponent } from './banner/banner.component';
import { FileListComponent } from './file-upload/file-list.component';
import { FileUploadComponent } from './file-upload/file-upload.component';
import { SearchbarComponent } from './searchbar/searchbar.component';
import { UploadCardComponent } from './upload/upload-card.component';

export const Components = [
  BannerComponent,
  FileListComponent,
  FileUploadComponent,
  SearchbarComponent,
  UploadCardComponent
];

```

### [Dialogs](#uploads)

Because uploads can be soft-deleted, it's important to have a way to easily access deleted uploads to either restore them, or permanently remove them. This is accomplished with an `UploadBinDialog` component.

> If the term dialog is unfamiliar to you, refer to the [Dialogs](./a3-dialogs.md) article.

**`upload-bin.dialog.ts`**

```ts
import {
  Component,
  OnInit
} from '@angular/core';

import { UploadService } from '../../services';
import { Upload } from '../../models';

@Component({
  selector: 'upload-bin-dialog',
  templateUrl: 'upload-bin.dialog.html',
  providers: [UploadService]
})
export class UploadBinDialog implements OnInit {
  constructor(
    public service: UploadService
  ) { }

  ngOnInit() {
    this.service.getDeletedUploads();
  }

  restoreUpload = async (upload: Upload) => {
    const res = await this.service.toggleUploadDeleted(upload);
    res && this.service.getDeletedUploads();
  }

  removeUpload = async (upload: Upload) => {
    const res = await this.service.removeUpload(upload);
    res && this.service.getDeletedUploads();
  }
}
```  

The `UploadBinDialog` component registers the `UploadService` with its providers array. In the **OnInit** lifecycle hook, it retrieves all of the deleted uploads. 

The `restoreUpload()` function receives an `Upload` from the template, and passes it to the `UploadService.toggleUploadDeleted()` function, effectively restoring it. If this completes successfully, the deleted uploads are refreshed.

The `removeUpload()` function receives an `Upload` from the template, and passes it to the `UploadService.removeUpload()` function, permanently removing both the file and database record. If this completes successfully, the deleted uploads are refreshed.

**`upload-bin.dialog.html`**

```html
<div class="mat-typography">
  <h2 mat-dialog-title>Upload Bin</h2>
  <mat-dialog-content>
    <ng-template #loading>
      <p class="mat-title">Loading Upload Bin</p>
      <mat-progress-bar mode="indeterminate"
                        color="accent"></mat-progress-bar>
    </ng-template>
    <ng-container *ngIf="service.uploads$ | async as uploads else loading">
      <section *ngIf="uploads.length > 0"
               fxLayout="column"
               fxLayoutAlign="start stretch"
               class="container">
        <section *ngFor="let u of uploads"
                 class="background card elevated"
                 fxLayout="column"
                 fxLayoutAlign="start stretch">
          <section fxLayout="row"
                   fxLayoutAlign="start center"
                   class="container">
            <p class="mat-title"
               fxFlex>{{u.file}}</p>
            <button mat-button
                    color="warn"
                    (click)="removeUpload(u)">Delete</button>
            <button mat-button
                    (click)="restoreUpload(u)">Restore</button>
          </section>
          <mat-divider></mat-divider>
          <section fxLayout="column"
                   fxLayoutAlign="start center"
                   class="container"
                   [style.margin.px]="8">
            <mat-chip-list selectable="false">
              <mat-chip [matTooltip]="u.fileType">{{u.fileType | truncate:'20'}}</mat-chip>
              <mat-chip>{{u.size | bytes}}</mat-chip>
              <mat-chip>{{u.uploadDate | date:'mediumDate'}}</mat-chip>
            </mat-chip-list>
          </section>
        </section>
      </section>
      <p *ngIf="!(uploads.length > 0)"
         class="mat-title">Recycle Bin is Empty</p>
    </ng-container>
  </mat-dialog-content>
  <mat-dialog-actions>
    <button mat-button
            mat-dialog-close>Close</button>
  </mat-dialog-actions>
</div>
```

Make sure to register the `UploadBinDialog` component with the `dialogs` TypeScript module:

**`index.ts`**  

```ts
import { ConfirmDialog } from './confirm.dialog';
import { UploadBinDialog } from './upload/upload-bin.dialog';

export const Dialogs = [
  ConfirmDialog,
  UploadBinDialog
];

export * from './confirm.dialog';
export * from './upload/upload-bin.dialog';
```

### [Routes](#uploads)

The main route for the application will be the `/uploads` route, which will correspond to an `UploadsComponent`. It will provide the following functionality:

* Select files to upload
* Clear selected files
* Upload selected files
* Open the upload bin
* Render the list of files pending upload
* Render a list of uploads from the database
* Search the list of uploads

**`uploads.component.html`**

```html
<mat-toolbar>
  <span>Uploads</span>
  <section [style.margin-left.px]="12">
    <file-upload (selected)="fileChange($event)"
                 accept="*/*"></file-upload>
    <button mat-button
            color="primary"
            (click)="uploadFiles()"
            *ngIf="formData"
            [disabled]="uploading">Upload</button>
    <button mat-button
            (click)="clearFiles()"
            *ngIf="formData"
            [disabled]="uploading">Cancel</button>
    <button mat-button
            color="warn"
            (click)="openUploadBin()">Recycle Bin</button>
  </section>
</mat-toolbar>
<ng-container *ngIf="files?.length > 0">
  <file-list [files]="files"></file-list>
</ng-container>
<section class="container"
         fxLayout="column"
         fxLayoutAlign="start stretch">
  <searchbar label="Search Uploads"
             [minimum]="1"
             (search)="service.searchUploads($event)"
             (clear)="service.getUploads()">
  </searchbar>
</section>
<ng-template #loading>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
<section fxLayout="row | wrap"
         fxLayoutAlign="start start"
         class="container"
         *ngIf="service.uploads$ | async as uploads else loading">
  <ng-container *ngIf="uploads.length > 0">
    <upload-card *ngFor="let u of uploads"
                 [upload]="u"
                 [size]="420"
                 (select)="selectUpload($event)"
                 (delete)="deleteUpload($event)"></upload-card>
  </ng-container>
  <h3 *ngIf="!(uploads.length > 0)">No Uploads Found</h3>
</section>
```

The `<mat-toolbar>` section at the top of the component template define actions that allow the first four features to be accomplished.

The `<file-upload>` component registers a `selected` event that allows the `File[]` and `FormData` objects returned from the event to be received when files have been selected.

The **upload** `<button>` element is only shown if the `formData` property has a value (indicating that files have been selected for upload), and is only enabled if the `uploading` property is false. When clicked, the `uploadFiles()` function is called.

The **cancel** `<button>` element is only shown if the `formData` property has a value (indicating that files have been selected for upload), and is only enabled if the `uploading` property is false. When clicked, the `clearFiles()` function is called.

The **recycle bin** `<button>` element is used to render the `UploadBinDialog` dialog, and when clicked, calls the `openUploadBin()` function.

If there are any files pending upload, they will be rendered using a `<file-list>` component.

The `<searchbar>` component is defined to allow the retrieved uploads to be searched. The `minimum` property is set to **1** so that as long as the typed value is greater than **0**, the `search` event will be emitted. The `search` event calls the `UploadService.searchUploads()` function. The `clear` event calls the `UploadService.getUploads()` function.

The `UploadService.uploads$` stream is subscribed to via the `async` pipe, and when it has a value, renders the contents inside of the `<section>` element where the stream is subscribed.

If the length of the array returned by the stream is greater than zero, each item in the array is rendered via an `<upload-card>` component. The `size` of the card is set to **420px**. The `select` event calls the `selectUpload()` function, and the `delete` event calls the `deleteUpload()` function.

**`uploads.component.ts`**

```ts
import {
  Component,
  OnInit
} from '@angular/core';

import {
  ConfirmDialog,
  UploadBinDialog
} from '../../dialogs';

import { Router } from '@angular/router';
import { MatDialog } from '@angular/material';
import { UploadService } from '../../services';
import { Upload } from '../../models';

@Component({
  selector: 'uploads-route',
  templateUrl: 'uploads.component.html',
  providers: [ UploadService ]
})
export class UploadsComponent implements OnInit {
  files: File[];
  formData: FormData;
  uploading = false;

  constructor(
    public service: UploadService,
    private router: Router,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.service.getUploads();
  }

  fileChange = (fileDetails: [File[], FormData]) => {
    this.files = fileDetails[0];
    this.formData = fileDetails[1];
  }

  clearFiles = () => {
    this.files = null;
    this.formData = null;
  }

  async uploadFiles() {
    this.uploading = true;
    const res = await this.service.uploadFiles(this.formData);
    this.uploading = false;
    this.clearFiles();
    res && this.service.getUploads();
  }

  selectUpload = (upload: Upload) => upload && this.router.navigate(['upload', upload.file]);

  deleteUpload = (upload: Upload) => this.dialog.open(ConfirmDialog)
    .afterClosed()
    .subscribe(async result => {
      const res = result && await this.service.toggleUploadDeleted(upload);
      res && this.service.getUploads();
    });

  openUploadBin = () => this.dialog.open(UploadBinDialog, {
    width: '800px'
  })
  .afterClosed()
  .subscribe(() => this.service.getUploads());
}
```  

The `UploadService` is registers with the `providers` array of `UploadsComponent`. In the **OnInit** lifecycle hook, the `UploadService.getUploads()` function is called, effectively initializing the `UploadService.uploads$` Observable.

The properties for the component are described as follows:

Property | Type | Description
---------|------|------------
`files` | `File[]` | Represents the `File[]` received from the `select` output event of the `FileUploadComponent`
`formData` | `FormData` | Represents the `FormData` received from the `select` output event of the `FileUploadComponent`
`uploading` | `boolean` | Used to track when files are being uploaded  

The `fileChange()` function is used with the `<file-upload>` component to receive the details of files that have been selected for upload.

The `clearFiles()` function nullifies the values of the `files` and `formData` properties, clearing any pending uploads.

The `uploadFiles()` asynchronous function sets the `uploading` flag to true, and passes the pending `formData` property to the `UploadService.uploadFiles()` function. When complete, the `uploading` flag is set to false and `clearFiles()` is called. If the operation completed successfully, the list of uploads is refreshed.

The `selectUpload()` function receives an `Upload` object. If the received object has a value, the router navigates the the `/upload/:file` route, using the `upload.file` property for the `file` URL parameter.

The `deleteUpload()` function receives an `Upload` object. The `ConfirmDialog` is opened, asking the user to confirm that they want to complete the specified action. After the dialog closes, if the result is true, the `upload` is passed to the `UploadService.toggleUploadDeleted()` function, effectively soft-deleting the upload. If the operation completes successfully, the list of uploads is refreshed.

The `openUploadBin()` function opens the `UploadBinDialog`. After it is closed, the list of uploads is refreshed.

In addition to the `UploadsComponent`, an `UploadComponent` is defined that allows a single `Upload` to be shown at a route, given the `Upload.file`.

**`upload.component.html`**

```html
<ng-template #loading>
  <p class="mat-title">Loading Upload</p>
  <mat-progress-bar mode="indeterminate"
                    color="accent"></mat-progress-bar>
</ng-template>
<ng-container *ngIf="service.upload$ | async as upload else loading">
  <section class="container">
    <upload-card [upload]="upload"
                 [expanded]="true"
                 [clickable]="false"
                 (delete)="deleteUpload($event)"></upload-card>
  </section>
</ng-container>
```

The route simply renders an `<upload-card>` component for the `Upload` object resolved by the route. The `expanded` input property is set to `true`, and `clickable` is set to `false`. The `delete` output event will call the `deleteUpload()` function when emitted.

**`upload.component.ts`**

```ts
import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  ActivatedRoute,
  Router,
  ParamMap
} from '@angular/router';

import { MatDialog } from '@angular/material';
import { Subscription } from 'rxjs';
import { UploadService } from '../../services';
import { ConfirmDialog } from '../../dialogs';
import { Upload } from '../../models';

@Component({
  selector: 'upload-route',
  templateUrl: 'upload.component.html',
  providers: [UploadService]
})
export class UploadComponent implements OnInit, OnDestroy {
  private subs = new Array<Subscription>();

  private navigate = () => this.router.navigate(['uploads']);

  constructor(
    public service: UploadService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.subs.push(this.route.paramMap.subscribe(async (params: ParamMap) => {
      if (params.has('file')) {
        const file = params.get('file');
        const res = await this.service.getUploadByName(file);
        !res && this.navigate();
      } else {
        this.navigate();
      }
    }));
  }

  ngOnDestroy() {
    this.subs.forEach(x => x.unsubscribe);
  }

  deleteUpload = (upload: Upload) => this.subs.push(this.dialog.open(ConfirmDialog)
    .afterClosed()
    .subscribe(async result => {
      const res = result && await this.service.toggleUploadDeleted(upload);
      res && this.navigate();
    }));
}
```  

The `UploadService` is registered with the `providers` array for the `UploadComponent`.

In the **OnInit** lifecycle hook, the `ActivatedRoute.paramMap` Observable is subscribed to, and if the received value has a `file` parameter, the `UploadService.getUploadByName()` function is called with the value of the `file` route parameter. If an upload is not found, or if there is not `file` route parameter, the router navigates back to the `/uploads` route. Otherwise, the `UploadService.upload$` Observable will be populated with the appropriate `Upload` object in the component template.

The `deleteUpload()` function works the same as in the `UploadsComponent`.

Make sure to register these route components with the `routes` TypeScript module:

**`index.ts`**

```ts
import { Route } from '@angular/router';
import { UploadsComponent } from './upload/uploads.component';
import { UploadComponent } from './upload/upload.component';

export const RouteComponents = [
  UploadsComponent,
  UploadComponent
];

export const Routes: Route[] = [
  { path: 'uploads', component: UploadsComponent },
  { path: 'upload/:file', component: UploadComponent },
  { path: '', redirectTo: 'uploads', pathMatch: 'full' },
  { path: '**', redirectTo: 'uploads', pathMatch: 'full' }
];
```

## [Related Data](#uploads)

Now that a workflow for managing uploads has been established, it's time to take it a step further and discuss how to associate uploads with other entities. In this section, we'll look at how to create folders and manage how uploads are related to folders using join tables.

### [Folders Back End](#uploads)

Before an upload can be associated with anything, it needs an entity to be associated with.

**`Folder.cs`**

``` cs
namespace UploadDemo.Data.Entities
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }
}
```

To join the `Upload` class to the `Folder` class, a `FolderUpload` class is created that provides a link between the two.

**`FolderUpload.cs`**

```cs
namespace UploadDemo.Data.Entities
{
    public class FolderUpload
    {
        public int Id { get; set; }
        public int FolderId { get; set; }
        public int UploadId { get; set; }

        public Folder Folder { get; set; }
        public Upload Upload { get; set; }
    }
}
```

With this in place, the following navigation property can be added to the bottom of the `Folder` class:

``` cs
public class Folder
{
  // Properties removed for brevity

  public List<FolderUpload> FolderUploads { get; set; }
}
```

And the following navigation property can be added to the `Upload` class:

```cs
public class Upload
{
  // Properties removed for brevity

  public List<FolderUpload> UploadFolders { get; set; }
}
```

Now the newly created classes can be added to `AppDbContext`, and the `UploadFolders` navigation property can be appropriately mapped in `OnModelCreating()`:

**`AppDbContext.cs`**

```cs
namespace UploadDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Folder> Folders { get; set; }
        public DbSet<FolderUpload> FolderUploads { get; set; }
        public DbSet<Upload> Uploads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Model
                .GetEntityTypes()
                .ToList()
                .ForEach(x =>
                {
                    modelBuilder
                        .Entity(x.Name)
                        .ToTable(x.Name.Split('.').Last());
                });
            
            modelBuilder
                .Entity<Upload>()
                .HasMany(x => x.UploadFolders)
                .WithOne(x => x.Upload)
                .HasForeignKey(x => x.UploadId)
                .IsRequired();
        }
    }
}
```

> If you're unfamiliar with managing entity relationships or configuring entities in `OnModelCreating`, refer to the [Data Access Layer](./02-data-access-layer.md) article.

The `FolderUpload` logic will be contained in both the `UploadExtensions` and `FolderExtensions` classes, with `FolderExtensions` containing the bulk of the logic.

With this in mind, the following modifications are made to the `UploadExtensions` class:

**`UploadExtensions.cs`**

```cs
namespace UploadDemo.Data.Extensions
{
    public static class UploadExtensions
    {
        private static IQueryable<Upload> SetUploadIncludes(this DbSet<Upload> uploads) =>
            uploads.Include(x => x.UploadFolders)
                .ThenInclude(x => x.Folder);

        public static async Task<List<Upload>> GetUploads(this AppDbContext db, bool isDeleted = false)
        {
            var uploads = await db.Uploads
                .SetUploadIncludes()
                .Where(x => x.IsDeleted == isDeleted)
                .OrderByDescending(x => x.UploadDate)
                .ToListAsync();

            return uploads;
        }

        public static async Task<List<Upload>> SearchUploads(this AppDbContext db, string search, bool isDeleted = false)
        {
            search = search.ToLower();
            var uploads = await db.Uploads
                .SetUploadIncludes()
                .Where(x => x.IsDeleted == isDeleted)
                .Where(x => 
                    x.File.ToLower().Contains(search) ||
                    x.UploadFolders.Any(y =>
                        y.Folder.Name.ToLower().Contains(search) ||
                        y.Folder.Description.ToLower().Contains(search)
                    )
                )
                .OrderByDescending(x => x.UploadDate)
                .ToListAsync();

            return uploads;
        }

        public static async Task<Upload> GetUpload(this AppDbContext db, int uploadId) => 
            await db.Uploads
                .SetUploadIncludes()
                .FirstOrDefaultAsync(x => x.Id == uploadId);

        public static async Task<Upload> GetUploadByName(this AppDbContext db, string file) => 
            await db.Uploads
                .SetUploadIncludes()
                .FirstOrDefaultAsync(x => x.File.ToLower() == file.ToLower());

        public static async Task<List<Folder>> GetUploadFolders(this AppDbContext db, int uploadId, bool isDeleted = false)
        {
            var folders = await db.FolderUploads
                .Where(x =>
                    x.UploadId == uploadId &&
                    x.Folder.IsDeleted == isDeleted
                )
                .Select(x => x.Folder)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return folders;
        }

        public static async Task<List<Upload>> GetExcludedUploads(this AppDbContext db, string name, bool isDeleted = false)
        {
            var uploads = await db.Uploads
                .Where(x => x.IsDeleted == isDeleted)
                .Where(x => !x.UploadFolders.Any(y => y.Folder.Name.ToLower() == name.ToLower()))
                .OrderByDescending(x => x.UploadDate)
                .ToListAsync();

            return uploads;
        }
    }
}
```

The `SetUploadIncludes()` method is a convenience method for ensuring that `UploadFolders`, and the `Folder` property for each item it contains, are added included in the query for each `Upload` that is returned when this method is called.

`GetUploads()` adds the `SetUploadIncludes()` extension method to its method chain.

`SearchUpload()` adds the `SetUploadIncludes()` extension method to its method chain and allows you to search for files by the `Folder.Name` and `Folder.Description` for any folders it is related to.

Both `GetUpload()` and `GetUploadByName()` add the `SetUploadIncludes()` extension method to its method chain.

`GetUploadFolders()` retrieves the folders that contain the provided upload ID, and accepts an optional `isDeleted` argument (which defaults to `false`) to determine whether to retrieve uploads based on the `Upload.IsDeleted` property.

`GetExcludedUploads()` retrieves all of the uploads that ***are not*** related to a specified folder name. It accepts an optional `isDeleted` argument (which defaults to `false`) to determine whether to retrieve uploads based on the` Upload.IsDeleted` property.

Now, the `FolderExtensions` class can be defined, which contains all of the boilerplate CRUD operations for `Folder`, similar to the operations defined for `Upload` in `UploadExtensions`, but it also contains the boilerplate CRUD operations for `FolderUpload` interactions.

**`FolderExtensions.cs`**

```cs
namespace UploadDemo.Data.Extensions
{
    public static class FolderExtensions
    {
        public static async Task<List<Folder>> GetFolders(this AppDbContext db, bool IsDeleted = false)
        {
            var folders = await db.Folders
                .Include(x => x.FolderUploads)
                .Where(x => x.IsDeleted == IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return folders;
        }

        public static async Task<List<Folder>> SearchFolders(this AppDbContext db, string search, bool IsDeleted = false)
        {
            search = search.ToLower();
            var folders = await db.Folders
                .Include(x => x.FolderUploads)
                .Where(x => x.IsDeleted == IsDeleted)
                .Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Description.ToLower().Contains(search) ||
                    x.FolderUploads.Any(y => y.Upload.Name.ToLower().Contains(search))
                )
                .OrderBy(x => x.Name)
                .ToListAsync();

            return folders;
        }

        public static async Task<Folder> GetFolder(this AppDbContext db, int id) => await db.Folders.FindAsync(id);

        public static async Task<Folder> GetFolderByName(this AppDbContext db, string name) =>
            await db.Folders.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

        public static async Task<List<Upload>> GetFolderUploads(this AppDbContext db, string name, bool isDeleted = false)
        {
            var uploads = await db.FolderUploads
                .Include(x => x.Upload)
                    .ThenInclude(x => x.UploadFolders)
                        .ThenInclude(x => x.Folder)
                .Where(x =>
                    x.Folder.Name.ToLower() == name.ToLower() &&
                    x.Upload.IsDeleted == isDeleted
                )
                .Select(x => x.Upload)
                    .Include(x => x.UploadFolders)
                        .ThenInclude(x => x.Folder)
                .OrderByDescending(x => x.UploadDate)
                .ToListAsync();

            Console.WriteLine(uploads);
            return uploads;
        }

        public static async Task<List<Folder>> GetExcludedFolders(this AppDbContext db, string file, bool isDeleted = false)
        {
            var folders = await db.Folders
                .Where(x => x.IsDeleted == isDeleted)
                .Where(x => !x.FolderUploads.Any(y => y.Upload.File.ToLower() == file.ToLower()))
                .OrderBy(x => x.Name)
                .ToListAsync();

            return folders;
        }

        public static async Task AddFolder(this AppDbContext db, Folder folder)
        {
            if (await folder.Validate(db))
            {
                await db.Folders.AddAsync(folder);
                await db.SaveChangesAsync();
            }
        }

        public static async Task UpdateFolder(this AppDbContext db, Folder folder)
        {
            if (await folder.Validate(db))
            {
                db.Folders.Update(folder);
                await db.SaveChangesAsync();
            }
        }

        public static async Task ToggleFolderDeleted(this AppDbContext db, Folder folder)
        {
            if (await folder.Validate(db))
            {
                db.Folders.Attach(folder);
                folder.IsDeleted = !folder.IsDeleted;
                await db.SaveChangesAsync();
            }
        }

        public static async Task RemoveFolder(this AppDbContext db, Folder folder)
        {
            db.Folders.Remove(folder);
            await db.SaveChangesAsync();
        }

        public static async Task AddFolderUploads(this AppDbContext db, List<FolderUpload> folderUploads)
        {
            if (await folderUploads.Validate(db))
            {
                await db.FolderUploads.AddRangeAsync(folderUploads);
                await db.SaveChangesAsync();
            }
        }

        public static async Task RemoveFolderUpload(this AppDbContext db, string name, Upload upload)
        {
            var folderUpload = await db.FolderUploads.FirstOrDefaultAsync(
                x => x.Folder.Name.ToLower() == name.ToLower() &&
                x.UploadId == upload.Id
            );

            if (folderUpload == null)
            {
                throw new Exception($"{name} does not contain ${upload.File}");
            }

            db.FolderUploads.Remove(folderUpload);
            await db.SaveChangesAsync();
        }

        public static async Task<bool> Validate(this Folder folder, AppDbContext db)
        {
            if (string.IsNullOrEmpty(folder.Name))
            {
                throw new Exception("An folder must have a name");
            }

            var check = await db.Folders
                .FirstOrDefaultAsync(x =>
                    x.Id != folder.Id &&
                    x.Name.ToLower() == folder.Name.ToLower()
                );

            if (check != null)
            {
                throw new Exception("An folder with this name already exists");
            }

            return true;
        }

        public static async Task<bool> Validate(this FolderUpload folderUpload, AppDbContext db)
        {
            if (folderUpload.FolderId < 1)
            {
                throw new Exception("An upload must be associated with an folder");
            }

            if (folderUpload.UploadId < 1)
            {
                throw new Exception("An folder must be associated with an upload");
            }

            var check = await db.FolderUploads
                .FirstOrDefaultAsync(x =>
                    x.Id != folderUpload.Id &&
                    x.FolderId == folderUpload.FolderId &&
                    x.UploadId == folderUpload.UploadId
                );

            if (check != null)
            {
                throw new Exception("This folder already contains the specified upload");
            }

            return true;
        }

        public static async Task<bool> Validate(this List<FolderUpload> folderUploads, AppDbContext db)
        {
            foreach (var a in folderUploads)
            {
                await a.Validate(db);
            }

            return true;
        }
    }
}
```

As mentioned, this class contains all of the same CRUD operations that are available to `Upload`, just relevant to the `Folder` entity, to include `GetFolderUploads()` for retrieving uploads that are related to a folder, and `GetExcludedFolders()` for retrieving folders that ***are not*** related to an upload.

The following extension methods define the logic for interacting with the `FolderUpload` entity.

`Validate(this List<FolderUpload> folderUploads)` executes the validation function for each `FolderUpload` contained in the collection.

`Validate(this FolderUpload folderUpload)` ensures that the provided `FolderUpload` specifies a `Folder` and an `Upload`, and that the upload specified is not already related to the specified folder.

`AddFolderUploads` accepts a `List<FolderUpload>`, calls the collection validation function mentioned above, adds the collection to the database, and saves the changes.

`RemoveFolderUpload` retrieves a `FolderUpload` object based on the provided folder name and `Upload` object. If no result is found, an exception is thrown indicating that the folder does not contain the upload. Otherwise, the `FolderUpload` retrieved is removed from the database, and the changes are saved.

Before defining the `FolderController` class, the following API endpoints need to be added to the `UploadController` to match the updated logic:

**`UploadController.cs`**

```cs
[Route("api/[controller]")]
public class UploadController : Controller
{
  private AppDbContext db;
  private UploadConfig config;

  public UploadController(AppDbContext db, UploadConfig config)
  {
    this.db = db;
    this.config = config;
  }

  // API endpoints up to GetUploadByName

  [HttpGet("[action]/{id}")]
  public async Task<List<Folder>> GetUploadFolders([FromRoute]int id) => await db.GetUploadFolders(id);

  [HttpGet("[action]/{name}")]
  public async Task<List<Upload>> GetExcludedUploads([FromRoute]string name) => await db.GetExcludedUploads(name);

  // Remaining API endpoints
}
```

`FolderController` is a pretty standard API controller. It simply maps an endpoint to each of the defined extension methods. No weird upload logic needed here.

**`FolderController.cs`**

```cs
namespace UploadData.Web.Controllers
{
    [Route("api/[controller]")]
    public class FolderController : Controller
    {
        private AppDbContext db;

        public FolderController(AppDbContext db)
        {
            this.db = db;
        }

        [HttpGet("[action]")]
        public async Task<List<Folder>> GetFolders() => await db.GetFolders();

        [HttpGet("[action]")]
        public async Task<List<Folder>> GetDeletedFolders() => await db.GetFolders(true);

        [HttpGet("[action]/{search}")]
        public async Task<List<Folder>> SearchFolders([FromRoute]string search) => await db.SearchFolders(search);

        [HttpGet("[action]/{search}")]
        public async Task<List<Folder>> SearchDeletedFolders([FromRoute]string search) => await db.SearchFolders(search, true);

        [HttpGet("[action]/{id}")]
        public async Task<Folder> GetFolder([FromRoute]int id) => await db.GetFolder(id);

        [HttpGet("[action]/{name}")]
        public async Task<Folder> GetFolderByName([FromRoute]string name) => await db.GetFolderByName(name);

        [HttpGet("[action]/{name}")]
        public async Task<List<Upload>> GetFolderUploads([FromRoute]string name) => await db.GetFolderUploads(name);

        [HttpGet("[action]/{file}")]
        public async Task<List<Folder>> GetExcludedFolders([FromRoute]string file) => await db.GetExcludedFolders(file);

        [HttpPost("[action]")]
        public async Task AddFolder([FromBody]Folder folder) => await db.AddFolder(folder);

        [HttpPost("[action]")]
        public async Task UpdateFolder([FromBody]Folder folder) => await db.UpdateFolder(folder);

        [HttpPost("[action]")]
        public async Task ToggleFolderDeleted([FromBody]Folder folder) => await db.ToggleFolderDeleted(folder);

        [HttpPost("[action]")]
        public async Task RemoveFolder([FromBody]Folder folder) => await db.RemoveFolder(folder);

        [HttpPost("[action]")]
        public async Task AddFolderUploads([FromBody]List<FolderUpload> folderUploads) => await db.AddFolderUploads(folderUploads);

        [HttpPost("[action]/{name}")]
        public async Task RemoveFolderUpload([FromRoute]string name, [FromBody]Upload upload) => await db.RemoveFolderUpload(name, upload);
    }
}
```

### [Folders Front End](#uploads)

**Models**

**Service**

**Updates**

**Components**

**Dialogs**

**Routes**

[Back to Top](#uploads)