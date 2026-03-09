import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ThemeService } from './core/services/theme.service';
import { SettingsService } from './core/services/settings.service';
import { ConfirmDialogComponent } from './shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ConfirmDialogComponent],
  template: `
    <router-outlet />
    <app-confirm-dialog />
  `,
  styles: []
})
export class AppComponent implements OnInit {
  constructor(
    private translate: TranslateService,
    private theme: ThemeService,
    private settings: SettingsService
  ) {}

  ngOnInit(): void {
    this.translate.addLangs(['en', 'ar']);
    const lang = this.settings.currentSettings().language || 'en';
    this.translate.use(lang);
    this.theme.setTheme(this.settings.currentSettings().theme || 'light');
  }
}
