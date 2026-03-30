import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';
import type { ApiResponse } from '../../../shared/models/api.model';
import type { CountryDto } from '../../../shared/models/api.model';
import { isMasterLookupSegment, type MasterLookupSegment } from '../master-lookup.model';

@Component({
  selector: 'app-master-lookup-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule],
  templateUrl: './master-lookup-form.component.html',
  styleUrl: './master-lookup-form.component.scss'
})
export class MasterLookupFormComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  segment = signal<MasterLookupSegment | null>(null);
  editId = signal<string | null>(null);
  code = signal('');
  name = signal('');
  countryId = signal('');
  countries = signal<CountryDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);

  isEdit = computed(() => this.editId() != null);

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const s = params.get('segment');
      if (!s || !isMasterLookupSegment(s)) {
        void this.router.navigate(['/masters']);
        return;
      }
      this.segment.set(s);
      this.error.set(null);
      const id = params.get('id');
      if (id) {
        this.editId.set(id);
        this.load(id);
      } else {
        this.editId.set(null);
        this.code.set('');
        this.name.set('');
        this.countryId.set('');
        if (s === 'emirates') {
          this.loadCountriesThenIdle();
        } else {
          this.loading.set(false);
        }
      }
    });
  }

  private loadCountriesThenIdle(): void {
    this.loading.set(true);
    this.api.getCountries().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.countries.set(res.data);
          const q = this.route.snapshot.queryParamMap.get('countryId')?.trim();
          if (q && res.data.some((c) => c.id === q)) {
            this.countryId.set(q);
          }
        } else this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
      },
      error: () => {
        this.loading.set(false);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
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

  private load(id: string): void {
    const seg = this.segment();
    if (!seg) return;
    this.loading.set(true);
    if (seg === 'emirates') {
      this.api.getCountries().subscribe({
        next: (cres) => {
          if (cres.success && cres.data) this.countries.set(cres.data);
          this.api.getEmirate(id).subscribe({
            next: (res) => {
              this.loading.set(false);
              if (res.success && res.data) {
                this.code.set(res.data.code);
                this.name.set(res.data.name);
                this.countryId.set(res.data.countryId);
              } else {
                this.error.set(res.message ?? this.translate.instant('common.errorGeneric'));
              }
            },
            error: () => {
              this.loading.set(false);
              this.error.set(this.translate.instant('common.errorGeneric'));
            }
          });
        },
        error: () => {
          this.loading.set(false);
          this.error.set(this.translate.instant('common.errorGeneric'));
        }
      });
      return;
    }
    this.api.getMasterLookup(seg, id).subscribe({
      next: (res: ApiResponse<CountryDto>) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.code.set(res.data.code);
          this.name.set(res.data.name);
        } else {
          this.error.set(res.message ?? this.translate.instant('common.errorGeneric'));
        }
      },
      error: () => {
        this.loading.set(false);
        this.error.set(this.translate.instant('common.errorGeneric'));
      }
    });
  }

  submit(): void {
    const seg = this.segment();
    if (!seg) return;
    const c = this.code().trim();
    const n = this.name().trim();
    if (seg === 'emirates') {
      const cid = this.countryId().trim();
      if (!cid || !c || !n) {
        this.notification.error(this.translate.instant('masters.emirateCountryCodeNameRequired'));
        return;
      }
      this.saving.set(true);
      this.error.set(null);
      const payload = { countryId: cid, code: c, name: n };
      const id = this.editId();
      const req$ = id ? this.api.updateEmirate(id, payload) : this.api.createEmirate(payload);
      req$.subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            void this.router.navigate(['/masters', seg]);
          } else {
            this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.notification.error(err?.error?.message ?? this.translate.instant('common.errorGeneric'));
        }
      });
      return;
    }
    if (!c || !n) {
      this.notification.error(this.translate.instant('masters.codeNameRequired'));
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    const payload = { code: c, name: n };
    const id = this.editId();
    if (id) {
      this.api.updateMasterLookup(seg, id, payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            void this.router.navigate(['/masters', seg]);
          } else {
            this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.notification.error(err?.error?.message ?? this.translate.instant('common.errorGeneric'));
        }
      });
    } else {
      this.api.createMasterLookup(seg, payload).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            this.notification.success(this.translate.instant('common.saveSuccess'));
            void this.router.navigate(['/masters', seg]);
          } else {
            this.notification.error(res.message ?? this.translate.instant('common.errorGeneric'));
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.notification.error(err?.error?.message ?? this.translate.instant('common.errorGeneric'));
        }
      });
    }
  }

  cancel(): void {
    const seg = this.segment();
    if (seg) void this.router.navigate(['/masters', seg]);
    else void this.router.navigate(['/masters']);
  }
}
