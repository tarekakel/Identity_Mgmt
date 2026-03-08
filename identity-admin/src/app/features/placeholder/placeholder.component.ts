import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-placeholder',
  standalone: true,
  imports: [TranslateModule],
  template: `
    <div class="animate-fade-in">
      <h1 class="text-2xl font-bold text-slate-800 dark:text-slate-100 mb-4">{{ 'app.placeholder' | translate }}</h1>
      <p class="text-slate-600 dark:text-slate-400">More pages will be added here later (reports, audit, etc.).</p>
    </div>
  `
})
export class PlaceholderComponent {}
