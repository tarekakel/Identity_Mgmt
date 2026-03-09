namespace AmlScreening.Domain.Entities;

public class SanctionListEntry
{
    public Guid Id { get; set; }
    public string ListSource { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? EntryType { get; set; }

    // UN XML: DATAID, VERSIONNUM, FIRST_NAME, SECOND_NAME
    public string? DataId { get; set; }
    public string? VersionNum { get; set; }
    public string? FirstName { get; set; }
    public string? SecondName { get; set; }

    // UN: UN_LIST_TYPE, LIST_TYPE, LISTED_ON, LAST_DAY_UPDATED
    public string? UnListType { get; set; }
    public string? ListType { get; set; }
    public DateTime? ListedOn { get; set; }
    public DateTime? LastDayUpdated { get; set; }

    // UN: GENDER, DESIGNATION, COMMENTS1
    public string? Gender { get; set; }
    public string? Designation { get; set; }
    public string? Comments { get; set; }

    // UN: INDIVIDUAL_ALIAS (comma-separated ALIAS_NAME)
    public string? Aliases { get; set; }

    // UN: INDIVIDUAL_ADDRESS
    public string? AddressCity { get; set; }
    public string? AddressCountry { get; set; }
    public string? AddressNote { get; set; }

    // UN: INDIVIDUAL_PLACE_OF_BIRTH, SORT_KEY
    public string? PlaceOfBirthCountry { get; set; }
    public string? SortKey { get; set; }

    // UAE / additional: Arabic & Latin name breakdown, document, dates, other info
    public string? FullNameArabic { get; set; }
    public string? FamilyNameArabic { get; set; }
    public string? FamilyNameLatin { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? OtherInformation { get; set; }
    /// <summary>e.g. Citizen, Individual, Entity (UAE النوع).</summary>
    public string? TypeDetail { get; set; }
}
