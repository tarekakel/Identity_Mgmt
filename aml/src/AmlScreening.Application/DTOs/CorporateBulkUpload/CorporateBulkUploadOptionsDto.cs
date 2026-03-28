namespace AmlScreening.Application.DTOs.CorporateBulkUpload;

public class CorporateBulkUploadOptionsDto
{
    public int MatchThreshold { get; set; } = 85;
    public bool CheckPepUkOnly { get; set; }
    public bool CheckDisqualifiedDirectorUkOnly { get; set; }
    public bool CheckSanctions { get; set; }
    public bool CheckProfileOfInterest { get; set; }
    public bool CheckReputationalRiskExposure { get; set; }
    public bool CheckRegulatoryEnforcementList { get; set; }
    public bool CheckInsolvencyUkIreland { get; set; }
}
