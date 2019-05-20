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

import { Folder } from '../../models';

@Component({
  selector: 'folder-selector',
  templateUrl: 'folder-selector.component.html'
})
export class FolderSelectorComponent {
  @Input() folders: Folder[];
  @Input() selectable = true;
  @Output() select = new EventEmitter<Folder[]>();

  selectedFolders = new Array<Folder>();

  drop(event: CdkDragDrop<Folder[]>) {
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
