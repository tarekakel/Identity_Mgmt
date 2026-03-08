import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import type { ApiResponse, PagedResult, CaseDto, PagedRequest } from '../../../shared/models/api.model';

@Component({
  selector: 'app-cases-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, LoaderComponent, PaginationComponent],
  templateUrl: './cases-list.component.html'
})
export class CasesListComponent implements OnInit {
  private request = signal<PagedRequest>({ pageNumber: 1, pageSize: 10 });
  list = signal<CaseDto[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  loading = signal(true);
  searchTerm = signal('');

  pageNumber = computed(() => this.request().pageNumber);
  pageSize = computed(() => this.request().pageSize);

  private readonly api = inject(ApiService);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getCases(this.request()).subscribe({
      next: (res: ApiResponse<PagedResult<CaseDto>>) => {
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

  deleteCase(id: string): void {
    if (!confirm('Are you sure you want to delete this case?')) return;
    this.api.deleteCase(id).subscribe({
      next: (res) => {
        if (res.success) this.load();
      }
    });
  }
}
