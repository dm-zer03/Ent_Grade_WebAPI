namespace EcomAPI.Common.Pagination;

public class ProductQueryRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string SortBy { get; set; } = "id";

    public bool SortDescending { get; set; }
}