namespace Maplay.Common;

/// <summary>分頁查詢參數</summary>
/// <remarks>用於清單端點的強制分頁，pageSize 上限為 100</remarks>
public class PageQuery
{
    private const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>頁碼 (預設 1，最小值 1)</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>每頁筆數 (預設 20，範圍 1-100)</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 20,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>跳過筆數 (用於資料庫查詢)</summary>
    public int Skip => (Page - 1) * PageSize;
}

/// <summary>分頁結果包裝器</summary>
/// <typeparam name="T">資料類型</typeparam>
public class PagedResult<T>
{
    /// <summary>資料項目列表</summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    /// <summary>當前頁碼</summary>
    public int Page { get; init; }
    /// <summary>每頁筆數</summary>
    public int PageSize { get; init; }
    /// <summary>總筆數</summary>
    public long Total { get; init; }
    /// <summary>總頁數</summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

    /// <summary>建立分頁結果</summary>
    /// <param name="items">資料項目列表</param>
    /// <param name="total">總筆數</param>
    /// <param name="q">分頁查詢參數</param>
    /// <returns>分頁結果</returns>
    public static PagedResult<T> Create(IReadOnlyList<T> items, long total, PageQuery q) =>
        new() { Items = items, Total = total, Page = q.Page, PageSize = q.PageSize };
}
