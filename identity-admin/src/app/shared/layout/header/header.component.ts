import { Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';
import { SettingsPanelService } from '../../../core/services/settings-panel.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [TranslateModule],
  template: `
    <header class="h-14 border-b border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 flex items-center justify-between px-4">
      <div class="flex items-center gap-2">
        <button
          type="button"
          (click)="settingsPanel.open()"
          class="p-2 rounded-lg text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
          [title]="'settings.openSettings' | translate"
          aria-label="Open settings"
        >
          <span class="text-lg" aria-hidden="true">⚙</span>
        </button>
      </div>
      <div class="flex items-center gap-2">
        <button
          type="button"
          (click)="authService.logout()"
          class="px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-700 text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-600 transition text-sm"
        >
          {{ 'app.logout' | translate }}
        </button>
      </div>
    </header>
  `
})
export class HeaderComponent {
  readonly authService = inject(AuthService);
  readonly settingsPanel = inject(SettingsPanelService);
}
