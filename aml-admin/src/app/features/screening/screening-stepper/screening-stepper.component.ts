import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  CreateCustomerRequest,
  ApiResponse,
  CustomerDto,
  CustomerTypeDto,
  GenderDto,
  NationalityDto,
  CountryDto,
  DocumentTypeDto,
  CustomerDocumentDto,
  OccupationDto,
  SourceOfFundsDto,
  SanctionsScreeningResultItemDto
} from '../../../shared/models/api.model';

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const ALLOWED_EXTENSIONS = ['.pdf', '.jpg', '.jpeg', '.png'];

const DEFAULT_TENANT_ID = '00000000-0000-0000-0000-000000000001';

@Component({
  selector: 'app-screening-stepper',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './screening-stepper.component.html'
})
export class ScreeningStepperComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  currentStep = signal(1);
  customerTypes = signal<CustomerTypeDto[]>([]);
  genders = signal<GenderDto[]>([]);
  nationalities = signal<NationalityDto[]>([]);
  countries = signal<CountryDto[]>([]);
  occupations = signal<OccupationDto[]>([]);
  sourceOfFundsList = signal<SourceOfFundsDto[]>([]);
  loadingLookups = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  createdCustomerId = signal<string | null>(null);

  steps = [
    { num: 1, key: 'screening.step1CustomerProfile' },
    { num: 2, key: 'screening.step2Kyc' },
    { num: 3, key: 'screening.step3Sanctions' },
    { num: 4, key: 'screening.step4Review' }
  ];
  isStep1 = computed(() => this.currentStep() === 1);
  isStep2 = computed(() => this.currentStep() === 2);
  isStep3 = computed(() => this.currentStep() === 3);
  step2CustomerId = computed(() => this.createdCustomerId());
  documentTypes = signal<DocumentTypeDto[]>([]);
  documents = signal<CustomerDocumentDto[]>([]);
  selectedDocumentTypeCode = signal('');
  documentExpiryDate = signal('');
  selectedFile = signal<File | null>(null);
  uploading = signal(false);
  uploadError = signal<string | null>(null);
  uploadSuccess = signal<string | null>(null);
  loadingDocuments = signal(false);
  isPassport = computed(() => this.selectedDocumentTypeCode() === 'Passport');

  // Step 3 – Sanctions screening
  screeningResults = signal<SanctionsScreeningResultItemDto[]>([]);
  screeningRunLoading = signal(false);
  screeningRunError = signal<string | null>(null);
  hasConfirmedMatch = signal(false);

  // Step 1 – Customer profile form
  firstName = signal('');
  lastName = signal('');
  fullName = signal('');
  dateOfBirth = signal('');
  genderId = signal('');
  nationalityId = signal('');
  passportNumber = signal('');
  passportExpiryDate = signal('');
  nationalId = signal('');
  countryOfResidenceId = signal('');
  email = signal('');
  phone = signal('');
  address = signal('');
  city = signal('');
  country = signal('');
  occupationId = signal('');
  employerName = signal('');
  sourceOfFundsId = signal('');
  annualIncome = signal<number | null>(null);
  expectedMonthlyTransactionVolume = signal<number | null>(null);
  expectedMonthlyTransactionValue = signal<number | null>(null);
  customerTypeId = signal('');
  accountPurpose = signal('');

  onFirstNameChange(value: string): void {
    this.firstName.set(value);
    this.updateFullName();
  }

  onLastNameChange(value: string): void {
    this.lastName.set(value);
    this.updateFullName();
  }

  private updateFullName(): void {
    const first = this.firstName().trim();
    const last = this.lastName().trim();
    this.fullName.set([first, last].filter(Boolean).join(' ').trim());
  }

  ngOnInit(): void {
    let pending = 6;
    const maybeDone = () => {
      pending--;
      if (pending <= 0) this.loadingLookups.set(false);
    };
    this.api.getCustomerTypes().subscribe({
      next: (res: ApiResponse<CustomerTypeDto[]>) => {
        if (res.success && res.data && res.data.length > 0) {
          this.customerTypes.set(res.data);
          if (!this.customerTypeId()) this.customerTypeId.set(res.data[0].id);
        }
        maybeDone();
      },
      error: maybeDone
    });
    this.api.getGenders().subscribe({
      next: (res: ApiResponse<GenderDto[]>) => {
        if (res.success && res.data) this.genders.set(res.data);
        maybeDone();
      },
      error: maybeDone
    });
    this.api.getNationalities().subscribe({
      next: (res: ApiResponse<NationalityDto[]>) => {
        if (res.success && res.data) this.nationalities.set(res.data);
        maybeDone();
      },
      error: maybeDone
    });
    this.api.getCountries().subscribe({
      next: (res: ApiResponse<CountryDto[]>) => {
        if (res.success && res.data) this.countries.set(res.data);
        maybeDone();
      },
      error: maybeDone
    });
    this.api.getOccupations().subscribe({
      next: (res) => {
        if (res.success && res.data) this.occupations.set(res.data);
        maybeDone();
      },
      error: maybeDone
    });
    this.api.getSourceOfFunds().subscribe({
      next: (res) => {
        if (res.success && res.data) this.sourceOfFundsList.set(res.data);
        maybeDone();
      },
      error: maybeDone
    });
  }

  submitStep1(): void {
    this.error.set(null);
    this.successMessage.set(null);
    const fn = this.fullName().trim();
    const ctId = this.customerTypeId();
    if (!fn) {
      this.error.set('Full name is required.');
      return;
    }
    if (!ctId) {
      this.error.set('Customer type is required.');
      return;
    }

    this.saving.set(true);
    const payload: CreateCustomerRequest = {
      tenantId: DEFAULT_TENANT_ID,
      fullName: fn,
      customerTypeId: ctId,
      firstName: this.firstName() || undefined,
      lastName: this.lastName() || undefined,
      dateOfBirth: this.dateOfBirth() ? new Date(this.dateOfBirth()).toISOString() : undefined,
      genderId: this.genderId() || undefined,
      nationalityId: this.nationalityId() || undefined,
      passportNumber: this.passportNumber() || undefined,
      passportExpiryDate: this.passportExpiryDate() ? new Date(this.passportExpiryDate()).toISOString() : undefined,
      nationalId: this.nationalId() || undefined,
      countryOfResidenceId: this.countryOfResidenceId() || undefined,
      email: this.email() || undefined,
      phone: this.phone() || undefined,
      address: this.address() || undefined,
      city: this.city() || undefined,
      country: this.country() || undefined,
      occupationId: this.occupationId() || undefined,
      employerName: this.employerName() || undefined,
      sourceOfFundsId: this.sourceOfFundsId() || undefined,
      annualIncome: this.annualIncome() ?? undefined,
      expectedMonthlyTransactionVolume: this.expectedMonthlyTransactionVolume() ?? undefined,
      expectedMonthlyTransactionValue: this.expectedMonthlyTransactionValue() ?? undefined,
      accountPurpose: this.accountPurpose() || undefined
    };

    this.api.createCustomer(payload).subscribe({
      next: (res: ApiResponse<CustomerDto>) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.createdCustomerId.set(res.data.id);
          const msg = this.translate.instant('screening.customerCreated');
          this.successMessage.set(msg);
          this.notification.success(msg);
        } else {
          const errMsg = res.message ?? this.translate.instant('common.saveFailed');
          this.error.set(errMsg);
          this.notification.error(errMsg);
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.notification.error(err?.error?.message ?? this.translate.instant('common.saveFailed'));
      }
    });
  }

  goToCustomers(): void {
    this.router.navigate(['/customers']);
  }

  goToStep(step: number): void {
    if (step === 1) {
      this.currentStep.set(1);
      return;
    }
    if (step === 2 && this.createdCustomerId()) {
      this.currentStep.set(2);
      this.uploadError.set(null);
      this.uploadSuccess.set(null);
      this.loadStep2Data();
      return;
    }
    if (step === 3 && this.createdCustomerId()) {
      this.currentStep.set(3);
      this.screeningRunError.set(null);
      this.loadStep3Data();
      return;
    }
    if (step > 3 && this.createdCustomerId()) this.currentStep.set(step);
  }

  loadStep2Data(): void {
    const cid = this.step2CustomerId();
    if (!cid) return;
    this.loadingDocuments.set(true);
    this.api.getDocumentTypes().subscribe({
      next: (res: ApiResponse<DocumentTypeDto[]>) => {
        if (res.success && res.data) this.documentTypes.set(res.data);
      },
      error: () => this.notification.error(this.translate.instant('common.errorGeneric'))
    });
    this.api.getCustomerDocuments(cid).subscribe({
      next: (res: ApiResponse<CustomerDocumentDto[]>) => {
        this.loadingDocuments.set(false);
        if (res.success && res.data) this.documents.set(res.data);
      },
      error: () => {
        this.loadingDocuments.set(false);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0];
    this.selectedFile.set(file ?? null);
    this.uploadError.set(null);
  }

  uploadDocument(): void {
    this.uploadError.set(null);
    this.uploadSuccess.set(null);
    const file = this.selectedFile();
    const docTypeCode = this.selectedDocumentTypeCode().trim();
    const cid = this.step2CustomerId();
    if (!cid) {
      this.uploadError.set('No customer selected.');
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
    const expiryDate = this.documentExpiryDate().trim() ? new Date(this.documentExpiryDate()).toISOString() : undefined;
    this.uploading.set(true);
    this.api.uploadCustomerDocument(cid, docTypeCode, file, expiryDate).subscribe({
      next: (res: ApiResponse<CustomerDocumentDto>) => {
        this.uploading.set(false);
        if (res.success && res.data) {
          this.documents.update(list => [res.data!, ...list]);
          this.selectedFile.set(null);
          this.documentExpiryDate.set('');
          const successMsg = this.translate.instant('screening.uploadSuccess');
          this.uploadSuccess.set(successMsg);
          this.notification.success(successMsg);
          const input = document.querySelector('input[type="file"][name="kycFile"]') as HTMLInputElement;
          if (input) input.value = '';
        } else {
          const errMsg = res.message ?? this.translate.instant('screening.uploadFailed');
          this.uploadError.set(errMsg);
          this.notification.error(errMsg);
        }
      },
      error: (err) => {
        this.uploading.set(false);
        const errMsg = err?.error?.message ?? this.translate.instant('screening.uploadFailed');
        this.uploadError.set(errMsg);
        this.notification.error(errMsg);
      }
    });
  }

  downloadDocument(doc: CustomerDocumentDto): void {
    const cid = this.step2CustomerId();
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

  canGoToStep(step: number): boolean {
    if (step === 1) return true;
    if (step >= 2 && step <= 4) return !!this.createdCustomerId();
    return false;
  }

  loadStep3Data(): void {
    const cid = this.createdCustomerId();
    if (!cid) return;
    this.screeningRunError.set(null);
    this.api.getCustomerSanctionsScreeningResults(cid).subscribe({
      next: (res: ApiResponse<SanctionsScreeningResultItemDto[]>) => {
        if (res.success && res.data) {
          this.screeningResults.set(res.data);
          this.hasConfirmedMatch.set(res.data.some(r => r.status === 'ConfirmedMatch'));
        } else {
          this.screeningResults.set([]);
          this.hasConfirmedMatch.set(false);
        }
      },
      error: () => {
        this.screeningResults.set([]);
        this.hasConfirmedMatch.set(false);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  runSanctionsScreening(): void {
    const cid = this.createdCustomerId();
    if (!cid) return;
    this.screeningRunError.set(null);
    this.screeningRunLoading.set(true);
    this.api.runCustomerSanctionsScreening(cid).subscribe({
      next: (res: ApiResponse<{ results: SanctionsScreeningResultItemDto[]; hasConfirmedMatch: boolean }>) => {
        this.screeningRunLoading.set(false);
        if (res.success && res.data) {
          this.screeningResults.set(res.data.results ?? []);
          this.hasConfirmedMatch.set(res.data.hasConfirmedMatch ?? false);
          if (res.data.hasConfirmedMatch) {
            this.notification.error(this.translate.instant('screening.onboardingBlocked'));
          } else {
            this.notification.success(this.translate.instant('screening.screeningComplete'));
          }
        } else {
          this.screeningRunError.set(res.message ?? this.translate.instant('common.errorGeneric'));
        }
      },
      error: (err) => {
        this.screeningRunLoading.set(false);
        const errMsg = err?.error?.message ?? this.translate.instant('common.errorGeneric');
        this.screeningRunError.set(errMsg);
        this.notification.error(errMsg);
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

  canShowCheckerActions(r: SanctionsScreeningResultItemDto): boolean {
    return (r.status === 'PossibleMatch' || r.status === 'ConfirmedMatch') && r.reviewStatus === 'PendingReview';
  }

  actionNote = signal('');
  actionScreeningId = signal<string | null>(null);
  pendingAction = signal<'Approve' | 'Reject' | null>(null);

  openAction(screeningId: string, action: 'Approve' | 'Reject'): void {
    this.actionScreeningId.set(screeningId);
    this.pendingAction.set(action);
    this.actionNote.set('');
  }

  cancelAction(): void {
    this.actionScreeningId.set(null);
    this.pendingAction.set(null);
    this.actionNote.set('');
  }

  submitCheckerAction(): void {
    const action = this.pendingAction();
    const cid = this.createdCustomerId();
    const screeningId = this.actionScreeningId();
    if (!cid || !screeningId || !action) return;
    this.api.recordSanctionScreeningAction(cid, screeningId, { action, notes: this.actionNote()?.trim() || undefined }).subscribe({
      next: (res) => {
        if (res.success) {
          this.cancelAction();
          this.loadStep3Data();
          this.notification.success(this.translate.instant('screening.actionRecorded'));
        } else {
          this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
        }
      },
      error: (err: unknown) => {
        const errMsg = (err as { error?: { message?: string } })?.error?.message ?? this.translate.instant('common.errorGeneric');
        this.notification.error(errMsg);
      }
    });
  }
}
