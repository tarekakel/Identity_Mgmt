import { Component, signal, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';

const SIDEBAR_COLLAPSED_KEY = 'sidebarCollapsed';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  template: `
    <aside
      class="min-h-screen bg-white dark:bg-slate-800 border-r border-slate-200 dark:border-slate-700 flex flex-col transition-all duration-300 shrink-0"
      [class.w-64]="!collapsed()"
      [class.w-20]="collapsed()"
    >
      <div
        class="border-b border-slate-200 dark:border-slate-700 flex items-center overflow-hidden transition-all duration-300"
        [class.p-4]="!collapsed()"
        [class.p-3]="collapsed()"
        [class.justify-center]="collapsed()"
      >
        <a routerLink="/dashboard" class="text-xl font-semibold text-indigo-600 dark:text-indigo-400 whitespace-nowrap flex items-center gap-2 min-w-0">
          <span class="shrink-0 text-center">📊</span>
          @if (!collapsed()) {
            <span class="truncate">{{ 'app.title' | translate }}</span>
          }
        </a>
      </div>
      <nav class="flex-1 p-4 space-y-1 overflow-hidden">
        <a
          routerLink="/dashboard"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          [routerLinkActiveOptions]="{ exact: true }"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.dashboard' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">📊</span>
          @if (!collapsed()) {
            <span>{{ 'app.dashboard' | translate }}</span>
          }
        </a>
        <a
          routerLink="/screening/new"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.screening' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">📋</span>
          @if (!collapsed()) {
            <span>{{ 'app.screening' | translate }}</span>
          }
        </a>
        <a
          routerLink="/customers"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.customers' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">👤</span>
          @if (!collapsed()) {
            <span>{{ 'app.customers' | translate }}</span>
          }
        </a>
        <a
          routerLink="/sanctions-screening"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.sanctionsScreening' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">🔍</span>
          @if (!collapsed()) {
            <span>{{ 'app.sanctionsScreening' | translate }}</span>
          }
        </a>
        <a
          routerLink="/sanction-lists"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.sanctionLists' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">📑</span>
          @if (!collapsed()) {
            <span>{{ 'app.sanctionLists' | translate }}</span>
          }
        </a>
        <a
          routerLink="/risk-assignment"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.riskAssignment' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">⚠</span>
          @if (!collapsed()) {
            <span>{{ 'app.riskAssignment' | translate }}</span>
          }
        </a>
        <a
          routerLink="/cases"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.cases' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">📄</span>
          @if (!collapsed()) {
            <span>{{ 'app.cases' | translate }}</span>
          }
        </a>
        <a
          routerLink="/audit-logs"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.auditLogs' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">📋</span>
          @if (!collapsed()) {
            <span>{{ 'app.auditLogs' | translate }}</span>
          }
        </a>
        <a
          routerLink="/sanction-action-history"
          routerLinkActive="bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300"
          class="flex items-center rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition min-h-10"
          [class.gap-3]="!collapsed()"
          [class.px-3]="!collapsed()"
          [class.justify-center]="collapsed()"
          [class.px-2]="collapsed()"
          [title]="collapsed() ? ('app.sanctionActionHistory' | translate) : null"
        >
          <span class="w-5 text-center shrink-0">📜</span>
          @if (!collapsed()) {
            <span>{{ 'app.sanctionActionHistory' | translate }}</span>
          }
        </a>
      </nav>
      <div class="p-2 border-t border-slate-200 dark:border-slate-700 flex justify-center">
        <button
          type="button"
          (click)="toggleCollapsed()"
          class="w-full flex items-center justify-center py-2 rounded-lg text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
          [attr.title]="collapsed() ? ('sidebar.expand' | translate) : ('sidebar.collapse' | translate)"
        >
          @if (collapsed()) {
            <span class="text-sm font-medium" aria-hidden="true">&gt;&gt;</span>
          } @else {
            <span class="text-sm font-medium" aria-hidden="true">&lt;&lt;</span>
          }
        </button>
      </div>
    </aside>
  `
})
export class SidebarComponent implements OnInit {
  collapsed = signal(false);

  ngOnInit(): void {
    try {
      const stored = localStorage.getItem(SIDEBAR_COLLAPSED_KEY);
      if (stored !== null) {
        this.collapsed.set(stored === 'true');
      }
    } catch {
      // ignore localStorage errors
    }
  }

  toggleCollapsed(): void {
    this.collapsed.update(c => !c);
    try {
      localStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(this.collapsed()));
    } catch {
      // ignore
    }
  }
}
