import { CommonModule } from '@angular/common';
import { Component, inject, signal, type OnInit, type WritableSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { IndividualScreeningService } from '../../../core/services/individual-screening.service';
import { IndividualKycService } from '../../../core/services/individual-kyc.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  ApiResponse,
  CountryDto,
  CustomerDto,
  DocumentTypeDto,
  GenderDto,
  IndividualKycDto,
  IndividualScreeningRequestDto,
  NationalityDto,
  SourceOfFundsDto,
  UpsertIndividualKycRequest,
  UploadIndividualKycDocumentRequest,
  UpsertIndividualScreeningRequest,
} from '../../../shared/models/api.model';

const DEFAULT_TENANT_ID = '00000000-0000-0000-0000-000000000001';
const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const ALLOWED_EXTENSIONS = ['.pdf', '.jpg', '.jpeg', '.png'];

type DocumentRow = {
  existingDocId?: string;
  documentNo: string;
  issuedDate: string; // yyyy-MM-dd for <input type="date">
  expiryDate: string; // yyyy-MM-dd for <input type="date">
  approvedBy: string;
  folderPath: string;
  selectedFile: File | null;
  existingFileName?: string;
};

@Component({
  selector: 'app-individual-kyc',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './individual-kyc.component.html',
  styleUrls: ['./individual-kyc.component.scss'],
})
export class IndividualKycComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly screeningApi = inject(IndividualScreeningService);
  private readonly kycApi = inject(IndividualKycService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // Customer picker
  routeCustomerId = signal('');
  customerSearchTerm = signal('');
  customerOptions = signal<CustomerDto[]>([]);
  customerLoading = signal(false);
  customerDropdownOpen = signal(false);
  selectedCustomer = signal<CustomerDto | null>(null);

  // Lookups
  genders = signal<GenderDto[]>([]);
  nationalities = signal<NationalityDto[]>([]);
  countries = signal<CountryDto[]>([]);
  documentTypes = signal<DocumentTypeDto[]>([]);
  sourceOfFunds = signal<SourceOfFundsDto[]>([]);

  // Screening-driven categories + reputation threshold
  matchThreshold = signal(85);
  checkPepUkOnly = signal(true);
  checkDisqualifiedDirectorUkOnly = signal(true);
  checkSanctions = signal(true);
  checkProfileOfInterest = signal(true);
  checkReputationalRiskExposure = signal(true);
  checkRegulatoryEnforcementList = signal(true);
  checkInsolvencyUkIreland = signal(true);
  referenceId = signal('');

  // KYC form (signals)
  applicantName = signal('');
  applicantAliases = signal('');
  applicantMobileNo = signal('');
  applicantNationalityId = signal('');
  applicantDualNationality = signal(false);
  applicantGenderId = signal('');
  applicantDateOfBirth = signal('');
  applicantResidenceStatus = signal('');
  applicantEmirate = signal('');
  applicantCountryOfBirth = signal('');
  applicantCity = signal('');
  applicantEmail = signal('');
  applicantResidentialAddress = signal('');
  applicantOfficeNoBuildingNameStreetArea = signal('');
  applicantPOBox = signal('');
  applicantCustomerRelationship = signal('');
  applicantPreferredChannel = signal('');
  applicantProductType = signal('');
  applicantIndustryType = signal('');
  applicantOccupationId = signal('');

  applicantSourceOfFundsId = signal('');
  applicantSourceOfFundsComments = signal('');
  applicantIsProofOfSourceFundsObtained = signal<boolean | null>(null);
  applicantSourceOfFundsProofComments = signal('');
  applicantSourceOfWealth = signal('');
  applicantSourceOfWealthComments = signal('');
  applicantIsProofOfSourceWealthObtained = signal<boolean | null>(null);
  applicantSourceOfWealthProofComments = signal('');

  clientIdTypeCode = signal('');
  clientIdNumber = signal('');
  clientEmiratesIdNumber = signal('');
  clientIdExpiryDate = signal('');
  clientPassportNumber = signal('');
  clientPassportDateOfIssue = signal('');
  clientCountryIssuanceId = signal('');

  sponsorName = signal('');
  sponsorAliases = signal('');
  sponsorIdTypeCode = signal('');
  sponsorIdNumber = signal('');
  sponsorDateOfBirth = signal('');
  sponsorNationalityId = signal('');
  sponsorGenderId = signal('');
  sponsorDualNationality = signal(false);
  sponsorOtherDetails = signal('');

  bankCountryId = signal('');
  bankIbanAccountNo = signal('');
  bankName = signal('');
  accountName = signal('');
  bankSwiftCode = signal('');
  bankAddress = signal('');
  bankCurrency = signal('');

  employerCompanyName = signal('');
  employerCompanyWebsite = signal('');
  employerEmailAddress = signal('');
  employerTelNo = signal('');
  employerAddress = signal('');
  employerIndustryAndBusinessDetails = signal('');

  pepFATFIncreasedMonitoringAnswer = signal('');
  pepSanctionListOrInverseMediaAnswer = signal('');
  pepProminentPublicFunctionsAnswer = signal('');
  pepAnyPEPsAfterScreeningAnswer = signal('');
  pepSpecificPEPsAfterScreeningDetails = signal('');

  followUpDate = signal('');
  followUpRemarks = signal('');

  // Documents
  documentRows = signal<DocumentRow[]>([]);

  submitting = signal(false);
  submitError = signal<string | null>(null);
  submitSuccess = signal<string | null>(null);

  ngOnInit(): void {
    this.loadLookups();

    this.route.paramMap.subscribe(async (pm) => {
      const cid = pm.get('customerId') ?? '';
      this.routeCustomerId.set(cid);
      await this.loadSelectedCustomer(cid);
      // Load KYC first, then overwrite overlapping base fields + categories from screening.
      await this.loadIndividualKyc(cid);
      await this.loadScreening(cid);
    });
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
    this.api.getSourceOfFunds().subscribe({
      next: (res: ApiResponse<SourceOfFundsDto[]>) => {
        if (res.success && res.data) this.sourceOfFunds.set(res.data);
      }
    });
  }

  private async loadSelectedCustomer(cid: string): Promise<void> {
    this.selectedCustomer.set(null);
    this.customerSearchTerm.set('');

    if (!cid) return;
    const res = await firstValueFrom(this.api.getCustomer(cid));
    if (res.success && res.data) {
      this.selectedCustomer.set(res.data);
      this.customerSearchTerm.set(res.data.fullName ?? '');
    }
  }

  async searchCustomers(): Promise<void> {
    this.customerDropdownOpen.set(true);
    this.customerLoading.set(true);
    try {
      const term = this.customerSearchTerm().trim();
      if (term.length < 2) {
        this.customerOptions.set([]);
        return;
      }
      const res = await firstValueFrom(this.api.getCustomers({ pageNumber: 1, pageSize: 10, searchTerm: term }));
      const items = res.success && res.data?.items ? res.data.items : [];
      this.customerOptions.set(items);
    } finally {
      this.customerLoading.set(false);
    }
  }

  selectCustomer(c: CustomerDto): void {
    this.customerDropdownOpen.set(false);
    this.router.navigate(['/kyc/individual', c.id]);
  }

  isAllCategoriesSelected(): boolean {
    return (
      this.checkPepUkOnly() &&
      this.checkDisqualifiedDirectorUkOnly() &&
      this.checkSanctions() &&
      this.checkProfileOfInterest() &&
      this.checkReputationalRiskExposure() &&
      this.checkRegulatoryEnforcementList() &&
      this.checkInsolvencyUkIreland()
    );
  }

  onToggleSelectAll(ev: Event): void {
    const checked = (ev.target as HTMLInputElement).checked;
    this.checkPepUkOnly.set(checked);
    this.checkDisqualifiedDirectorUkOnly.set(checked);
    this.checkSanctions.set(checked);
    this.checkProfileOfInterest.set(checked);
    this.checkReputationalRiskExposure.set(checked);
    this.checkRegulatoryEnforcementList.set(checked);
    this.checkInsolvencyUkIreland.set(checked);
  }

  private async loadIndividualKyc(cid: string): Promise<void> {
    if (!cid) {
      this.documentRows.set([]);
      return;
    }

    // Active KYC payload
    const kycRes = await firstValueFrom(this.kycApi.getActiveIndividualKyc(cid));
    if (kycRes.success && kycRes.data) this.applyKycToForm(kycRes.data);
    else this.resetKycFields();

    // Active KYC documents
    const docsRes = await firstValueFrom(this.kycApi.getIndividualKycDocuments(cid));
    const docs = docsRes.success && docsRes.data ? docsRes.data : [];
    this.documentRows.set(
      docs.map((d) => ({
        existingDocId: d.id,
        documentNo: d.documentNo ?? '',
        issuedDate: this.toDateInputValue(d.issuedDate ?? ''),
        expiryDate: this.toDateInputValue(d.expiryDate ?? ''),
        approvedBy: d.approvedBy ?? '',
        folderPath: d.folderPath ?? '',
        selectedFile: null,
        existingFileName: d.fileName
      }))
    );
  }

  private resetKycFields(): void {
    this.applicantName.set('');
    this.applicantAliases.set('');
    this.applicantMobileNo.set('');
    this.applicantNationalityId.set('');
    this.applicantDualNationality.set(false);
    this.applicantGenderId.set('');
    this.applicantDateOfBirth.set('');
    this.applicantResidenceStatus.set('');
    this.applicantEmirate.set('');
    this.applicantCountryOfBirth.set('');
    this.applicantCity.set('');
    this.applicantEmail.set('');
    this.applicantResidentialAddress.set('');
    this.applicantOfficeNoBuildingNameStreetArea.set('');
    this.applicantPOBox.set('');
    this.applicantCustomerRelationship.set('');
    this.applicantPreferredChannel.set('');
    this.applicantProductType.set('');
    this.applicantIndustryType.set('');
    this.applicantOccupationId.set('');

    this.applicantSourceOfFundsId.set('');
    this.applicantSourceOfFundsComments.set('');
    this.applicantIsProofOfSourceFundsObtained.set(null);
    this.applicantSourceOfFundsProofComments.set('');
    this.applicantSourceOfWealth.set('');
    this.applicantSourceOfWealthComments.set('');
    this.applicantIsProofOfSourceWealthObtained.set(null);
    this.applicantSourceOfWealthProofComments.set('');

    this.clientIdTypeCode.set('');
    this.clientIdNumber.set('');
    this.clientEmiratesIdNumber.set('');
    this.clientIdExpiryDate.set('');
    this.clientPassportNumber.set('');
    this.clientPassportDateOfIssue.set('');
    this.clientCountryIssuanceId.set('');

    this.sponsorName.set('');
    this.sponsorAliases.set('');
    this.sponsorIdTypeCode.set('');
    this.sponsorIdNumber.set('');
    this.sponsorDateOfBirth.set('');
    this.sponsorNationalityId.set('');
    this.sponsorGenderId.set('');
    this.sponsorDualNationality.set(false);
    this.sponsorOtherDetails.set('');

    this.bankCountryId.set('');
    this.bankIbanAccountNo.set('');
    this.bankName.set('');
    this.accountName.set('');
    this.bankSwiftCode.set('');
    this.bankAddress.set('');
    this.bankCurrency.set('');

    this.employerCompanyName.set('');
    this.employerCompanyWebsite.set('');
    this.employerEmailAddress.set('');
    this.employerTelNo.set('');
    this.employerAddress.set('');
    this.employerIndustryAndBusinessDetails.set('');

    this.pepFATFIncreasedMonitoringAnswer.set('');
    this.pepSanctionListOrInverseMediaAnswer.set('');
    this.pepProminentPublicFunctionsAnswer.set('');
    this.pepAnyPEPsAfterScreeningAnswer.set('');
    this.pepSpecificPEPsAfterScreeningDetails.set('');

    this.followUpDate.set('');
    this.followUpRemarks.set('');
  }

  private applyKycToForm(kyc: IndividualKycDto): void {
    this.applicantName.set(kyc.applicantName ?? '');
    this.applicantAliases.set(kyc.applicantAliases ?? '');
    this.applicantMobileNo.set(kyc.applicantMobileNo ?? '');
    this.applicantNationalityId.set(kyc.applicantNationalityId ?? '');
    this.applicantDualNationality.set(!!kyc.applicantDualNationality);
    this.applicantGenderId.set(kyc.applicantGenderId ?? '');
    this.applicantDateOfBirth.set(this.toDateInputValue(kyc.applicantDateOfBirth ?? ''));
    this.applicantResidenceStatus.set(kyc.applicantResidenceStatus ?? '');
    this.applicantEmirate.set(kyc.applicantEmirate ?? '');
    this.applicantCountryOfBirth.set(kyc.applicantCountryOfBirth ?? '');
    this.applicantCity.set(kyc.applicantCity ?? '');
    this.applicantEmail.set(kyc.applicantEmail ?? '');
    this.applicantResidentialAddress.set(kyc.applicantResidentialAddress ?? '');
    this.applicantOfficeNoBuildingNameStreetArea.set(kyc.applicantOfficeNoBuildingNameStreetArea ?? '');
    this.applicantPOBox.set(kyc.applicantPOBox ?? '');
    this.applicantCustomerRelationship.set(kyc.applicantCustomerRelationship ?? '');
    this.applicantPreferredChannel.set(kyc.applicantPreferredChannel ?? '');
    this.applicantProductType.set(kyc.applicantProductType ?? '');
    this.applicantIndustryType.set(kyc.applicantIndustryType ?? '');
    this.applicantOccupationId.set(kyc.applicantOccupationId ?? '');

    this.applicantSourceOfFundsId.set(kyc.applicantSourceOfFundsId ?? '');
    this.applicantSourceOfFundsComments.set(kyc.applicantSourceOfFundsComments ?? '');
    this.applicantIsProofOfSourceFundsObtained.set(kyc.applicantIsProofOfSourceFundsObtained ?? null);
    this.applicantSourceOfFundsProofComments.set(kyc.applicantSourceOfFundsProofComments ?? '');

    this.applicantSourceOfWealth.set(kyc.applicantSourceOfWealth ?? '');
    this.applicantSourceOfWealthComments.set(kyc.applicantSourceOfWealthComments ?? '');
    this.applicantIsProofOfSourceWealthObtained.set(kyc.applicantIsProofOfSourceWealthObtained ?? null);
    this.applicantSourceOfWealthProofComments.set(kyc.applicantSourceOfWealthProofComments ?? '');

    this.clientIdTypeCode.set(kyc.clientIdTypeCode ?? '');
    this.clientIdNumber.set(kyc.clientIdNumber ?? '');
    this.clientEmiratesIdNumber.set(kyc.clientEmiratesIdNumber ?? '');
    this.clientIdExpiryDate.set(this.toDateInputValue(kyc.clientIdExpiryDate ?? ''));
    this.clientPassportNumber.set(kyc.clientPassportNumber ?? '');
    this.clientPassportDateOfIssue.set(this.toDateInputValue(kyc.clientPassportDateOfIssue ?? ''));
    this.clientCountryIssuanceId.set(kyc.clientCountryIssuanceId ?? '');

    this.sponsorName.set(kyc.sponsorName ?? '');
    this.sponsorAliases.set(kyc.sponsorAliases ?? '');
    this.sponsorIdTypeCode.set(kyc.sponsorIdTypeCode ?? '');
    this.sponsorIdNumber.set(kyc.sponsorIdNumber ?? '');
    this.sponsorDateOfBirth.set(this.toDateInputValue(kyc.sponsorDateOfBirth ?? ''));
    this.sponsorNationalityId.set(kyc.sponsorNationalityId ?? '');
    this.sponsorGenderId.set(kyc.sponsorGenderId ?? '');
    this.sponsorDualNationality.set(!!kyc.sponsorDualNationality);
    this.sponsorOtherDetails.set(kyc.sponsorOtherDetails ?? '');

    this.bankCountryId.set(kyc.bankCountryId ?? '');
    this.bankIbanAccountNo.set(kyc.bankIbanAccountNo ?? '');
    this.bankName.set(kyc.bankName ?? '');
    this.accountName.set(kyc.accountName ?? '');
    this.bankSwiftCode.set(kyc.bankSwiftCode ?? '');
    this.bankAddress.set(kyc.bankAddress ?? '');
    this.bankCurrency.set(kyc.bankCurrency ?? '');

    this.employerCompanyName.set(kyc.employerCompanyName ?? '');
    this.employerCompanyWebsite.set(kyc.employerCompanyWebsite ?? '');
    this.employerEmailAddress.set(kyc.employerEmailAddress ?? '');
    this.employerTelNo.set(kyc.employerTelNo ?? '');
    this.employerAddress.set(kyc.employerAddress ?? '');
    this.employerIndustryAndBusinessDetails.set(kyc.employerIndustryAndBusinessDetails ?? '');

    this.pepFATFIncreasedMonitoringAnswer.set(kyc.pepFATFIncreasedMonitoringAnswer ?? '');
    this.pepSanctionListOrInverseMediaAnswer.set(kyc.pepSanctionListOrInverseMediaAnswer ?? '');
    this.pepProminentPublicFunctionsAnswer.set(kyc.pepProminentPublicFunctionsAnswer ?? '');
    this.pepAnyPEPsAfterScreeningAnswer.set(kyc.pepAnyPEPsAfterScreeningAnswer ?? '');
    this.pepSpecificPEPsAfterScreeningDetails.set(kyc.pepSpecificPEPsAfterScreeningDetails ?? '');

    this.followUpDate.set(this.toDateInputValue(kyc.followUpDate ?? ''));
    this.followUpRemarks.set(kyc.followUpRemarks ?? '');
  }

  private async loadScreening(cid: string): Promise<void> {
    if (!cid) return;
    const res = await firstValueFrom(this.screeningApi.getLatest(cid));
    if (!res.success || !res.data) {
      // Don't clear form completely; just reset categories to defaults.
      return;
    }
    this.applyScreeningToForm(res.data);
  }

  private applyScreeningToForm(req: IndividualScreeningRequestDto): void {
    this.referenceId.set(req.referenceId ?? '');
    this.applicantName.set(req.fullName ?? '');
    this.applicantDateOfBirth.set(req.dateOfBirth ? this.toDateInputValue(req.dateOfBirth) : '');
    this.applicantNationalityId.set(req.nationalityId ?? '');
    this.applicantCountryOfBirth.set(req.placeOfBirthCountryId ?? '');
    this.clientIdTypeCode.set(req.idType ?? '');
    this.clientIdNumber.set(req.idNumber ?? '');
    this.applicantResidentialAddress.set(req.address ?? '');
    this.applicantGenderId.set(req.genderId ?? '');

    this.matchThreshold.set(req.matchThreshold ?? 85);
    // Optional fields (not shown in this form, but keep their values if present).
    // this.birthYearRange.set(req.birthYearRange ?? null);

    this.checkPepUkOnly.set(!!req.checkPepUkOnly);
    this.checkSanctions.set(!!req.checkSanctions);
    this.checkProfileOfInterest.set(!!req.checkProfileOfInterest);
    this.checkDisqualifiedDirectorUkOnly.set(!!req.checkDisqualifiedDirectorUkOnly);
    this.checkReputationalRiskExposure.set(!!req.checkReputationalRiskExposure);
    this.checkRegulatoryEnforcementList.set(!!req.checkRegulatoryEnforcementList);
    this.checkInsolvencyUkIreland.set(!!req.checkInsolvencyUkIreland);
  }

  addDocumentRow(): void {
    this.documentRows.update((rows) => [
      ...rows,
      {
        documentNo: '',
        issuedDate: '',
        expiryDate: '',
        approvedBy: '',
        folderPath: '',
        selectedFile: null
      }
    ]);
  }

  updateDocumentRow(index: number, patch: Partial<DocumentRow>): void {
    this.documentRows.update((rows) =>
      rows.map((r, i) => (i === index ? { ...r, ...patch } : r))
    );
  }

  onDocumentFileSelected(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files?.[0] ?? null;
    if (!file) return;

    const ext = '.' + (file.name.split('.').pop() ?? '').toLowerCase();
    if (!ALLOWED_EXTENSIONS.includes(ext)) {
      this.submitError.set(this.translate.instant('individualKyc.onlyPdfPng'));
      return;
    }
    if (file.size > MAX_FILE_SIZE_BYTES) {
      this.submitError.set(this.translate.instant('individualKyc.fileSizeMax'));
      return;
    }

    this.submitError.set(null);
    this.updateDocumentRow(index, { selectedFile: file });
  }

  async removeDocumentRow(index: number): Promise<void> {
    const row = this.documentRows()[index];
    if (!row) return;

    // Remove from backend immediately for existing docs.
    if (row.existingDocId && this.routeCustomerId()) {
      this.submitting.set(true);
      try {
        const res = await firstValueFrom(
          this.kycApi.deleteIndividualKycDocument(this.routeCustomerId(), row.existingDocId)
        );
        if (!res.success) throw new Error(res.message ?? 'Delete failed');
        await this.loadIndividualKyc(this.routeCustomerId());
      } catch (e) {
        this.submitError.set((e as Error).message);
      } finally {
        this.submitting.set(false);
      }
      return;
    }

    // Local-only row (not uploaded yet).
    this.documentRows.update((rows) => rows.filter((_, i) => i !== index));
  }

  downloadDocumentRow(row: DocumentRow): void {
    const cid = this.routeCustomerId();
    if (!cid || !row.existingDocId) return;
    this.kycApi.downloadIndividualKycDocument(cid, row.existingDocId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = row.existingFileName ?? 'document';
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  setProofObtained(target: WritableSignal<boolean | null>, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    target.set(checked);
  }

  setYesNoStringAnswer(target: WritableSignal<string>, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    target.set(checked ? 'Yes' : 'No');
  }

  private dateInputToIso(dateInput: string): string | null {
    const v = (dateInput ?? '').trim();
    if (!v) return null;
    return new Date(v).toISOString();
  }

  private toDateInputValue(value: string): string {
    if (!value) return '';
    if (/^\\d{4}-\\d{2}-\\d{2}$/.test(value)) return value;
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

  async submit(): Promise<void> {
    this.submitError.set(null);
    this.submitSuccess.set(null);

    const cid = this.routeCustomerId();
    if (!cid) {
      this.submitError.set(this.translate.instant('individualKyc.customerRequired'));
      return;
    }
    if (!this.applicantName().trim()) {
      this.submitError.set(this.translate.instant('individualKyc.applicantNameRequired'));
      return;
    }

    this.submitting.set(true);
    try {
      const kycBody: UpsertIndividualKycRequest = {
        tenantId: DEFAULT_TENANT_ID,

        applicantName: this.applicantName().trim(),
        applicantAliases: this.applicantAliases().trim() || undefined,
        applicantMobileNo: this.applicantMobileNo().trim() || undefined,
        applicantNationalityId: this.applicantNationalityId() || undefined,
        applicantDualNationality: this.applicantDualNationality(),
        applicantGenderId: this.applicantGenderId() || undefined,
        applicantDateOfBirth: this.dateInputToIso(this.applicantDateOfBirth()) ?? undefined,
        applicantResidenceStatus: this.applicantResidenceStatus().trim() || undefined,
        applicantEmirate: this.applicantEmirate().trim() || undefined,
        applicantCountryOfBirth: this.applicantCountryOfBirth().trim() || undefined,
        applicantCity: this.applicantCity().trim() || undefined,
        applicantEmail: this.applicantEmail().trim() || undefined,
        applicantResidentialAddress: this.applicantResidentialAddress().trim() || undefined,
        applicantOfficeNoBuildingNameStreetArea: this.applicantOfficeNoBuildingNameStreetArea().trim() || undefined,
        applicantPOBox: this.applicantPOBox().trim() || undefined,
        applicantCustomerRelationship: this.applicantCustomerRelationship().trim() || undefined,
        applicantPreferredChannel: this.applicantPreferredChannel().trim() || undefined,
        applicantProductType: this.applicantProductType().trim() || undefined,
        applicantIndustryType: this.applicantIndustryType().trim() || undefined,
        applicantOccupationId: this.applicantOccupationId() || undefined,

        applicantSourceOfFundsId: this.applicantSourceOfFundsId() || undefined,
        applicantSourceOfFundsComments: this.applicantSourceOfFundsComments().trim() || undefined,
        applicantIsProofOfSourceFundsObtained: this.applicantIsProofOfSourceFundsObtained(),
        applicantSourceOfFundsProofComments: this.applicantSourceOfFundsProofComments().trim() || undefined,

        applicantSourceOfWealth: this.applicantSourceOfWealth().trim() || undefined,
        applicantSourceOfWealthComments: this.applicantSourceOfWealthComments().trim() || undefined,
        applicantIsProofOfSourceWealthObtained: this.applicantIsProofOfSourceWealthObtained(),
        applicantSourceOfWealthProofComments: this.applicantSourceOfWealthProofComments().trim() || undefined,

        clientIdTypeCode: this.clientIdTypeCode().trim() || undefined,
        clientIdNumber: this.clientIdNumber().trim() || undefined,
        clientEmiratesIdNumber: this.clientEmiratesIdNumber().trim() || undefined,
        clientIdExpiryDate: this.dateInputToIso(this.clientIdExpiryDate()) ?? undefined,
        clientPassportNumber: this.clientPassportNumber().trim() || undefined,
        clientPassportDateOfIssue: this.dateInputToIso(this.clientPassportDateOfIssue()) ?? undefined,
        clientCountryIssuanceId: this.clientCountryIssuanceId() || undefined,

        sponsorName: this.sponsorName().trim() || undefined,
        sponsorAliases: this.sponsorAliases().trim() || undefined,
        sponsorIdTypeCode: this.sponsorIdTypeCode().trim() || undefined,
        sponsorIdNumber: this.sponsorIdNumber().trim() || undefined,
        sponsorDateOfBirth: this.dateInputToIso(this.sponsorDateOfBirth()) ?? undefined,
        sponsorNationalityId: this.sponsorNationalityId() || undefined,
        sponsorGenderId: this.sponsorGenderId() || undefined,
        sponsorDualNationality: this.sponsorDualNationality(),
        sponsorOtherDetails: this.sponsorOtherDetails().trim() || undefined,

        bankCountryId: this.bankCountryId() || undefined,
        bankIbanAccountNo: this.bankIbanAccountNo().trim() || undefined,
        bankName: this.bankName().trim() || undefined,
        accountName: this.accountName().trim() || undefined,
        bankSwiftCode: this.bankSwiftCode().trim() || undefined,
        bankAddress: this.bankAddress().trim() || undefined,
        bankCurrency: this.bankCurrency().trim() || undefined,

        employerCompanyName: this.employerCompanyName().trim() || undefined,
        employerCompanyWebsite: this.employerCompanyWebsite().trim() || undefined,
        employerEmailAddress: this.employerEmailAddress().trim() || undefined,
        employerTelNo: this.employerTelNo().trim() || undefined,
        employerAddress: this.employerAddress().trim() || undefined,
        employerIndustryAndBusinessDetails: this.employerIndustryAndBusinessDetails().trim() || undefined,

        pepFATFIncreasedMonitoringAnswer: this.pepFATFIncreasedMonitoringAnswer() || undefined,
        pepSanctionListOrInverseMediaAnswer: this.pepSanctionListOrInverseMediaAnswer() || undefined,
        pepProminentPublicFunctionsAnswer: this.pepProminentPublicFunctionsAnswer() || undefined,
        pepAnyPEPsAfterScreeningAnswer: this.pepAnyPEPsAfterScreeningAnswer() || undefined,
        pepSpecificPEPsAfterScreeningDetails: this.pepSpecificPEPsAfterScreeningDetails().trim() || undefined,

        followUpDate: this.dateInputToIso(this.followUpDate()) ?? undefined,
        followUpRemarks: this.followUpRemarks().trim() || undefined
      };

      const createRes = await firstValueFrom(this.kycApi.createIndividualKyc(cid, kycBody));
      if (!createRes.success) throw new Error(createRes.message ?? 'KYC save failed');

      // Sync categories + base applicant fields into the existing IndividualScreeningRequest.
      const screeningBody: UpsertIndividualScreeningRequest = {
        tenantId: DEFAULT_TENANT_ID,
        referenceId: this.referenceId().trim() || undefined,
        fullName: this.applicantName().trim(),
        dateOfBirth: this.dateInputToIso(this.applicantDateOfBirth()) ?? undefined,
        nationalityId: this.applicantNationalityId() || undefined,
        placeOfBirthCountryId: this.applicantCountryOfBirth() || undefined,
        idType: this.clientIdTypeCode().trim() || undefined,
        idNumber: this.clientIdNumber().trim() || undefined,
        address: this.applicantResidentialAddress().trim() || undefined,
        genderId: this.applicantGenderId() || undefined,
        matchThreshold: Number(this.matchThreshold() ?? 85),
        birthYearRange: null,

        checkPepUkOnly: this.checkPepUkOnly(),
        checkSanctions: this.checkSanctions(),
        checkProfileOfInterest: this.checkProfileOfInterest(),
        checkDisqualifiedDirectorUkOnly: this.checkDisqualifiedDirectorUkOnly(),
        checkReputationalRiskExposure: this.checkReputationalRiskExposure(),
        checkRegulatoryEnforcementList: this.checkRegulatoryEnforcementList(),
        checkInsolvencyUkIreland: this.checkInsolvencyUkIreland()
      };

      const screeningRes = await firstValueFrom(this.screeningApi.upsert(cid, screeningBody));
      if (!screeningRes.success) throw new Error(screeningRes.message ?? 'Screening sync failed');

      // Upload/replace KYC document rows that have a file selected.
      for (const row of this.documentRows()) {
        if (!row.selectedFile) continue;

        if (row.existingDocId) {
          const delRes = await firstValueFrom(this.kycApi.deleteIndividualKycDocument(cid, row.existingDocId));
          if (!delRes.success) throw new Error(delRes.message ?? 'Document replace delete failed');
        }

        const uploadReq: UploadIndividualKycDocumentRequest = {
          documentNo: row.documentNo.trim() || undefined,
          issuedDate: this.dateInputToIso(row.issuedDate) ?? undefined,
          expiryDate: this.dateInputToIso(row.expiryDate) ?? undefined,
          approvedBy: row.approvedBy.trim() || undefined,
          folderPath: row.folderPath.trim() || undefined
        };

        const upRes = await firstValueFrom(this.kycApi.uploadIndividualKycDocument(cid, uploadReq, row.selectedFile));
        if (!upRes.success) throw new Error(upRes.message ?? 'Document upload failed');
      }

      this.notification.success('KYC saved successfully.');
      const okMsg = this.translate.instant('individualKyc.savedSuccess');
      this.notification.success(okMsg);
      this.submitSuccess.set(okMsg);

      // Reload everything to reflect latest active KYC + documents.
      await this.loadIndividualKyc(cid);
      await this.loadScreening(cid);
    } catch (e) {
      const msg = (e as Error).message ?? 'Save failed';
      this.submitError.set(msg);
      this.notification.error(msg);
    } finally {
      this.submitting.set(false);
    }
  }

}

