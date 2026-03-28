import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { MASTER_LOOKUP_SEGMENTS, type MasterLookupSegment } from '../master-lookup.model';

const SEGMENT_LABEL_KEYS: Record<MasterLookupSegment, string> = {
  countries: 'masters.segments.countries',
  nationalities: 'masters.segments.nationalities',
  genders: 'masters.segments.genders',
  'customer-types': 'masters.segments.customerTypes',
  'customer-statuses': 'masters.segments.customerStatuses',
  'document-types': 'masters.segments.documentTypes',
  occupations: 'masters.segments.occupations',
  'source-of-funds': 'masters.segments.sourceOfFunds'
};

@Component({
  selector: 'app-masters',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule],
  templateUrl: './masters.component.html',
  styleUrl: './masters.component.scss'
})
export class MastersComponent {
  readonly segments = MASTER_LOOKUP_SEGMENTS;

  labelKey(segment: MasterLookupSegment): string {
    return SEGMENT_LABEL_KEYS[segment];
  }
}
