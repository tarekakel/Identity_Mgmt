import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ToastService, type Toast, type ToastType } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div
      class="fixed top-4 right-4 z-[90] flex flex-col gap-3 max-w-sm w-full pointer-events-none"
      aria-live="polite"
      aria-label="Notifications"
    >
      @if (toasts$ | async; as toasts) {
        @for (toast of toasts; track toast.id) {
          <div
            class="pointer-events-auto rounded-lg border shadow-lg p-4 flex items-start gap-3 animate-fade-in"
            [ngClass]="toastClasses(toast.type)"
            [attr.role]="'alert'"
          >
            <span class="flex-shrink-0 mt-0.5" [attr.aria-hidden]="true">
              @switch (toast.type) {
                @case ('success') {
                  <span class="text-emerald-500 text-lg">✓</span>
                }
                @case ('error') {
                  <span class="text-red-500 text-lg">✕</span>
                }
                @case ('info') {
                  <span class="text-indigo-500 text-lg">ℹ</span>
                }
              }
            </span>
            <p class="flex-1 text-sm font-medium break-words">
              {{ toast.message }}
            </p>
            <button
              type="button"
              (click)="toastService.dismiss(toast.id)"
              class="flex-shrink-0 p-1 rounded opacity-80 hover:opacity-100 transition"
              [attr.aria-label]="'Close'"
            >
              <span class="sr-only">Close</span>
              <span aria-hidden="true">×</span>
            </button>
          </div>
        }
      }
    </div>
  `
})
export class ToastComponent {
  readonly toastService = inject(ToastService);
  readonly toasts$ = this.toastService.getToasts$();

  toastClasses(type: ToastType): Record<string, boolean> {
    return {
      'bg-emerald-50 dark:bg-emerald-950/90 border-2 border-emerald-500 dark:border-emerald-400 text-emerald-900 dark:text-emerald-100':
        type === 'success',
      'bg-red-50 dark:bg-red-950/90 border-2 border-red-500 dark:border-red-400 text-red-900 dark:text-red-100':
        type === 'error',
      'bg-indigo-50 dark:bg-indigo-950/90 border-2 border-indigo-500 dark:border-indigo-400 text-indigo-900 dark:text-indigo-100':
        type === 'info'
    };
  }
}
