import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    @if (fullPage) {
      <div class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/20 dark:bg-slate-950/40 backdrop-blur-sm animate-fade-in">
        <div class="flex flex-col items-center gap-4">
          <div class="relative h-14 w-14">
            <div class="absolute inset-0 rounded-full border-4 border-indigo-200 dark:border-indigo-900"></div>
            <div class="absolute inset-0 rounded-full border-4 border-transparent border-t-indigo-600 animate-spin"></div>
          </div>
          @if (message) {
            <p class="text-sm text-slate-600 dark:text-slate-400">{{ message | translate }}</p>
          }
        </div>
      </div>
    } @else {
      <div class="flex items-center justify-center p-4">
        <div class="relative h-10 w-10">
          <div class="absolute inset-0 rounded-full border-2 border-indigo-200 dark:border-indigo-900"></div>
          <div class="absolute inset-0 rounded-full border-2 border-transparent border-t-indigo-600 animate-spin"></div>
        </div>
      </div>
    }
  `
})
export class LoaderComponent {
  @Input() fullPage = false;
  @Input() message = 'common.loading';
}
