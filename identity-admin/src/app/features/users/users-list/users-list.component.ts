import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ToastService } from '../../../core/services/toast.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import type { ApiResponse, PagedResult, UserDto, PagedRequest } from '../../../shared/models/api.model';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, TranslateModule, LoaderComponent, PaginationComponent],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent implements OnInit {
  private request = signal<PagedRequest>({ pageNumber: 1, pageSize: 10 });
  list = signal<UserDto[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  loading = signal(true);
  searchTerm = signal('');

  pageNumber = computed(() => this.request().pageNumber);
  pageSize = computed(() => this.request().pageSize);

  private readonly api = inject(ApiService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly toast = inject(ToastService);
  private readonly translate = inject(TranslateService);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const req = this.request();
    this.api.getUsers(req).subscribe({
      next: (res: ApiResponse<PagedResult<UserDto>>) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.list.set(res.data.items);
          this.totalCount.set(res.data.totalCount);
          this.totalPages.set(res.data.totalPages);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(page: number): void {
    this.request.update((r) => ({ ...r, pageNumber: page }));
    this.load();
  }

  onSearch(): void {
    this.request.update((r) => ({ ...r, pageNumber: 1, searchTerm: this.searchTerm() || undefined }));
    this.load();
  }

  deleteUser(id: string, userName: string): void {
    const title = this.translate.instant('dialog.confirmDelete');
    const message = this.translate.instant('dialog.confirmDeleteUser', { name: userName });
    this.confirmDialog.confirm({ title, message }).subscribe((confirmed) => {
      if (!confirmed) return;
      this.api.deleteUser(id).subscribe({
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
