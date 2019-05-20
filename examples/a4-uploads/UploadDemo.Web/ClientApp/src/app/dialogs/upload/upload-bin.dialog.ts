import {
  Component,
  OnInit
} from '@angular/core';

import { UploadService } from '../../services';
import { Upload } from '../../models';

@Component({
  selector: 'upload-bin-dialog',
  templateUrl: 'upload-bin.dialog.html',
  providers: [UploadService]
})
export class UploadBinDialog implements OnInit {
  constructor(
    public service: UploadService
  ) { }

  ngOnInit() {
    this.service.getDeletedUploads();
  }

  restoreUpload = async (upload: Upload) => {
    const res = await this.service.toggleUploadDeleted(upload);
    res && this.service.getDeletedUploads();
  }

  removeUpload = async (upload: Upload) => {
    const res = await this.service.removeUpload(upload);
    res && this.service.getDeletedUploads();
  }
}
