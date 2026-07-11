using System.Collections.Generic;

namespace EloDoacoes.ViewModels
{
    /// <summary>
    /// Generic paged result container for server-side pagination.
    /// Contains items list, total count, current page, and calculated total pages.
    /// </summary>
    /// <typeparam name="T">Type of items in the paged result</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// List of items for the current page
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Total number of items in the entire dataset (before pagination)
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-indexed)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages calculated from TotalCount / PageSize
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling((double)TotalCount / PageSize) : 0;

        /// <summary>
        /// Indicates if there is a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Indicates if there is a next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Gets the previous page number (or 1 if already on first page)
        /// </summary>
        public int PreviousPageNumber => CurrentPage > 1 ? CurrentPage - 1 : 1;

        /// <summary>
        /// Gets the next page number (or total pages if already on last page)
        /// </summary>
        public int NextPageNumber => CurrentPage < TotalPages ? CurrentPage + 1 : TotalPages;
    }
}
