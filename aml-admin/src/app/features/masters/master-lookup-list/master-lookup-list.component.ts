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
import type { CountryDto, EmirateDto } from '../../../shared/models/api.model';
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
  list = signal<(CountryDto | EmirateDto)[]>([]);
  loading = signal(true);
  searchTerm = signal('');

  /** Countries segment: expandable emirates per country */
  expandedCountryId = signal<string | null>(null);
  emiratesByCountry = signal<Record<string, EmirateDto[]>>({});
  emiratesLoadingCountryId = signal<string | null>(null);

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
      'source-of-funds': 'masters.segments.sourceOfFunds',
      emirates: 'masters.segments.emirates',
      'residence-statuses': 'masters.segments.residenceStatuses'
    };
    return keys[seg];
  }

  filteredList(): (CountryDto | EmirateDto)[] {
    const q = this.searchTerm().trim().toLowerCase();
    if (!q) return this.list();
    return this.list().filter((row) => {
      const countryMatch =
        'countryName' in row && row.countryName
          ? row.countryName.toLowerCase().includes(q)
          : false;
      return (
        row.code.toLowerCase().includes(q) ||
        row.name.toLowerCase().includes(q) ||
        row.id.toLowerCase().includes(q) ||
        countryMatch
      );
    });
  }

  load(): void {
    const seg = this.segment();
    if (!seg) return;
    if (seg === 'countries') {
      this.expandedCountryId.set(null);
      this.emiratesByCountry.set({});
      this.emiratesLoadingCountryId.set(null);
    }
    this.loading.set(true);
    if (seg === 'emirates') {
      this.api.getEmirates().subscribe({
        next: (res: ApiResponse<EmirateDto[]>) => {
          this.loading.set(false);
          if (res.success && res.data) this.list.set(res.data);
          else this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
        },
        error: () => {
          this.loading.set(false);
          this.notification.error(this.translate.instant('common.errorGeneric'));
        }
      });
      return;
    }
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

  toggleCountryEmirates(countryId: string): void {
    if (this.expandedCountryId() === countryId) {
      this.expandedCountryId.set(null);
      return;
    }
    this.expandedCountryId.set(countryId);
    this.emiratesLoadingCountryId.set(countryId);
    this.api.getEmirates(countryId).subscribe({
      next: (res: ApiResponse<EmirateDto[]>) => {
        this.emiratesLoadingCountryId.set(null);
        if (res.success && res.data) {
          this.emiratesByCountry.update((m) => ({ ...m, [countryId]: res.data! }));
        } else {
          this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
        }
      },
      error: () => {
        this.emiratesLoadingCountryId.set(null);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  emiratesForExpandedCountry(): EmirateDto[] {
    const cid = this.expandedCountryId();
    if (!cid) return [];
    return this.emiratesByCountry()[cid] ?? [];
  }

  deleteEmirateUnderCountry(emirateId: string, countryId: string): void {
    const title = this.translate.instant('common.confirmTitle');
    const message = this.translate.instant('masters.confirmDelete');
    this.confirmService.confirm(title, message).subscribe((ok) => {
      if (!ok) return;
      this.api.deleteEmirate(emirateId).subscribe({
        next: (res) => {
          if (res.success) {
            this.notification.success(this.translate.instant('common.deleteSuccess'));
            this.refreshEmiratesForCountry(countryId);
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

  private refreshEmiratesForCountry(countryId: string): void {
    this.api.getEmirates(countryId).subscribe({
      next: (res: ApiResponse<EmirateDto[]>) => {
        if (res.success && res.data) {
          this.emiratesByCountry.update((m) => ({ ...m, [countryId]: res.data! }));
        }
      },
      error: () => {
        /* ignore */
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
      const del$ =
        seg === 'emirates' ? this.api.deleteEmirate(id) : this.api.deleteMasterLookup(seg, id);
      del$.subscribe({
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
