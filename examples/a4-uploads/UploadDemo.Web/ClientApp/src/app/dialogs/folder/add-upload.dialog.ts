import {
  Component,
  Inject,
  OnInit
} from '@angular/core';

import {
  MatDialogRef,
  MAT_DIALOG_DATA
} from '@angular/material';

import {
  FolderService,
  UploadService
} from '../../services';

import {
  Folder,
  FolderUpload,
  Upload
} from '../../models';

@Component({
  selector: 'add-upload-dialog',
  templateUrl: 'add-upload.dialog.html',
  providers: [FolderService, UploadService]
})
export class AddUploadDialog implements OnInit {
  files: File[];
  formData: FormData;
  uploading = false;

  constructor(
    private dialogRef: MatDialogRef<AddUploadDialog>,
    @Inject(MAT_DIALOG_DATA) public data: Folder,
    public folder: FolderService,
    public upload: UploadService
  ) { }

  ngOnInit() {
    this.upload.getExcludedUploads(this.data.name);
  }

  fileChange = (fileDetails: [File[], FormData]) => {
    this.files = fileDetails[0];
    this.formData = fileDetails[1];
  }

  clearFiles = () => {
    this.files = null;
    this.formData = null;
  }

  addNewUploads = async () => {
    this.uploading = true;
    const res = await this.upload.uploadFiles(this.formData);
    this.uploading = false;
    res && this.addFolderUploads(res);
  }

  addFolderUploads = async (u: Upload[]) => {
    this.uploading = true;
    const uploads = u.map(x => Object.assign(new FolderUpload(), {
      folderId: this.data.id,
      uploadId: x.id
    }));

    const res = await this.folder.addFolderUploads(uploads);
    this.uploading = false;
    res && this.dialogRef.close(true);
  }
}
