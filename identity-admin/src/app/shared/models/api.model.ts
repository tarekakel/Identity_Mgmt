export interface ApiResponse<T = unknown> {
  success: boolean;
  message?: string;
  errors?: string[];
  data?: T;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface PagedRequest {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
  searchTerm?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  tenantCode?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType?: string;
}

export interface UserDto {
  id: string;
  tenantId: string;
  userName: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  roleNames: string[];
}

export interface CreateUserRequest {
  userName: string;
  email: string;
  password: string;
  isActive: boolean;
  roleIds?: string[];
}

export interface UpdateUserRequest {
  userName: string;
  email: string;
  isActive: boolean;
}

export interface AssignRolesRequest {
  roleIds: string[];
}

export interface RoleDto {
  id: string;
  tenantId: string;
  name: string;
  description?: string;
  createdAt: string;
  permissionCodes: string[];
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  permissionIds?: string[];
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
}

export interface AssignPermissionsRequest {
  permissionIds: string[];
}

export interface PermissionDto {
  id: string;
  name: string;
  code: string;
  description?: string;
}

export interface CreatePermissionRequest {
  name: string;
  code: string;
  description?: string;
}

export interface UpdatePermissionRequest {
  name: string;
  code: string;
  description?: string;
}

export interface TenantDto {
  id: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAt: string;
}
