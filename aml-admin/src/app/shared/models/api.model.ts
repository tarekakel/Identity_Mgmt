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

export interface IndividualKycDto {
  id: string;
  tenantId: string;
  customerId: string;

  isActive: boolean;
  isDeleted: boolean;

  // 1) Applicant personal details
  applicantName: string;
  applicantAliases?: string | null;
  applicantMobileNo?: string | null;
  applicantNationalityId?: string | null;
  applicantDualNationality?: boolean | null;
  applicantGenderId?: string | null;
  applicantDateOfBirth?: string | null;
  applicantResidenceStatus?: string | null;
  applicantEmirate?: string | null;
  applicantCountryOfBirth?: string | null;
  applicantCity?: string | null;
  applicantEmail?: string | null;
  applicantResidentialAddress?: string | null;
  applicantOfficeNoBuildingNameStreetArea?: string | null;
  applicantPOBox?: string | null;
  applicantCustomerRelationship?: string | null;
  applicantPreferredChannel?: string | null;
  applicantProductType?: string | null;
  applicantIndustryType?: string | null;
  applicantOccupationId?: string | null;

  applicantSourceOfFundsId?: string | null;
  applicantSourceOfFundsComments?: string | null;
  applicantIsProofOfSourceFundsObtained?: boolean | null;
  applicantSourceOfFundsProofComments?: string | null;

  applicantSourceOfWealth?: string | null;
  applicantSourceOfWealthComments?: string | null;
  applicantIsProofOfSourceWealthObtained?: boolean | null;
  applicantSourceOfWealthProofComments?: string | null;

  // 2) Client identification document details
  clientIdTypeCode?: string | null;
  clientIdNumber?: string | null;
  clientEmiratesIdNumber?: string | null;
  clientIdExpiryDate?: string | null;
  clientPassportNumber?: string | null;
  clientPassportDateOfIssue?: string | null;
  clientCountryIssuanceId?: string | null;

  // 3) Sponsor details
  sponsorName?: string | null;
  sponsorAliases?: string | null;
  sponsorIdTypeCode?: string | null;
  sponsorIdNumber?: string | null;
  sponsorDateOfBirth?: string | null;
  sponsorNationalityId?: string | null;
  sponsorGenderId?: string | null;
  sponsorDualNationality?: boolean | null;
  sponsorOtherDetails?: string | null;

  // 4) Bank details
  bankCountryId?: string | null;
  bankIbanAccountNo?: string | null;
  bankName?: string | null;
  accountName?: string | null;
  bankSwiftCode?: string | null;
  bankAddress?: string | null;
  bankCurrency?: string | null;

  // 5) Employer details
  employerCompanyName?: string | null;
  employerCompanyWebsite?: string | null;
  employerEmailAddress?: string | null;
  employerTelNo?: string | null;
  employerAddress?: string | null;
  employerIndustryAndBusinessDetails?: string | null;

  // 6) Politically exposed person status
  pepFATFIncreasedMonitoringAnswer?: string | null;
  pepSanctionListOrInverseMediaAnswer?: string | null;
  pepProminentPublicFunctionsAnswer?: string | null;
  pepAnyPEPsAfterScreeningAnswer?: string | null;
  pepSpecificPEPsAfterScreeningDetails?: string | null;

  // 7) Follow-up document details
  followUpDate?: string | null;
  followUpRemarks?: string | null;
}

export interface UpsertIndividualKycRequest {
  tenantId: string;

  // 1) Applicant personal details
  applicantName: string;
  applicantAliases?: string | null;
  applicantMobileNo?: string | null;
  applicantNationalityId?: string | null;
  applicantDualNationality?: boolean | null;
  applicantGenderId?: string | null;
  applicantDateOfBirth?: string | null;
  applicantResidenceStatus?: string | null;
  applicantEmirate?: string | null;
  applicantCountryOfBirth?: string | null;
  applicantCity?: string | null;
  applicantEmail?: string | null;
  applicantResidentialAddress?: string | null;
  applicantOfficeNoBuildingNameStreetArea?: string | null;
  applicantPOBox?: string | null;
  applicantCustomerRelationship?: string | null;
  applicantPreferredChannel?: string | null;
  applicantProductType?: string | null;
  applicantIndustryType?: string | null;
  applicantOccupationId?: string | null;

  applicantSourceOfFundsId?: string | null;
  applicantSourceOfFundsComments?: string | null;
  applicantIsProofOfSourceFundsObtained?: boolean | null;
  applicantSourceOfFundsProofComments?: string | null;

  applicantSourceOfWealth?: string | null;
  applicantSourceOfWealthComments?: string | null;
  applicantIsProofOfSourceWealthObtained?: boolean | null;
  applicantSourceOfWealthProofComments?: string | null;

  // 2) Client identification document details
  clientIdTypeCode?: string | null;
  clientIdNumber?: string | null;
  clientEmiratesIdNumber?: string | null;
  clientIdExpiryDate?: string | null;
  clientPassportNumber?: string | null;
  clientPassportDateOfIssue?: string | null;
  clientCountryIssuanceId?: string | null;

  // 3) Sponsor details
  sponsorName?: string | null;
  sponsorAliases?: string | null;
  sponsorIdTypeCode?: string | null;
  sponsorIdNumber?: string | null;
  sponsorDateOfBirth?: string | null;
  sponsorNationalityId?: string | null;
  sponsorGenderId?: string | null;
  sponsorDualNationality?: boolean | null;
  sponsorOtherDetails?: string | null;

  // 4) Bank details
  bankCountryId?: string | null;
  bankIbanAccountNo?: string | null;
  bankName?: string | null;
  accountName?: string | null;
  bankSwiftCode?: string | null;
  bankAddress?: string | null;
  bankCurrency?: string | null;

  // 5) Employer details
  employerCompanyName?: string | null;
  employerCompanyWebsite?: string | null;
  employerEmailAddress?: string | null;
  employerTelNo?: string | null;
  employerAddress?: string | null;
  employerIndustryAndBusinessDetails?: string | null;

  // 6) Politically exposed person status
  pepFATFIncreasedMonitoringAnswer?: string | null;
  pepSanctionListOrInverseMediaAnswer?: string | null;
  pepProminentPublicFunctionsAnswer?: string | null;
  pepAnyPEPsAfterScreeningAnswer?: string | null;
  pepSpecificPEPsAfterScreeningDetails?: string | null;

  // 7) Follow-up document details
  followUpDate?: string | null;
  followUpRemarks?: string | null;
}

export interface IndividualKycDocumentDto {
  id: string;
  individualKycId: string;
  customerId: string;

  documentNo?: string | null;
  issuedDate?: string | null;
  expiryDate?: string | null;
  approvedBy?: string | null;
  folderPath?: string | null;

  fileName: string;
  uploadedDate: string;
  uploadedBy?: string | null;
}

export interface UploadIndividualKycDocumentRequest {
  documentNo?: string | null;
  issuedDate?: string | null;
  expiryDate?: string | null;
  approvedBy?: string | null;
  folderPath?: string | null;
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
  corporateScreeningRequestId?: string | null;
  matchedName?: string;
  sanctionList?: string;
  matchScore?: number;
  matchType?: string;
  screeningDate: string;
  status: string;
  reviewStatus?: string | null;
  reviewedAt?: string | null;
  reviewedBy?: string | null;
}

export interface RecordSanctionScreeningActionRequest {
  action: string;
  notes?: string;
}

export interface SanctionActionAuditLogDto {
  id: string;
  sanctionsScreeningId: string;
  customerId: string;
  customerName?: string | null;
  matchedName?: string | null;
  sanctionList?: string | null;
  action: string;
  notes?: string | null;
  createdAt: string;
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
}

export interface RunSanctionsScreeningResultDto {
  results: SanctionsScreeningResultItemDto[];
  hasConfirmedMatch: boolean;
}

export interface IndividualScreeningRequestDto {
  id: string;
  tenantId: string;
  customerId: string;
  referenceId?: string | null;
  fullName: string;
  dateOfBirth?: string | null;
  nationalityId?: string | null;
  placeOfBirthCountryId?: string | null;
  idType?: string | null;
  idNumber?: string | null;
  address?: string | null;
  genderId?: string | null;
  matchThreshold: number;
  birthYearRange?: number | null;
  checkPepUkOnly: boolean;
  checkSanctions: boolean;
  checkProfileOfInterest: boolean;
  checkDisqualifiedDirectorUkOnly: boolean;
  checkReputationalRiskExposure: boolean;
  checkRegulatoryEnforcementList: boolean;
  checkInsolvencyUkIreland: boolean;
}

export interface UpsertIndividualScreeningRequest {
  tenantId: string;
  referenceId?: string | null;
  fullName: string;
  dateOfBirth?: string | null;
  nationalityId?: string | null;
  placeOfBirthCountryId?: string | null;
  idType?: string | null;
  idNumber?: string | null;
  address?: string | null;
  genderId?: string | null;
  matchThreshold: number;
  birthYearRange?: number | null;
  checkPepUkOnly: boolean;
  checkSanctions: boolean;
  checkProfileOfInterest: boolean;
  checkDisqualifiedDirectorUkOnly: boolean;
  checkReputationalRiskExposure: boolean;
  checkRegulatoryEnforcementList: boolean;
  checkInsolvencyUkIreland: boolean;
}
export interface CorporateScreeningCompanyDocumentDto {
  id: string;
  documentNo?: string | null;
  issuedDate?: string | null;
  expiryDate?: string | null;
  details?: string | null;
  remarks?: string | null;
}

export interface CorporateScreeningShareholderDocumentDto {
  id: string;
  documentNo?: string | null;
  issuedDate?: string | null;
  expiryDate?: string | null;
  details?: string | null;
  remarks?: string | null;
}

export interface CorporateScreeningShareholderDto {
  id: string;
  fullName: string;
  nationalityId?: string | null;
  dateOfBirth?: string | null;
  sharePercent: number;
  documents: CorporateScreeningShareholderDocumentDto[];
}

export interface CorporateScreeningRequestDto {
  id: string;
  tenantId: string;
  customerId: string;
  companyCode?: string | null;
  fullName: string;
  countryId?: string | null;
  dateOfRegistration?: string | null;
  tradeLicenceNo?: string | null;
  address?: string | null;
  matchThreshold: number;
  checkPepUkOnly: boolean;
  checkSanctions: boolean;
  checkProfileOfInterest: boolean;
  checkDisqualifiedDirectorUkOnly: boolean;
  checkReputationalRiskExposure: boolean;
  checkRegulatoryEnforcementList: boolean;
  checkInsolvencyUkIreland: boolean;
  companyDocuments: CorporateScreeningCompanyDocumentDto[];
  shareholders: CorporateScreeningShareholderDto[];
  createdAt?: string | null;
  updatedAt?: string | null;
}

export interface UpsertCorporateScreeningCompanyDocumentDto {
  documentNo?: string | null;
  issuedDate?: string | null;
  expiryDate?: string | null;
  details?: string | null;
  remarks?: string | null;
}

export interface UpsertCorporateScreeningShareholderDocumentDto {
  documentNo?: string | null;
  issuedDate?: string | null;
  expiryDate?: string | null;
  details?: string | null;
  remarks?: string | null;
}

export interface UpsertCorporateScreeningShareholderDto {
  fullName: string;
  nationalityId?: string | null;
  dateOfBirth?: string | null;
  sharePercent: number;
  documents: UpsertCorporateScreeningShareholderDocumentDto[];
}

export interface UpsertCorporateScreeningRequest {
  id?: string | null;
  tenantId: string;
  companyCode?: string | null;
  fullName: string;
  countryId?: string | null;
  dateOfRegistration?: string | null;
  tradeLicenceNo?: string | null;
  address?: string | null;
  matchThreshold: number;
  checkPepUkOnly: boolean;
  checkSanctions: boolean;
  checkProfileOfInterest: boolean;
  checkDisqualifiedDirectorUkOnly: boolean;
  checkReputationalRiskExposure: boolean;
  checkRegulatoryEnforcementList: boolean;
  checkInsolvencyUkIreland: boolean;
  companyDocuments: UpsertCorporateScreeningCompanyDocumentDto[];
  shareholders: UpsertCorporateScreeningShareholderDto[];
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

/** Individual bulk upload – report row (matches upload dialog columns). */
export interface IndividualBulkUploadReportRow {
  customerId: string;
  fullName: string;
  country: string;
  dob: string;
  placeOfBirth: string;
  error?: string;
}

export type IndividualBulkUploadReportMode = 'validationFailed' | 'queued';

export interface IndividualBulkUploadResultDto {
  mode: string;
  batchId: string;
  rows: IndividualBulkUploadReportRow[];
}

export interface IndividualBulkUploadBatchListItemDto {
  id: string;
  fileName: string;
  uploadedOn: string;
  uploadedBy: string;
  screeningFinished: boolean;
}

export interface IndividualBulkUploadLineDetailDto {
  index: number;
  customerId: string;
  customerName: string;
  nationality: string;
  dateOfBirth: string;
  status: string;
  source: string;
  uid: string;
  date: string;
}
