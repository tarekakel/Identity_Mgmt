import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import type {
  CustomerDto,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  ApiResponse
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

  id = signal<string | null>(null);
  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);

  fullName = signal('');
  nationalIdOrPassport = signal('');
  dateOfBirth = signal('');
  nationality = signal('');
  address = signal('');
  occupation = signal('');
  sourceOfFunds = signal('');
  isPep = signal(false);
  businessActivity = signal('');
  riskClassification = signal('');
  isActive = signal(true);

  isEdit = () => this.id() !== null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.api.getCustomer(id).subscribe({
        next: (res: ApiResponse<CustomerDto>) => {
          this.loading.set(false);
          if (res.success && res.data) {
            const d = res.data;
            this.fullName.set(d.fullName ?? '');
            this.nationalIdOrPassport.set(d.nationalIdOrPassport ?? '');
            this.dateOfBirth.set(d.dateOfBirth ? d.dateOfBirth.slice(0, 10) : '');
            this.nationality.set(d.nationality ?? '');
            this.address.set(d.address ?? '');
            this.occupation.set(d.occupation ?? '');
            this.sourceOfFunds.set(d.sourceOfFunds ?? '');
            this.isPep.set(d.isPep ?? false);
            this.businessActivity.set(d.businessActivity ?? '');
            this.riskClassification.set(d.riskClassification ?? '');
            this.isActive.set(d.isActive ?? true);
          }
        },
        error: () => this.loading.set(false)
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
      const payload: UpdateCustomerRequest = {
        fullName: this.fullName().trim() || ' ',
        nationalIdOrPassport: this.nationalIdOrPassport() || undefined,
        dateOfBirth: this.dateOfBirth() ? new Date(this.dateOfBirth()).toISOString() : undefined,
        nationality: this.nationality() || undefined,
        address: this.address() || undefined,
        occupation: this.occupation() || undefined,
        sourceOfFunds: this.sourceOfFunds() || undefined,
        isPep: this.isPep(),
        businessActivity: this.businessActivity() || undefined,
        riskClassification: this.riskClassification().trim() || 'Low',
        isActive: this.isActive()
      };
      this.api.updateCustomer(id, payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) this.router.navigate(['/customers']);
          else this.error.set(res.message ?? 'Update failed');
        },
        error: () => this.saving.set(false)
      });
    } else {
      const payload: CreateCustomerRequest = {
        tenantId: DEFAULT_TENANT_ID,
        fullName: this.fullName().trim() || ' ',
        nationalIdOrPassport: this.nationalIdOrPassport() || undefined,
        dateOfBirth: this.dateOfBirth() ? new Date(this.dateOfBirth()).toISOString() : undefined,
        nationality: this.nationality() || undefined,
        address: this.address() || undefined,
        occupation: this.occupation() || undefined,
        sourceOfFunds: this.sourceOfFunds() || undefined,
        isPep: this.isPep(),
        businessActivity: this.businessActivity() || undefined,
        riskClassification: this.riskClassification().trim() || 'Low'
      };
      this.api.createCustomer(payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) this.router.navigate(['/customers']);
          else this.error.set(res.message ?? 'Create failed');
        },
        error: () => this.saving.set(false)
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/customers']);
  }
}
