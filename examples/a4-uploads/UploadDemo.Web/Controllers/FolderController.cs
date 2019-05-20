using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UploadDemo.Data;
using UploadDemo.Data.Entities;
using UploadDemo.Data.Extensions;

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

        [HttpPost("[action]")]
        public async Task UpdateFolderUpload([FromBody]FolderUpload folderUpload) => await db.UpdateFolderUpload(folderUpload);

        [HttpPost("[action]/{name}")]
        public async Task RemoveFolderUpload([FromRoute]string name, [FromBody]Upload upload) => await db.RemoveFolderUpload(name, upload);
    }
}