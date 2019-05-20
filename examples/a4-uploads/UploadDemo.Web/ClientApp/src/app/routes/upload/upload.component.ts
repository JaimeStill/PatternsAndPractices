import {
  Component,
  OnInit,
  OnDestroy
} from '@angular/core';

import {
  ActivatedRoute,
  Router,
  ParamMap
} from '@angular/router';

import {
  AddFolderDialog,
  ConfirmDialog
} from '../../dialogs';

import { MatDialog } from '@angular/material';
import { Subscription } from 'rxjs';
import { UploadService } from '../../services';
import { Upload } from '../../models';

@Component({
  selector: 'upload-route',
  templateUrl: 'upload.component.html',
  providers: [UploadService]
})
export class UploadComponent implements OnInit, OnDestroy {
  private subs = new Array<Subscription>();

  private navigate = () => this.router.navigate(['uploads']);

  constructor(
    public service: UploadService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.subs.push(this.route.paramMap.subscribe(async (params: ParamMap) => {
      if (params.has('file')) {
        const file = params.get('file');
        const res = await this.service.getUploadByName(file);
        !res && this.navigate();
      } else {
        this.navigate();
      }
    }));
  }

  ngOnDestroy() {
    this.subs.forEach(x => x.unsubscribe);
  }

  addFolders = (upload: Upload) => this.subs.push(this.dialog.open(AddFolderDialog, {
    data: Object.assign(new Upload(), upload),
    width: '800px',
    disableClose: true
  })
  .afterClosed()
  .subscribe(res => res && this.service.getUploadByName(upload.file)));

  deleteUpload = (upload: Upload) => this.subs.push(this.dialog.open(ConfirmDialog)
    .afterClosed()
    .subscribe(async result => {
      const res = result && await this.service.toggleUploadDeleted(upload);
      res && this.navigate();
    }));
}
