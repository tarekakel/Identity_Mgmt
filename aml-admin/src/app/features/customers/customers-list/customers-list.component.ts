import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmService } from '../../../shared/services/confirm.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import type { ApiResponse, PagedResult, CustomerDto, PagedRequest } from '../../../shared/models/api.model';

@Component({
  selector: 'app-customers-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, LoaderComponent, PaginationComponent],
  templateUrl: './customers-list.component.html'
})
export class CustomersListComponent implements OnInit {
  private request = signal<PagedRequest>({ pageNumber: 1, pageSize: 10 });
  list = signal<CustomerDto[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  loading = signal(true);
  searchTerm = signal('');

  pageNumber = computed(() => this.request().pageNumber);
  pageSize = computed(() => this.request().pageSize);

  private readonly api = inject(ApiService);
  private readonly confirmService = inject(ConfirmService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const req = this.request();
    this.api.getCustomers(req).subscribe({
      next: (res: ApiResponse<PagedResult<CustomerDto>>) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.list.set(res.data.items);
          this.totalCount.set(res.data.totalCount);
          this.totalPages.set(res.data.totalPages);
        }
      },
      error: () => {
        this.loading.set(false);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
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

  deleteCustomer(id: string): void {
    const title = this.translate.instant('common.confirmTitle');
    const message = this.translate.instant('common.confirmDeleteCustomer');
    this.confirmService.confirm(title, message).subscribe((ok) => {
      if (!ok) return;
      this.api.deleteCustomer(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.notification.success(this.translate.instant('common.deleteSuccess'));
            this.load();
          } else {
            this.notification.error(res.message ?? this.translate.instant('common.deleteFailed'));
          }
        },
        error: (err) => {
          this.notification.error(err?.error?.message ?? this.translate.instant('common.deleteFailed'));
        }
      });
    });
  }
}
