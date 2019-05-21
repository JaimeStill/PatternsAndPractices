import {
  Component,
  Input,
  Output,
  EventEmitter
} from '@angular/core';

import {
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem
} from '@angular/cdk/drag-drop';

import { Upload } from '../../models';

@Component({
  selector: 'upload-selector',
  templateUrl: 'upload-selector.component.html'
})
export class UploadSelectorComponent {
  @Input() uploads: Upload[];
  @Input() pending = false;
  @Output() select = new EventEmitter<Upload[]>();

  selectedUploads = new Array<Upload>();

  drop(event: CdkDragDrop<Upload[]>) {
    event.previousContainer !== event.container ?
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      ) :
      moveItemInArray(
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
  }
}
