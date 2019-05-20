using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UploadDemo.Data.Entities;

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

        public static async Task UpdateFolderUpload(this AppDbContext db, FolderUpload folderUpload)
        {
            if (await folderUpload.Validate(db))
            {
                db.FolderUploads.Update(folderUpload);
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