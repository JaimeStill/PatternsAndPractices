import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit
} from '@angular/core';

import { Upload } from '../../models';

@Component({
  selector: 'upload-card',
  templateUrl: 'upload-card.component.html'
})
export class UploadCardComponent implements OnInit {
  expandable: boolean;
  filetype: string;
  @Input() expanded = false;
  @Input() clickable = true;
  @Input() upload: Upload;
  @Input() size = 600;
  @Output() select = new EventEmitter<Upload>();
  @Output() delete = new EventEmitter<Upload>();

  toggleExpanded = () => this.expanded = !this.expanded;

  ngOnInit() {
    this.filetype = this.upload.fileType.split('/')[0];

    switch (this.filetype) {
      case 'image':
      case 'audio':
      case 'video':
        this.expandable = true;
        break;
      default:
        this.expandable = false;
        this.expanded = false;
    }
  }
}
