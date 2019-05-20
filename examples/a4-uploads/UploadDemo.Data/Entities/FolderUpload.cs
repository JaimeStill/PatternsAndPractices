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