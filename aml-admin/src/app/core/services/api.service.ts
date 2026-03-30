import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  ApiResponse,
  PagedRequest,
  PagedResult,
  CustomerDto,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  CustomerTypeDto,
  CustomerStatusDto,
  GenderDto,
  NationalityDto,
  CountryDto,
  DocumentTypeDto,
  OccupationDto,
  SourceOfFundsDto,
  CustomerDocumentDto,
  SanctionsScreeningDto,
  SanctionsScreeningResultItemDto,
  RunSanctionsScreeningResultDto,
  CreateSanctionsScreeningRequest,
  UpdateSanctionsScreeningRequest,
  RiskAssignmentDto,
  CreateRiskAssignmentRequest,
  UpdateRiskAssignmentRequest,
  CaseDto,
  CreateCaseRequest,
  UpdateCaseRequest,
  AuditLogDto,
  SanctionListSourceDto,
  SanctionListUploadResultDto,
  SanctionListEntryDto,
  CreateSanctionListEntryRequest,
  RecordSanctionScreeningActionRequest,
  SanctionActionAuditLogDto,
  IndividualBulkUploadResultDto,
  IndividualBulkUploadBatchListItemDto,
  IndividualBulkUploadLineDetailDto,
  CorporateBulkUploadResultDto,
  CorporateBulkUploadBatchListItemDto,
  CorporateBulkUploadLineDetailDto,
  UpsertMasterLookupRequest,
  UpsertEmirateRequest,
  EmirateDto,
  ResidenceStatusDto,
  BulkUploadKind,
  InstantSanctionScreeningSearchRequest,
  InstantSanctionScreeningResultItem,
  CustomerDashboardKpisDto
} from '../../shared/models/api.model';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  private pagedParams(req: PagedRequest): HttpParams {
    let params = new HttpParams()
      .set('pageNumber', req.pageNumber.toString())
      .set('pageSize', req.pageSize.toString());
    if (req.sortBy) params = params.set('sortBy', req.sortBy);
    if (req.sortDescending != null) params = params.set('sortDescending', req.sortDescending.toString());
    if (req.searchTerm) params = params.set('searchTerm', req.searchTerm);
    return params;
  }

  getCustomerTypes(): Observable<ApiResponse<CustomerTypeDto[]>> {
    return this.http.get<ApiResponse<CustomerTypeDto[]>>(`${BASE}/api/Lookups/customer-types`);
  }
  getCustomerStatuses(): Observable<ApiResponse<CustomerStatusDto[]>> {
    return this.http.get<ApiResponse<CustomerStatusDto[]>>(`${BASE}/api/Lookups/customer-statuses`);
  }
  getGenders(): Observable<ApiResponse<GenderDto[]>> {
    return this.http.get<ApiResponse<GenderDto[]>>(`${BASE}/api/Lookups/genders`);
  }
  getNationalities(): Observable<ApiResponse<NationalityDto[]>> {
    return this.http.get<ApiResponse<NationalityDto[]>>(`${BASE}/api/Lookups/nationalities`);
  }
  getCountries(): Observable<ApiResponse<CountryDto[]>> {
    return this.http.get<ApiResponse<CountryDto[]>>(`${BASE}/api/Lookups/countries`);
  }
  getDocumentTypes(): Observable<ApiResponse<DocumentTypeDto[]>> {
    return this.http.get<ApiResponse<DocumentTypeDto[]>>(`${BASE}/api/Lookups/document-types`);
  }
  getOccupations(): Observable<ApiResponse<OccupationDto[]>> {
    return this.http.get<ApiResponse<OccupationDto[]>>(`${BASE}/api/Lookups/occupations`);
  }
  getSourceOfFunds(): Observable<ApiResponse<SourceOfFundsDto[]>> {
    return this.http.get<ApiResponse<SourceOfFundsDto[]>>(`${BASE}/api/Lookups/source-of-funds`);
  }

  getCustomerDashboardKpis(): Observable<ApiResponse<CustomerDashboardKpisDto>> {
    return this.http.get<ApiResponse<CustomerDashboardKpisDto>>(`${BASE}/api/dashboard/customer-kpis`);
  }

  getCustomers(req: PagedRequest): Observable<ApiResponse<PagedResult<CustomerDto>>> {
    return this.http.get<ApiResponse<PagedResult<CustomerDto>>>(`${BASE}/api/Customers`, { params: this.pagedParams(req) });
  }
  getCustomer(id: string): Observable<ApiResponse<CustomerDto>> {
    return this.http.get<ApiResponse<CustomerDto>>(`${BASE}/api/Customers/${id}`);
  }
  createCustomer(payload: CreateCustomerRequest): Observable<ApiResponse<CustomerDto>> {
    return this.http.post<ApiResponse<CustomerDto>>(`${BASE}/api/Customers`, payload);
  }
  updateCustomer(id: string, payload: UpdateCustomerRequest): Observable<ApiResponse<CustomerDto>> {
    return this.http.put<ApiResponse<CustomerDto>>(`${BASE}/api/Customers/${id}`, payload);
  }
  deleteCustomer(id: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/Customers/${id}`);
  }
  getCustomerDocuments(customerId: string): Observable<ApiResponse<CustomerDocumentDto[]>> {
    return this.http.get<ApiResponse<CustomerDocumentDto[]>>(`${BASE}/api/Customers/${customerId}/documents`);
  }
  uploadCustomerDocument(customerId: string, documentTypeCode: string, file: File, expiryDate?: string): Observable<ApiResponse<CustomerDocumentDto>> {
    const form = new FormData();
    form.append('file', file);
    form.append('documentTypeCode', documentTypeCode);
    if (expiryDate) form.append('expiryDate', expiryDate);
    return this.http.post<ApiResponse<CustomerDocumentDto>>(`${BASE}/api/Customers/${customerId}/documents`, form);
  }
  downloadCustomerDocument(customerId: string, documentId: string): Observable<Blob> {
    return this.http.get(`${BASE}/api/Customers/${customerId}/documents/${documentId}/download`, { responseType: 'blob' });
  }
  runCustomerSanctionsScreening(customerId: string): Observable<ApiResponse<RunSanctionsScreeningResultDto>> {
    return this.http.post<ApiResponse<RunSanctionsScreeningResultDto>>(`${BASE}/api/Customers/${customerId}/sanctions-screening/run`, {});
  }
  getCustomerSanctionsScreeningResults(customerId: string): Observable<ApiResponse<SanctionsScreeningResultItemDto[]>> {
    return this.http.get<ApiResponse<SanctionsScreeningResultItemDto[]>>(`${BASE}/api/Customers/${customerId}/sanctions-screening/results`);
  }
  recordSanctionScreeningAction(customerId: string, screeningId: string, body: RecordSanctionScreeningActionRequest): Observable<ApiResponse<SanctionsScreeningResultItemDto>> {
    return this.http.post<ApiResponse<SanctionsScreeningResultItemDto>>(`${BASE}/api/Customers/${customerId}/sanctions-screening/${screeningId}/action`, body);
  }
  getSanctionActionAuditLogs(params: { customerId?: string; sanctionsScreeningId?: string; fromDate?: string; toDate?: string }): Observable<ApiResponse<SanctionActionAuditLogDto[]>> {
    let httpParams = new HttpParams();
    if (params.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params.sanctionsScreeningId) httpParams = httpParams.set('sanctionsScreeningId', params.sanctionsScreeningId);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    return this.http.get<ApiResponse<SanctionActionAuditLogDto[]>>(`${BASE}/api/SanctionActionAuditLogs`, { params: httpParams });
  }

  getCases(req: PagedRequest): Observable<ApiResponse<PagedResult<CaseDto>>> {
    return this.http.get<ApiResponse<PagedResult<CaseDto>>>(`${BASE}/api/Cases`, { params: this.pagedParams(req) });
  }
  getCase(id: string): Observable<ApiResponse<CaseDto>> {
    return this.http.get<ApiResponse<CaseDto>>(`${BASE}/api/Cases/${id}`);
  }
  createCase(payload: CreateCaseRequest): Observable<ApiResponse<CaseDto>> {
    return this.http.post<ApiResponse<CaseDto>>(`${BASE}/api/Cases`, payload);
  }
  updateCase(id: string, payload: UpdateCaseRequest): Observable<ApiResponse<CaseDto>> {
    return this.http.put<ApiResponse<CaseDto>>(`${BASE}/api/Cases/${id}`, payload);
  }
  deleteCase(id: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/Cases/${id}`);
  }

  getRiskAssignments(req: PagedRequest): Observable<ApiResponse<PagedResult<RiskAssignmentDto>>> {
    return this.http.get<ApiResponse<PagedResult<RiskAssignmentDto>>>(`${BASE}/api/RiskAssignment`, { params: this.pagedParams(req) });
  }
  getRiskAssignment(id: string): Observable<ApiResponse<RiskAssignmentDto>> {
    return this.http.get<ApiResponse<RiskAssignmentDto>>(`${BASE}/api/RiskAssignment/${id}`);
  }
  createRiskAssignment(payload: CreateRiskAssignmentRequest): Observable<ApiResponse<RiskAssignmentDto>> {
    return this.http.post<ApiResponse<RiskAssignmentDto>>(`${BASE}/api/RiskAssignment`, payload);
  }
  updateRiskAssignment(id: string, payload: UpdateRiskAssignmentRequest): Observable<ApiResponse<RiskAssignmentDto>> {
    return this.http.put<ApiResponse<RiskAssignmentDto>>(`${BASE}/api/RiskAssignment/${id}`, payload);
  }
  deleteRiskAssignment(id: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/RiskAssignment/${id}`);
  }

  getSanctionsScreening(req: PagedRequest): Observable<ApiResponse<PagedResult<SanctionsScreeningDto>>> {
    return this.http.get<ApiResponse<PagedResult<SanctionsScreeningDto>>>(`${BASE}/api/SanctionsScreening`, { params: this.pagedParams(req) });
  }
  getSanctionsScreeningById(id: string): Observable<ApiResponse<SanctionsScreeningDto>> {
    return this.http.get<ApiResponse<SanctionsScreeningDto>>(`${BASE}/api/SanctionsScreening/${id}`);
  }
  createSanctionsScreening(payload: CreateSanctionsScreeningRequest): Observable<ApiResponse<SanctionsScreeningDto>> {
    return this.http.post<ApiResponse<SanctionsScreeningDto>>(`${BASE}/api/SanctionsScreening`, payload);
  }
  updateSanctionsScreening(id: string, payload: UpdateSanctionsScreeningRequest): Observable<ApiResponse<SanctionsScreeningDto>> {
    return this.http.put<ApiResponse<SanctionsScreeningDto>>(`${BASE}/api/SanctionsScreening/${id}`, payload);
  }
  deleteSanctionsScreening(id: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/SanctionsScreening/${id}`);
  }

  getAuditLogs(req: PagedRequest): Observable<ApiResponse<PagedResult<AuditLogDto>>> {
    return this.http.get<ApiResponse<PagedResult<AuditLogDto>>>(`${BASE}/api/AuditLogs`, { params: this.pagedParams(req) });
  }
  getAuditLog(id: string): Observable<ApiResponse<AuditLogDto>> {
    return this.http.get<ApiResponse<AuditLogDto>>(`${BASE}/api/AuditLogs/${id}`);
  }

  getSanctionListSources(): Observable<ApiResponse<SanctionListSourceDto[]>> {
    return this.http.get<ApiResponse<SanctionListSourceDto[]>>(`${BASE}/api/SanctionLists/sources`);
  }
  uploadSanctionList(file: File, listSource: string): Observable<ApiResponse<SanctionListUploadResultDto>> {
    const form = new FormData();
    form.append('file', file);
    form.append('listSource', listSource);
    return this.http.post<ApiResponse<SanctionListUploadResultDto>>(`${BASE}/api/SanctionLists/upload`, form);
  }

  getSanctionListEntries(params: { searchTerm?: string; listSource?: string; pageNumber?: number; pageSize?: number }): Observable<ApiResponse<PagedResult<SanctionListEntryDto>>> {
    const { searchTerm, listSource, pageNumber = 1, pageSize = 20 } = params;
    let url = `${BASE}/api/SanctionLists/entries?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (searchTerm?.trim()) url += `&searchTerm=${encodeURIComponent(searchTerm.trim())}`;
    if (listSource?.trim()) url += `&listSource=${encodeURIComponent(listSource.trim())}`;
    return this.http.get<ApiResponse<PagedResult<SanctionListEntryDto>>>(url);
  }

  deleteSanctionListBySource(listSource: string): Observable<ApiResponse<number>> {
    return this.http.delete<ApiResponse<number>>(`${BASE}/api/SanctionLists/entries?listSource=${encodeURIComponent(listSource)}`);
  }

  createSanctionEntry(body: CreateSanctionListEntryRequest): Observable<ApiResponse<SanctionListEntryDto>> {
    return this.http.post<ApiResponse<SanctionListEntryDto>>(`${BASE}/api/SanctionLists/entries`, body);
  }
  downloadBulkSample(kind: BulkUploadKind): Observable<Blob> {
    const path =
      kind === 'cor' ? `${BASE}/api/corporate-bulk-upload/sample` : `${BASE}/api/individual-bulk-upload/sample`;
    return this.http.get(path, { responseType: 'blob' });
  }

  /** @deprecated Use downloadBulkSample('ind') */
  downloadIndividualBulkSample(): Observable<Blob> {
    return this.downloadBulkSample('ind');
  }

  uploadBulk(
    kind: BulkUploadKind,
    file: File,
    options: {
      matchThreshold: number;
      checkPepUkOnly: boolean;
      checkDisqualifiedDirectorUkOnly: boolean;
      checkSanctions: boolean;
      checkProfileOfInterest: boolean;
      checkReputationalRiskExposure: boolean;
      checkRegulatoryEnforcementList: boolean;
      checkInsolvencyUkIreland: boolean;
    }
  ): Observable<ApiResponse<IndividualBulkUploadResultDto | CorporateBulkUploadResultDto>> {
    const form = new FormData();
    form.append('file', file);
    form.append('matchThreshold', String(options.matchThreshold));
    form.append('checkPepUkOnly', String(options.checkPepUkOnly));
    form.append('checkDisqualifiedDirectorUkOnly', String(options.checkDisqualifiedDirectorUkOnly));
    form.append('checkSanctions', String(options.checkSanctions));
    form.append('checkProfileOfInterest', String(options.checkProfileOfInterest));
    form.append('checkReputationalRiskExposure', String(options.checkReputationalRiskExposure));
    form.append('checkRegulatoryEnforcementList', String(options.checkRegulatoryEnforcementList));
    form.append('checkInsolvencyUkIreland', String(options.checkInsolvencyUkIreland));
    const url =
      kind === 'cor'
        ? `${BASE}/api/corporate-bulk-upload/upload`
        : `${BASE}/api/individual-bulk-upload/upload`;
    return this.http.post<ApiResponse<IndividualBulkUploadResultDto | CorporateBulkUploadResultDto>>(url, form);
  }

  /** @deprecated Use uploadBulk('ind', ...) */
  uploadIndividualBulk(
    file: File,
    options: {
      matchThreshold: number;
      checkPepUkOnly: boolean;
      checkDisqualifiedDirectorUkOnly: boolean;
      checkSanctions: boolean;
      checkProfileOfInterest: boolean;
      checkReputationalRiskExposure: boolean;
      checkRegulatoryEnforcementList: boolean;
      checkInsolvencyUkIreland: boolean;
    }
  ): Observable<ApiResponse<IndividualBulkUploadResultDto>> {
    return this.uploadBulk('ind', file, options) as Observable<ApiResponse<IndividualBulkUploadResultDto>>;
  }

  getBulkBatches(
    kind: BulkUploadKind,
    params: { from?: string; to?: string; uploadedBy?: string }
  ): Observable<ApiResponse<IndividualBulkUploadBatchListItemDto[] | CorporateBulkUploadBatchListItemDto[]>> {
    let httpParams = new HttpParams();
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.uploadedBy?.trim()) httpParams = httpParams.set('uploadedBy', params.uploadedBy.trim());
    const base =
      kind === 'cor' ? `${BASE}/api/corporate-bulk-upload/batches` : `${BASE}/api/individual-bulk-upload/batches`;
    return this.http.get<ApiResponse<IndividualBulkUploadBatchListItemDto[] | CorporateBulkUploadBatchListItemDto[]>>(
      base,
      { params: httpParams }
    );
  }

  getBulkBatchLines(
    kind: BulkUploadKind,
    batchId: string,
    caseStatus?: string
  ): Observable<ApiResponse<IndividualBulkUploadLineDetailDto[] | CorporateBulkUploadLineDetailDto[]>> {
    let params = new HttpParams();
    if (caseStatus && caseStatus !== 'all') params = params.set('caseStatus', caseStatus);
    const base =
      kind === 'cor'
        ? `${BASE}/api/corporate-bulk-upload/batches/${batchId}/lines`
        : `${BASE}/api/individual-bulk-upload/batches/${batchId}/lines`;
    return this.http.get<ApiResponse<IndividualBulkUploadLineDetailDto[] | CorporateBulkUploadLineDetailDto[]>>(
      base,
      { params }
    );
  }

  /** @deprecated Use getBulkBatches('ind', params) */
  getIndividualBulkBatches(params: { from?: string; to?: string; uploadedBy?: string }): Observable<ApiResponse<IndividualBulkUploadBatchListItemDto[]>> {
    return this.getBulkBatches('ind', params) as Observable<ApiResponse<IndividualBulkUploadBatchListItemDto[]>>;
  }

  /** @deprecated Use getBulkBatchLines('ind', ...) */
  getIndividualBulkBatchLines(batchId: string, caseStatus?: string): Observable<ApiResponse<IndividualBulkUploadLineDetailDto[]>> {
    return this.getBulkBatchLines('ind', batchId, caseStatus) as Observable<
      ApiResponse<IndividualBulkUploadLineDetailDto[]>
    >;
  }

  getMasterLookups(segment: string): Observable<ApiResponse<CountryDto[]>> {
    return this.http.get<ApiResponse<CountryDto[]>>(`${BASE}/api/MasterLookups/${segment}`);
  }

  getMasterLookup(segment: string, id: string): Observable<ApiResponse<CountryDto>> {
    return this.http.get<ApiResponse<CountryDto>>(`${BASE}/api/MasterLookups/${segment}/${id}`);
  }

  createMasterLookup(segment: string, body: UpsertMasterLookupRequest): Observable<ApiResponse<CountryDto>> {
    return this.http.post<ApiResponse<CountryDto>>(`${BASE}/api/MasterLookups/${segment}`, body);
  }

  updateMasterLookup(segment: string, id: string, body: UpsertMasterLookupRequest): Observable<ApiResponse<CountryDto>> {
    return this.http.put<ApiResponse<CountryDto>>(`${BASE}/api/MasterLookups/${segment}/${id}`, body);
  }

  deleteMasterLookup(segment: string, id: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/MasterLookups/${segment}/${id}`);
  }

  getEmirates(countryId?: string): Observable<ApiResponse<EmirateDto[]>> {
    let params = new HttpParams();
    if (countryId?.trim()) params = params.set('countryId', countryId.trim());
    return this.http.get<ApiResponse<EmirateDto[]>>(`${BASE}/api/MasterLookups/emirates`, { params });
  }

  getEmirate(id: string): Observable<ApiResponse<EmirateDto>> {
    return this.http.get<ApiResponse<EmirateDto>>(`${BASE}/api/MasterLookups/emirates/${id}`);
  }

  createEmirate(body: UpsertEmirateRequest): Observable<ApiResponse<EmirateDto>> {
    return this.http.post<ApiResponse<EmirateDto>>(`${BASE}/api/MasterLookups/emirates`, body);
  }

  updateEmirate(id: string, body: UpsertEmirateRequest): Observable<ApiResponse<EmirateDto>> {
    return this.http.put<ApiResponse<EmirateDto>>(`${BASE}/api/MasterLookups/emirates/${id}`, body);
  }

  deleteEmirate(id: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/MasterLookups/emirates/${id}`);
  }

  getResidenceStatuses(): Observable<ApiResponse<ResidenceStatusDto[]>> {
    return this.http.get<ApiResponse<ResidenceStatusDto[]>>(`${BASE}/api/MasterLookups/residence-statuses`);
  }

  searchInstantSanctionScreening(
    body: InstantSanctionScreeningSearchRequest
  ): Observable<ApiResponse<InstantSanctionScreeningResultItem[]>> {
    return this.http.post<ApiResponse<InstantSanctionScreeningResultItem[]>>(
      `${BASE}/api/instant-sanction-screening/search`,
      body
    );
  }
}


