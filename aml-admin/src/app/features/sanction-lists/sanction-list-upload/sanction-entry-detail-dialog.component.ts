import { CommonModule } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import type { SanctionListEntryDto } from '../../../shared/models/api.model';

/**
 * Read-only modal that shows every captured field of a SanctionListEntryDto.
 * Sections render only when their underlying data exists, so the dialog stays compact
 * for sparse legacy entries and expands for richly-populated UN/UAE rows.
 */
@Component({
  selector: 'app-sanction-entry-detail-dialog',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './sanction-entry-detail-dialog.component.html'
})
export class SanctionEntryDetailDialogComponent {
  entry = input.required<SanctionListEntryDto>();
  closed = output<void>();

  readonly aliases = computed(() => this.entry().aliasItems ?? []);
  readonly dobs = computed(() => this.entry().datesOfBirth ?? []);
  readonly addresses = computed(() => this.entry().addresses ?? []);
  readonly placesOfBirth = computed(() => this.entry().placesOfBirth ?? []);
  readonly documents = computed(() => this.entry().documents ?? []);
  readonly nationalities = computed(() => this.entry().nationalities ?? []);
  readonly designations = computed(() => this.entry().designations ?? []);
  readonly lastDayUpdates = computed(() => this.entry().lastDayUpdates ?? []);

  readonly hasIdentity = computed(() => {
    const e = this.entry();
    return !!(e.fullName || e.fullNameArabic || e.firstName || e.secondName ||
      e.familyNameLatin || e.familyNameArabic || e.gender || e.entryType || e.typeDetail);
  });

  readonly hasSourceMetadata = computed(() => {
    const e = this.entry();
    return !!(e.listSource || e.dataId || e.versionNum || e.referenceNumber ||
      e.unListType || e.listType || e.listedOn || (this.lastDayUpdates().length > 0) || e.lastDayUpdated);
  });

  readonly hasNarrative = computed(() => {
    const e = this.entry();
    return !!((e.comments && e.comments.trim()) || (e.otherInformation && e.otherInformation.trim()));
  });

  /**
   * True when *no* extra section has anything to render.
   * Used to show a small "no additional details" message instead of an empty dialog.
   */
  readonly isEmpty = computed(() => {
    return !this.hasIdentity() && !this.hasSourceMetadata() && !this.hasNarrative()
      && this.aliases().length === 0
      && this.dobs().length === 0
      && this.addresses().length === 0
      && this.placesOfBirth().length === 0
      && this.documents().length === 0
      && this.nationalities().length === 0
      && this.designations().length === 0;
  });

  formatDate(value: string | null | undefined): string {
    if (!value) return '';
    try {
      const d = new Date(value);
      if (isNaN(d.getTime())) return value;
      return d.toLocaleDateString();
    } catch {
      return value;
    }
  }

  formatDob(d: { date?: string | null; year?: number | null; fromYear?: number | null; toYear?: number | null; typeOfDate?: string | null; note?: string | null }): string {
    if (d.date) return this.formatDate(d.date);
    if (d.fromYear || d.toYear) {
      const from = d.fromYear ?? d.toYear;
      const to = d.toYear ?? d.fromYear;
      return from === to ? String(from) : `${from} – ${to}`;
    }
    if (d.year) return String(d.year);
    return '';
  }

  isGoodAlias(quality: string | null | undefined): boolean {
    if (!quality) return false;
    const q = quality.trim().toLowerCase();
    return q === 'good' || q === 'a.k.a.' || q === 'aka';
  }

  close(): void {
    this.closed.emit();
  }

  onBackdropKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') this.close();
  }
}
