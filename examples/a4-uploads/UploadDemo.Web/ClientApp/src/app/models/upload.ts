import { FolderUpload } from './folder-upload';

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

  uploadFolders: FolderUpload[];
}
