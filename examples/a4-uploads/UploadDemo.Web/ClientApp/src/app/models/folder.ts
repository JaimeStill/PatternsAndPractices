import { FolderUpload } from './folder-upload';

export class Folder {
  id: number;
  name: string;
  description: string;
  isDeleted: boolean;

  folderUploads: FolderUpload[];
}
