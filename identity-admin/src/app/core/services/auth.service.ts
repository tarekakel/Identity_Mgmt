import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse } from '../../shared/models/api.model';
import type { LoginRequest, LoginResponse } from '../../shared/models/api.model';

const TOKEN_KEY = 'identity_admin_access_token';
const REFRESH_KEY = 'identity_admin_refresh_token';
const EXPIRES_KEY = 'identity_admin_expires_at';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/api/auth`;
  private accessToken = signal<string | null>(this.getStoredToken());
  private refreshToken = signal<string | null>(this.getStoredRefreshToken());

  isAuthenticated = computed(() => !!this.accessToken());
  currentToken = computed(() => this.accessToken());

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    const body = {
      ...credentials,
      tenantCode: credentials.tenantCode ?? environment.defaultTenantCode
    };
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, body).pipe(
      tap((res) => {
        if (res.success && res.data) {
          this.setTokens(res.data);
        }
      })
    );
  }

  refresh(): Observable<ApiResponse<LoginResponse> | null> {
    const refresh = this.refreshToken();
    if (!refresh) return of(null);
    return this.http
      .post<ApiResponse<LoginResponse>>(`${this.apiUrl}/refresh`, { refreshToken: refresh })
      .pipe(
        tap((res) => {
          if (res.success && res.data) this.setTokens(res.data);
        }),
        catchError(() => {
          this.logout();
          return of(null);
        })
      );
  }

  logout(): void {
    const refresh = this.refreshToken();
    if (refresh) {
      this.http
        .post(`${this.apiUrl}/revoke`, { refreshToken: refresh }, { headers: this.authHeaders() })
        .subscribe({ error: () => {} });
    }
    this.clearTokens();
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  authHeaders(): { [key: string]: string } {
    const token = this.accessToken();
    const headers: { [key: string]: string } = {
      'Content-Type': 'application/json',
      'X-Tenant-Code': environment.defaultTenantCode
    };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return headers;
  }

  private setTokens(data: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, data.accessToken);
    localStorage.setItem(REFRESH_KEY, data.refreshToken);
    localStorage.setItem(EXPIRES_KEY, data.expiresAt);
    this.accessToken.set(data.accessToken);
    this.refreshToken.set(data.refreshToken);
  }

  private clearTokens(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(EXPIRES_KEY);
    this.accessToken.set(null);
    this.refreshToken.set(null);
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private getStoredRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }
}
