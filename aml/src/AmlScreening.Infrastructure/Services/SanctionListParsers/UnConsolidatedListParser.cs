using System.Globalization;
using System.Xml.Linq;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Entities.SanctionList;

namespace AmlScreening.Infrastructure.Services.SanctionListParsers;

public class UnConsolidatedListParser : IUnConsolidatedListParser
{
    public IReadOnlyList<SanctionListEntry> Parse(Stream xmlStream, string listSourceName)
    {
        var doc = XDocument.Load(xmlStream);
        var root = doc.Root;
        if (root == null)
            return Array.Empty<SanctionListEntry>();

        var list = new List<SanctionListEntry>();

        var individuals = root.Element("INDIVIDUALS")?.Elements("INDIVIDUAL") ?? Enumerable.Empty<XElement>();
        foreach (var ind in individuals)
        {
            var entry = ParseIndividual(ind, listSourceName);
            if (entry != null)
                list.Add(entry);
        }

        var entities = root.Element("ENTITIES")?.Elements("ENTITY") ?? Enumerable.Empty<XElement>();
        foreach (var ent in entities)
        {
            var entry = ParseEntity(ent, listSourceName);
            if (entry != null)
                list.Add(entry);
        }

        return list;
    }

    private static SanctionListEntry? ParseIndividual(XElement ind, string listSourceName)
    {
        var firstName = ind.Element("FIRST_NAME")?.Value?.Trim() ?? string.Empty;
        var secondName = ind.Element("SECOND_NAME")?.Value?.Trim() ?? string.Empty;
        var fullName = $"{firstName} {secondName}".Trim();
        if (string.IsNullOrEmpty(fullName))
            return null;

        var referenceNumber = ind.Element("REFERENCE_NUMBER")?.Value?.Trim()
            ?? ind.Element("DATAID")?.Value?.Trim();
        var dataId = ind.Element("DATAID")?.Value?.Trim();
        var versionNum = ind.Element("VERSIONNUM")?.Value?.Trim();
        var unListType = ind.Element("UN_LIST_TYPE")?.Value?.Trim();
        var listType = ind.Element("LIST_TYPE")?.Element("VALUE")?.Value?.Trim();
        var gender = ind.Element("GENDER")?.Value?.Trim();
        var comments1 = ind.Element("COMMENTS1")?.Value?.Trim();
        var sortKey = ind.Element("SORT_KEY")?.Value?.Trim();
        DateTime? listedOn = ParseDate(ind.Element("LISTED_ON")?.Value?.Trim());

        var nationalities = CollectValues(ind.Element("NATIONALITY"));
        var designations = CollectValues(ind.Element("DESIGNATION"));
        var lastDayUpdates = CollectValues(ind.Element("LAST_DAY_UPDATED"))
            .Select(ParseDate)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToList();

        var aliasItems = ind.Elements("INDIVIDUAL_ALIAS")
            .Select(a => new SanctionAlias
            {
                Name = a.Element("ALIAS_NAME")?.Value?.Trim() ?? string.Empty,
                Quality = NullIfEmpty(a.Element("QUALITY")?.Value?.Trim())
            })
            .Where(a => !string.IsNullOrEmpty(a.Name))
            .ToList();

        var addresses = ind.Elements("INDIVIDUAL_ADDRESS")
            .Select(a => new SanctionAddress
            {
                Street = NullIfEmpty(a.Element("STREET")?.Value?.Trim()),
                City = NullIfEmpty(a.Element("CITY")?.Value?.Trim()),
                StateProvince = NullIfEmpty(a.Element("STATE_PROVINCE")?.Value?.Trim()),
                Country = NullIfEmpty(a.Element("COUNTRY")?.Value?.Trim()),
                Note = NullIfEmpty(a.Element("NOTE")?.Value?.Trim())
            })
            .Where(a => a.Street != null || a.City != null || a.StateProvince != null || a.Country != null || a.Note != null)
            .ToList();

        var placesOfBirth = ind.Elements("INDIVIDUAL_PLACE_OF_BIRTH")
            .Select(p => new SanctionPlaceOfBirth
            {
                City = NullIfEmpty(p.Element("CITY")?.Value?.Trim()),
                StateProvince = NullIfEmpty(p.Element("STATE_PROVINCE")?.Value?.Trim()),
                Country = NullIfEmpty(p.Element("COUNTRY")?.Value?.Trim())
            })
            .Where(p => p.City != null || p.StateProvince != null || p.Country != null)
            .ToList();

        var datesOfBirth = ind.Elements("INDIVIDUAL_DATE_OF_BIRTH")
            .Select(ParseDob)
            .Where(d => d.Date.HasValue || d.Year.HasValue || d.FromYear.HasValue || d.ToYear.HasValue || !string.IsNullOrEmpty(d.Note))
            .ToList();

        var documents = ind.Elements("INDIVIDUAL_DOCUMENT")
            .Select(d => new SanctionDocument
            {
                Type = NullIfEmpty(d.Element("TYPE_OF_DOCUMENT")?.Value?.Trim()),
                Type2 = NullIfEmpty(d.Element("TYPE_OF_DOCUMENT2")?.Value?.Trim()),
                Number = NullIfEmpty(d.Element("NUMBER")?.Value?.Trim()),
                IssuingCountry = NullIfEmpty(d.Element("ISSUING_COUNTRY")?.Value?.Trim()),
                DateOfIssue = ParseDate(d.Element("DATE_OF_ISSUE")?.Value?.Trim()),
                Note = NullIfEmpty(d.Element("NOTE")?.Value?.Trim())
            })
            .Where(d => d.Type != null || d.Number != null || d.IssuingCountry != null)
            .ToList();

        // Primary scalar fields = collection[0] for backward-compat with existing UI/admin screens.
        var primaryAddress = addresses.FirstOrDefault();
        var primaryPlaceOfBirth = placesOfBirth.FirstOrDefault();
        var primaryDob = datesOfBirth.FirstOrDefault();
        var primaryDocument = documents.FirstOrDefault();
        var primaryNationality = nationalities.FirstOrDefault();
        var primaryDesignation = designations.FirstOrDefault();
        var primaryLastDay = lastDayUpdates.Count > 0 ? lastDayUpdates[0] : (DateTime?)null;

        DateTime? dob = primaryDob?.Date
            ?? (primaryDob?.Year is int y && y >= 1900 && y <= 2100 ? new DateTime(y, 1, 1) : (DateTime?)null);

        var aliasesStr = aliasItems.Count > 0 ? string.Join("; ", aliasItems.Select(a => a.Name)) : null;

        return new SanctionListEntry
        {
            Id = Guid.NewGuid(),
            ListSource = listSourceName,
            FullName = Truncate(fullName, 512)!,
            FirstName = Truncate(firstName, 256),
            SecondName = Truncate(secondName, 256),
            Nationality = Truncate(primaryNationality, 128),
            DateOfBirth = dob,
            ReferenceNumber = Truncate(referenceNumber, 128),
            EntryType = "Individual",
            DataId = Truncate(dataId, 64),
            VersionNum = Truncate(versionNum, 16),
            UnListType = Truncate(unListType, 64),
            ListType = Truncate(listType, 64),
            ListedOn = listedOn,
            LastDayUpdated = primaryLastDay,
            Gender = Truncate(gender, 32),
            Designation = Truncate(primaryDesignation, 256),
            Comments = Truncate(comments1, 2000),
            Aliases = Truncate(aliasesStr, 1000),
            AddressCity = Truncate(primaryAddress?.City, 128),
            AddressCountry = Truncate(primaryAddress?.Country, 128),
            AddressNote = Truncate(primaryAddress?.Note, 512),
            PlaceOfBirthCountry = Truncate(primaryPlaceOfBirth?.Country, 128),
            SortKey = Truncate(sortKey, 64),
            DocumentNumber = Truncate(primaryDocument?.Number, 128),
            IssuingAuthority = Truncate(primaryDocument?.IssuingCountry, 256),
            IssueDate = primaryDocument?.DateOfIssue,
            AliasItems = aliasItems,
            DatesOfBirth = datesOfBirth,
            Addresses = addresses,
            PlacesOfBirth = placesOfBirth,
            Documents = documents,
            Nationalities = nationalities,
            Designations = designations,
            LastDayUpdates = lastDayUpdates
        };
    }

    private static SanctionListEntry? ParseEntity(XElement ent, string listSourceName)
    {
        var firstName = ent.Element("FIRST_NAME")?.Value?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(firstName))
            return null;

        var referenceNumber = ent.Element("REFERENCE_NUMBER")?.Value?.Trim()
            ?? ent.Element("DATAID")?.Value?.Trim();
        var dataId = ent.Element("DATAID")?.Value?.Trim();
        var versionNum = ent.Element("VERSIONNUM")?.Value?.Trim();
        var unListType = ent.Element("UN_LIST_TYPE")?.Value?.Trim();
        var listType = ent.Element("LIST_TYPE")?.Element("VALUE")?.Value?.Trim();
        var comments1 = ent.Element("COMMENTS1")?.Value?.Trim();
        var sortKey = ent.Element("SORT_KEY")?.Value?.Trim();
        DateTime? listedOn = ParseDate(ent.Element("LISTED_ON")?.Value?.Trim());

        var designations = CollectValues(ent.Element("DESIGNATION"));
        var lastDayUpdates = CollectValues(ent.Element("LAST_DAY_UPDATED"))
            .Select(ParseDate)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToList();

        var aliasItems = ent.Elements("ENTITY_ALIAS")
            .Select(a => new SanctionAlias
            {
                Name = a.Element("ALIAS_NAME")?.Value?.Trim() ?? string.Empty,
                Quality = NullIfEmpty(a.Element("QUALITY")?.Value?.Trim())
            })
            .Where(a => !string.IsNullOrEmpty(a.Name))
            .ToList();

        var addresses = ent.Elements("ENTITY_ADDRESS")
            .Select(a => new SanctionAddress
            {
                Street = NullIfEmpty(a.Element("STREET")?.Value?.Trim()),
                City = NullIfEmpty(a.Element("CITY")?.Value?.Trim()),
                StateProvince = NullIfEmpty(a.Element("STATE_PROVINCE")?.Value?.Trim()),
                Country = NullIfEmpty(a.Element("COUNTRY")?.Value?.Trim()),
                Note = NullIfEmpty(a.Element("NOTE")?.Value?.Trim())
            })
            .Where(a => a.Street != null || a.City != null || a.StateProvince != null || a.Country != null || a.Note != null)
            .ToList();

        var primaryAddress = addresses.FirstOrDefault();
        var primaryDesignation = designations.FirstOrDefault();
        var primaryLastDay = lastDayUpdates.Count > 0 ? lastDayUpdates[0] : (DateTime?)null;
        var aliasesStr = aliasItems.Count > 0 ? string.Join("; ", aliasItems.Select(a => a.Name)) : null;

        return new SanctionListEntry
        {
            Id = Guid.NewGuid(),
            ListSource = listSourceName,
            FullName = Truncate(firstName, 512)!,
            FirstName = Truncate(firstName, 256),
            ReferenceNumber = Truncate(referenceNumber, 128),
            EntryType = "Entity",
            DataId = Truncate(dataId, 64),
            VersionNum = Truncate(versionNum, 16),
            UnListType = Truncate(unListType, 64),
            ListType = Truncate(listType, 64),
            ListedOn = listedOn,
            LastDayUpdated = primaryLastDay,
            Designation = Truncate(primaryDesignation, 256),
            Comments = Truncate(comments1, 2000),
            Aliases = Truncate(aliasesStr, 1000),
            AddressCity = Truncate(primaryAddress?.City, 128),
            AddressCountry = Truncate(primaryAddress?.Country, 128),
            AddressNote = Truncate(primaryAddress?.Note, 512),
            SortKey = Truncate(sortKey, 64),
            AliasItems = aliasItems,
            Addresses = addresses,
            Designations = designations,
            LastDayUpdates = lastDayUpdates
        };
    }

    private static SanctionDob ParseDob(XElement dobEl)
    {
        var typeOfDate = NullIfEmpty(dobEl.Element("TYPE_OF_DATE")?.Value?.Trim());
        var note = NullIfEmpty(dobEl.Element("NOTE")?.Value?.Trim());

        DateTime? date = ParseDate(dobEl.Element("DATE")?.Value?.Trim());

        int? year = null;
        var yearStr = dobEl.Element("YEAR")?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) && y >= 1900 && y <= 2100)
            year = y;

        int? fromYear = null;
        var fromYearStr = dobEl.Element("FROM_YEAR")?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(fromYearStr) && int.TryParse(fromYearStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fy) && fy >= 1900 && fy <= 2100)
            fromYear = fy;

        int? toYear = null;
        var toYearStr = dobEl.Element("TO_YEAR")?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(toYearStr) && int.TryParse(toYearStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ty) && ty >= 1900 && ty <= 2100)
            toYear = ty;

        return new SanctionDob
        {
            Date = date,
            Year = year ?? date?.Year,
            FromYear = fromYear,
            ToYear = toYear,
            TypeOfDate = typeOfDate,
            Note = note
        };
    }

    private static List<string> CollectValues(XElement? container)
    {
        if (container == null) return new List<string>();
        return container.Elements("VALUE")
            .Select(v => v.Value?.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(v => v!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DateTime? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        return null;
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string? Truncate(string? value, int maxLen)
    {
        if (value == null) return null;
        return value.Length <= maxLen ? value : value[..maxLen];
    }
}
