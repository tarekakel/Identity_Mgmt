import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import type { ApiResponse, PermissionDto, RoleDto } from '../../../shared/models/api.model';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule, LoaderComponent],
  templateUrl: './role-form.component.html'
})
export class RoleFormComponent implements OnInit {
  form: FormGroup;
  id: string | null = null;
  isEdit = false;
  loading = false;
  loadingPermissions = true;
  permissions: PermissionDto[] = [];
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
      description: [''],
      permissionIds: [[] as string[]]
    });
  }

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.id;

    this.api.getPermissions().subscribe({
      next: (res: ApiResponse<PermissionDto[]>) => {
        this.loadingPermissions = false;
        if (res.success && res.data) this.permissions = res.data;
        if (this.isEdit && this.id) this.loadRole();
      }
    });
  }

  private loadRole(): void {
    if (!this.id) return;
    this.loading = true;
    this.api.getRole(this.id).subscribe({
      next: (res: ApiResponse<RoleDto>) => {
        this.loading = false;
        if (res.success && res.data) {
          const r = res.data;
          const permissionIds = (r.permissionCodes || []).map((code: string) => this.permissions.find((p) => p.code === code)?.id).filter(Boolean) as string[];
          this.form.patchValue({
            name: r.name,
            description: r.description || '',
            permissionIds
          });
        }
      },
      error: () => { this.loading = false; }
    });
  }

  togglePermission(permId: string): void {
    const control = this.form.get('permissionIds');
    const current: string[] = control?.value || [];
    const next = current.includes(permId) ? current.filter((id) => id !== permId) : [...current, permId];
    control?.setValue(next);
  }

  onSubmit(): void {
    if (this.form.invalid || this.loading) return;
    this.error = null;
    this.loading = true;
    const value = this.form.getRawValue();
    if (this.isEdit && this.id) {
      this.api.updateRole(this.id, { name: value.name, description: value.description }).subscribe({
        next: () => {
          this.api.assignRolePermissions(this.id!, { permissionIds: value.permissionIds || [] }).subscribe({
            next: () => {
              this.loading = false;
              this.toast.success(this.translate.instant('toast.updateSuccess'));
              this.router.navigate(['/roles']);
            },
            error: (e: { error?: { message?: string } }) => {
              this.loading = false;
              const msg = e.error?.message || this.translate.instant('toast.saveFailed');
              this.error = msg;
              this.toast.error(msg);
            }
          });
        },
        error: (e: { error?: { message?: string } }) => {
          this.loading = false;
          const msg = e.error?.message || this.translate.instant('toast.saveFailed');
          this.error = msg;
          this.toast.error(msg);
        }
      });
    } else {
      this.api.createRole({
        name: value.name,
        description: value.description,
        permissionIds: value.permissionIds
      }).subscribe({
        next: () => {
          this.loading = false;
          this.toast.success(this.translate.instant('toast.createSuccess'));
          this.router.navigate(['/roles']);
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
