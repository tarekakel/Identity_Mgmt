using System.Xml.Linq;
using AmlScreening.Domain.Entities;

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

        var nationality = ind.Element("NATIONALITY")?.Element("VALUE")?.Value?.Trim();
        var referenceNumber = ind.Element("REFERENCE_NUMBER")?.Element("VALUE")?.Value?.Trim()
            ?? ind.Element("DATAID")?.Value?.Trim();
        var dataId = ind.Element("DATAID")?.Value?.Trim();
        var versionNum = ind.Element("VERSIONNUM")?.Value?.Trim();
        var unListType = ind.Element("UN_LIST_TYPE")?.Value?.Trim();
        var listType = ind.Element("LIST_TYPE")?.Element("VALUE")?.Value?.Trim();
        var gender = ind.Element("GENDER")?.Value?.Trim();
        var designation = ind.Element("DESIGNATION")?.Element("VALUE")?.Value?.Trim();
        var comments1 = ind.Element("COMMENTS1")?.Value?.Trim();

        DateTime? listedOn = ParseDate(ind.Element("LISTED_ON")?.Value?.Trim());
        DateTime? lastDayUpdated = null;
        var lastDayEl = ind.Element("LAST_DAY_UPDATED")?.Element("VALUE")?.Value?.Trim();
        if (!string.IsNullOrEmpty(lastDayEl)) lastDayUpdated = ParseDate(lastDayEl);

        var aliases = ind.Elements("INDIVIDUAL_ALIAS")
            .Select(a => a.Element("ALIAS_NAME")?.Value?.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
        var aliasesStr = aliases.Count > 0 ? string.Join("; ", aliases) : null;

        var address = ind.Element("INDIVIDUAL_ADDRESS");
        var addressCity = address?.Element("CITY")?.Value?.Trim();
        var addressCountry = address?.Element("COUNTRY")?.Value?.Trim();
        var addressNote = address?.Element("NOTE")?.Value?.Trim();

        var placeOfBirth = ind.Element("INDIVIDUAL_PLACE_OF_BIRTH");
        var placeOfBirthCountry = placeOfBirth?.Element("COUNTRY")?.Value?.Trim();

        DateTime? dob = null;
        var dobEl = ind.Element("INDIVIDUAL_DATE_OF_BIRTH");
        if (dobEl != null)
        {
            var yearStr = dobEl.Element("YEAR")?.Value;
            if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr.Trim(), out var year) && year >= 1900 && year <= 2100)
                dob = new DateTime(year, 1, 1);
        }

        var sortKey = ind.Element("SORT_KEY")?.Value?.Trim();

        return new SanctionListEntry
        {
            Id = Guid.NewGuid(),
            ListSource = listSourceName,
            FullName = Truncate(fullName, 512),
            FirstName = Truncate(firstName, 256),
            SecondName = Truncate(secondName, 256),
            Nationality = Truncate(nationality, 128),
            DateOfBirth = dob,
            ReferenceNumber = Truncate(referenceNumber, 128),
            EntryType = "Individual",
            DataId = Truncate(dataId, 64),
            VersionNum = Truncate(versionNum, 16),
            UnListType = Truncate(unListType, 64),
            ListType = Truncate(listType, 64),
            ListedOn = listedOn,
            LastDayUpdated = lastDayUpdated,
            Gender = Truncate(gender, 32),
            Designation = Truncate(designation, 256),
            Comments = Truncate(comments1, 2000),
            Aliases = Truncate(aliasesStr, 1000),
            AddressCity = Truncate(addressCity, 128),
            AddressCountry = Truncate(addressCountry, 128),
            AddressNote = Truncate(addressNote, 512),
            PlaceOfBirthCountry = Truncate(placeOfBirthCountry, 128),
            SortKey = Truncate(sortKey, 64)
        };
    }

    private static SanctionListEntry? ParseEntity(XElement ent, string listSourceName)
    {
        var firstName = ent.Element("FIRST_NAME")?.Value?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(firstName))
            return null;

        var referenceNumber = ent.Element("REFERENCE_NUMBER")?.Element("VALUE")?.Value?.Trim()
            ?? ent.Element("DATAID")?.Value?.Trim();
        var dataId = ent.Element("DATAID")?.Value?.Trim();
        var versionNum = ent.Element("VERSIONNUM")?.Value?.Trim();
        var unListType = ent.Element("UN_LIST_TYPE")?.Value?.Trim();
        var listType = ent.Element("LIST_TYPE")?.Element("VALUE")?.Value?.Trim();
        var designation = ent.Element("DESIGNATION")?.Element("VALUE")?.Value?.Trim();
        var comments1 = ent.Element("COMMENTS1")?.Value?.Trim();
        DateTime? listedOn = ParseDate(ent.Element("LISTED_ON")?.Value?.Trim());
        DateTime? lastDayUpdated = null;
        var lastDayEl = ent.Element("LAST_DAY_UPDATED")?.Element("VALUE")?.Value?.Trim();
        if (!string.IsNullOrEmpty(lastDayEl)) lastDayUpdated = ParseDate(lastDayEl);
        var address = ent.Element("ENTITY_ADDRESS");
        var addressCity = address?.Element("CITY")?.Value?.Trim();
        var addressCountry = address?.Element("COUNTRY")?.Value?.Trim();
        var addressNote = address?.Element("NOTE")?.Value?.Trim();
        var sortKey = ent.Element("SORT_KEY")?.Value?.Trim();

        return new SanctionListEntry
        {
            Id = Guid.NewGuid(),
            ListSource = listSourceName,
            FullName = Truncate(firstName, 512),
            FirstName = Truncate(firstName, 256),
            ReferenceNumber = Truncate(referenceNumber, 128),
            EntryType = "Entity",
            DataId = Truncate(dataId, 64),
            VersionNum = Truncate(versionNum, 16),
            UnListType = Truncate(unListType, 64),
            ListType = Truncate(listType, 64),
            ListedOn = listedOn,
            LastDayUpdated = lastDayUpdated,
            Designation = Truncate(designation, 256),
            Comments = Truncate(comments1, 2000),
            AddressCity = Truncate(addressCity, 128),
            AddressCountry = Truncate(addressCountry, 128),
            AddressNote = Truncate(addressNote, 512),
            SortKey = Truncate(sortKey, 64)
        };
    }

    private static DateTime? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
            return d;
        return null;
    }

    private static string? Truncate(string? value, int maxLen)
    {
        if (value == null) return null;
        return value.Length <= maxLen ? value : value[..maxLen];
    }
}
