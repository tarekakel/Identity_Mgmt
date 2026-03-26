import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { IndividualScreeningService } from '../../../core/services/individual-screening.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  ApiResponse,
  CountryDto,
  CustomerDocumentDto,
  CustomerDto,
  CustomerTypeDto,
  DocumentTypeDto,
  GenderDto,
  CreateCustomerRequest,
  IndividualScreeningRequestDto,
  NationalityDto,
  PagedResult,
  SanctionsScreeningResultItemDto,
  UpsertIndividualScreeningRequest
} from '../../../shared/models/api.model';

const DEFAULT_TENANT_ID = '00000000-0000-0000-0000-000000000001';
const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const ALLOWED_EXTENSIONS = ['.pdf', '.jpg', '.jpeg', '.png'];

@Component({
  selector: 'app-individual-screening',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './individual-screening.component.html',
  styleUrls: ['./individual-screening.component.scss']
})
export class IndividualScreeningComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly screeningApi = inject(IndividualScreeningService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);

  // Customer picker
  customerSearchTerm = signal('');
  customerOptions = signal<CustomerDto[]>([]);
  customerLoading = signal(false);
  customerDropdownOpen = signal(false);
  selectedCustomer = signal<CustomerDto | null>(null);
  selectedCustomerId = computed(() => this.selectedCustomer()?.id ?? '');

  // Lookups
  genders = signal<GenderDto[]>([]);
  nationalities = signal<NationalityDto[]>([]);
  countries = signal<CountryDto[]>([]);
  documentTypes = signal<DocumentTypeDto[]>([]);
  customerTypes = signal<CustomerTypeDto[]>([]);

  // Form fields
  referenceId = signal('');
  fullName = signal('');
  dateOfBirth = signal(''); // yyyy-MM-dd
  nationalityId = signal('');
  placeOfBirthCountryId = signal('');
  idType = signal('');
  idNumber = signal('');
  address = signal('');
  genderId = signal('');
  matchThreshold = signal(85);
  birthYearRange = signal<number | null>(null);

  // Categories
  checkPepUkOnly = signal(true);
  checkDisqualifiedDirectorUkOnly = signal(true);
  checkSanctions = signal(true);
  checkProfileOfInterest = signal(true);
  checkReputationalRiskExposure = signal(true);
  checkRegulatoryEnforcementList = signal(true);
  checkInsolvencyUkIreland = signal(true);

  // State
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  lastSaved = signal<IndividualScreeningRequestDto | null>(null);

  // Results
  results = signal<SanctionsScreeningResultItemDto[]>([]);
  resultsLoading = signal(false);

  // Documents
  documents = signal<CustomerDocumentDto[]>([]);
  selectedDocumentTypeCode = signal('');
  documentExpiryDate = signal('');
  selectedFile = signal<File | null>(null);
  uploading = signal(false);
  uploadError = signal<string | null>(null);
  uploadSuccess = signal<string | null>(null);
  loadingDocuments = signal(false);
  isPassport = computed(() => this.selectedDocumentTypeCode() === 'Passport');

  ngOnInit(): void {
    this.loadLookups();
  }

  private loadLookups(): void {
    this.api.getGenders().subscribe({
      next: (res: ApiResponse<GenderDto[]>) => {
        if (res.success && res.data) this.genders.set(res.data);
      }
    });
    this.api.getNationalities().subscribe({
      next: (res: ApiResponse<NationalityDto[]>) => {
        if (res.success && res.data) this.nationalities.set(res.data);
      }
    });
    this.api.getCountries().subscribe({
      next: (res: ApiResponse<CountryDto[]>) => {
        if (res.success && res.data) this.countries.set(res.data);
      }
    });
    this.api.getDocumentTypes().subscribe({
      next: (res: ApiResponse<DocumentTypeDto[]>) => {
        if (res.success && res.data) this.documentTypes.set(res.data);
      }
    });

    this.api.getCustomerTypes().subscribe({
      next: (res: ApiResponse<CustomerTypeDto[]>) => {
        if (res.success && res.data) this.customerTypes.set(res.data);
      }
    });
  }

  onCustomerSearchChange(value: string): void {
    this.customerSearchTerm.set(value);
    const term = value.trim();
    this.customerDropdownOpen.set(true);
    if (term.length < 2) {
      this.customerOptions.set([]);
      return;
    }
    this.customerLoading.set(true);
    this.api.getCustomers({ pageNumber: 1, pageSize: 10, searchTerm: term }).subscribe({
      next: (res: ApiResponse<PagedResult<CustomerDto>>) => {
        this.customerLoading.set(false);
        const items = res.success && res.data?.items ? res.data.items : [];
        this.customerOptions.set(items);
      },
      error: () => {
        this.customerLoading.set(false);
        this.customerOptions.set([]);
      }
    });
  }

  selectCustomer(c: CustomerDto): void {
    this.selectedCustomer.set(c);
    this.customerSearchTerm.set(c.fullName ?? '');
    this.customerDropdownOpen.set(false);
    this.customerOptions.set([]);
    this.error.set(null);
    this.success.set(null);

    this.loadLatestCriteria();
    this.refreshResults();
    this.loadDocuments();
  }

  private loadLatestCriteria(): void {
    const cid = this.selectedCustomerId();
    if (!cid) return;
    this.loading.set(true);
    this.screeningApi.getLatest(cid).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.lastSaved.set(res.data);
          this.applyRequestToForm(res.data);
        } else {
          this.lastSaved.set(null);
          this.resetFormToDefaults();
        }
      },
      error: () => {
        this.loading.set(false);
        this.lastSaved.set(null);
        this.resetFormToDefaults();
      }
    });
  }

  private applyRequestToForm(req: IndividualScreeningRequestDto): void {
    this.referenceId.set(req.referenceId ?? '');
    this.fullName.set(req.fullName ?? '');
    this.dateOfBirth.set(req.dateOfBirth ? this.toDateInputValue(req.dateOfBirth) : '');
    this.nationalityId.set(req.nationalityId ?? '');
    this.placeOfBirthCountryId.set(req.placeOfBirthCountryId ?? '');
    this.idType.set(req.idType ?? '');
    this.idNumber.set(req.idNumber ?? '');
    this.address.set(req.address ?? '');
    this.genderId.set(req.genderId ?? '');
    this.matchThreshold.set(req.matchThreshold ?? 85);
    this.birthYearRange.set(req.birthYearRange ?? null);

    this.checkPepUkOnly.set(!!req.checkPepUkOnly);
    this.checkDisqualifiedDirectorUkOnly.set(!!req.checkDisqualifiedDirectorUkOnly);
    this.checkSanctions.set(!!req.checkSanctions);
    this.checkProfileOfInterest.set(!!req.checkProfileOfInterest);
    this.checkReputationalRiskExposure.set(!!req.checkReputationalRiskExposure);
    this.checkRegulatoryEnforcementList.set(!!req.checkRegulatoryEnforcementList);
    this.checkInsolvencyUkIreland.set(!!req.checkInsolvencyUkIreland);
  }

  private resetFormToDefaults(): void {
    this.referenceId.set('');
    this.fullName.set('');
    this.dateOfBirth.set('');
    this.nationalityId.set('');
    this.placeOfBirthCountryId.set('');
    this.idType.set('');
    this.idNumber.set('');
    this.address.set('');
    this.genderId.set('');
    this.matchThreshold.set(85);
    this.birthYearRange.set(null);

    this.selectAllCategories();
  }

  resetToLastSaved(): void {
    this.error.set(null);
    this.success.set(null);
    const last = this.lastSaved();
    if (last) {
      this.applyRequestToForm(last);
      return;
    }
    this.resetFormToDefaults();
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

  submit(): void {
    this.error.set(null);
    this.success.set(null);
    const cid = this.selectedCustomerId();
    if (!this.fullName().trim()) {
      this.error.set(this.translate.instant('individualScreening.errors.fullNameRequired'));
      return;
    }

    const buildUpsertBody = (): UpsertIndividualScreeningRequest => ({
      tenantId: DEFAULT_TENANT_ID,
      referenceId: this.referenceId().trim() || null,
      fullName: this.fullName().trim(),
      dateOfBirth: this.dateOfBirth().trim() ? new Date(this.dateOfBirth()).toISOString() : null,
      nationalityId: this.nationalityId() || null,
      placeOfBirthCountryId: this.placeOfBirthCountryId() || null,
      idType: this.idType().trim() || null,
      idNumber: this.idNumber().trim() || null,
      address: this.address().trim() || null,
      genderId: this.genderId() || null,
      matchThreshold: Number(this.matchThreshold() ?? 85),
      birthYearRange: this.birthYearRange() ?? null,
      checkPepUkOnly: !!this.checkPepUkOnly(),
      checkSanctions: !!this.checkSanctions(),
      checkProfileOfInterest: !!this.checkProfileOfInterest(),
      checkDisqualifiedDirectorUkOnly: !!this.checkDisqualifiedDirectorUkOnly(),
      checkReputationalRiskExposure: !!this.checkReputationalRiskExposure(),
      checkRegulatoryEnforcementList: !!this.checkRegulatoryEnforcementList(),
      checkInsolvencyUkIreland: !!this.checkInsolvencyUkIreland()
    });

    const upsertForCustomer = (customerId: string): void => {
      this.loading.set(true);
      const body = buildUpsertBody();
      this.screeningApi.upsert(customerId, body).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.lastSaved.set(res.data);
            this.applyRequestToForm(res.data);
            this.runAndRefresh();
          } else {
            this.loading.set(false);
            const msg = res.message ?? this.translate.instant('common.saveFailed');
            this.error.set(msg);
            this.notification.error(msg);
          }
        },
        error: (err) => {
          this.loading.set(false);
          const msg = err?.error?.message ?? this.translate.instant('common.saveFailed');
          this.error.set(msg);
          this.notification.error(msg);
        }
      });
    };

    if (cid) {
      upsertForCustomer(cid);
      return;
    }

    // No customer selected: auto-create an "Individual" customer, then upsert screening.
    const individualTypeId = this.customerTypes().find(t => t.code === 'Individual')?.id;
    if (!individualTypeId) {
      this.error.set(this.translate.instant('common.errorGeneric'));
      return;
    }

    const rawIdType = this.idType().trim();
    const isPassportIdType = rawIdType.toLowerCase().includes('passport');

    const createPayload: CreateCustomerRequest = {
      tenantId: DEFAULT_TENANT_ID,
      fullName: this.fullName().trim(),
      dateOfBirth: this.dateOfBirth().trim() ? new Date(this.dateOfBirth()).toISOString() : undefined,
      genderId: this.genderId() || undefined,
      nationalityId: this.nationalityId() || undefined,
      passportNumber: isPassportIdType ? (this.idNumber().trim() || undefined) : undefined,
      passportExpiryDate: isPassportIdType && this.documentExpiryDate().trim()
        ? new Date(this.documentExpiryDate()).toISOString()
        : undefined,
      nationalId: !isPassportIdType ? (this.idNumber().trim() || undefined) : undefined,
      countryOfResidenceId: this.placeOfBirthCountryId() || undefined,
      address: this.address().trim() || undefined,
      customerTypeId: individualTypeId
    };

    this.loading.set(true);
    this.api.createCustomer(createPayload).subscribe({
      next: (res: ApiResponse<CustomerDto>) => {
        if (res.success && res.data) {
          this.selectedCustomer.set(res.data);
          this.referenceId.set(res.data.customerNumber ?? res.data.id);
          this.loadDocuments();
          upsertForCustomer(res.data.id);
        } else {
          this.loading.set(false);
          const msg = res.message ?? this.translate.instant('common.saveFailed');
          this.error.set(msg);
          this.notification.error(msg);
        }
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.message ?? this.translate.instant('common.saveFailed');
        this.error.set(msg);
        this.notification.error(msg);
      }
    });
  }

  private runAndRefresh(): void {
    const cid = this.selectedCustomerId();
    if (!cid) return;
    this.screeningApi.run(cid).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.success.set(this.translate.instant('individualScreening.savedAndRun'));
          this.notification.success(this.translate.instant('individualScreening.savedAndRun'));
          this.refreshResults();
        } else {
          const msg = res.message ?? this.translate.instant('common.errorGeneric');
          this.error.set(msg);
          this.notification.error(msg);
        }
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.message ?? this.translate.instant('common.errorGeneric');
        this.error.set(msg);
        this.notification.error(msg);
      }
    });
  }

  refreshResults(): void {
    const cid = this.selectedCustomerId();
    if (!cid) {
      this.results.set([]);
      return;
    }
    this.resultsLoading.set(true);
    this.screeningApi.getResults(cid).subscribe({
      next: (res) => {
        this.resultsLoading.set(false);
        if (res.success && res.data) {
          this.results.set(res.data);
        } else {
          this.results.set([]);
        }
      },
      error: () => {
        this.resultsLoading.set(false);
        this.results.set([]);
      }
    });
  }

  proceedToKyc(): void {
    const cid = this.selectedCustomerId();
    if (!cid) {
      this.notification.error(this.translate.instant('individualScreening.errors.customerRequired'));
      return;
    }
    this.router.navigate(['/kyc/individual', cid]);
  }

  loadDocuments(): void {
    const cid = this.selectedCustomerId();
    if (!cid) {
      this.documents.set([]);
      return;
    }
    this.loadingDocuments.set(true);
    this.api.getCustomerDocuments(cid).subscribe({
      next: (res: ApiResponse<CustomerDocumentDto[]>) => {
        this.loadingDocuments.set(false);
        if (res.success && res.data) {
          this.documents.set(res.data);
        } else {
          this.documents.set([]);
        }
      },
      error: () => {
        this.loadingDocuments.set(false);
        this.documents.set([]);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0] ?? null;
    this.selectedFile.set(file);
    this.uploadError.set(null);
  }

  uploadDocument(): void {
    this.uploadError.set(null);
    this.uploadSuccess.set(null);
    const cid = this.selectedCustomerId();
    const docTypeCode = this.selectedDocumentTypeCode().trim();
    const file = this.selectedFile();
    if (!cid) {
      this.uploadError.set(this.translate.instant('individualScreening.errors.customerRequired'));
      return;
    }
    if (!docTypeCode) {
      this.uploadError.set('Document type is required.');
      return;
    }
    if (!file) {
      this.uploadError.set('Please select a file.');
      return;
    }
    const ext = '.' + (file.name.split('.').pop() ?? '').toLowerCase();
    if (!ALLOWED_EXTENSIONS.includes(ext)) {
      this.uploadError.set('Only PDF, JPG, and PNG are allowed.');
      return;
    }
    if (file.size > MAX_FILE_SIZE_BYTES) {
      this.uploadError.set('File size must be less than 10MB.');
      return;
    }
    if (docTypeCode === 'Passport' && !this.documentExpiryDate().trim()) {
      this.uploadError.set('Passport expiry date is required.');
      return;
    }
    const expiryDate = this.documentExpiryDate().trim()
      ? new Date(this.documentExpiryDate()).toISOString()
      : undefined;
    this.uploading.set(true);
    this.api.uploadCustomerDocument(cid, docTypeCode, file, expiryDate).subscribe({
      next: (res: ApiResponse<CustomerDocumentDto>) => {
        this.uploading.set(false);
        if (res.success && res.data) {
          this.documents.update(list => [res.data!, ...list]);
          this.selectedFile.set(null);
          this.documentExpiryDate.set('');
          this.uploadSuccess.set(this.translate.instant('screening.uploadSuccess'));
          this.notification.success(this.translate.instant('screening.uploadSuccess'));
          const input = document.querySelector('input[type="file"][name="individualKycFile"]') as HTMLInputElement | null;
          if (input) input.value = '';
        } else {
          const msg = res.message ?? this.translate.instant('screening.uploadFailed');
          this.uploadError.set(msg);
          this.notification.error(msg);
        }
      },
      error: (err) => {
        this.uploading.set(false);
        const msg = err?.error?.message ?? this.translate.instant('screening.uploadFailed');
        this.uploadError.set(msg);
        this.notification.error(msg);
      }
    });
  }

  downloadDocument(doc: CustomerDocumentDto): void {
    const cid = this.selectedCustomerId();
    if (!cid) return;
    this.api.downloadCustomerDocument(cid, doc.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = doc.fileName;
        a.click();
        URL.revokeObjectURL(url);
      }
    });
  }

  getStatusLabel(status: string | null | undefined): string {
    if (status === 'ConfirmedMatch') return this.translate.instant('screening.statusConfirmedMatch');
    if (status === 'PossibleMatch') return this.translate.instant('screening.statusPossibleMatch');
    return this.translate.instant('screening.statusClear');
  }

  getReviewStatusLabel(reviewStatus: string | null | undefined): string {
    if (reviewStatus === 'PendingReview') return this.translate.instant('screening.reviewPendingReview');
    if (reviewStatus === 'Approved') return this.translate.instant('screening.reviewApproved');
    if (reviewStatus === 'Rejected') return this.translate.instant('screening.reviewRejected');
    return '—';
  }

  private toDateInputValue(value: string): string {
    // Accepts ISO string or yyyy-MM-dd.
    if (!value) return '';
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return value;
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return '';
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  toNumber(value: unknown): number {
    const n = typeof value === 'number' ? value : Number(value);
    return Number.isFinite(n) ? n : 0;
  }
}

