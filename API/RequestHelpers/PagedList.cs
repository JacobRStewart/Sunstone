using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace Sunstone;

public class PagedList<T> : List<T>
{

    
    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        MetaData = new MetaData
        {
            TotalCount = count,
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize)
        };
        AddRange(items);
    }

    public MetaData MetaData { get; set; }

    public static async Task<PagedList<T>> ToPagedList(IQueryable<T> query, 
        int pageNumber, int pageSize)
    {
        var count = await query.CountAsync();
        var items = await query.Skip(pageSize*(pageNumber-1)).Take(pageSize).ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
