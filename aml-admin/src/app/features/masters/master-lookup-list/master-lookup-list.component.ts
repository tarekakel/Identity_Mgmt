import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmService } from '../../../shared/services/confirm.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import type { ApiResponse } from '../../../shared/models/api.model';
import type { CountryDto } from '../../../shared/models/api.model';
import { isMasterLookupSegment, type MasterLookupSegment } from '../master-lookup.model';

@Component({
  selector: 'app-master-lookup-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, LoaderComponent],
  templateUrl: './master-lookup-list.component.html',
  styleUrl: './master-lookup-list.component.scss'
})
export class MasterLookupListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);
  private readonly confirmService = inject(ConfirmService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  segment = signal<MasterLookupSegment | null>(null);
  list = signal<CountryDto[]>([]);
  loading = signal(true);
  searchTerm = signal('');

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const s = params.get('segment');
      if (!s || !isMasterLookupSegment(s)) {
        void this.router.navigate(['/masters']);
        return;
      }
      this.segment.set(s);
      this.searchTerm.set('');
      this.load();
    });
  }

  titleKey(): string {
    const seg = this.segment();
    if (!seg) return 'pages.masters.title';
    const keys: Record<MasterLookupSegment, string> = {
      countries: 'masters.segments.countries',
      nationalities: 'masters.segments.nationalities',
      genders: 'masters.segments.genders',
      'customer-types': 'masters.segments.customerTypes',
      'customer-statuses': 'masters.segments.customerStatuses',
      'document-types': 'masters.segments.documentTypes',
      occupations: 'masters.segments.occupations',
      'source-of-funds': 'masters.segments.sourceOfFunds'
    };
    return keys[seg];
  }

  filteredList(): CountryDto[] {
    const q = this.searchTerm().trim().toLowerCase();
    if (!q) return this.list();
    return this.list().filter(
      (row) =>
        row.code.toLowerCase().includes(q) ||
        row.name.toLowerCase().includes(q) ||
        row.id.toLowerCase().includes(q)
    );
  }

  load(): void {
    const seg = this.segment();
    if (!seg) return;
    this.loading.set(true);
    this.api.getMasterLookups(seg).subscribe({
      next: (res: ApiResponse<CountryDto[]>) => {
        this.loading.set(false);
        if (res.success && res.data) this.list.set(res.data);
        else this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
      },
      error: () => {
        this.loading.set(false);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  deleteRow(id: string): void {
    const seg = this.segment();
    if (!seg) return;
    const title = this.translate.instant('common.confirmTitle');
    const message = this.translate.instant('masters.confirmDelete');
    this.confirmService.confirm(title, message).subscribe((ok) => {
      if (!ok) return;
      this.api.deleteMasterLookup(seg, id).subscribe({
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
