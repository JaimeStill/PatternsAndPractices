import {
  Component,
  Input
} from '@angular/core';

@Component({
  selector: 'banner',
  templateUrl: 'banner.component.html'
})
export class BannerComponent {
  @Input() label: string;
  @Input() background: string;
  @Input() color: string;
}
