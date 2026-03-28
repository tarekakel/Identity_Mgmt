import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type { IndividualBulkUploadBatchListItemDto, IndividualBulkUploadLineDetailDto } from '../../../shared/models/api.model';

export interface BulkUploadLogFileRow {
  id: string;
  fileName: string;
  uploadedOn: string;
  uploadedBy: string;
  screeningFinished: boolean;
}

export interface BulkUploadLogDetailRow {
  id: string;
  fileId: string;
  index: number;
  customerId: string;
  customerName: string;
  nationality: string;
  dateOfBirth: string;
  status: string;
  source: string;
  uid: string;
  date: string;
}

@Component({
  selector: 'app-customer-bulk-upload-logs',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterLink],
  templateUrl: './customer-bulk-upload-logs.component.html',
  styleUrls: ['./customer-bulk-upload-logs.component.scss']
})
export class CustomerBulkUploadLogsComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly notification = inject(NotificationService);

  readonly mockUsers = ['Hazem Barakat', 'Admin User', 'Compliance Officer'];

  fileRows = signal<BulkUploadLogFileRow[]>([]);
  detailRows = signal<BulkUploadLogDetailRow[]>([]);
  loadingFiles = signal(false);
  loadingDetails = signal(false);

  startDate = signal('2026-03-21');
  endDate = signal('2026-03-28');
  uploadedByFilter = signal('');
  caseStatusFilter = signal('all');

  selectedFileId = signal<string | null>(null);


  ngOnInit(): void {
    this.loadBatches();
  }

  loadBatches(): void {
    this.loadingFiles.set(true);
    this.api
      .getIndividualBulkBatches({
        from: this.startDate() ? `${this.startDate()}T00:00:00Z` : undefined,
        to: this.endDate() ? `${this.endDate()}T23:59:59Z` : undefined,
        uploadedBy: this.uploadedByFilter().trim() || undefined
      })
      .subscribe({
        next: (res) => {
          this.loadingFiles.set(false);
          if (!res.success || !res.data) {
            this.fileRows.set([]);
                        return;
          }
          const rows = res.data.map((b) => this.mapBatch(b));
          this.fileRows.set(rows);
                    if (rows.length > 0) {
            const sel = this.selectedFileId();
            const still = sel && rows.some((r) => r.id === sel);
            if (!still) {
              this.selectedFileId.set(rows[0].id);
              this.loadLines();
            } else {
              this.loadLines();
            }
          } else {
            this.selectedFileId.set(null);
            this.detailRows.set([]);
          }
        },
        error: () => {
          this.loadingFiles.set(false);
          this.notification.error('Failed to load upload logs.');
        }
      });
  }

  private mapBatch(b: IndividualBulkUploadBatchListItemDto): BulkUploadLogFileRow {
    const uploaded = b.uploadedOn ? new Date(b.uploadedOn) : new Date();
    return {
      id: b.id,
      fileName: b.fileName,
      uploadedOn: uploaded.toLocaleString(),
      uploadedBy: b.uploadedBy,
      screeningFinished: b.screeningFinished
    };
  }

  loadLines(): void {
    const batchId = this.selectedFileId();
    if (!batchId) {
      this.detailRows.set([]);
      return;
    }
    this.loadingDetails.set(true);
    const cs = this.caseStatusFilter();
    this.api.getIndividualBulkBatchLines(batchId, cs === 'all' ? undefined : cs).subscribe({
      next: (res) => {
        this.loadingDetails.set(false);
        if (!res.success || !res.data) {
          this.detailRows.set([]);
          return;
        }
        const mapped: BulkUploadLogDetailRow[] = res.data.map((d) => ({
          id: d.uid,
          fileId: batchId,
          index: d.index,
          customerId: d.customerId,
          customerName: d.customerName,
          nationality: d.nationality,
          dateOfBirth: d.dateOfBirth,
          status: d.status,
          source: d.source,
          uid: d.uid,
          date: d.date
        }));
        this.detailRows.set(mapped);
      },
      error: () => {
        this.loadingDetails.set(false);
        this.notification.error('Failed to load line details.');
      }
    });
  }

  onSearchUpper(): void {
    this.loadBatches();
  }

  onSearchLower(): void {
    this.loadLines();
  }

  exportPdf(): void {
    this.notification.info('Export to PDF — coming soon.');
  }

  exportExcel(): void {
    this.notification.info('Export to Excel — coming soon.');
  }

  selectFileRow(row: BulkUploadLogFileRow): void {
    this.selectedFileId.set(row.id);
    this.loadLines();
  }
}


