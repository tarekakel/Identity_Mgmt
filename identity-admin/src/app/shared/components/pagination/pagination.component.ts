import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center justify-between border-t border-slate-200 dark:border-slate-700 px-4 py-3">
      <div class="text-sm text-slate-600 dark:text-slate-400">
        {{ (pageNumber - 1) * pageSize + 1 }} - {{ min((pageNumber * pageSize), totalCount) }} of {{ totalCount }}
      </div>
      <div class="flex gap-2">
        <button
          type="button"
          [disabled]="pageNumber <= 1"
          (click)="pageChange.emit(pageNumber - 1)"
          class="px-3 py-1 rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition"
        >
          Previous
        </button>
        <span class="px-3 py-1 text-slate-700 dark:text-slate-300 self-center">
          {{ pageNumber }} / {{ totalPages || 1 }}
        </span>
        <button
          type="button"
          [disabled]="pageNumber >= (totalPages || 1)"
          (click)="pageChange.emit(pageNumber + 1)"
          class="px-3 py-1 rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition"
        >
          Next
        </button>
      </div>
    </div>
  `
})
export class PaginationComponent {
  @Input() pageNumber = 1;
  @Input() pageSize = 10;
  @Input() totalCount = 0;
  @Input() totalPages = 0;
  @Output() pageChange = new EventEmitter<number>();

  min(a: number, b: number): number {
    return Math.min(a, b);
  }
}
