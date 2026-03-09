import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ConfirmService } from '../../services/confirm.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    @if (confirm.state.visible) {
      <div
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-title"
      >
        <div
          class="bg-white dark:bg-slate-800 rounded-lg shadow-xl max-w-md w-full p-6 animate-fade-in"
          (click)="$event.stopPropagation()"
        >
          <h2 id="confirm-title" class="text-lg font-semibold text-slate-800 dark:text-slate-100 mb-2">
            {{ confirm.state.title }}
          </h2>
          <p class="text-slate-600 dark:text-slate-300 mb-6">
            {{ confirm.state.message }}
          </p>
          <div class="flex justify-end gap-3">
            <button
              type="button"
              (click)="confirm.cancel()"
              class="px-4 py-2 rounded-lg border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700"
            >
              {{ confirm.state.cancelLabel || ('common.cancel' | translate) }}
            </button>
            <button
              type="button"
              (click)="confirm.accept()"
              class="px-4 py-2 rounded-lg bg-red-600 text-white hover:bg-red-700 focus:ring-2 focus:ring-red-500"
            >
              {{ confirm.state.confirmLabel || ('common.confirm' | translate) }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: []
})
export class ConfirmDialogComponent {
  readonly confirm = inject(ConfirmService);
}
