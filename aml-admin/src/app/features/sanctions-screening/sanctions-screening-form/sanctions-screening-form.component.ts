import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type {
  SanctionsScreeningDto,
  CreateSanctionsScreeningRequest,
  UpdateSanctionsScreeningRequest,
  ApiResponse
} from '../../../shared/models/api.model';

@Component({
  selector: 'app-sanctions-screening-form',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './sanctions-screening-form.component.html'
})
export class SanctionsScreeningFormComponent implements OnInit {
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
  screeningList = signal('');
  result = signal('');
  matchedName = signal('');
  score = signal<number | null>(null);
  screenedAt = signal('');
  isActive = signal(true);

  isEdit = () => this.id() !== null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.api.getSanctionsScreeningById(id).subscribe({
        next: (res: ApiResponse<SanctionsScreeningDto>) => {
          this.loading.set(false);
          if (res.success && res.data) {
            const d = res.data;
            this.customerId.set(d.customerId ?? '');
            this.screeningList.set(d.screeningList ?? '');
            this.result.set(d.result ?? '');
            this.matchedName.set(d.matchedName ?? '');
            this.score.set(d.score ?? null);
            this.screenedAt.set(d.screenedAt ? d.screenedAt.slice(0, 16) : '');
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
      this.screenedAt.set(new Date().toISOString().slice(0, 16));
    }
  }

  submit(): void {
    this.error.set(null);
    this.saving.set(true);
    const id = this.id();
    const screenedAtVal = this.screenedAt() ? new Date(this.screenedAt()).toISOString() : new Date().toISOString();
    if (id) {
      const payload: UpdateSanctionsScreeningRequest = {
        screeningList: this.screeningList().trim() || 'Default',
        result: this.result().trim() || 'Clear',
        matchedName: this.matchedName() || undefined,
        score: this.score() ?? undefined,
        screenedAt: screenedAtVal,
        isActive: this.isActive()
      };
      this.api.updateSanctionsScreening(id, payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/sanctions-screening']);
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
      const payload: CreateSanctionsScreeningRequest = {
        customerId: this.customerId().trim(),
        screeningList: this.screeningList().trim() || 'Default',
        result: this.result().trim() || 'Clear',
        matchedName: this.matchedName() || undefined,
        score: this.score() ?? undefined,
        screenedAt: screenedAtVal
      };
      if (!payload.customerId) {
        const msg = this.translate.instant('common.customerIdRequired');
        this.error.set(msg);
        this.notification.error(msg);
        this.saving.set(false);
        return;
      }
      this.api.createSanctionsScreening(payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            this.router.navigate(['/sanctions-screening']);
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
    this.router.navigate(['/sanctions-screening']);
  }
}
