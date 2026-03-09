import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  RiskAssignmentDto,
  CreateRiskAssignmentRequest,
  UpdateRiskAssignmentRequest,
  ApiResponse
} from '../../../shared/models/api.model';

@Component({
  selector: 'app-risk-assignment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './risk-assignment-form.component.html'
})
export class RiskAssignmentFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  id = signal<string | null>(null);
  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);

  customerId = signal('');
  countryRisk = signal(0);
  customerTypeRisk = signal(0);
  pepRisk = signal(0);
  transactionRisk = signal(0);
  industryRisk = signal(0);
  totalScore = signal(0);
  riskLevel = signal('');
  isActive = signal(true);

  isEdit = () => this.id() !== null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.api.getRiskAssignment(id).subscribe({
        next: (res: ApiResponse<RiskAssignmentDto>) => {
          this.loading.set(false);
          if (res.success && res.data) {
            const d = res.data;
            this.customerId.set(d.customerId ?? '');
            this.countryRisk.set(d.countryRisk ?? 0);
            this.customerTypeRisk.set(d.customerTypeRisk ?? 0);
            this.pepRisk.set(d.pepRisk ?? 0);
            this.transactionRisk.set(d.transactionRisk ?? 0);
            this.industryRisk.set(d.industryRisk ?? 0);
            this.totalScore.set(d.totalScore ?? 0);
            this.riskLevel.set(d.riskLevel ?? '');
            this.isActive.set(d.isActive ?? true);
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
    const payloadBase = {
      countryRisk: this.countryRisk(),
      customerTypeRisk: this.customerTypeRisk(),
      pepRisk: this.pepRisk(),
      transactionRisk: this.transactionRisk(),
      industryRisk: this.industryRisk(),
      totalScore: this.totalScore(),
      riskLevel: this.riskLevel().trim() || 'Medium'
    };
    if (id) {
      const payload: UpdateRiskAssignmentRequest = { ...payloadBase, isActive: this.isActive() };
      this.api.updateRiskAssignment(id, payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/risk-assignment']);
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
      const payload: CreateRiskAssignmentRequest = {
        customerId: this.customerId().trim(),
        ...payloadBase
      };
      if (!payload.customerId) {
        const msg = this.translate.instant('common.customerIdRequired');
        this.error.set(msg);
        this.notification.error(msg);
        this.saving.set(false);
        return;
      }
      this.api.createRiskAssignment(payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/risk-assignment']);
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
    this.router.navigate(['/risk-assignment']);
  }
}
