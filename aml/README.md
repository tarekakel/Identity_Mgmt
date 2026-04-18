# AML Screening Service

ASP.NET Core 8 backend that powers individual / corporate / instant sanctions screening
backed by Elasticsearch.

## Elasticsearch prerequisite

The screening engine requires an Elasticsearch 8.x instance with two analysis plugins:

- `analysis-icu` (provides the `icu_folding` token filter used for Unicode normalisation)
- `analysis-phonetic` (provides the `phonetic` token filter using the `double_metaphone` encoder)

A custom Docker image that installs both plugins is defined in
[`docker/elasticsearch.Dockerfile`](../docker/elasticsearch.Dockerfile) and wired through
[`docker-compose.yml`](../docker-compose.yml).

### First-time setup

From the repository root:

```bash
docker compose build elasticsearch
docker compose up -d elasticsearch
```

The first command builds the custom image (this is the step most often missed when running
`docker compose up` directly with a stock image already pulled). The second starts the
container and exposes Elasticsearch on `http://localhost:9200`.

### Verify the installation

```bash
curl http://localhost:9200/_cat/plugins?v
```

The output must list both `analysis-icu` and `analysis-phonetic`. If either is missing,
stop the container, run `docker compose build elasticsearch` and start it again.

You can also confirm the cluster is healthy:

```bash
curl http://localhost:9200/_cluster/health?pretty
```

### Reindexing

The API runs a background sync on startup
([`ReindexHostedService`](src/AmlScreening.Infrastructure/Services/Search/ReindexHostedService.cs))
that performs a pre-flight ping + plugin check against Elasticsearch. If the container
is unreachable, or the required plugins are missing, you will see a clear `LogError`
message in the API output explaining how to fix it - screening searches will return empty
until the issue is resolved, but the API itself keeps running.

To force a full reindex from SQL into Elasticsearch on demand:

```bash
curl -X POST http://localhost:5000/api/SanctionLists/reindex
```

### Configuration

Connection settings live under the `Elasticsearch` section in
[`appsettings.json`](src/AmlScreening.Api/appsettings.json) /
[`appsettings.Development.json`](src/AmlScreening.Api/appsettings.Development.json):

```json
"Elasticsearch": {
  "Url": "http://localhost:9200",
  "IndexName": "sanction-entries-v2",
  "TopScoreReference": 30.0,
  "MaxCandidates": 50
}
```

## Sanction-list data fidelity (v2 schema)

`SanctionListEntry` now persists every multi-valued field the source feeds publish, as JSON
columns:

| JSON column            | Source                                                                        |
|------------------------|-------------------------------------------------------------------------------|
| `AliasesJson`          | UN `INDIVIDUAL_ALIAS` / `ENTITY_ALIAS` (with `QUALITY` = Good / Low / a.k.a.) |
| `DatesOfBirthJson`     | UN `INDIVIDUAL_DATE_OF_BIRTH` (every `DATE`, `YEAR`, and `BETWEEN` range)     |
| `AddressesJson`        | UN `INDIVIDUAL_ADDRESS` / `ENTITY_ADDRESS` (street / city / state / country)  |
| `PlacesOfBirthJson`    | UN `INDIVIDUAL_PLACE_OF_BIRTH` (every entry)                                  |
| `DocumentsJson`        | UN `INDIVIDUAL_DOCUMENT` (passport / national-ID / issuing country)           |
| `NationalitiesJson`    | UN `NATIONALITY/VALUE[]` (supports dual citizenship) + UAE col 3              |
| `DesignationsJson`     | UN `DESIGNATION/VALUE[]`                                                      |
| `LastDayUpdatesJson`   | UN `LAST_DAY_UPDATED/VALUE[]`                                                 |

The legacy scalar columns (`Nationality`, `DateOfBirth`, `PlaceOfBirthCountry`,
`AddressCity`/`Country`, `Aliases`, `Designation`, `DocumentNumber`, `IssuingAuthority`,
`IssueDate`, `LastDayUpdated`) are still populated from index 0 of each new collection so
admin screens stay backward-compatible.

### Upgrading an existing environment

1. Pull and run the new EF migration `AddSanctionListMultiValuedJson` (8 nullable
   `nvarchar(max)` columns on `SanctionListEntries`).
2. Restart the API. `ReindexHostedService` sees the bumped index name
   `sanction-entries-v2`, creates it with the new mapping (arrays for `nationalities`,
   `addressCountries`, `placeOfBirthCountries`, `birthDates`, `birthYears` plus a nested
   `birthYearRanges`), and reindexes every row from SQL. Existing rows still match
   correctly: `SanctionEntryDocument.FromEntity` falls back to the legacy scalars when the
   new collections are empty.
3. **Re-upload** the UN consolidated XML and the UAE Excel via
   `POST /api/SanctionLists/upload` to populate the multi-valued JSON columns. Without a
   re-upload, only the previously-stored single values are searched. After re-upload the
   engine matches against every alias / DOB / nationality / place of birth on record.
4. The old `sanction-entries` index is left in place (harmless) and can be deleted with
   `curl -X DELETE http://localhost:9200/sanction-entries` once you are happy with v2.

### Engine scoring (v2)

[`ElasticScreeningEngine`](src/AmlScreening.Infrastructure/Services/Search/ElasticScreeningEngine.cs)
matches against the array fields directly:

- `multi_match` over `fullName^3`, `aliasesGood^4`, `aliases^2.5`, `firstName`,
  `secondName`, `fullNameArabic` plus the `*.phonetic` variants. Aliases the source list
  flagged as high-quality dominate over weak transliterations.
- `function_score` boosts on **any-of-many**: nationality (+1.3), POB (+1.2), gender
  (+1.05), DOB exact (+1.5), DOB year window (+1.2), and nested overlap against
  `birthYearRanges` for UN `BETWEEN` entries (+1.1).
- Penalties (×0.7 / ×0.85) only fire when the entry has at least one value on a field
  AND none of them match the query - dual-citizen / multi-DOB entries are scored
  fairly without false demotion on partially-known data.
- Identity-document numbers are stored but intentionally not used for scoring.
