import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  ApiResponse,
  IndividualKycDocumentDto,
  IndividualKycDto,
  UpsertIndividualKycRequest,
  UploadIndividualKycDocumentRequest
} from '../../shared/models/api.model';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class IndividualKycService {
  constructor(private http: HttpClient) {}

  getActiveIndividualKyc(customerId: string): Observable<ApiResponse<IndividualKycDto>> {
    return this.http.get<ApiResponse<IndividualKycDto>>(`${BASE}/api/individual-kyc/${customerId}`);
  }

  createIndividualKyc(customerId: string, body: UpsertIndividualKycRequest): Observable<ApiResponse<IndividualKycDto>> {
    return this.http.post<ApiResponse<IndividualKycDto>>(`${BASE}/api/individual-kyc/${customerId}`, body);
  }

  getIndividualKycDocuments(customerId: string): Observable<ApiResponse<IndividualKycDocumentDto[]>> {
    return this.http.get<ApiResponse<IndividualKycDocumentDto[]>>(`${BASE}/api/individual-kyc/${customerId}/documents`);
  }

  uploadIndividualKycDocument(
    customerId: string,
    body: UploadIndividualKycDocumentRequest,
    file: File
  ): Observable<ApiResponse<IndividualKycDocumentDto>> {
    const form = new FormData();
    form.append('file', file);

    if (body.documentNo) form.append('documentNo', body.documentNo);
    if (body.issuedDate) form.append('issuedDate', body.issuedDate);
    if (body.expiryDate) form.append('expiryDate', body.expiryDate);
    if (body.approvedBy) form.append('approvedBy', body.approvedBy);
    if (body.folderPath) form.append('folderPath', body.folderPath);

    return this.http.post<ApiResponse<IndividualKycDocumentDto>>(
      `${BASE}/api/individual-kyc/${customerId}/documents`,
      form
    );
  }

  downloadIndividualKycDocument(customerId: string, documentId: string): Observable<Blob> {
    return this.http.get(
      `${BASE}/api/individual-kyc/${customerId}/documents/${documentId}/download`,
      { responseType: 'blob' }
    );
  }

  deleteIndividualKycDocument(customerId: string, documentId: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${BASE}/api/individual-kyc/${customerId}/documents/${documentId}`);
  }
}

