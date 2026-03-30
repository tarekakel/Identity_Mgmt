/** URL segment and API path under /api/MasterLookups/ */
export const MASTER_LOOKUP_SEGMENTS = [
  'countries',
  'nationalities',
  'genders',
  'customer-types',
  'customer-statuses',
  'document-types',
  'occupations',
  'source-of-funds',
  'emirates',
  'residence-statuses'
] as const;

export type MasterLookupSegment = (typeof MASTER_LOOKUP_SEGMENTS)[number];

export function isMasterLookupSegment(s: string): s is MasterLookupSegment {
  return (MASTER_LOOKUP_SEGMENTS as readonly string[]).includes(s);
}
