namespace AmlScreening.Application.Common;

public class PagedRequest
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }

    public int GetPageSize() => Math.Min(Math.Max(PageSize, 1), MaxPageSize);
    public int GetPageNumber() => Math.Max(PageNumber, 1);
}
