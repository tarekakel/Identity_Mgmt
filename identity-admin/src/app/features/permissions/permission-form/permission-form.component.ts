import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import type { ApiResponse, PermissionDto } from '../../../shared/models/api.model';

@Component({
  selector: 'app-permission-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule, LoaderComponent],
  templateUrl: './permission-form.component.html'
})
export class PermissionFormComponent implements OnInit {
  form: FormGroup;
  id: string | null = null;
  isEdit = false;
  loading = false;
  error: string | null = null;

  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.form = this.fb.nonNullable.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      code: ['', [Validators.required, Validators.minLength(2)]],
      description: ['']
    });
  }

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.id;
    if (this.isEdit && this.id) this.loadPermission();
  }

  private loadPermission(): void {
    if (!this.id) return;
    this.loading = true;
    this.api.getPermission(this.id).subscribe({
      next: (res: ApiResponse<PermissionDto>) => {
        this.loading = false;
        if (res.success && res.data) {
          const p = res.data;
          this.form.patchValue({
            name: p.name,
            code: p.code,
            description: p.description || ''
          });
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.loading) return;
    this.error = null;
    this.loading = true;
    const value = this.form.getRawValue();
    const body = {
      name: value.name,
      code: value.code,
      description: value.description || undefined
    };
    if (this.isEdit && this.id) {
      this.api.updatePermission(this.id, body).subscribe({
        next: () => {
          this.loading = false;
          this.toast.success(this.translate.instant('toast.updateSuccess'));
          this.router.navigate(['/permissions']);
        },
        error: (e: { error?: { message?: string } }) => {
          this.loading = false;
          const msg = e.error?.message || this.translate.instant('toast.saveFailed');
          this.error = msg;
          this.toast.error(msg);
        }
      });
    } else {
      this.api.createPermission(body).subscribe({
        next: () => {
          this.loading = false;
          this.toast.success(this.translate.instant('toast.createSuccess'));
          this.router.navigate(['/permissions']);
        },
        error: (e: { error?: { message?: string } }) => {
          this.loading = false;
          const msg = e.error?.message || this.translate.instant('toast.saveFailed');
          this.error = msg;
          this.toast.error(msg);
        }
      });
    }
  }
}
