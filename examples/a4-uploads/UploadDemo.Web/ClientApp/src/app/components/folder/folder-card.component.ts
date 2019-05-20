import {
  Component,
  Input,
  Output,
  EventEmitter
} from '@angular/core';

import { Folder } from '../../models';

@Component({
  selector: 'folder-card',
  templateUrl: 'folder-card.component.html'
})
export class FolderCardComponent {
  @Input() folder: Folder;
  @Input() size = 420;
  @Output() edit = new EventEmitter<Folder>();
  @Output() delete = new EventEmitter<Folder>();
  @Output() select = new EventEmitter<Folder>();
}
