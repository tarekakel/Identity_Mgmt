import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { combineLatest } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import type {
  BulkUploadKind,
  CorporateBulkUploadReportRow,
  CorporateBulkUploadResultDto,
  IndividualBulkUploadReportMode,
  IndividualBulkUploadReportRow,
  IndividualBulkUploadResultDto
} from '../../../shared/models/api.model';
import { UploadReportDialogComponent } from './upload-report-dialog.component';

@Component({
  selector: 'app-bulk-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, UploadReportDialogComponent],
  templateUrl: './bulk-upload.component.html',
  styleUrls: ['./bulk-upload.component.scss']
})
export class BulkUploadComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly translate = inject(TranslateService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  bulkKind = signal<BulkUploadKind>('ind');

  checkPepUkOnly = signal(true);
  checkDisqualifiedDirectorUkOnly = signal(true);
  checkSanctions = signal(true);
  checkProfileOfInterest = signal(true);
  checkReputationalRiskExposure = signal(true);
  checkRegulatoryEnforcementList = signal(true);
  checkInsolvencyUkIreland = signal(true);

  matchThreshold = signal(85);
  selectedFile = signal<File | null>(null);
  dragOver = signal(false);
  submitting = signal(false);
  submitError = signal<string | null>(null);
  sampleDownloading = signal(false);

  reportOpen = signal(false);
  indReportRows = signal<IndividualBulkUploadReportRow[]>([]);
  corReportRows = signal<CorporateBulkUploadReportRow[]>([]);
  reportMode = signal<IndividualBulkUploadReportMode>('queued');

  ngOnInit(): void {
    combineLatest([this.route.queryParamMap, this.route.data])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([q, d]) => {
        const qk = q.get('kind');
        if (qk === 'ind' || qk === 'cor') {
          this.bulkKind.set(qk);
          return;
        }
        const dk = d['bulkKind'] as BulkUploadKind | undefined;
        if (dk === 'ind' || dk === 'cor') {
          this.bulkKind.set(dk);
          return;
        }
        this.bulkKind.set('ind');
      });
  }

  selectAllCategories(): void {
    this.checkPepUkOnly.set(true);
    this.checkDisqualifiedDirectorUkOnly.set(true);
    this.checkSanctions.set(true);
    this.checkProfileOfInterest.set(true);
    this.checkReputationalRiskExposure.set(true);
    this.checkRegulatoryEnforcementList.set(true);
    this.checkInsolvencyUkIreland.set(true);
  }

  clearAllCategories(): void {
    this.checkPepUkOnly.set(false);
    this.checkDisqualifiedDirectorUkOnly.set(false);
    this.checkSanctions.set(false);
    this.checkProfileOfInterest.set(false);
    this.checkReputationalRiskExposure.set(false);
    this.checkRegulatoryEnforcementList.set(false);
    this.checkInsolvencyUkIreland.set(false);
  }

  setThreshold(value: unknown): void {
    let n = typeof value === 'string' ? parseInt(value, 10) : Number(value);
    if (Number.isNaN(n)) n = 85;
    n = Math.min(100, Math.max(75, Math.round(n)));
    this.matchThreshold.set(n);
  }

  downloadSample(): void {
    this.sampleDownloading.set(true);
    this.submitError.set(null);
    const kind = this.bulkKind();
    this.api.downloadBulkSample(kind).subscribe({
      next: (blob) => {
        this.sampleDownloading.set(false);
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download =
          kind === 'cor'
            ? 'Data-Customer_Bulk_upload_Sample_Corporate.xlsx'
            : 'Data-Customer_Bulk_upload_Sample_Individual.xlsx';
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.sampleDownloading.set(false);
        const key =
          kind === 'cor' ? 'corporateBulkUpload.sampleDownloadFailed' : 'individualBulkUpload.sampleDownloadFailed';
        this.submitError.set(this.translate.instant(key));
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.selectedFile.set(file ?? null);
    this.submitError.set(null);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.selectedFile.set(file);
      this.submitError.set(null);
    }
  }

  onCancel(): void {
    this.selectedFile.set(null);
    this.submitError.set(null);
  }

  onSubmit(): void {
    this.submitError.set(null);
    const file = this.selectedFile();
    if (!file) {
      const key = this.bulkKind() === 'cor' ? 'corporateBulkUpload.fileRequired' : 'individualBulkUpload.fileRequired';
      this.submitError.set(this.translate.instant(key));
      return;
    }
    this.submitting.set(true);
    const kind = this.bulkKind();
    const options = {
      matchThreshold: this.matchThreshold(),
      checkPepUkOnly: this.checkPepUkOnly(),
      checkDisqualifiedDirectorUkOnly: this.checkDisqualifiedDirectorUkOnly(),
      checkSanctions: this.checkSanctions(),
      checkProfileOfInterest: this.checkProfileOfInterest(),
      checkReputationalRiskExposure: this.checkReputationalRiskExposure(),
      checkRegulatoryEnforcementList: this.checkRegulatoryEnforcementList(),
      checkInsolvencyUkIreland: this.checkInsolvencyUkIreland()
    };
    this.api.uploadBulk(kind, file, options).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.success && res.data) {
          if (kind === 'cor') {
            this.applyCorporateUploadResult(res.data as CorporateBulkUploadResultDto);
          } else {
            this.applyIndividualUploadResult(res.data as IndividualBulkUploadResultDto);
          }
          return;
        }
        this.submitError.set(res.message ?? this.translate.instant('common.errorGeneric'));
      },
      error: (err: { error?: { message?: string } }) => {
        this.submitting.set(false);
        const msg = err?.error?.message ?? this.translate.instant('common.errorGeneric');
        this.submitError.set(msg);
      }
    });
  }

  private applyIndividualUploadResult(data: IndividualBulkUploadResultDto): void {
    const rows: IndividualBulkUploadReportRow[] = (data.rows ?? []).map((r) => ({
      customerId: r.customerId,
      fullName: r.fullName,
      country: r.country,
      dob: r.dob,
      placeOfBirth: r.placeOfBirth,
      error: r.error
    }));
    this.indReportRows.set(rows);
    this.corReportRows.set([]);
    const mode: IndividualBulkUploadReportMode =
      data.mode === 'validationFailed' ? 'validationFailed' : 'queued';
    this.reportMode.set(mode);
    this.reportOpen.set(true);
  }

  private applyCorporateUploadResult(data: CorporateBulkUploadResultDto): void {
    const rows: CorporateBulkUploadReportRow[] = (data.rows ?? []).map((r) => ({
      customerId: r.customerId,
      entityName: r.entityName,
      incorporatedCountry: r.incorporatedCountry,
      dateOfIncorporation: r.dateOfIncorporation,
      companyReferenceCode: r.companyReferenceCode,
      tradeLicense: r.tradeLicense,
      error: r.error
    }));
    this.corReportRows.set(rows);
    this.indReportRows.set([]);
    const mode: IndividualBulkUploadReportMode =
      data.mode === 'validationFailed' ? 'validationFailed' : 'queued';
    this.reportMode.set(mode);
    this.reportOpen.set(true);
  }

  closeReport(): void {
    this.reportOpen.set(false);
  }
}
