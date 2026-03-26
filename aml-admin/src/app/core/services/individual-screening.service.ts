import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  ApiResponse,
  IndividualScreeningRequestDto,
  RunSanctionsScreeningResultDto,
  SanctionsScreeningResultItemDto,
  UpsertIndividualScreeningRequest
} from '../../shared/models/api.model';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class IndividualScreeningService {
  constructor(private http: HttpClient) {}

  getLatest(customerId: string): Observable<ApiResponse<IndividualScreeningRequestDto>> {
    return this.http.get<ApiResponse<IndividualScreeningRequestDto>>(`${BASE}/api/individual-screening/${customerId}`);
  }

  upsert(customerId: string, body: UpsertIndividualScreeningRequest): Observable<ApiResponse<IndividualScreeningRequestDto>> {
    return this.http.post<ApiResponse<IndividualScreeningRequestDto>>(`${BASE}/api/individual-screening/${customerId}`, body);
  }

  run(customerId: string): Observable<ApiResponse<RunSanctionsScreeningResultDto>> {
    return this.http.post<ApiResponse<RunSanctionsScreeningResultDto>>(`${BASE}/api/individual-screening/${customerId}/run`, {});
  }

  getResults(customerId: string): Observable<ApiResponse<SanctionsScreeningResultItemDto[]>> {
    return this.http.get<ApiResponse<SanctionsScreeningResultItemDto[]>>(`${BASE}/api/individual-screening/${customerId}/results`);
  }
}

