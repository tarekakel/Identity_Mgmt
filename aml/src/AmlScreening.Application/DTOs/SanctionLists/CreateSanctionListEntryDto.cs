namespace AmlScreening.Application.DTOs.SanctionLists;

public class CreateSanctionListEntryDto
{
    public string ListSource { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? EntryType { get; set; }

    public string? FirstName { get; set; }
    public string? SecondName { get; set; }
    public string? Gender { get; set; }
    public string? Designation { get; set; }
    public string? Comments { get; set; }
    public string? Aliases { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressCountry { get; set; }
    public string? AddressNote { get; set; }
    public string? PlaceOfBirthCountry { get; set; }
    public string? FullNameArabic { get; set; }
    public string? FamilyNameArabic { get; set; }
    public string? FamilyNameLatin { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? OtherInformation { get; set; }
    public string? TypeDetail { get; set; }
}
