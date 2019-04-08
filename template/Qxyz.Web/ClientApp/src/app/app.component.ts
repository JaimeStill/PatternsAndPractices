import { Theme } from './models';

import {
  Component,
  OnInit
} from '@angular/core';

import {
  BannerService,
  ThemeService
} from './services';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
  themeClass = 'default';

  constructor(
    public banner: BannerService,
    public theme: ThemeService
  ) { }

  ngOnInit() {
    this.banner.getConfig();
    this.theme.theme$.subscribe((t: Theme) => this.themeClass = t.name);
  }
}
