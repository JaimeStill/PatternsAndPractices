import {
  Component,
  Inject,
  OnInit
} from '@angular/core';

import {
  MatDialogRef,
  MAT_DIALOG_DATA
} from '@angular/material';

import { FolderService } from '../../services';
import { Folder } from '../../models';

@Component({
  selector: 'folder-dialog',
  templateUrl: 'folder.dialog.html',
  providers: [FolderService]
})
export class FolderDialog implements OnInit {
  dialogTitle = 'Add Folder';
  folder: Folder;

  constructor(
    private dialogRef: MatDialogRef<FolderDialog>,
    @Inject(MAT_DIALOG_DATA) public data: Folder,
    public service: FolderService
  ) { }

  ngOnInit() {
    this.folder = this.data ?
      this.data :
      new Folder();

    this.dialogTitle = this.data && this.data.id ?
      'Update Folder' :
      'Add Folder';
  }

  saveFolder = async () => {
    const res = this.folder.id ?
      await this.service.updateFolder(this.folder) :
      await this.service.addFolder(this.folder);

    res && this.dialogRef.close(true);
  }
}
