import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import type { ApiResponse, PagedResult, SanctionsScreeningDto, PagedRequest } from '../../../shared/models/api.model';

@Component({
  selector: 'app-sanctions-screening-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, LoaderComponent, PaginationComponent],
  templateUrl: './sanctions-screening-list.component.html'
})
export class SanctionsScreeningListComponent implements OnInit {
  private request = signal<PagedRequest>({ pageNumber: 1, pageSize: 10 });
  list = signal<SanctionsScreeningDto[]>([]);
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
    this.api.getSanctionsScreening(this.request()).subscribe({
      next: (res: ApiResponse<PagedResult<SanctionsScreeningDto>>) => {
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

  deleteSanctionsScreening(id: string): void {
    if (!confirm('Are you sure you want to delete this sanctions screening?')) return;
    this.api.deleteSanctionsScreening(id).subscribe({
      next: (res) => {
        if (res.success) this.load();
      }
    });
  }
}
