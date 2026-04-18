using AmlScreening.Domain.Entities.SanctionList;

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

    /// <summary>Legacy semicolon-joined alias names. New uploads also populate <see cref="AliasItems"/>.</summary>
    public string? Aliases { get; set; }

    // UN: INDIVIDUAL_ADDRESS — primary (collection[0]) for backward-compat
    public string? AddressCity { get; set; }
    public string? AddressCountry { get; set; }
    public string? AddressNote { get; set; }

    // UN: INDIVIDUAL_PLACE_OF_BIRTH — primary
    public string? PlaceOfBirthCountry { get; set; }
    public string? SortKey { get; set; }

    // UAE / additional: Arabic & Latin name breakdown, primary document, dates, other info
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

    // ----- Multi-valued data (persisted as JSON columns) -----

    /// <summary>All alias names with their source-list quality.</summary>
    public List<SanctionAlias> AliasItems { get; set; } = new();

    /// <summary>All declared dates of birth (full date, year-only, or BETWEEN ranges).</summary>
    public List<SanctionDob> DatesOfBirth { get; set; } = new();

    /// <summary>All addresses on record.</summary>
    public List<SanctionAddress> Addresses { get; set; } = new();

    /// <summary>All declared places of birth.</summary>
    public List<SanctionPlaceOfBirth> PlacesOfBirth { get; set; } = new();

    /// <summary>All identity documents on record (stored only, not used for scoring).</summary>
    public List<SanctionDocument> Documents { get; set; } = new();

    /// <summary>All declared nationalities (supports dual citizenship).</summary>
    public List<string> Nationalities { get; set; } = new();

    /// <summary>All designations / titles.</summary>
    public List<string> Designations { get; set; } = new();

    /// <summary>All <c>LAST_DAY_UPDATED</c> values from UN XML.</summary>
    public List<DateTime> LastDayUpdates { get; set; } = new();
}
