import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  ActivatedRoute,
  ParamMap,
  Router
} from '@angular/router';

import {
  FolderService
} from '../../services';

import {
  AddUploadDialog,
  FolderDialog,
  ConfirmDialog
} from '../../dialogs';

import {
  Folder,
  Upload
} from '../../models';

import { MatDialog } from '@angular/material';
import { Subscription } from 'rxjs';

@Component({
  selector: 'folder-route',
  templateUrl: 'folder.component.html',
  providers: [FolderService]
})
export class FolderComponent {
  private subs = new Array<Subscription>();
  private navigate = () => this.router.navigate(['folders']);
  private name: string;

  constructor(
    public service: FolderService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.subs.push(this.route.paramMap.subscribe(async (params: ParamMap) => {
      if (params.has('name')) {
        this.name = params.get('name');
        const res = await this.service.getFolderByName(this.name);
        res && this.service.getFolderUploads(this.name);
        !res && this.navigate();
      } else {
        this.navigate();
      }
    }));
  }

  ngOnDestroy() {
    this.subs.forEach(x => x.unsubscribe());
  }

  addUploads = (folder: Folder) => this.subs.push(this.dialog.open(AddUploadDialog, {
    data: Object.assign(new Folder(), folder),
    width: '800px',
    disableClose: true
  })
  .afterClosed()
  .subscribe(res => res && this.service.getFolderUploads(folder.name)));

  editFolder = (folder: Folder) => this.subs.push(this.dialog.open(FolderDialog, {
    data: Object.assign(new Folder(), folder),
    width: '800px',
    disableClose: true
  })
  .afterClosed()
  .subscribe(res => res && this.service.getFolderByName(folder.name)));

  deleteFolder = (folder: Folder) => this.subs.push(this.dialog.open(ConfirmDialog)
    .afterClosed()
    .subscribe(async result => {
      const res = result && await this.service.toggleFolderDeleted(folder);
      res && this.navigate();
    }));

  selectUpload = (upload: Upload) => this.router.navigate(['upload', upload.name]);

  deleteUpload = (upload: Upload) => this.subs.push(this.dialog.open(ConfirmDialog)
    .afterClosed()
    .subscribe(async result => {
      const res = result && await this.service.removeFolderUpload(this.name, upload);
      res && this.service.getFolderUploads(this.name);
    }));
}
