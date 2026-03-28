import { CommonModule } from '@angular/common';
import { Component, computed, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import type {
  BulkUploadKind,
  CorporateBulkUploadReportRow,
  IndividualBulkUploadReportMode,
  IndividualBulkUploadReportRow
} from '../../../shared/models/api.model';

type ReportMode = IndividualBulkUploadReportMode;

type IndSortKey = keyof IndividualBulkUploadReportRow | 'error';
type CorSortKey = keyof CorporateBulkUploadReportRow | 'error';

@Component({
  selector: 'app-upload-report-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterLink],
  templateUrl: './upload-report-dialog.component.html',
  styleUrls: ['./upload-report-dialog.component.scss']
})
export class UploadReportDialogComponent {
  readonly logsRoute = '/reports/customer-bulk-upload-logs';

  kind = input.required<BulkUploadKind>();
  indRows = input<IndividualBulkUploadReportRow[]>([]);
  corRows = input<CorporateBulkUploadReportRow[]>([]);
  mode = input.required<ReportMode>();
  closed = output<void>();

  searchTerm = signal('');
  pageSize = signal(10);
  pageIndex = signal(0);
  indSortKey = signal<IndSortKey>('customerId');
  corSortKey = signal<CorSortKey>('customerId');
  sortAsc = signal(true);

  filteredIndRows = computed(() => {
    const q = this.searchTerm().trim().toLowerCase();
    const list = this.indRows();
    if (!q) return [...list];
    return list.filter((r) => {
      const blob = [r.customerId, r.fullName, r.country, r.dob, r.placeOfBirth, r.error ?? '']
        .join(' ')
        .toLowerCase();
      return blob.includes(q);
    });
  });

  filteredCorRows = computed(() => {
    const q = this.searchTerm().trim().toLowerCase();
    const list = this.corRows();
    if (!q) return [...list];
    return list.filter((r) => {
      const blob = [
        r.customerId,
        r.entityName,
        r.incorporatedCountry,
        r.dateOfIncorporation,
        r.companyReferenceCode,
        r.tradeLicense,
        r.error ?? ''
      ]
        .join(' ')
        .toLowerCase();
      return blob.includes(q);
    });
  });

  sortedIndRows = computed(() => {
    const list = this.filteredIndRows();
    const key = this.indSortKey();
    const asc = this.sortAsc();
    return [...list].sort((a, b) => {
      const av = String((a as unknown as Record<string, unknown>)[key] ?? '');
      const bv = String((b as unknown as Record<string, unknown>)[key] ?? '');
      const cmp = av.localeCompare(bv, undefined, { numeric: true, sensitivity: 'base' });
      return asc ? cmp : -cmp;
    });
  });

  sortedCorRows = computed(() => {
    const list = this.filteredCorRows();
    const key = this.corSortKey();
    const asc = this.sortAsc();
    return [...list].sort((a, b) => {
      const av = String((a as unknown as Record<string, unknown>)[key] ?? '');
      const bv = String((b as unknown as Record<string, unknown>)[key] ?? '');
      const cmp = av.localeCompare(bv, undefined, { numeric: true, sensitivity: 'base' });
      return asc ? cmp : -cmp;
    });
  });

  activeSortedRows = computed(() => (this.kind() === 'cor' ? this.sortedCorRows() : this.sortedIndRows()));

  totalFiltered = computed(() =>
    this.kind() === 'cor' ? this.filteredCorRows().length : this.filteredIndRows().length
  );

  totalPages = computed(() => {
    const len = this.activeSortedRows().length;
    const size = this.pageSize();
    return Math.max(1, Math.ceil(len / size) || 1);
  });

  pagedIndRows = computed(() => {
    if (this.kind() !== 'ind') return [] as IndividualBulkUploadReportRow[];
    const size = this.pageSize();
    const page = this.pageIndex();
    const all = this.sortedIndRows();
    const start = page * size;
    return all.slice(start, start + size);
  });

  pagedCorRows = computed(() => {
    if (this.kind() !== 'cor') return [] as CorporateBulkUploadReportRow[];
    const size = this.pageSize();
    const page = this.pageIndex();
    const all = this.sortedCorRows();
    const start = page * size;
    return all.slice(start, start + size);
  });

  showingFrom = computed(() => {
    const total = this.activeSortedRows().length;
    if (total === 0) return 0;
    return this.pageIndex() * this.pageSize() + 1;
  });

  showingTo = computed(() => {
    const total = this.activeSortedRows().length;
    if (total === 0) return 0;
    return Math.min(total, (this.pageIndex() + 1) * this.pageSize());
  });

  failedCount = computed(() => {
    const rows = this.kind() === 'cor' ? this.corRows() : this.indRows();
    return rows.filter((r) => (r.error ?? '').trim().length > 0).length;
  });

  queuedCount = computed(() => {
    const rows = this.kind() === 'cor' ? this.corRows() : this.indRows();
    return rows.filter((r) => !(r.error ?? '').trim()).length;
  });

  queryParamsForLogs = computed(() => ({ kind: this.kind() }));

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

  toggleSortInd(key: IndSortKey): void {
    if (this.indSortKey() === key) {
      this.sortAsc.update((a) => !a);
    } else {
      this.indSortKey.set(key);
      this.sortAsc.set(true);
    }
  }

  toggleSortCor(key: CorSortKey): void {
    if (this.corSortKey() === key) {
      this.sortAsc.update((a) => !a);
    } else {
      this.corSortKey.set(key);
      this.sortAsc.set(true);
    }
  }

  sortIndicatorInd(key: IndSortKey): string {
    if (this.indSortKey() !== key) return '\u21C5';
    return this.sortAsc() ? '\u2191' : '\u2193';
  }

  sortIndicatorCor(key: CorSortKey): string {
    if (this.corSortKey() !== key) return '\u21C5';
    return this.sortAsc() ? '\u2191' : '\u2193';
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
