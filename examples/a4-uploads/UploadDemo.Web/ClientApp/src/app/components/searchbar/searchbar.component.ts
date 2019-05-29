import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  ElementRef,
  OnDestroy,
  ViewEncapsulation
} from '@angular/core';

import { Subscription } from 'rxjs';
import { CoreService } from '../../services';

@Component({
  selector: 'searchbar',
  templateUrl: 'searchbar.component.html'
})
export class SearchbarComponent implements OnDestroy {
  sub: Subscription;

  @Input() label = "Search";
  @Input() minimum: number = 2;
  @Output() search = new EventEmitter<string>();
  @Output() clear = new EventEmitter();

  constructor(
    private core: CoreService
  ) { }

  @ViewChild('searchbar', { static: false })
  set searchbar (input: ElementRef) {
    if (input) {
      this.sub = this.core.generateInputObservable(input)
        .subscribe((val: string) => {
          val && val.length >= this.minimum ?
            this.search.emit(val) :
            this.clear.emit();
        });
    }
  }

  ngOnDestroy() {
    this.core.unsubscribe(this.sub);
  }
}
