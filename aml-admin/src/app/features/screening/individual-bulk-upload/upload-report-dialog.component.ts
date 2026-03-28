import { CommonModule } from '@angular/common';
import { Component, computed, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import type { IndividualBulkUploadReportMode as BulkUploadReportMode, IndividualBulkUploadReportRow as BulkUploadReportRow } from '../../../shared/models/api.model';

type SortKey = keyof BulkUploadReportRow | 'error';

@Component({
  selector: 'app-upload-report-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterLink],
  templateUrl: './upload-report-dialog.component.html',
  styleUrls: ['./upload-report-dialog.component.scss']
})
export class UploadReportDialogComponent {
  readonly logsRoute = '/reports/customer-bulk-upload-logs';

  rows = input.required<BulkUploadReportRow[]>();
  mode = input.required<BulkUploadReportMode>();
  closed = output<void>();

  searchTerm = signal('');
  pageSize = signal(10);
  pageIndex = signal(0);
  sortKey = signal<SortKey>('customerId');
  sortAsc = signal(true);

  filteredRows = computed(() => {
    const q = this.searchTerm().trim().toLowerCase();
    const list = this.rows();
    if (!q) return [...list];
    return list.filter((r) => {
      const blob = [r.customerId, r.fullName, r.country, r.dob, r.placeOfBirth, r.error ?? '']
        .join(' ')
        .toLowerCase();
      return blob.includes(q);
    });
  });

  sortedRows = computed(() => {
    const list = this.filteredRows();
    const key = this.sortKey();
    const asc = this.sortAsc();
    return [...list].sort((a, b) => {
      const av = String(a[key] ?? '');
      const bv = String(b[key] ?? '');
      const cmp = av.localeCompare(bv, undefined, { numeric: true, sensitivity: 'base' });
      return asc ? cmp : -cmp;
    });
  });

  totalFiltered = computed(() => this.filteredRows().length);
  totalPages = computed(() => Math.max(1, Math.ceil(this.sortedRows().length / this.pageSize()) || 1));

  pagedRows = computed(() => {
    const size = this.pageSize();
    const page = this.pageIndex();
    const all = this.sortedRows();
    const start = page * size;
    return all.slice(start, start + size);
  });

  showingFrom = computed(() => {
    const total = this.sortedRows().length;
    if (total === 0) return 0;
    return this.pageIndex() * this.pageSize() + 1;
  });

  showingTo = computed(() => {
    const total = this.sortedRows().length;
    if (total === 0) return 0;
    return Math.min(total, (this.pageIndex() + 1) * this.pageSize());
  });

  failedCount = computed(() => this.rows().filter((r) => (r.error ?? '').trim().length > 0).length);

  queuedCount = computed(() => this.rows().filter((r) => !(r.error ?? '').trim()).length);

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.pageIndex.set(0);
  }

  onPageSizeChange(value: string): void {
    const n = parseInt(value, 10);
    if (!Number.isNaN(n) && n > 0) {
      this.pageSize.set(n);
      this.pageIndex.set(0);
    }
  }

  toggleSort(key: SortKey): void {
    if (this.sortKey() === key) {
      this.sortAsc.update((a) => !a);
    } else {
      this.sortKey.set(key);
      this.sortAsc.set(true);
    }
  }

  sortIndicator(key: SortKey): string {
    if (this.sortKey() !== key) return 'â‡…';
    return this.sortAsc() ? 'â†‘' : 'â†“';
  }

  firstPage(): void {
    this.pageIndex.set(0);
  }

  prevPage(): void {
    this.pageIndex.update((p) => Math.max(0, p - 1));
  }

  nextPage(): void {
    const max = Math.max(0, this.totalPages() - 1);
    this.pageIndex.update((p) => Math.min(max, p + 1));
  }

  lastPage(): void {
    this.pageIndex.set(Math.max(0, this.totalPages() - 1));
  }

  ok(): void {
    this.closed.emit();
  }
}

