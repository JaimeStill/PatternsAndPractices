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