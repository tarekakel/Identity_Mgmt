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
  CreateSanctionListEntryRequest
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
}
