import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  FolderDialog,
  FolderBinDialog,
  ConfirmDialog
} from '../../dialogs';

import { Router } from '@angular/router';
import { MatDialog } from '@angular/material';
import { Subscription } from 'rxjs';
import { FolderService } from '../../services';
import { Folder } from '../../models';

@Component({
  selector: 'folders-route',
  templateUrl: 'folders.component.html',
  providers: [ FolderService ]
})
export class FoldersComponent implements OnInit, OnDestroy {
  private subs = new Array<Subscription>();
  constructor(
    public service: FolderService,
    private router: Router,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.service.getFolders();
  }

  ngOnDestroy() {
    this.subs.forEach(x => x.unsubscribe());
  }

  selectFolder = (folder: Folder) => folder && this.router.navigate(['folder', folder.name]);

  addFolder = () => this.subs.push(this.dialog.open(FolderDialog, {
    data: new Folder(),
    width: '800px',
    disableClose: true
  })
  .afterClosed()
  .subscribe(res => res && this.service.getFolders()));

  editFolder = (folder: Folder) => this.subs.push(this.dialog.open(FolderDialog, {
    data: Object.assign(new Folder(), folder),
    width: '800px',
    disableClose: true
  })
  .afterClosed()
  .subscribe(res => res && this.service.getFolders()));

  deleteFolder = (folder: Folder) => this.subs.push(this.dialog.open(ConfirmDialog)
    .afterClosed()
    .subscribe(async result => {
      const res = result && await this.service.toggleFolderDeleted(folder);
      res && this.service.getFolders();
    }));

  openFolderBin = () => this.subs.push(this.dialog.open(FolderBinDialog, {
    width: '800px'
  })
  .afterClosed()
  .subscribe(() => this.service.getFolders()));
}
