import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ToastService } from '../../../core/services/toast.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import type { ApiResponse, PermissionDto } from '../../../shared/models/api.model';

@Component({
  selector: 'app-permissions-list',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule, LoaderComponent],
  templateUrl: './permissions-list.component.html'
})
export class PermissionsListComponent implements OnInit {
  permissions: PermissionDto[] = [];
  loading = true;
  error: string | null = null;

  private readonly api = inject(ApiService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly toast = inject(ToastService);
  private readonly translate = inject(TranslateService);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.api.getPermissions().subscribe({
      next: (res: ApiResponse<PermissionDto[]>) => {
        this.loading = false;
        if (res.success && res.data) this.permissions = res.data;
        else this.error = res.message || 'errors.generic';
      },
      error: () => {
        this.loading = false;
        this.error = 'errors.generic';
      }
    });
  }

  deletePermission(id: string, name: string): void {
    const title = this.translate.instant('dialog.confirmDelete');
    const message = this.translate.instant('dialog.confirmDeletePermission', { name });
    this.confirmDialog.confirm({ title, message }).subscribe((confirmed) => {
      if (!confirmed) return;
      this.api.deletePermission(id).subscribe({
        next: () => {
          this.load();
          this.toast.success(this.translate.instant('toast.deleteSuccess'));
        },
        error: (err: { error?: { message?: string } }) =>
          this.toast.error(err.error?.message || this.translate.instant('dialog.deleteFailed'))
      });
    });
  }
}
