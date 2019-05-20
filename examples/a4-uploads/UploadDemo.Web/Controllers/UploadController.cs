using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UploadDemo.Core.Upload;
using UploadDemo.Data;
using UploadDemo.Data.Entities;
using UploadDemo.Data.Extensions;

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

        [HttpGet("[action]/{id}")]
        public async Task<List<Folder>> GetUploadFolders([FromRoute]int id) => await db.GetUploadFolders(id);

        [HttpGet("[action]/{name}")]
        public async Task<List<Upload>> GetExcludedUploads([FromRoute]string name) => await db.GetExcludedUploads(name);

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