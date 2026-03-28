import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  ApiResponse,
  CorporateScreeningRequestDto,
  RunSanctionsScreeningResultDto,
  SanctionsScreeningResultItemDto,
  UpsertCorporateScreeningRequest
} from '../../shared/models/api.model';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class CorporateScreeningService {
  constructor(private http: HttpClient) {}

  getLatest(customerId: string): Observable<ApiResponse<CorporateScreeningRequestDto>> {
    return this.http.get<ApiResponse<CorporateScreeningRequestDto>>(`${BASE}/api/corporate-screening/${customerId}`);
  }

  listRequests(customerId: string): Observable<ApiResponse<CorporateScreeningRequestDto[]>> {
    return this.http.get<ApiResponse<CorporateScreeningRequestDto[]>>(
      `${BASE}/api/corporate-screening/${customerId}/requests`
    );
  }

  getById(customerId: string, requestId: string): Observable<ApiResponse<CorporateScreeningRequestDto>> {
    return this.http.get<ApiResponse<CorporateScreeningRequestDto>>(
      `${BASE}/api/corporate-screening/${customerId}/requests/${requestId}`
    );
  }

  deleteRequest(customerId: string, requestId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(
      `${BASE}/api/corporate-screening/${customerId}/requests/${requestId}`
    );
  }

  upsert(
    customerId: string,
    body: UpsertCorporateScreeningRequest
  ): Observable<ApiResponse<CorporateScreeningRequestDto>> {
    return this.http.post<ApiResponse<CorporateScreeningRequestDto>>(
      `${BASE}/api/corporate-screening/${customerId}`,
      body
    );
  }

  /** Legacy: runs the most recently updated corporate request for the customer. */
  runLatest(customerId: string): Observable<ApiResponse<RunSanctionsScreeningResultDto>> {
    return this.http.post<ApiResponse<RunSanctionsScreeningResultDto>>(
      `${BASE}/api/corporate-screening/${customerId}/run`,
      {}
    );
  }

  runForRequest(
    customerId: string,
    requestId: string
  ): Observable<ApiResponse<RunSanctionsScreeningResultDto>> {
    return this.http.post<ApiResponse<RunSanctionsScreeningResultDto>>(
      `${BASE}/api/corporate-screening/${customerId}/requests/${requestId}/run`,
      {}
    );
  }

  getResults(
    customerId: string,
    corporateScreeningRequestId?: string | null
  ): Observable<ApiResponse<SanctionsScreeningResultItemDto[]>> {
    let params = new HttpParams();
    if (corporateScreeningRequestId) {
      params = params.set('corporateScreeningRequestId', corporateScreeningRequestId);
    }
    return this.http.get<ApiResponse<SanctionsScreeningResultItemDto[]>>(
      `${BASE}/api/corporate-screening/${customerId}/results`,
      { params }
    );
  }
}
