import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  ApiResponse,
  InstantSanctionScreeningResultItem,
  NationalityDto
} from '../../../shared/models/api.model';

@Component({
  selector: 'app-instant-sanction-screening',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './instant-sanction-screening.component.html',
  styleUrls: ['./instant-sanction-screening.component.scss']
})
export class InstantSanctionScreeningComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  nationalities = signal<NationalityDto[]>([]);
  fullName = signal('');
  nationalityId = signal('');
  dateOfBirth = signal('');

  loading = signal(false);
  hasSearched = signal(false);
  results = signal<InstantSanctionScreeningResultItem[]>([]);

  ngOnInit(): void {
    this.api.getNationalities().subscribe({
      next: (res: ApiResponse<NationalityDto[]>) => {
        if (res.success && res.data) this.nationalities.set(res.data);
      }
    });
  }

  search(): void {
    const name = this.fullName().trim();
    if (name.length < 2) {
      this.notification.info(this.translate.instant('instantSanctionScreening.nameMinLength'));
      return;
    }

    const dob = this.dateOfBirth().trim();
    const payload = {
      fullName: name,
      nationalityId: this.nationalityId().trim() || null,
      dateOfBirth: dob ? `${dob}T12:00:00` : null
    };

    this.loading.set(true);
    this.api.searchInstantSanctionScreening(payload).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.hasSearched.set(true);
        if (res.success && res.data) {
          this.results.set(res.data);
        } else {
          this.results.set([]);
          if (res.message) this.notification.error(res.message);
        }
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.hasSearched.set(true);
        this.results.set([]);
        const msg = err instanceof HttpErrorResponse ? (err.error as ApiResponse | null)?.message : undefined;
        this.notification.error(msg?.trim() || this.translate.instant('instantSanctionScreening.searchFailed'));
      }
    });
  }

  reset(): void {
    this.fullName.set('');
    this.nationalityId.set('');
    this.dateOfBirth.set('');
    this.results.set([]);
    this.hasSearched.set(false);
  }

  exportPdf(): void {
    const rows = this.results();
    if (rows.length === 0) {
      this.notification.info(this.translate.instant('instantSanctionScreening.noRowsToExport'));
      return;
    }

    const doc = new jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a4' });
    const title = this.translate.instant('pages.instantSanctionScreening.title');
    doc.setFontSize(11);
    doc.text(title, 40, 32);

    const head = [
      [
        this.translate.instant('instantSanctionScreening.colScore'),
        this.translate.instant('instantSanctionScreening.colCustomerId'),
        this.translate.instant('instantSanctionScreening.colUid'),
        this.translate.instant('instantSanctionScreening.colType'),
        this.translate.instant('instantSanctionScreening.colName'),
        this.translate.instant('instantSanctionScreening.colNationality'),
        this.translate.instant('instantSanctionScreening.colDob'),
        this.translate.instant('instantSanctionScreening.colIdNumber'),
        this.translate.instant('instantSanctionScreening.colSource'),
        this.translate.instant('instantSanctionScreening.colCreatedOn'),
        this.translate.instant('instantSanctionScreening.colRemarks')
      ]
    ];

    const body = rows.map((r) => [
      String(r.matchScore),
      r.customerId ?? '—',
      r.uid,
      r.entryType ?? '—',
      r.name ?? '—',
      r.nationalityOrCountry ?? '—',
      this.formatCellDate(r.dateOfBirth),
      r.idNumber ?? '—',
      r.source,
      this.formatCellDate(r.createdOn),
      r.remarks ?? '—'
    ]);

    autoTable(doc, {
      startY: 44,
      head,
      body,
      styles: { fontSize: 7, cellPadding: 3 },
      headStyles: { fillColor: [74, 144, 226], textColor: 255 },
      alternateRowStyles: { fillColor: [245, 245, 245] },
      margin: { left: 36, right: 36 }
    });

    doc.save('instant-sanction-screening.pdf');
  }

  formatCellDate(iso?: string | null): string {
    if (!iso) return '—';
    const d = new Date(iso);
    return Number.isNaN(d.getTime()) ? iso : d.toLocaleDateString();
  }
}
