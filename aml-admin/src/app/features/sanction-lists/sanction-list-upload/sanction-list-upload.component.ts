import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmService } from '../../../shared/services/confirm.service';
import type {
    ApiResponse,
    PagedResult,
    SanctionListSourceDto,
    SanctionListUploadResultDto,
    SanctionListEntryDto
  } from '../../../shared/models/api.model';

@Component({
  selector: 'app-sanction-list-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './sanction-list-upload.component.html'
})
export class SanctionListUploadComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);
  private readonly confirm = inject(ConfirmService);

  sources = signal<SanctionListSourceDto[]>([]);
  loadingSources = signal(true);
  selectedSourceId = signal('');
  selectedFile = signal<File | null>(null);
  uploading = signal(false);
  uploadError = signal<string | null>(null);
  uploadSuccess = signal<string | null>(null);
  lastResult = signal<SanctionListUploadResultDto | null>(null);

  showUploadSection = signal(true);
  entriesResult = signal<PagedResult<SanctionListEntryDto> | null>(null);
  loadingEntries = signal(false);
  searchTerm = signal('');
  filterListSource = signal('');
  pageNumber = signal(1);
  readonly pageSize = 20;
  deletingBySource = signal(false);

  showAddManualSection = signal(false);
  manualListSource = signal('');
  manualFullName = signal('');
  manualNationality = signal('');
  manualDateOfBirth = signal('');
  manualReferenceNumber = signal('');
  manualEntryType = signal('');
  savingManual = signal(false);
  manualError = signal<string | null>(null);

  toggleAddManualSection(): void {
    this.showAddManualSection.update(v => !v);
    if (!this.showAddManualSection()) this.manualError.set(null);
  }

  saveManualEntry(): void {
    const listSource = this.manualListSource().trim();
    const fullName = this.manualFullName().trim();
    if (!listSource) {
      this.manualError.set(this.translate.instant('sanctionLists.selectSource'));
      return;
    }
    if (!fullName) {
      this.manualError.set(this.translate.instant('sanctionLists.fullNameRequired'));
      return;
    }
    this.manualError.set(null);
    this.savingManual.set(true);
    const dob = this.manualDateOfBirth().trim();
    this.api.createSanctionEntry({
      listSource,
      fullName,
      nationality: this.manualNationality().trim() || null,
      dateOfBirth: dob ? (this.parseDate(dob) ?? undefined) : undefined,
      referenceNumber: this.manualReferenceNumber().trim() || null,
      entryType: this.manualEntryType().trim() || null
    }).subscribe({
      next: (res: ApiResponse<SanctionListEntryDto>) => {
        this.savingManual.set(false);
        if (res.success) {
          this.notification.success(this.translate.instant('sanctionLists.manualEntrySuccess'));
          this.manualFullName.set('');
          this.manualNationality.set('');
          this.manualDateOfBirth.set('');
          this.manualReferenceNumber.set('');
          this.manualEntryType.set('');
          this.loadEntries();
        } else {
          this.manualError.set(res.message ?? this.translate.instant('common.errorGeneric'));
        }
      },
      error: (err: unknown) => {
        this.savingManual.set(false);
        const errMsg: string = (err as { error?: { message?: string } })?.error?.message ?? this.translate.instant('common.errorGeneric');
        this.manualError.set(errMsg);
        this.notification.error(errMsg);
      }
    });
  }

  private parseDate(s: string): string | null {
    if (!s) return null;
    const d = new Date(s);
    return isNaN(d.getTime()) ? null : d.toISOString().slice(0, 10);
  }

  toggleUploadSection(): void {
    this.showUploadSection.update(v => !v);
  }

  loadEntries(): void {
    this.loadingEntries.set(true);
    this.api.getSanctionListEntries({
      searchTerm: this.searchTerm() || undefined,
      listSource: this.filterListSource() || undefined,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize
    }).subscribe({
      next: (res: ApiResponse<PagedResult<SanctionListEntryDto>>) => {
        this.loadingEntries.set(false);
        if (res.success && res.data) this.entriesResult.set(res.data);
        else this.entriesResult.set(null);
      },
      error: () => {
        this.loadingEntries.set(false);
        this.entriesResult.set(null);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  onSearch(): void {
    this.pageNumber.set(1);
    this.loadEntries();
  }

  goToPage(page: number): void {
    this.pageNumber.set(page);
    this.loadEntries();
  }

  deleteEntriesForSource(): void {
    const listSource = this.filterListSource()?.trim();
    if (!listSource) {
      this.notification.error(this.translate.instant('sanctionLists.selectSourceToDelete'));
      return;
    }
    const sourceName = this.sources().find(s => s.id === listSource)?.name ?? listSource;
    const title = this.translate.instant('sanctionLists.deleteEntriesTitle');
    const message = this.translate.instant('sanctionLists.deleteEntriesMessage', { source: sourceName });
    this.confirm.confirm(title, message, this.translate.instant('common.delete')).subscribe(ok => {
      if (!ok) return;
      this.deletingBySource.set(true);
      this.api.deleteSanctionListBySource(listSource).subscribe({
        next: (res: ApiResponse<number>) => {
          this.deletingBySource.set(false);
          if (res.success && res.data != null) {
            this.notification.success(this.translate.instant('sanctionLists.deleteEntriesSuccess', { count: res.data }));
            this.loadEntries();
          } else {
            this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
          }
        },
        error: (err: unknown) => {
          this.deletingBySource.set(false);
          const errMsg: string = (err as { error?: { message?: string } })?.error?.message ?? this.translate.instant('common.errorGeneric');
          this.notification.error(errMsg);
        }
      });
    });
  }

  formatDate(value: string | null | undefined): string {
    if (!value) return '—';
    try {
      const d = new Date(value);
      return isNaN(d.getTime()) ? value : d.toLocaleDateString();
    } catch {
      return value;
    }
  }

  /** Returns at most 4 page numbers plus ellipsis (-1) for pagination. */
  getVisiblePageNumbers(currentPage: number, totalPages: number): number[] {
    const total = Math.max(0, totalPages);
    const current = Math.max(1, Math.min(currentPage, total));
    if (total <= 4) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }
    const out: number[] = [];
    out.push(1);
    if (current > 2) out.push(-1);
    if (current > 1 && current < total) out.push(current);
    if (current < total - 1) out.push(current + 1);
    else if (current === total && total > 2) out.push(current - 1);
    if (current < total - 2) out.push(-1);
    if (total > 1) out.push(total);
    return out;
  }

  ngOnInit(): void {
    this.api.getSanctionListSources().subscribe({
      next: (res: ApiResponse<SanctionListSourceDto[]>) => {
        this.loadingSources.set(false);
        if (res.success && res.data && res.data.length > 0) {
          this.sources.set(res.data);
          if (!this.selectedSourceId()) this.selectedSourceId.set(res.data[0].id);
        }
        this.loadEntries();
      },
      error: () => {
        this.loadingSources.set(false);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  onSourceChange(id: string): void {
    this.selectedSourceId.set(id);
    this.uploadError.set(null);
    this.uploadSuccess.set(null);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0];
    this.selectedFile.set(file ?? null);
    this.uploadError.set(null);
    this.uploadSuccess.set(null);
  }

  upload(): void {
    this.uploadError.set(null);
    this.uploadSuccess.set(null);
    this.lastResult.set(null);
    const file = this.selectedFile();
    const sourceId = this.selectedSourceId();
    if (!sourceId) {
      this.uploadError.set(this.translate.instant('sanctionLists.selectSource'));
      return;
    }
    if (!file) {
      this.uploadError.set(this.translate.instant('sanctionLists.selectFile'));
      return;
    }
    this.uploading.set(true);
    this.api.uploadSanctionList(file, sourceId).subscribe({
      next: (res: ApiResponse<SanctionListUploadResultDto>) => {
        this.uploading.set(false);
        if (res.success && res.data) {
          this.lastResult.set(res.data);
          const msg = this.translate.instant('sanctionLists.uploadSuccess', {
            count: res.data.importedCount,
            source: this.sources().find(s => s.id === sourceId)?.name ?? sourceId
          });
          this.uploadSuccess.set(msg);
          this.notification.success(msg);
          this.selectedFile.set(null);
          const input = document.querySelector('input[type="file"][name="sanctionListFile"]') as HTMLInputElement;
          if (input) input.value = '';
          this.loadEntries();
        } else {
          const errMsg: string = res.message ?? this.translate.instant('common.errorGeneric');
          this.uploadError.set(errMsg);
          this.notification.error(errMsg);
        }
      },
      error: (err: unknown) => {
        this.uploading.set(false);
        const errMsg: string = (err as { error?: { message?: string } })?.error?.message ?? this.translate.instant('common.errorGeneric');
        this.uploadError.set(errMsg);
        this.notification.error(errMsg);
      }
    });
  }
}
