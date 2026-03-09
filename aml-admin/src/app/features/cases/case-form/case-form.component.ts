import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type { CaseDto, CreateCaseRequest, UpdateCaseRequest, ApiResponse } from '../../../shared/models/api.model';

@Component({
  selector: 'app-case-form',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './case-form.component.html'
})
export class CaseFormComponent implements OnInit {
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
  alertId = signal('');
  status = signal('');
  assignedToId = signal('');
  createdByRole = signal('');
  isActive = signal(true);

  isEdit = () => this.id() !== null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.api.getCase(id).subscribe({
        next: (res: ApiResponse<CaseDto>) => {
          this.loading.set(false);
          if (res.success && res.data) {
            const d = res.data;
            this.customerId.set(d.customerId ?? '');
            this.alertId.set(d.alertId ?? '');
            this.status.set(d.status ?? '');
            this.assignedToId.set(d.assignedToId ?? '');
            this.createdByRole.set(d.createdByRole ?? '');
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
    const payloadCreate: CreateCaseRequest = {
      customerId: this.customerId() || undefined,
      alertId: this.alertId() || undefined,
      status: this.status().trim() || 'Open',
      assignedToId: this.assignedToId() || undefined,
      createdByRole: this.createdByRole() || undefined
    };
    if (id) {
      const payload: UpdateCaseRequest = { ...payloadCreate, isActive: this.isActive() };
      this.api.updateCase(id, payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/cases']);
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
      this.api.createCase(payloadCreate).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/cases']);
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
    this.router.navigate(['/cases']);
  }
}
