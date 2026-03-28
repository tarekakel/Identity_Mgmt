import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import type {
  IndividualBulkUploadReportMode,
  IndividualBulkUploadReportRow,
  IndividualBulkUploadResultDto
} from '../../../shared/models/api.model';
import { UploadReportDialogComponent } from './upload-report-dialog.component';

@Component({
  selector: 'app-individual-bulk-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, UploadReportDialogComponent],
  templateUrl: './individual-bulk-upload.component.html',
  styleUrls: ['./individual-bulk-upload.component.scss']
})
export class IndividualBulkUploadComponent {
  private readonly api = inject(ApiService);
  private readonly translate = inject(TranslateService);

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
  reportRows = signal<IndividualBulkUploadReportRow[]>([]);
  reportMode = signal<IndividualBulkUploadReportMode>('queued');

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
    this.api.downloadIndividualBulkSample().subscribe({
      next: (blob) => {
        this.sampleDownloading.set(false);
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Data-Customer_Bulk_upload_Sample_Individual.xlsx';
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.sampleDownloading.set(false);
        this.submitError.set(this.translate.instant('individualBulkUpload.sampleDownloadFailed'));
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
      this.submitError.set(this.translate.instant('individualBulkUpload.fileRequired'));
      return;
    }
    this.submitting.set(true);
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
    this.api.uploadIndividualBulk(file, options).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.success && res.data) {
          this.applyUploadResult(res.data);
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

  private applyUploadResult(data: IndividualBulkUploadResultDto): void {
    const rows: IndividualBulkUploadReportRow[] = (data.rows ?? []).map((r) => ({
      customerId: r.customerId,
      fullName: r.fullName,
      country: r.country,
      dob: r.dob,
      placeOfBirth: r.placeOfBirth,
      error: r.error
    }));
    this.reportRows.set(rows);
    const mode: IndividualBulkUploadReportMode =
      data.mode === 'validationFailed' ? 'validationFailed' : 'queued';
    this.reportMode.set(mode);
    this.reportOpen.set(true);
  }

  closeReport(): void {
    this.reportOpen.set(false);
  }
}
