import {
  Component,
  OnInit
} from '@angular/core';

import {
  ConfirmDialog,
  UploadBinDialog
} from '../../dialogs';

import { Router } from '@angular/router';
import { MatDialog } from '@angular/material';
import { UploadService } from '../../services';
import { Upload } from '../../models';

@Component({
  selector: 'uploads-route',
  templateUrl: 'uploads.component.html',
  providers: [ UploadService ]
})
export class UploadsComponent implements OnInit {
  files: File[];
  formData: FormData;
  uploading = false;

  constructor(
    public service: UploadService,
    private router: Router,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.service.getUploads();
  }

  fileChange = (fileDetails: [File[], FormData]) => {
    this.files = fileDetails[0];
    this.formData = fileDetails[1];
  }

  clearFiles = () => {
    this.files = null;
    this.formData = null;
  }

  async uploadFiles() {
    this.uploading = true;
    const res = await this.service.uploadFiles(this.formData);
    this.uploading = false;
    this.clearFiles();
    res && this.service.getUploads();
  }

  selectUpload = (upload: Upload) => upload && this.router.navigate(['upload', upload.file]);

  deleteUpload = (upload: Upload) => this.dialog.open(ConfirmDialog)
    .afterClosed()
    .subscribe(async result => {
      const res = result && await this.service.toggleUploadDeleted(upload);
      res && this.service.getUploads();
    });

  openUploadBin = () => this.dialog.open(UploadBinDialog, {
    width: '800px'
  })
  .afterClosed()
  .subscribe(() => this.service.getUploads());
}
