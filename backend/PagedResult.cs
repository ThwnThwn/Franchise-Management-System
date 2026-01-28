using System;

public class PagedResult<T>
{
    public List<T> Items { get; set; }     
    public int PageIndex { get; set; } 
    public int PageSize { get; set; }       
    public long TotalCount { get; set; }
    public int TotalPages { get; set; }     

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public PagedResult(List<T> items, long totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;

        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}