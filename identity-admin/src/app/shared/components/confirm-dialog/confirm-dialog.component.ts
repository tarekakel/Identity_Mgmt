import {
  AfterViewChecked,
  Component,
  ElementRef,
  inject,
  HostListener,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    @if ((state$ | async); as state) {
      @if (state.visible) {
        <div
          class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/30 dark:bg-slate-950/50 backdrop-blur-sm animate-fade-in"
          role="dialog"
          aria-modal="true"
          aria-labelledby="dialog-title"
          aria-describedby="dialog-description"
          (click)="onBackdropClick($event)"
        >
          <div
            #dialogCard
            class="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-xl max-w-md w-full mx-4 p-6 animate-fade-in"
            (click)="$event.stopPropagation()"
            (keydown)="onDialogKeydown($event)"
          >
            <h2 id="dialog-title" class="text-lg font-semibold text-slate-800 dark:text-slate-100 mb-2">
              {{ state.title }}
            </h2>
            <p id="dialog-description" class="text-slate-600 dark:text-slate-400 mb-6">{{ state.message }}</p>
            <div class="flex justify-end gap-3">
              @if (state.type === 'confirm' && state.cancelLabel) {
                <button
                  type="button"
                  (click)="dialog.close(false)"
                  class="px-4 py-2 rounded-lg border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
                >
                  {{ state.cancelLabel | translate }}
                </button>
              }
              <button
                type="button"
                (click)="dialog.close(true)"
                class="px-4 py-2 rounded-lg bg-red-600 hover:bg-red-700 text-white font-medium transition"
              >
                {{ state.confirmLabel | translate }}
              </button>
            </div>
          </div>
        </div>
      }
    }
  `
})
export class ConfirmDialogComponent implements AfterViewChecked {
  readonly dialog = inject(ConfirmDialogService);
  readonly state$ = this.dialog.getState$();

  @ViewChild('dialogCard') private dialogCard?: ElementRef<HTMLElement>;
  private focusPending = false;

  ngAfterViewChecked(): void {
    if (this.focusPending && this.dialog.visible && this.dialogCard?.nativeElement) {
      const firstButton = this.dialogCard.nativeElement.querySelector<HTMLButtonElement>('button');
      firstButton?.focus();
      this.focusPending = false;
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.dialog.close(false);
    }
  }

  onDialogKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Tab') return;
    const card = this.dialogCard?.nativeElement;
    if (!card) return;
    const focusable = card.querySelectorAll<HTMLElement>(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (event.shiftKey) {
      if (document.activeElement === first) {
        event.preventDefault();
        last?.focus();
      }
    } else {
      if (document.activeElement === last) {
        event.preventDefault();
        first?.focus();
      }
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.dialog.visible) {
      this.dialog.close(false);
    }
  }

  constructor() {
    this.dialog.getState$().subscribe((state) => {
      if (state.visible) this.focusPending = true;
    });
  }
}
