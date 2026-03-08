import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  ApiResponse,
  PagedRequest,
  PagedResult,
  UserDto,
  CreateUserRequest,
  UpdateUserRequest,
  AssignRolesRequest,
  RoleDto,
  CreateRoleRequest,
  UpdateRoleRequest,
  AssignPermissionsRequest,
  PermissionDto,
  CreatePermissionRequest,
  UpdatePermissionRequest
} from '../../shared/models/api.model';

const BASE = `${environment.apiUrl}/api`;

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  // Users
  getUsers(params: PagedRequest): Observable<ApiResponse<PagedResult<UserDto>>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.sortDescending != null) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    return this.http.get<ApiResponse<PagedResult<UserDto>>>(`${BASE}/users`, { params: httpParams });
  }

  getUser(id: string): Observable<ApiResponse<UserDto>> {
    return this.http.get<ApiResponse<UserDto>>(`${BASE}/users/${id}`);
  }

  createUser(body: CreateUserRequest): Observable<ApiResponse<UserDto>> {
    return this.http.post<ApiResponse<UserDto>>(`${BASE}/users`, body);
  }

  updateUser(id: string, body: UpdateUserRequest): Observable<ApiResponse<UserDto>> {
    return this.http.put<ApiResponse<UserDto>>(`${BASE}/users/${id}`, body);
  }

  deleteUser(id: string): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${BASE}/users/${id}`);
  }

  assignUserRoles(userId: string, body: AssignRolesRequest): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${BASE}/users/${userId}/roles`, body);
  }

  // Roles
  getRoles(params: PagedRequest): Observable<ApiResponse<PagedResult<RoleDto>>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.sortDescending != null) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    return this.http.get<ApiResponse<PagedResult<RoleDto>>>(`${BASE}/roles`, { params: httpParams });
  }

  getRole(id: string): Observable<ApiResponse<RoleDto>> {
    return this.http.get<ApiResponse<RoleDto>>(`${BASE}/roles/${id}`);
  }

  createRole(body: CreateRoleRequest): Observable<ApiResponse<RoleDto>> {
    return this.http.post<ApiResponse<RoleDto>>(`${BASE}/roles`, body);
  }

  updateRole(id: string, body: UpdateRoleRequest): Observable<ApiResponse<RoleDto>> {
    return this.http.put<ApiResponse<RoleDto>>(`${BASE}/roles/${id}`, body);
  }

  deleteRole(id: string): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${BASE}/roles/${id}`);
  }

  assignRolePermissions(roleId: string, body: AssignPermissionsRequest): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${BASE}/roles/${roleId}/permissions`, body);
  }

  // Permissions
  getPermissions(): Observable<ApiResponse<PermissionDto[]>> {
    return this.http.get<ApiResponse<PermissionDto[]>>(`${BASE}/permissions`);
  }

  getPermission(id: string): Observable<ApiResponse<PermissionDto>> {
    return this.http.get<ApiResponse<PermissionDto>>(`${BASE}/permissions/${id}`);
  }

  createPermission(body: CreatePermissionRequest): Observable<ApiResponse<PermissionDto>> {
    return this.http.post<ApiResponse<PermissionDto>>(`${BASE}/permissions`, body);
  }

  updatePermission(id: string, body: UpdatePermissionRequest): Observable<ApiResponse<PermissionDto>> {
    return this.http.put<ApiResponse<PermissionDto>>(`${BASE}/permissions/${id}`, body);
  }

  deletePermission(id: string): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${BASE}/permissions/${id}`);
  }
}
