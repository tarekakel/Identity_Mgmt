import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  CustomerDto,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  ApiResponse,
  CustomerTypeDto,
  NationalityDto,
  OccupationDto,
  SourceOfFundsDto
} from '../../../shared/models/api.model';

const DEFAULT_TENANT_ID = '00000000-0000-0000-0000-000000000001';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './customer-form.component.html'
})
export class CustomerFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  id = signal<string | null>(null);
  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);

  fullName = signal('');
  nationalIdOrPassport = signal('');
  dateOfBirth = signal('');
  nationalityId = signal('');
  nationalities = signal<NationalityDto[]>([]);
  address = signal('');
  occupations = signal<OccupationDto[]>([]);
  sourceOfFundsList = signal<SourceOfFundsDto[]>([]);
  occupationId = signal('');
  sourceOfFundsId = signal('');
  isPep = signal(false);
  businessActivity = signal('');
  riskClassification = signal('');
  isActive = signal(true);
  customerTypeId = signal('');
  customerTypes = signal<CustomerTypeDto[]>([]);
  customerData = signal<CustomerDto | null>(null);

  isEdit = () => this.id() !== null;

  ngOnInit(): void {
    this.api.getCustomerTypes().subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.length > 0) {
          this.customerTypes.set(res.data);
          if (!this.customerTypeId()) this.customerTypeId.set(res.data[0].id);
        }
      }
    });
    this.api.getNationalities().subscribe({
      next: (res) => {
        if (res.success && res.data) this.nationalities.set(res.data);
      }
    });
    this.api.getOccupations().subscribe({
      next: (res) => {
        if (res.success && res.data) this.occupations.set(res.data);
      }
    });
    this.api.getSourceOfFunds().subscribe({
      next: (res) => {
        if (res.success && res.data) this.sourceOfFundsList.set(res.data);
      }
    });
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.api.getCustomer(id).subscribe({
        next: (res: ApiResponse<CustomerDto>) => {
          this.loading.set(false);
          if (res.success && res.data) {
            const d = res.data;
            this.customerData.set(d);
            this.fullName.set(d.fullName ?? '');
            this.nationalIdOrPassport.set(d.nationalIdOrPassport ?? '');
            this.dateOfBirth.set(d.dateOfBirth ? d.dateOfBirth.slice(0, 10) : '');
            this.nationalityId.set(d.nationalityId ?? '');
            this.address.set(d.address ?? '');
            this.occupationId.set(d.occupationId ?? '');
            this.sourceOfFundsId.set(d.sourceOfFundsId ?? '');
            this.isPep.set(d.isPep ?? false);
            this.businessActivity.set(d.businessActivity ?? '');
            this.riskClassification.set(d.riskClassification ?? '');
            this.isActive.set(d.isActive ?? true);
            if (d.customerTypeId) this.customerTypeId.set(d.customerTypeId);
          }
        },
        error: () => {
          this.loading.set(false);
          this.notification.error(this.translate.instant('common.errorGeneric'));
        }
      });
    } else {
      this.loading.set(false);
    }
  }

  submit(): void {
    this.error.set(null);
    this.saving.set(true);
    const id = this.id();
    if (id) {
      const d = this.customerData();
      const payload: UpdateCustomerRequest = {
        fullName: this.fullName().trim() || ' ',
        customerTypeId: (this.customerTypeId() || d?.customerTypeId) ?? this.customerTypes()[0]?.id ?? '',
        firstName: d?.firstName,
        lastName: d?.lastName,
        dateOfBirth: this.dateOfBirth() ? new Date(this.dateOfBirth()).toISOString() : undefined,
        genderId: d?.genderId,
        nationalityId: this.nationalityId() || undefined,
        countryOfResidenceId: d?.countryOfResidenceId,
        address: this.address() || undefined,
        city: d?.city,
        country: d?.country,
        email: d?.email,
        phone: d?.phone,
        occupationId: this.occupationId() || undefined,
        employerName: d?.employerName,
        sourceOfFundsId: this.sourceOfFundsId() || undefined,
        annualIncome: d?.annualIncome,
        expectedMonthlyTransactionVolume: d?.expectedMonthlyTransactionVolume,
        expectedMonthlyTransactionValue: d?.expectedMonthlyTransactionValue,
        accountPurpose: d?.accountPurpose,
        isActive: this.isActive()
      };
      this.api.updateCustomer(id, payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/customers']);
          } else {
            const msg = res.message ?? this.translate.instant('common.saveFailed');
            this.error.set(msg);
            this.notification.error(msg);
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.notification.error(err?.error?.message ?? this.translate.instant('common.saveFailed'));
        }
      });
    } else {
      const payload: CreateCustomerRequest = {
        tenantId: DEFAULT_TENANT_ID,
        fullName: this.fullName().trim() || ' ',
        customerTypeId: this.customerTypeId() || (this.customerTypes()[0]?.id ?? ''),
        nationalId: this.nationalIdOrPassport() || undefined,
        dateOfBirth: this.dateOfBirth() ? new Date(this.dateOfBirth()).toISOString() : undefined,
        nationalityId: this.nationalityId() || undefined,
        address: this.address() || undefined,
        occupationId: this.occupationId() || undefined,
        sourceOfFundsId: this.sourceOfFundsId() || undefined
      };
      this.api.createCustomer(payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/customers']);
          } else {
            const msg = res.message ?? this.translate.instant('common.saveFailed');
            this.error.set(msg);
            this.notification.error(msg);
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.notification.error(err?.error?.message ?? this.translate.instant('common.saveFailed'));
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/customers']);
  }
}
