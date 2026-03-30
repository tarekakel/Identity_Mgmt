import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { MASTER_LOOKUP_SEGMENTS, type MasterLookupSegment } from '../master-lookup.model';

/** Overview tiles: emirates are managed from Countries + sidebar "All emirates". */
const MASTER_OVERVIEW_SEGMENTS = MASTER_LOOKUP_SEGMENTS.filter((s) => s !== 'emirates');

const SEGMENT_LABEL_KEYS: Record<MasterLookupSegment, string> = {
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

@Component({
  selector: 'app-masters',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule],
  templateUrl: './masters.component.html',
  styleUrl: './masters.component.scss'
})
export class MastersComponent {
  readonly segments = MASTER_OVERVIEW_SEGMENTS;

  labelKey(segment: MasterLookupSegment): string {
    return SEGMENT_LABEL_KEYS[segment];
  }
}
