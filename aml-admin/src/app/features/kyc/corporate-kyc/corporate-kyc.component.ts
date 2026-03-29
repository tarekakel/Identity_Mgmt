import { CommonModule } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { TranslateModule, TranslateService } from "@ngx-translate/core";
import { firstValueFrom } from "rxjs";
import { ApiService } from "../../../core/services/api.service";
import { CorporateKycService } from "../../../core/services/corporate-kyc.service";
import { NotificationService } from "../../../core/services/notification.service";
import type { CustomerDto, UploadCorporateKycDocumentRequest, UpsertCorporateKycRequest } from "../../../shared/models/api.model";

const DEFAULT_TENANT_ID = "00000000-0000-0000-0000-000000000001";

type GroupEntityRow = { entityName: string; country: string };
type OwnershipRow = { typeSelect: string; line2: string; partnerName: string; sharePct: string; mainNationality: string; dobDoinc: string; eidTrade: string; eidExp: string; isPep: boolean; row2Type: string; row2Fields: string; dualNationality: string; passportNo: string; passportExp: string };
type PersonRow = { name: string; designation: string; nationality: string; dualNationality: string; dob: string; eidNo: string; eidExpiry: string; passportNo: string; passportExpiry: string };
type DocumentRow = { existingDocId?: string; documentNo: string; issuedDate: string; expiryDate: string; approvedBy: string; folderPath: string; selectedFile: File | null; existingFileName?: string };
type PersonSectionBlock = { id: number; key: "ubo" | "senior" | "signatory"; titleKey: string };

@Component({
  selector: "app-corporate-kyc",
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: "./corporate-kyc.component.html",
  styleUrls: ["./corporate-kyc.component.scss"],
})
export class CorporateKycComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly kycApi = inject(CorporateKycService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly personSectionBlocks: PersonSectionBlock[] = [
    { id: 8, key: "ubo", titleKey: "section8" },
    { id: 9, key: "senior", titleKey: "section9" },
    { id: 10, key: "signatory", titleKey: "section10" },
  ];

  routeCustomerId = signal("");
  customerSearchTerm = signal("");
  customerOptions = signal<CustomerDto[]>([]);
  customerLoading = signal(false);
  customerDropdownOpen = signal(false);
  selectedCustomer = signal<CustomerDto | null>(null);

  searchId = signal("");
  searchCustomerName = signal("");
  searchLicenseNumber = signal("");
  checkSelectAll = signal(false);
  checkPep = signal(true);
  checkDisqualifiedDirector = signal(true);
  checkSanction = signal(true);
  checkProfileOfInterest = signal(true);
  checkReputational = signal(true);
  checkRegulatory = signal(true);
  checkInsolvency = signal(true);
  insolvencyThreshold = signal(85);
  expanded = signal<Record<number, boolean>>(Object.fromEntries(Array.from({ length: 13 }, (_, i) => [i + 1, true])) as Record<number, boolean>);

  companyName = signal("");
  formerNames = signal("");
  typeOfEntity = signal("");
  taxVat = signal("");
  customerRelationship = signal("New");
  typeOfRelationship = signal("Customer");
  purposeOfRelation = signal("");
  showCompanyNameError = signal(false);
  licenseNumber = signal("");
  licenseIssueDate = signal("");
  licenseExpiryDate = signal("");
  placeOfIssue = signal("");
  incorporatedIn = signal("");
  licenseType = signal("");
  licenseIssuingAuthority = signal("");
  dateOfIncorporation = signal("");
  registeredOffice = signal("");
  businessAddress = signal("");
  poBox = signal("");
  telNo = signal("");
  emailId = signal("");
  corporateWebsite = signal("");
  city = signal("");
  emirateState = signal("");
  country = signal("");
  countriesOperate = signal("");
  primaryContact = signal("");
  contactJobTitle = signal("");
  contactEmail = signal("");
  contactFax = signal("");
  contactAddress = signal("");
  natureOfBusiness = signal("");
  productType = signal("");
  modeOfPayment = signal("");
  deliveryChannel = signal("");
  otherDetails = signal("");
  industryType = signal("");
  proofSourceFund = signal(false);
  sourceOfFunds = signal("");
  sourceOfFundsComments = signal("");
  sourceOfWealth = signal("");
  proofSourceWealth = signal(false);
  taxResidency = signal("");
  companyRegulated = signal(false);
  sourceOfWealthComments = signal("");
  bankCountry = signal("");
  bankName = signal("");
  bankAddress = signal("");
  ibanAccount = signal("");
  accountName = signal("");
  currency = signal("");
  swiftCode = signal("");
  groupEntities = signal<GroupEntityRow[]>([{ entityName: "", country: "" }]);
  ownershipRows = signal<OwnershipRow[]>([this.emptyOwnershipRow()]);
  uboRows = signal<PersonRow[]>([this.emptyPersonRow()]);
  seniorRows = signal<PersonRow[]>([this.emptyPersonRow()]);
  signatoryRows = signal<PersonRow[]>([this.emptyPersonRow()]);
  documentRows = signal<DocumentRow[]>([this.emptyDocumentRow()]);
  fatfQ1 = signal("No");
  fatfQ2 = signal("");
  pepAfterScreening = signal("No");
  followUpDate = signal("");
  followUpRemarks = signal("");
  folderPath = signal("");
  approvedBy = signal("");
  validateSharePct = signal(true);
  ownershipTotal = signal("0.00");
  submitting = signal(false);
  submitError = signal<string | null>(null);
  submitSuccess = signal<string | null>(null);

  ngOnInit(): void {
    this.route.paramMap.subscribe(async (pm) => {
      const cid = pm.get("customerId") ?? "";
      this.routeCustomerId.set(cid);
      await this.loadSelectedCustomer(cid);
      await this.loadCorporateKyc(cid);
    });
  }

  private async loadSelectedCustomer(cid: string): Promise<void> {
    this.selectedCustomer.set(null);
    this.customerSearchTerm.set("");
    if (!cid) return;
    const res = await firstValueFrom(this.api.getCustomer(cid));
    if (res.success && res.data) {
      this.selectedCustomer.set(res.data);
      this.customerSearchTerm.set(res.data.fullName ?? "");
    }
  }

  async searchCustomers(): Promise<void> {
    this.customerDropdownOpen.set(true);
    this.customerLoading.set(true);
    try {
      const term = this.customerSearchTerm().trim();
      if (term.length < 2) {
        this.customerOptions.set([]);
        return;
      }
      const res = await firstValueFrom(this.api.getCustomers({ pageNumber: 1, pageSize: 10, searchTerm: term }));
      const items = res.success && res.data?.items ? res.data.items : [];
      this.customerOptions.set(items);
    } finally {
      this.customerLoading.set(false);
    }
  }

  selectCustomer(c: CustomerDto): void {
    this.customerDropdownOpen.set(false);
    this.router.navigate(["/kyc/corporate", c.id]);
  }

  search(): void {
    void this.searchCustomers();
  }

  toggleSection(n: number): void {
    this.expanded.update((e) => ({ ...e, [n]: !e[n] }));
  }
  isExpanded(n: number): boolean {
    return this.expanded()[n] !== false;
  }
  onToggleSelectAll(ev: Event): void {
    const checked = (ev.target as HTMLInputElement).checked;
    this.checkSelectAll.set(checked);
    this.checkPep.set(checked);
    this.checkDisqualifiedDirector.set(checked);
    this.checkSanction.set(checked);
    this.checkProfileOfInterest.set(checked);
    this.checkReputational.set(checked);
    this.checkRegulatory.set(checked);
    this.checkInsolvency.set(checked);
  }
  isAllRiskSelected(): boolean {
    return (
      this.checkPep() &&
      this.checkDisqualifiedDirector() &&
      this.checkSanction() &&
      this.checkProfileOfInterest() &&
      this.checkReputational() &&
      this.checkRegulatory() &&
      this.checkInsolvency()
    );
  }

  async submit(): Promise<void> {
    this.submitError.set(null);
    this.submitSuccess.set(null);
    const cid = this.routeCustomerId();
    if (!cid) {
      this.submitError.set(this.translate.instant("corporateKyc.customerRequired"));
      return;
    }
    if (!this.companyName().trim()) {
      this.showCompanyNameError.set(true);
      this.submitError.set(this.translate.instant("corporateKyc.companyNameRequired"));
      return;
    }
    this.submitting.set(true);
    try {
      const body: UpsertCorporateKycRequest = { tenantId: DEFAULT_TENANT_ID, formPayload: this.buildFormPayload() };
      const createRes = await firstValueFrom(this.kycApi.createCorporateKyc(cid, body));
      if (!createRes.success) throw new Error(createRes.message ?? "Corporate KYC save failed");
      for (const row of this.documentRows()) {
        if (!row.selectedFile) continue;
        if (row.existingDocId) {
          const delRes = await firstValueFrom(this.kycApi.deleteCorporateKycDocument(cid, row.existingDocId));
          if (!delRes.success) throw new Error(delRes.message ?? "Document replace delete failed");
        }
        const uploadReq: UploadCorporateKycDocumentRequest = {
          documentNo: row.documentNo.trim() || undefined,
          issuedDate: this.dateInputToIso(row.issuedDate) ?? undefined,
          expiryDate: this.dateInputToIso(row.expiryDate) ?? undefined,
          approvedBy: row.approvedBy.trim() || undefined,
          folderPath: row.folderPath.trim() || undefined,
        };
        const upRes = await firstValueFrom(this.kycApi.uploadCorporateKycDocument(cid, uploadReq, row.selectedFile));
        if (!upRes.success) throw new Error(upRes.message ?? "Document upload failed");
      }
      const okMsg = this.translate.instant("corporateKyc.savedSuccess");
      this.notification.success(okMsg);
      this.submitSuccess.set(okMsg);
      await this.loadCorporateKyc(cid);
    } catch (e) {
      const msg = (e as Error).message ?? "Save failed";
      this.submitError.set(msg);
      this.notification.error(msg);
    } finally {
      this.submitting.set(false);
    }
  }

  private buildFormPayload(): Record<string, unknown> {
    return {
      v: 1,
      searchId: this.searchId(),
      searchCustomerName: this.searchCustomerName(),
      searchLicenseNumber: this.searchLicenseNumber(),
      checkPep: this.checkPep(),
      checkDisqualifiedDirector: this.checkDisqualifiedDirector(),
      checkSanction: this.checkSanction(),
      checkProfileOfInterest: this.checkProfileOfInterest(),
      checkReputational: this.checkReputational(),
      checkRegulatory: this.checkRegulatory(),
      checkInsolvency: this.checkInsolvency(),
      insolvencyThreshold: this.insolvencyThreshold(),
      expanded: this.expanded(),
      companyName: this.companyName(),
      formerNames: this.formerNames(),
      typeOfEntity: this.typeOfEntity(),
      taxVat: this.taxVat(),
      customerRelationship: this.customerRelationship(),
      typeOfRelationship: this.typeOfRelationship(),
      purposeOfRelation: this.purposeOfRelation(),
      licenseNumber: this.licenseNumber(),
      licenseIssueDate: this.licenseIssueDate(),
      licenseExpiryDate: this.licenseExpiryDate(),
      placeOfIssue: this.placeOfIssue(),
      incorporatedIn: this.incorporatedIn(),
      licenseType: this.licenseType(),
      licenseIssuingAuthority: this.licenseIssuingAuthority(),
      dateOfIncorporation: this.dateOfIncorporation(),
      registeredOffice: this.registeredOffice(),
      businessAddress: this.businessAddress(),
      poBox: this.poBox(),
      telNo: this.telNo(),
      emailId: this.emailId(),
      corporateWebsite: this.corporateWebsite(),
      city: this.city(),
      emirateState: this.emirateState(),
      country: this.country(),
      countriesOperate: this.countriesOperate(),
      primaryContact: this.primaryContact(),
      contactJobTitle: this.contactJobTitle(),
      contactEmail: this.contactEmail(),
      contactFax: this.contactFax(),
      contactAddress: this.contactAddress(),
      natureOfBusiness: this.natureOfBusiness(),
      productType: this.productType(),
      modeOfPayment: this.modeOfPayment(),
      deliveryChannel: this.deliveryChannel(),
      otherDetails: this.otherDetails(),
      industryType: this.industryType(),
      proofSourceFund: this.proofSourceFund(),
      sourceOfFunds: this.sourceOfFunds(),
      sourceOfFundsComments: this.sourceOfFundsComments(),
      sourceOfWealth: this.sourceOfWealth(),
      proofSourceWealth: this.proofSourceWealth(),
      taxResidency: this.taxResidency(),
      companyRegulated: this.companyRegulated(),
      sourceOfWealthComments: this.sourceOfWealthComments(),
      bankCountry: this.bankCountry(),
      bankName: this.bankName(),
      bankAddress: this.bankAddress(),
      ibanAccount: this.ibanAccount(),
      accountName: this.accountName(),
      currency: this.currency(),
      swiftCode: this.swiftCode(),
      groupEntities: this.groupEntities(),
      ownershipRows: this.ownershipRows(),
      uboRows: this.uboRows(),
      seniorRows: this.seniorRows(),
      signatoryRows: this.signatoryRows(),
      fatfQ1: this.fatfQ1(),
      fatfQ2: this.fatfQ2(),
      pepAfterScreening: this.pepAfterScreening(),
      followUpDate: this.followUpDate(),
      followUpRemarks: this.followUpRemarks(),
      folderPath: this.folderPath(),
      approvedBy: this.approvedBy(),
      validateSharePct: this.validateSharePct(),
      ownershipTotal: this.ownershipTotal(),
    };
  }

  private applyFormPayload(p: Record<string, unknown> | null | undefined): void {
    if (!p || typeof p !== "object") return;
    const s = (k: string): string => (typeof p[k] === "string" ? (p[k] as string) : "");
    const b = (k: string): boolean => p[k] === true;
    const n = (k: string, d: number): number => {
      const v = p[k];
      if (typeof v === "number" && Number.isFinite(v)) return v;
      const x = Number(v);
      return Number.isFinite(x) ? x : d;
    };
    this.searchId.set(s("searchId"));
    this.searchCustomerName.set(s("searchCustomerName"));
    this.searchLicenseNumber.set(s("searchLicenseNumber"));
    this.checkPep.set(p["checkPep"] === false ? false : true);
    this.checkDisqualifiedDirector.set(p["checkDisqualifiedDirector"] === false ? false : true);
    this.checkSanction.set(p["checkSanction"] === false ? false : true);
    this.checkProfileOfInterest.set(p["checkProfileOfInterest"] === false ? false : true);
    this.checkReputational.set(p["checkReputational"] === false ? false : true);
    this.checkRegulatory.set(p["checkRegulatory"] === false ? false : true);
    this.checkInsolvency.set(p["checkInsolvency"] === false ? false : true);
    this.insolvencyThreshold.set(n("insolvencyThreshold", 85));
    const ex = p["expanded"];
    if (ex && typeof ex === "object" && !Array.isArray(ex)) this.expanded.set(ex as Record<number, boolean>);
    this.companyName.set(s("companyName"));
    this.formerNames.set(s("formerNames"));
    this.typeOfEntity.set(s("typeOfEntity"));
    this.taxVat.set(s("taxVat"));
    this.customerRelationship.set(s("customerRelationship") || "New");
    this.typeOfRelationship.set(s("typeOfRelationship") || "Customer");
    this.purposeOfRelation.set(s("purposeOfRelation"));
    this.licenseNumber.set(s("licenseNumber"));
    this.licenseIssueDate.set(this.toDateInputValue(s("licenseIssueDate")));
    this.licenseExpiryDate.set(this.toDateInputValue(s("licenseExpiryDate")));
    this.placeOfIssue.set(s("placeOfIssue"));
    this.incorporatedIn.set(s("incorporatedIn"));
    this.licenseType.set(s("licenseType"));
    this.licenseIssuingAuthority.set(s("licenseIssuingAuthority"));
    this.dateOfIncorporation.set(this.toDateInputValue(s("dateOfIncorporation")));
    this.registeredOffice.set(s("registeredOffice"));
    this.businessAddress.set(s("businessAddress"));
    this.poBox.set(s("poBox"));
    this.telNo.set(s("telNo"));
    this.emailId.set(s("emailId"));
    this.corporateWebsite.set(s("corporateWebsite"));
    this.city.set(s("city"));
    this.emirateState.set(s("emirateState"));
    this.country.set(s("country"));
    this.countriesOperate.set(s("countriesOperate"));
    this.primaryContact.set(s("primaryContact"));
    this.contactJobTitle.set(s("contactJobTitle"));
    this.contactEmail.set(s("contactEmail"));
    this.contactFax.set(s("contactFax"));
    this.contactAddress.set(s("contactAddress"));
    this.natureOfBusiness.set(s("natureOfBusiness"));
    this.productType.set(s("productType"));
    this.modeOfPayment.set(s("modeOfPayment"));
    this.deliveryChannel.set(s("deliveryChannel"));
    this.otherDetails.set(s("otherDetails"));
    this.industryType.set(s("industryType"));
    this.proofSourceFund.set(b("proofSourceFund"));
    this.sourceOfFunds.set(s("sourceOfFunds"));
    this.sourceOfFundsComments.set(s("sourceOfFundsComments"));
    this.sourceOfWealth.set(s("sourceOfWealth"));
    this.proofSourceWealth.set(b("proofSourceWealth"));
    this.taxResidency.set(s("taxResidency"));
    this.companyRegulated.set(b("companyRegulated"));
    this.sourceOfWealthComments.set(s("sourceOfWealthComments"));
    this.bankCountry.set(s("bankCountry"));
    this.bankName.set(s("bankName"));
    this.bankAddress.set(s("bankAddress"));
    this.ibanAccount.set(s("ibanAccount"));
    this.accountName.set(s("accountName"));
    this.currency.set(s("currency"));
    this.swiftCode.set(s("swiftCode"));
    const parseGe = p["groupEntities"];
    if (Array.isArray(parseGe) && parseGe.length > 0) {
      this.groupEntities.set(
        parseGe.map((row) => ({
          entityName: typeof row === "object" && row && "entityName" in row ? String((row as GroupEntityRow).entityName) : "",
          country: typeof row === "object" && row && "country" in row ? String((row as GroupEntityRow).country) : "",
        }))
      );
    } else this.groupEntities.set([{ entityName: "", country: "" }]);
    const parseOwn = p["ownershipRows"];
    if (Array.isArray(parseOwn) && parseOwn.length > 0) this.ownershipRows.set(parseOwn.map((row) => this.normalizeOwnershipRow(row)));
    else this.ownershipRows.set([this.emptyOwnershipRow()]);
    const mapPerson = (k: string): void => {
      const arr = p[k];
      if (Array.isArray(arr) && arr.length > 0) {
        const rows = arr.map((row) => this.normalizePersonRow(row));
        if (k === "uboRows") this.uboRows.set(rows);
        else if (k === "seniorRows") this.seniorRows.set(rows);
        else if (k === "signatoryRows") this.signatoryRows.set(rows);
      }
    };
    mapPerson("uboRows");
    mapPerson("seniorRows");
    mapPerson("signatoryRows");
    this.fatfQ1.set(s("fatfQ1") || "No");
    this.fatfQ2.set(s("fatfQ2"));
    this.pepAfterScreening.set(s("pepAfterScreening") || "No");
    this.followUpDate.set(this.toDateInputValue(s("followUpDate")));
    this.followUpRemarks.set(s("followUpRemarks"));
    this.folderPath.set(s("folderPath"));
    this.approvedBy.set(s("approvedBy"));
    this.validateSharePct.set(p["validateSharePct"] !== false);
    this.ownershipTotal.set(s("ownershipTotal") || "0.00");
  }

  private normalizeOwnershipRow(row: unknown): OwnershipRow {
    const r = row && typeof row === "object" ? (row as Partial<OwnershipRow>) : {};
    return {
      typeSelect: r.typeSelect ?? "",
      line2: r.line2 ?? "",
      partnerName: r.partnerName ?? "",
      sharePct: r.sharePct ?? "",
      mainNationality: r.mainNationality ?? "",
      dobDoinc: this.toDateInputValue(r.dobDoinc ?? ""),
      eidTrade: r.eidTrade ?? "",
      eidExp: this.toDateInputValue(r.eidExp ?? ""),
      isPep: !!r.isPep,
      row2Type: r.row2Type ?? "Main Corporate",
      row2Fields: r.row2Fields ?? "",
      dualNationality: r.dualNationality ?? "",
      passportNo: r.passportNo ?? "",
      passportExp: this.toDateInputValue(r.passportExp ?? ""),
    };
  }

  private normalizePersonRow(row: unknown): PersonRow {
    const r = row && typeof row === "object" ? (row as Partial<PersonRow>) : {};
    return {
      name: r.name ?? "",
      designation: r.designation ?? "",
      nationality: r.nationality ?? "",
      dualNationality: r.dualNationality ?? "",
      dob: this.toDateInputValue(r.dob ?? ""),
      eidNo: r.eidNo ?? "",
      eidExpiry: this.toDateInputValue(r.eidExpiry ?? ""),
      passportNo: r.passportNo ?? "",
      passportExpiry: this.toDateInputValue(r.passportExpiry ?? ""),
    };
  }

  private async loadCorporateKyc(cid: string): Promise<void> {
    if (!cid) {
      this.documentRows.set([this.emptyDocumentRow()]);
      return;
    }
    const kycRes = await firstValueFrom(this.kycApi.getActiveCorporateKyc(cid));
    if (kycRes.success && kycRes.data?.formPayload) this.applyFormPayload(kycRes.data.formPayload as Record<string, unknown>);
    else this.resetFormToDefaults();
    const docsRes = await firstValueFrom(this.kycApi.getCorporateKycDocuments(cid));
    const docs = docsRes.success && docsRes.data ? docsRes.data : [];
    this.documentRows.set(
      docs.length > 0
        ? docs.map((d) => ({
            existingDocId: d.id,
            documentNo: d.documentNo ?? "",
            issuedDate: this.toDateInputValue(d.issuedDate ?? ""),
            expiryDate: this.toDateInputValue(d.expiryDate ?? ""),
            approvedBy: d.approvedBy ?? "",
            folderPath: d.folderPath ?? "",
            selectedFile: null,
            existingFileName: d.fileName,
          }))
        : [this.emptyDocumentRow()]
    );
  }

  private resetFormToDefaults(): void {
    this.applyFormPayload({
      v: 1,
      checkPep: true,
      checkDisqualifiedDirector: true,
      checkSanction: true,
      checkProfileOfInterest: true,
      checkReputational: true,
      checkRegulatory: true,
      checkInsolvency: true,
      insolvencyThreshold: 85,
      customerRelationship: "New",
      typeOfRelationship: "Customer",
      fatfQ1: "No",
      pepAfterScreening: "No",
      validateSharePct: true,
      ownershipTotal: "0.00",
    });
    this.groupEntities.set([{ entityName: "", country: "" }]);
    this.ownershipRows.set([this.emptyOwnershipRow()]);
    this.uboRows.set([this.emptyPersonRow()]);
    this.seniorRows.set([this.emptyPersonRow()]);
    this.signatoryRows.set([this.emptyPersonRow()]);
  }

  private dateInputToIso(dateInput: string): string | null {
    const v = (dateInput ?? "").trim();
    if (!v) return null;
    return new Date(v).toISOString();
  }

  private toDateInputValue(value: string): string {
    if (!value) return "";
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return value;
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "";
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, "0");
    const dd = String(d.getDate()).padStart(2, "0");
    return `${yyyy}-${mm}-${dd}`;
  }

  addGroupEntity(): void {
    this.groupEntities.update((rows) => [...rows, { entityName: "", country: "" }]);
  }
  removeGroupEntity(i: number): void {
    this.groupEntities.update((rows) => rows.filter((_, idx) => idx !== i));
  }
  addOwnership(): void {
    this.ownershipRows.update((rows) => [...rows, this.emptyOwnershipRow()]);
  }
  removeOwnership(i: number): void {
    this.ownershipRows.update((rows) => rows.filter((_, idx) => idx !== i));
  }
  addUbo(): void {
    this.uboRows.update((rows) => [...rows, this.emptyPersonRow()]);
  }
  removeUbo(i: number): void {
    this.uboRows.update((rows) => rows.filter((_, idx) => idx !== i));
  }
  addSenior(): void {
    this.seniorRows.update((rows) => [...rows, this.emptyPersonRow()]);
  }
  removeSenior(i: number): void {
    this.seniorRows.update((rows) => rows.filter((_, idx) => idx !== i));
  }
  addSignatory(): void {
    this.signatoryRows.update((rows) => [...rows, this.emptyPersonRow()]);
  }
  removeSignatory(i: number): void {
    this.signatoryRows.update((rows) => rows.filter((_, idx) => idx !== i));
  }
  addDocument(): void {
    this.documentRows.update((rows) => [...rows, this.emptyDocumentRow()]);
  }
  removeDocument(i: number): void {
    this.documentRows.update((rows) => rows.filter((_, idx) => idx !== i));
  }

  onDocFile(ev: Event, rowIndex: number): void {
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.documentRows.update((rows) => {
      const next = [...rows];
      if (next[rowIndex]) next[rowIndex] = { ...next[rowIndex], selectedFile: file };
      return next;
    });
  }

  private emptyOwnershipRow(): OwnershipRow {
    return {
      typeSelect: "",
      line2: "",
      partnerName: "",
      sharePct: "",
      mainNationality: "",
      dobDoinc: "",
      eidTrade: "",
      eidExp: "",
      isPep: false,
      row2Type: "Main Corporate",
      row2Fields: "",
      dualNationality: "",
      passportNo: "",
      passportExp: "",
    };
  }
  private emptyPersonRow(): PersonRow {
    return { name: "", designation: "", nationality: "", dualNationality: "", dob: "", eidNo: "", eidExpiry: "", passportNo: "", passportExpiry: "" };
  }
  private emptyDocumentRow(): DocumentRow {
    return { documentNo: "", issuedDate: "", expiryDate: "", approvedBy: "", folderPath: "", selectedFile: null };
  }

  patchGroupEntity(i: number, key: keyof GroupEntityRow, value: string): void {
    this.groupEntities.update((rows) => rows.map((r, idx) => (idx === i ? { ...r, [key]: value } : r)));
  }
  patchOwnership(i: number, key: keyof OwnershipRow, value: string | boolean): void {
    this.ownershipRows.update((rows) => rows.map((r, idx) => (idx === i ? { ...r, [key]: value } : r)));
  }
  patchPerson(kind: "ubo" | "senior" | "signatory", i: number, key: keyof PersonRow, value: string): void {
    const upd = (rows: PersonRow[]) => rows.map((r, idx) => (idx === i ? { ...r, [key]: value } : r));
    if (kind === "ubo") this.uboRows.update(upd);
    else if (kind === "senior") this.seniorRows.update(upd);
    else this.signatoryRows.update(upd);
  }
  patchDocument(i: number, key: keyof DocumentRow, value: string): void {
    this.documentRows.update((rows) => rows.map((r, idx) => (idx === i ? { ...r, [key]: value } : r)));
  }
}
