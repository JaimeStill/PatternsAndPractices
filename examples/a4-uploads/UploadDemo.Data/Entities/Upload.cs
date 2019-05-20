using System;
using System.Collections.Generic;

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

        public List<FolderUpload> UploadFolders { get; set; }
    }
}