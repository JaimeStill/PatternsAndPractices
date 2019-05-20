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
  FolderService
} from '../../services';

import {
  Folder,
  FolderUpload,
  Upload
} from '../../models';

@Component({
  selector: 'add-folder-dialog',
  templateUrl: 'add-folder.dialog.html',
  providers: [FolderService]
})
export class AddFolderDialog implements OnInit {
  uploading = false;

  constructor(
    private dialogRef: MatDialogRef<AddFolderDialog>,
    @Inject(MAT_DIALOG_DATA) public data: Upload,
    public folder: FolderService
  ) { }

  ngOnInit() {
    this.folder.getExcludedFolders(this.data.file);
  }

  addUploadFolders = async (a: Folder[]) => {
    this.uploading = true;
    const folders = a.map(x => Object.assign(new FolderUpload(), {
      folderId: x.id,
      uploadId: this.data.id
    }));

    const res = await this.folder.addFolderUploads(folders);
    this.uploading = false;
    res && this.dialogRef.close(true);
  }
}
