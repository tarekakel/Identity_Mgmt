namespace AmlScreening.Application.DTOs.Dashboard;

public class CustomerDashboardKpisDto
{
    public int TotalCustomers { get; set; }
    public int AutoApproved { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int PendingMaker { get; set; }
    public int PendingChecker { get; set; }
    public int PendingScheduler { get; set; }
}
