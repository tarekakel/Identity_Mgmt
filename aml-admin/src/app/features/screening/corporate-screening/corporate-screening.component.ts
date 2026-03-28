import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { CorporateScreeningService } from '../../../core/services/corporate-screening.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  ApiResponse,
  CorporateScreeningCompanyDocumentDto,
  CorporateScreeningRequestDto,
  CorporateScreeningShareholderDocumentDto,
  CorporateScreeningShareholderDto,
  CountryDto,
  CreateCustomerRequest,
  CustomerDto,
  CustomerTypeDto,
  NationalityDto,
  PagedResult,
  SanctionsScreeningResultItemDto,
  UpsertCorporateScreeningRequest,
  UpsertCorporateScreeningCompanyDocumentDto,
  UpsertCorporateScreeningShareholderDocumentDto
} from '../../../shared/models/api.model';

const DEFAULT_TENANT_ID = '00000000-0000-0000-0000-000000000001';

export interface CorporateDocumentRow {
  id: string;
  documentNo: string;
  issuedDate: string;
  expiryDate: string;
  details: string;
  remarks: string;
}

export interface ShareholderRow {
  id: string;
  fullName: string;
  nationalityId: string;
  dateOfBirth: string;
  share: number;
  documents: CorporateDocumentRow[];
}

/** One repeatable corporate entity (maps to a CorporateScreeningRequest). */
export interface CorporateFormBlock {
  clientId: string;
  serverRequestId?: string;
  checkPepUkOnly: boolean;
  checkDisqualifiedDirectorUkOnly: boolean;
  checkSanctions: boolean;
  checkProfileOfInterest: boolean;
  checkReputationalRiskExposure: boolean;
  checkRegulatoryEnforcementList: boolean;
  checkInsolvencyUkIreland: boolean;
  companyCode: string;
  fullName: string;
  countryId: string;
  dateOfRegistration: string;
  tradeLicenceNo: string;
  address: string;
  documents: CorporateDocumentRow[];
  shareholders: ShareholderRow[];
  matchThreshold: number;
}

function newId(): string {
  return typeof crypto !== 'undefined' && crypto.randomUUID ? crypto.randomUUID() : String(Date.now() + Math.random());
}

function emptyDocumentRow(): CorporateDocumentRow {
  return {
    id: newId(),
    documentNo: '',
    issuedDate: '',
    expiryDate: '',
    details: '',
    remarks: ''
  };
}

function emptyShareholder(): ShareholderRow {
  return {
    id: newId(),
    fullName: '',
    nationalityId: '',
    dateOfBirth: '',
    share: 0,
    documents: [emptyDocumentRow()]
  };
}

function createEmptyBlock(): CorporateFormBlock {
  return {
    clientId: newId(),
    checkPepUkOnly: true,
    checkDisqualifiedDirectorUkOnly: true,
    checkSanctions: true,
    checkProfileOfInterest: true,
    checkReputationalRiskExposure: true,
    checkRegulatoryEnforcementList: true,
    checkInsolvencyUkIreland: true,
    companyCode: '',
    fullName: '',
    countryId: '',
    dateOfRegistration: '',
    tradeLicenceNo: '',
    address: '',
    documents: [emptyDocumentRow()],
    shareholders: [emptyShareholder()],
    matchThreshold: 75
  };
}

@Component({
  selector: 'app-corporate-screening',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './corporate-screening.component.html',
  styleUrls: ['./corporate-screening.component.scss']
})
export class CorporateScreeningComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly corporateApi = inject(CorporateScreeningService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  customerSearchTerm = signal('');
  customerOptions = signal<CustomerDto[]>([]);
  customerLoading = signal(false);
  customerDropdownOpen = signal(false);
  selectedCustomer = signal<CustomerDto | null>(null);
  selectedCustomerId = computed(() => this.selectedCustomer()?.id ?? '');

  countries = signal<CountryDto[]>([]);
  nationalities = signal<NationalityDto[]>([]);
  customerTypes = signal<CustomerTypeDto[]>([]);

  corporates = signal<CorporateFormBlock[]>([createEmptyBlock()]);

  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  /** Filter results: '' = all for customer, else request id */
  resultsFilterRequestId = signal<string>('');

  results = signal<SanctionsScreeningResultItemDto[]>([]);
  resultsLoading = signal(false);

  ngOnInit(): void {
    this.api.getCountries().subscribe({
      next: (res: ApiResponse<CountryDto[]>) => {
        if (res.success && res.data) this.countries.set(res.data);
      }
    });
    this.api.getNationalities().subscribe({
      next: (res: ApiResponse<NationalityDto[]>) => {
        if (res.success && res.data) this.nationalities.set(res.data);
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
    this.resultsFilterRequestId.set('');

    this.loadListCriteria();
    this.refreshResults();
  }

  private loadListCriteria(): void {
    const cid = this.selectedCustomerId();
    if (!cid) return;
    this.loading.set(true);
    this.corporateApi.listRequests(cid).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data?.length) {
          this.corporates.set(res.data.map((dto) => this.dtoToBlock(dto)));
        } else {
          this.corporates.set([createEmptyBlock()]);
        }
      },
      error: () => {
        this.loading.set(false);
        this.corporates.set([createEmptyBlock()]);
      }
    });
  }

  private dtoToBlock(req: CorporateScreeningRequestDto): CorporateFormBlock {
    const companyDocs =
      req.companyDocuments?.length ?
        req.companyDocuments.map((d) => this.mapCompanyDtoToRow(d))
      : [emptyDocumentRow()];
    const sh =
      req.shareholders?.length ?
        req.shareholders.map((s) => this.mapShareholderDtoToRow(s))
      : [emptyShareholder()];
    return {
      clientId: newId(),
      serverRequestId: req.id,
      checkPepUkOnly: !!req.checkPepUkOnly,
      checkDisqualifiedDirectorUkOnly: !!req.checkDisqualifiedDirectorUkOnly,
      checkSanctions: !!req.checkSanctions,
      checkProfileOfInterest: !!req.checkProfileOfInterest,
      checkReputationalRiskExposure: !!req.checkReputationalRiskExposure,
      checkRegulatoryEnforcementList: !!req.checkRegulatoryEnforcementList,
      checkInsolvencyUkIreland: !!req.checkInsolvencyUkIreland,
      companyCode: req.companyCode ?? '',
      fullName: req.fullName ?? '',
      countryId: req.countryId ?? '',
      dateOfRegistration: req.dateOfRegistration ? this.toDateInputValue(req.dateOfRegistration) : '',
      tradeLicenceNo: req.tradeLicenceNo ?? '',
      address: req.address ?? '',
      matchThreshold: req.matchThreshold ?? 75,
      documents: companyDocs,
      shareholders: sh
    };
  }

  private mapCompanyDtoToRow(d: CorporateScreeningCompanyDocumentDto): CorporateDocumentRow {
    return {
      id: d.id ?? newId(),
      documentNo: d.documentNo ?? '',
      issuedDate: d.issuedDate ? this.toDateInputValue(d.issuedDate) : '',
      expiryDate: d.expiryDate ? this.toDateInputValue(d.expiryDate) : '',
      details: d.details ?? '',
      remarks: d.remarks ?? ''
    };
  }

  private mapShDocDtoToRow(d: CorporateScreeningShareholderDocumentDto): CorporateDocumentRow {
    return {
      id: d.id ?? newId(),
      documentNo: d.documentNo ?? '',
      issuedDate: d.issuedDate ? this.toDateInputValue(d.issuedDate) : '',
      expiryDate: d.expiryDate ? this.toDateInputValue(d.expiryDate) : '',
      details: d.details ?? '',
      remarks: d.remarks ?? ''
    };
  }

  private mapShareholderDtoToRow(s: CorporateScreeningShareholderDto): ShareholderRow {
    return {
      id: s.id ?? newId(),
      fullName: s.fullName ?? '',
      nationalityId: s.nationalityId ?? '',
      dateOfBirth: s.dateOfBirth ? this.toDateInputValue(s.dateOfBirth) : '',
      share: s.sharePercent,
      documents:
        s.documents?.length ? s.documents.map((d) => this.mapShDocDtoToRow(d)) : [emptyDocumentRow()]
    };
  }

  updateBlock(index: number, patch: Partial<CorporateFormBlock>): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => (i === index ? { ...row, ...patch } : row))
    );
  }

  selectAllCategories(): void {
    this.corporates.update((rows) =>
      rows.map((r) => ({
        ...r,
        checkPepUkOnly: true,
        checkDisqualifiedDirectorUkOnly: true,
        checkSanctions: true,
        checkProfileOfInterest: true,
        checkReputationalRiskExposure: true,
        checkRegulatoryEnforcementList: true,
        checkInsolvencyUkIreland: true
      }))
    );
  }

  clearAllCategories(): void {
    this.corporates.update((rows) =>
      rows.map((r) => ({
        ...r,
        checkPepUkOnly: false,
        checkDisqualifiedDirectorUkOnly: false,
        checkSanctions: false,
        checkProfileOfInterest: false,
        checkReputationalRiskExposure: false,
        checkRegulatoryEnforcementList: false,
        checkInsolvencyUkIreland: false
      }))
    );
  }

  addCorporate(): void {
    this.corporates.update((rows) => [...rows, createEmptyBlock()]);
  }

  removeCorporate(index: number): void {
    if (this.corporates().length <= 1) return;
    const cid = this.selectedCustomerId();
    const block = this.corporates()[index];
    const removeLocal = (): void => {
      this.corporates.update((rows) => rows.filter((_, i) => i !== index));
    };

    if (cid && block.serverRequestId) {
      this.loading.set(true);
      this.corporateApi.deleteRequest(cid, block.serverRequestId).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success) {
            removeLocal();
            this.refreshResults();
          } else {
            const msg = res.message ?? this.translate.instant('common.deleteFailed');
            this.notification.error(msg);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notification.error(err?.error?.message ?? this.translate.instant('common.deleteFailed'));
        }
      });
    } else {
      removeLocal();
    }
  }

  addDocument(bi: number): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => (i === bi ? { ...row, documents: [...row.documents, emptyDocumentRow()] } : row))
    );
  }

  removeDocument(bi: number, docIndex: number): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => {
        if (i !== bi) return row;
        if (row.documents.length <= 1) return row;
        return { ...row, documents: row.documents.filter((_, di) => di !== docIndex) };
      })
    );
  }

  updateCompanyDoc(bi: number, docIndex: number, patch: Partial<CorporateDocumentRow>): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => {
        if (i !== bi) return row;
        return {
          ...row,
          documents: row.documents.map((d, di) => (di === docIndex ? { ...d, ...patch } : d))
        };
      })
    );
  }

  addShareholder(bi: number): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => (i === bi ? { ...row, shareholders: [...row.shareholders, emptyShareholder()] } : row))
    );
  }

  removeShareholder(bi: number, shIndex: number): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => {
        if (i !== bi) return row;
        if (row.shareholders.length <= 1) return row;
        return { ...row, shareholders: row.shareholders.filter((_, si) => si !== shIndex) };
      })
    );
  }

  updateShareholder(bi: number, shIndex: number, patch: Partial<Omit<ShareholderRow, 'documents'>>): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => {
        if (i !== bi) return row;
        return {
          ...row,
          shareholders: row.shareholders.map((sh, si) => (si === shIndex ? { ...sh, ...patch } : sh))
        };
      })
    );
  }

  addShareholderDocument(bi: number, shIndex: number): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => {
        if (i !== bi) return row;
        return {
          ...row,
          shareholders: row.shareholders.map((sh, si) =>
            si === shIndex ? { ...sh, documents: [...sh.documents, emptyDocumentRow()] } : sh
          )
        };
      })
    );
  }

  removeShareholderDocument(bi: number, shIndex: number, docIndex: number): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => {
        if (i !== bi) return row;
        return {
          ...row,
          shareholders: row.shareholders.map((sh, si) => {
            if (si !== shIndex) return sh;
            if (sh.documents.length <= 1) return sh;
            return { ...sh, documents: sh.documents.filter((_, di) => di !== docIndex) };
          })
        };
      })
    );
  }

  updateShareholderDoc(
    bi: number,
    shIndex: number,
    docIndex: number,
    patch: Partial<CorporateDocumentRow>
  ): void {
    this.corporates.update((rows) =>
      rows.map((row, i) => {
        if (i !== bi) return row;
        return {
          ...row,
          shareholders: row.shareholders.map((sh, si) => {
            if (si !== shIndex) return sh;
            return {
              ...sh,
              documents: sh.documents.map((d, di) => (di === docIndex ? { ...d, ...patch } : d))
            };
          })
        };
      })
    );
  }

  resetToLastSaved(): void {
    this.error.set(null);
    this.success.set(null);
    this.loadListCriteria();
  }

  onSubmit(): void {
    this.error.set(null);
    this.success.set(null);

    const toSave = this.corporates().map((b, index) => ({ b, index })).filter((x) => x.b.fullName.trim());
    if (!toSave.length) {
      const msg = this.translate.instant('individualScreening.errors.fullNameRequired');
      this.error.set(msg);
      this.notification.error(msg);
      return;
    }

    const buildBody = (block: CorporateFormBlock): UpsertCorporateScreeningRequest => ({
      id: block.serverRequestId ?? undefined,
      tenantId: DEFAULT_TENANT_ID,
      companyCode: block.companyCode.trim() || null,
      fullName: block.fullName.trim(),
      countryId: block.countryId || null,
      dateOfRegistration: this.optionalDateIso(block.dateOfRegistration),
      tradeLicenceNo: block.tradeLicenceNo.trim() || null,
      address: block.address.trim() || null,
      matchThreshold: Number(block.matchThreshold ?? 75),
      checkPepUkOnly: !!block.checkPepUkOnly,
      checkSanctions: !!block.checkSanctions,
      checkProfileOfInterest: !!block.checkProfileOfInterest,
      checkDisqualifiedDirectorUkOnly: !!block.checkDisqualifiedDirectorUkOnly,
      checkReputationalRiskExposure: !!block.checkReputationalRiskExposure,
      checkRegulatoryEnforcementList: !!block.checkRegulatoryEnforcementList,
      checkInsolvencyUkIreland: !!block.checkInsolvencyUkIreland,
      companyDocuments: block.documents.map((d) => this.mapCompanyRowToUpsert(d)),
      shareholders: block.shareholders.map((sh) => this.mapShareholderRowToUpsert(sh))
    });

    const startPipeline = (customerId: string): void => {
      this.loading.set(true);
      const requestIds: string[] = [];

      const runUpsert = (pos: number): void => {
        if (pos >= toSave.length) {
          this.runAllForRequests(customerId, requestIds);
          return;
        }
        const { index } = toSave[pos];
        const block = this.corporates()[index];
        const body = buildBody(block);
        this.corporateApi.upsert(customerId, body).subscribe({
          next: (res) => {
            if (res.success && res.data) {
              this.updateBlock(index, {
                serverRequestId: res.data.id,
                ...this.partialBlockFromDto(res.data)
              });
              requestIds.push(res.data.id);
              runUpsert(pos + 1);
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

      runUpsert(0);
    };

    const cid = this.selectedCustomerId();
    if (cid) {
      startPipeline(cid);
      return;
    }

    const first = toSave[0].b;
    const corporateTypeId = this.customerTypes().find((t) => t.code === 'Corporate')?.id;
    if (!corporateTypeId) {
      const msg = this.translate.instant('corporateScreening.errors.corporateTypeMissing');
      this.error.set(msg);
      this.notification.error(msg);
      return;
    }

    const createPayload: CreateCustomerRequest = {
      tenantId: DEFAULT_TENANT_ID,
      fullName: first.fullName.trim(),
      address: first.address.trim() || undefined,
      countryOfResidenceId: first.countryId || undefined,
      customerTypeId: corporateTypeId
    };

    this.loading.set(true);
    this.api.createCustomer(createPayload).subscribe({
      next: (res: ApiResponse<CustomerDto>) => {
        if (res.success && res.data) {
          this.selectedCustomer.set(res.data);
          this.customerSearchTerm.set(res.data.fullName ?? '');
          this.loading.set(false);
          startPipeline(res.data.id);
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

  private partialBlockFromDto(d: CorporateScreeningRequestDto): Partial<CorporateFormBlock> {
    return {
      companyCode: d.companyCode ?? '',
      fullName: d.fullName ?? '',
      countryId: d.countryId ?? '',
      dateOfRegistration: d.dateOfRegistration ? this.toDateInputValue(d.dateOfRegistration) : '',
      tradeLicenceNo: d.tradeLicenceNo ?? '',
      address: d.address ?? '',
      matchThreshold: d.matchThreshold ?? 75
    };
  }

  private runAllForRequests(customerId: string, requestIds: string[]): void {
    if (!requestIds.length) {
      this.loading.set(false);
      return;
    }
    const runs = requestIds.map((rid) => this.corporateApi.runForRequest(customerId, rid));
    forkJoin(runs).subscribe({
      next: () => {
        this.loading.set(false);
        const msg = this.translate.instant('individualScreening.savedAndRun');
        this.success.set(msg);
        this.notification.success(msg);
        this.refreshResults();
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.message ?? this.translate.instant('common.errorGeneric');
        this.error.set(msg);
        this.notification.error(msg);
      }
    });
  }

  onResultsFilterChange(value: string): void {
    this.resultsFilterRequestId.set(value ?? '');
    this.refreshResults();
  }

  refreshResults(): void {
    const cid = this.selectedCustomerId();
    if (!cid) {
      this.results.set([]);
      return;
    }
    const filter = this.resultsFilterRequestId().trim();
    this.resultsLoading.set(true);
    this.corporateApi.getResults(cid, filter || null).subscribe({
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

  onCancel(): void {
    this.resetToLastSaved();
  }

  hasAtLeastOneFullName(): boolean {
    return this.corporates().some((b) => b.fullName.trim().length > 0);
  }

  blockLabel(block: CorporateFormBlock, index: number): string {
    const n = block.fullName.trim();
    if (n) return n;
    return `${this.translate.instant('corporateScreening.corporateEntity')} ${index + 1}`;
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

  private mapCompanyRowToUpsert(d: CorporateDocumentRow): UpsertCorporateScreeningCompanyDocumentDto {
    return {
      documentNo: d.documentNo.trim() || null,
      issuedDate: this.optionalDateIso(d.issuedDate),
      expiryDate: this.optionalDateIso(d.expiryDate),
      details: d.details.trim() || null,
      remarks: d.remarks.trim() || null
    };
  }

  private mapShDocRowToUpsert(d: CorporateDocumentRow): UpsertCorporateScreeningShareholderDocumentDto {
    return {
      documentNo: d.documentNo.trim() || null,
      issuedDate: this.optionalDateIso(d.issuedDate),
      expiryDate: this.optionalDateIso(d.expiryDate),
      details: d.details.trim() || null,
      remarks: d.remarks.trim() || null
    };
  }

  private mapShareholderRowToUpsert(sh: ShareholderRow) {
    return {
      fullName: sh.fullName.trim(),
      nationalityId: sh.nationalityId || null,
      dateOfBirth: this.optionalDateIso(sh.dateOfBirth),
      sharePercent: sh.share,
      documents: sh.documents.map((d) => this.mapShDocRowToUpsert(d))
    };
  }

  private optionalDateIso(value: string): string | null {
    const v = value?.trim();
    if (!v) return null;
    return new Date(v).toISOString();
  }

  private toDateInputValue(value: string): string {
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
