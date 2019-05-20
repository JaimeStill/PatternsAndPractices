import {
  Component,
  Input
} from '@angular/core';

@Component({
  selector: 'file-list',
  templateUrl: 'file-list.component.html'
})
export class FileListComponent {
  @Input() files: File[];
  @Input() layout = "row | wrap";
  @Input() align = "start start"
  @Input() elevated = true;
}
