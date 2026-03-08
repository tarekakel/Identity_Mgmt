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
  SanctionsScreeningDto,
  CreateSanctionsScreeningRequest,
  UpdateSanctionsScreeningRequest,
  RiskAssignmentDto,
  CreateRiskAssignmentRequest,
  UpdateRiskAssignmentRequest,
  CaseDto,
  CreateCaseRequest,
  UpdateCaseRequest,
  AuditLogDto
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
}
