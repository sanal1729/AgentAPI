// <copyright file="PagedList.cs" company="Agent">
// © Agent 2025
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class PagedList<T>
{
    public int CurrentPage { get; }

    public int TotalPages { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public List<T> Items { get; }

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items ?? new List<T>();
    }

    // Static factory method to create a PagedList from an IQueryable source
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();  // Count the total items in the source
        var items = await source
            .Skip((pageNumber - 1) * pageSize) // Skip the appropriate number of items
            .Take(pageSize) // Take the number of items specified by pageSize
            .ToListAsync();  // Convert to list asynchronously

        return new PagedList<T>(items, count, pageNumber, pageSize);  // Return the PagedList
    }
}