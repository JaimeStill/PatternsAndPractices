import {
  Component,
  OnInit
} from '@angular/core';

import { FolderService } from '../../services';
import { Folder } from '../../models';

@Component({
  selector: 'folder-bin-dialog',
  templateUrl: 'folder-bin.dialog.html',
  providers: [FolderService]
})
export class FolderBinDialog implements OnInit {
  constructor(
    public service: FolderService
  ) { }

  ngOnInit() {
    this.service.getDeletedFolders();
  }

  restoreFolder = async (folder: Folder) => {
    const res = await this.service.toggleFolderDeleted(folder);
    res && this.service.getDeletedFolders();
  }

  removeFolder = async (folder: Folder) => {
    const res = await this.service.removeFolder(folder);
    res && this.service.getDeletedFolders();
  }
}
