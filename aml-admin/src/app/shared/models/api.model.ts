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

export interface CustomerDto {
  id: string;
  tenantId: string;
  fullName?: string;
  nationalIdOrPassport?: string;
  dateOfBirth?: string;
  nationality?: string;
  address?: string;
  occupation?: string;
  sourceOfFunds?: string;
  isPep?: boolean;
  businessActivity?: string;
  riskClassification?: string;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  isActive?: boolean;
  isDeleted?: boolean;
}

export interface CreateCustomerRequest {
  tenantId: string;
  fullName: string;
  nationalIdOrPassport?: string;
  dateOfBirth?: string;
  nationality?: string;
  address?: string;
  occupation?: string;
  sourceOfFunds?: string;
  isPep: boolean;
  businessActivity?: string;
  riskClassification: string;
}

export interface UpdateCustomerRequest {
  fullName: string;
  nationalIdOrPassport?: string;
  dateOfBirth?: string;
  nationality?: string;
  address?: string;
  occupation?: string;
  sourceOfFunds?: string;
  isPep: boolean;
  businessActivity?: string;
  riskClassification: string;
  isActive: boolean;
}

export interface CaseDto {
  id: string;
  customerId?: string;
  alertId?: string;
  caseNumber?: string;
  status?: string;
  assignedToId?: string;
  createdByRole?: string;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  isActive?: boolean;
  isDeleted?: boolean;
}

export interface CreateCaseRequest {
  customerId?: string;
  alertId?: string;
  status: string;
  assignedToId?: string;
  createdByRole?: string;
}

export interface UpdateCaseRequest {
  customerId?: string;
  alertId?: string;
  status: string;
  assignedToId?: string;
  createdByRole?: string;
  isActive: boolean;
}

export interface RiskAssignmentDto {
  id: string;
  customerId: string;
  countryRisk: number;
  customerTypeRisk: number;
  pepRisk: number;
  transactionRisk: number;
  industryRisk: number;
  totalScore: number;
  riskLevel?: string;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  isActive?: boolean;
  isDeleted?: boolean;
}

export interface CreateRiskAssignmentRequest {
  customerId: string;
  countryRisk: number;
  customerTypeRisk: number;
  pepRisk: number;
  transactionRisk: number;
  industryRisk: number;
  totalScore: number;
  riskLevel: string;
}

export interface UpdateRiskAssignmentRequest {
  countryRisk: number;
  customerTypeRisk: number;
  pepRisk: number;
  transactionRisk: number;
  industryRisk: number;
  totalScore: number;
  riskLevel: string;
  isActive: boolean;
}

export interface SanctionsScreeningDto {
  id: string;
  customerId: string;
  screeningList?: string;
  result?: string;
  matchedName?: string;
  score?: number;
  screenedAt?: string;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  isActive?: boolean;
  isDeleted?: boolean;
}

export interface CreateSanctionsScreeningRequest {
  customerId: string;
  screeningList: string;
  result: string;
  matchedName?: string;
  score?: number;
  screenedAt: string;
}

export interface UpdateSanctionsScreeningRequest {
  screeningList: string;
  result: string;
  matchedName?: string;
  score?: number;
  screenedAt: string;
  isActive: boolean;
}

export interface AuditLogDto {
  id: string;
  userId?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
  timestamp?: string;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  isActive?: boolean;
  isDeleted?: boolean;
}
