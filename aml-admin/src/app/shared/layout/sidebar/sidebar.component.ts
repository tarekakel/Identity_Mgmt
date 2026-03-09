import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  template: `
    <aside class="w-64 min-h-screen bg-white dark:bg-slate-800 border-r border-slate-200 dark:border-slate-700 flex flex-col transition-all duration-300">
      <div class="p-4 border-b border-slate-200 dark:border-slate-700">
        <a routerLink="/dashboard" class="text-xl font-semibold text-indigo-600 dark:text-indigo-400">{{ 'app.title' | translate }}</a>
      </div>
      <nav class="flex-1 p-4 space-y-1">
        <a
          routerLink="/dashboard"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          [routerLinkActiveOptions]="{ exact: true }"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">📊</span>
          <span>{{ 'app.dashboard' | translate }}</span>
        </a>
        <a
          routerLink="/screening/new"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">📋</span>
          <span>{{ 'app.screening' | translate }}</span>
        </a>
        <a
          routerLink="/customers"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">👤</span>
          <span>{{ 'app.customers' | translate }}</span>
        </a>
        <a
          routerLink="/sanctions-screening"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">🔍</span>
          <span>{{ 'app.sanctionsScreening' | translate }}</span>
        </a>
        <a
          routerLink="/sanction-lists"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">📑</span>
          <span>{{ 'app.sanctionLists' | translate }}</span>
        </a>
        <a
          routerLink="/risk-assignment"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">⚠</span>
          <span>{{ 'app.riskAssignment' | translate }}</span>
        </a>
        <a
          routerLink="/cases"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">📄</span>
          <span>{{ 'app.cases' | translate }}</span>
        </a>
        <a
          routerLink="/audit-logs"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
        >
          <span class="w-5 text-center">📋</span>
          <span>{{ 'app.auditLogs' | translate }}</span>
        </a>
      </nav>
    </aside>
  `
})
export class SidebarComponent {}
