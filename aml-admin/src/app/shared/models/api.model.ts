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

export interface CustomerTypeDto {
  id: string;
  code: string;
  name: string;
}

export interface CustomerStatusDto {
  id: string;
  code: string;
  name: string;
}

export interface GenderDto {
  id: string;
  code: string;
  name: string;
}

export interface NationalityDto {
  id: string;
  code: string;
  name: string;
}

export interface CountryDto {
  id: string;
  code: string;
  name: string;
}

export interface DocumentTypeDto {
  id: string;
  code: string;
  name: string;
}

export interface OccupationDto {
  id: string;
  code: string;
  name: string;
}

export interface SourceOfFundsDto {
  id: string;
  code: string;
  name: string;
}

export interface CustomerDocumentDto {
  id: string;
  customerId: string;
  documentTypeId: string;
  documentTypeCode?: string;
  documentTypeName?: string;
  fileName: string;
  uploadedBy?: string;
  uploadedDate: string;
  expiryDate?: string;
}

export interface CustomerDto {
  id: string;
  tenantId: string;
  customerNumber?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  dateOfBirth?: string;
  genderId?: string;
  genderName?: string;
  nationalityId?: string;
  nationalityName?: string;
  passportNumber?: string;
  passportExpiryDate?: string;
  nationalId?: string;
  countryOfResidenceId?: string;
  countryOfResidenceName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  occupationId?: string;
  occupationName?: string;
  employerName?: string;
  sourceOfFundsId?: string;
  sourceOfFundsName?: string;
  annualIncome?: number;
  expectedMonthlyTransactionVolume?: number;
  expectedMonthlyTransactionValue?: number;
  customerTypeId?: string;
  customerTypeCode?: string;
  customerTypeName?: string;
  accountPurpose?: string;
  riskScore?: number;
  riskLevel?: string;
  statusId?: string;
  statusCode?: string;
  statusName?: string;
  isPep?: boolean;
  businessActivity?: string;
  nationalIdOrPassport?: string;
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
  firstName?: string;
  lastName?: string;
  fullName: string;
  dateOfBirth?: string;
  genderId?: string;
  nationalityId?: string;
  passportNumber?: string;
  passportExpiryDate?: string;
  nationalId?: string;
  countryOfResidenceId?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  occupationId?: string;
  employerName?: string;
  sourceOfFundsId?: string;
  annualIncome?: number;
  expectedMonthlyTransactionVolume?: number;
  expectedMonthlyTransactionValue?: number;
  customerTypeId: string;
  accountPurpose?: string;
}

export interface UpdateCustomerRequest {
  firstName?: string;
  lastName?: string;
  fullName: string;
  dateOfBirth?: string;
  genderId?: string;
  nationalityId?: string;
  passportNumber?: string;
  passportExpiryDate?: string;
  nationalId?: string;
  countryOfResidenceId?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  occupationId?: string;
  employerName?: string;
  sourceOfFundsId?: string;
  annualIncome?: number;
  expectedMonthlyTransactionVolume?: number;
  expectedMonthlyTransactionValue?: number;
  customerTypeId: string;
  accountPurpose?: string;
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
  matchType?: string;
  score?: number;
  screenedAt?: string;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  isActive?: boolean;
  isDeleted?: boolean;
}

export interface SanctionsScreeningResultItemDto {
  id: string;
  customerId: string;
  matchedName?: string;
  sanctionList?: string;
  matchScore?: number;
  matchType?: string;
  screeningDate: string;
  status: string;
}

export interface RunSanctionsScreeningResultDto {
  results: SanctionsScreeningResultItemDto[];
  hasConfirmedMatch: boolean;
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

export interface SanctionListSourceDto {
  id: string;
  name: string;
  fileFormat: string;
}

export interface SanctionListUploadResultDto {
  importedCount: number;
  replacedCount: number;
  errors: string[];
}

export interface SanctionListEntryDto {
  id: string;
  listSource: string;
  fullName: string;
  nationality?: string | null;
  dateOfBirth?: string | null;
  referenceNumber?: string | null;
  entryType?: string | null;
  dataId?: string | null;
  versionNum?: string | null;
  firstName?: string | null;
  secondName?: string | null;
  unListType?: string | null;
  listType?: string | null;
  listedOn?: string | null;
  lastDayUpdated?: string | null;
  gender?: string | null;
  designation?: string | null;
  comments?: string | null;
  aliases?: string | null;
  addressCity?: string | null;
  addressCountry?: string | null;
  addressNote?: string | null;
  placeOfBirthCountry?: string | null;
  sortKey?: string | null;
  fullNameArabic?: string | null;
  familyNameArabic?: string | null;
  familyNameLatin?: string | null;
  documentNumber?: string | null;
  issuingAuthority?: string | null;
  issueDate?: string | null;
  endDate?: string | null;
  otherInformation?: string | null;
  typeDetail?: string | null;
}

export interface CreateSanctionListEntryRequest {
  listSource: string;
  fullName: string;
  nationality?: string | null;
  dateOfBirth?: string | null;
  referenceNumber?: string | null;
  entryType?: string | null;
  firstName?: string | null;
  secondName?: string | null;
  gender?: string | null;
  designation?: string | null;
  comments?: string | null;
  aliases?: string | null;
  addressCity?: string | null;
  addressCountry?: string | null;
  addressNote?: string | null;
  placeOfBirthCountry?: string | null;
  fullNameArabic?: string | null;
  familyNameArabic?: string | null;
  familyNameLatin?: string | null;
  documentNumber?: string | null;
  issuingAuthority?: string | null;
  issueDate?: string | null;
  endDate?: string | null;
  otherInformation?: string | null;
  typeDetail?: string | null;
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
