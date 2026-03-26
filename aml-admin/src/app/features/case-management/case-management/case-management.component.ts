import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-case-management',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="animate-fade-in w-full">
      <h1 class="text-2xl font-bold text-slate-800 dark:text-slate-100 mb-6">
        {{ 'pages.caseManagement.title' | translate }}
      </h1>
      <p class="text-slate-600 dark:text-slate-400">
        {{ 'common.comingSoon' | translate }}
      </p>
    </div>
  `
})
export class CaseManagementComponent {}

