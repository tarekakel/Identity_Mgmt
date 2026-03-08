import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import type { ApiResponse, PagedResult, RoleDto, UserDto } from '../../../shared/models/api.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule, LoaderComponent],
  templateUrl: './user-form.component.html'
})
export class UserFormComponent implements OnInit {
  form: FormGroup;
  id: string | null = null;
  isEdit = false;
  loading = false;
  loadingRoles = true;
  roles: RoleDto[] = [];
  error: string | null = null;

  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.form = this.fb.nonNullable.group({
      userName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', []],
      isActive: [true],
      roleIds: [[] as string[]]
    });
  }

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.id;
    if (this.isEdit) this.form.get('password')?.clearValidators();
    else this.form.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);

    this.api.getRoles({ pageNumber: 1, pageSize: 500 }).subscribe({
      next: (res: ApiResponse<PagedResult<RoleDto>>) => {
        this.loadingRoles = false;
        if (res.success && res.data) this.roles = res.data.items;
        if (this.isEdit && this.id) this.loadUser();
      }
    });
  }

  private loadUser(): void {
    if (!this.id) return;
    this.loading = true;
      this.api.getUser(this.id).subscribe({
        next: (res: ApiResponse<UserDto>) => {
        this.loading = false;
        if (res.success && res.data) {
          const u = res.data;
          const roleIds = (u.roleNames || []).map((name: string) => this.roles.find((r) => r.name === name)?.id).filter(Boolean) as string[];
          this.form.patchValue({
            userName: u.userName,
            email: u.email,
            isActive: u.isActive,
            roleIds
          });
        }
      },
      error: () => { this.loading = false; }
    });
  }

  toggleRole(roleId: string): void {
    const control = this.form.get('roleIds');
    const current: string[] = control?.value || [];
    const next = current.includes(roleId) ? current.filter((id) => id !== roleId) : [...current, roleId];
    control?.setValue(next);
  }

  onSubmit(): void {
    if (this.form.invalid || this.loading) return;
    this.error = null;
    this.loading = true;
    const value = this.form.getRawValue();
    if (this.isEdit && this.id) {
      this.api.updateUser(this.id, { userName: value.userName, email: value.email, isActive: value.isActive }).subscribe({
        next: () => {
          this.api.assignUserRoles(this.id!, { roleIds: value.roleIds || [] }).subscribe({
            next: () => {
              this.loading = false;
              this.toast.success(this.translate.instant('toast.updateSuccess'));
              this.router.navigate(['/users']);
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
      this.api.createUser({
        userName: value.userName,
        email: value.email,
        password: value.password,
        isActive: value.isActive,
        roleIds: value.roleIds
      }).subscribe({
        next: () => {
          this.loading = false;
          this.toast.success(this.translate.instant('toast.createSuccess'));
          this.router.navigate(['/users']);
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
