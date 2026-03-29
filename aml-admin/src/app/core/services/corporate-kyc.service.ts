import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  ApiResponse,
  CorporateKycDocumentDto,
  CorporateKycDto,
  UploadCorporateKycDocumentRequest,
  UpsertCorporateKycRequest
} from '../../shared/models/api.model';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class CorporateKycService {
  constructor(private http: HttpClient) {}

  getActiveCorporateKyc(customerId: string): Observable<ApiResponse<CorporateKycDto>> {
    return this.http.get<ApiResponse<CorporateKycDto>>(`${BASE}/api/corporate-kyc/${customerId}`);
  }

  createCorporateKyc(customerId: string, body: UpsertCorporateKycRequest): Observable<ApiResponse<CorporateKycDto>> {
    return this.http.post<ApiResponse<CorporateKycDto>>(`${BASE}/api/corporate-kyc/${customerId}`, body);
  }

  getCorporateKycDocuments(customerId: string): Observable<ApiResponse<CorporateKycDocumentDto[]>> {
    return this.http.get<ApiResponse<CorporateKycDocumentDto[]>>(`${BASE}/api/corporate-kyc/${customerId}/documents`);
  }

  uploadCorporateKycDocument(
    customerId: string,
    body: UploadCorporateKycDocumentRequest,
    file: File
  ): Observable<ApiResponse<CorporateKycDocumentDto>> {
    const form = new FormData();
    form.append('file', file);
    if (body.documentNo) form.append('documentNo', body.documentNo);
    if (body.issuedDate) form.append('issuedDate', body.issuedDate);
    if (body.expiryDate) form.append('expiryDate', body.expiryDate);
    if (body.approvedBy) form.append('approvedBy', body.approvedBy);
    if (body.folderPath) form.append('folderPath', body.folderPath);
    return this.http.post<ApiResponse<CorporateKycDocumentDto>>(
      `${BASE}/api/corporate-kyc/${customerId}/documents`,
      form
    );
  }

  downloadCorporateKycDocument(customerId: string, documentId: string): Observable<Blob> {
    return this.http.get(`${BASE}/api/corporate-kyc/${customerId}/documents/${documentId}/download`, {
      responseType: 'blob'
    });
  }

  deleteCorporateKycDocument(customerId: string, documentId: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/corporate-kyc/${customerId}/documents/${documentId}`);
  }
}
