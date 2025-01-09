using Core.Auth.Services;
using Core.Models;

namespace Core.Helpers
{
    public class PaginationHelper
    {
        public static PagedResponse<List<T>> CreatePagedResponse<T>(
     List<T> pagedData,
     PaginationFilter validFilter,
     int totalRecords,
     IUriService uriService,
     string route)
        {
            var response = new PagedResponse<List<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize);
            var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

            // Build the query parameters for pagination and filtering
            var baseFilter = new PaginationFilter(
                validFilter.PageNumber,
                validFilter.PageSize,
                validFilter.SortBy,
                validFilter.SortOrder,
                validFilter.SearchTerm,
                validFilter.FilterBy,
                validFilter.FilterValue
            );

            response.NextPage =
                validFilter.PageNumber >= 1 && validFilter.PageNumber < roundedTotalPages
                ? uriService.GetPageUri(baseFilter, route)
                : null;

            response.PreviousPage =
                validFilter.PageNumber - 1 >= 1 && validFilter.PageNumber <= roundedTotalPages
                ? uriService.GetPageUri(baseFilter, route)
                : null;

            response.CurrentPage = uriService.GetPageUri(baseFilter, route);
            response.FirstPage = uriService.GetPageUri(new PaginationFilter(1, validFilter.PageSize), route);
            response.LastPage = uriService.GetPageUri(new PaginationFilter(roundedTotalPages, validFilter.PageSize), route);
            response.TotalPages = roundedTotalPages;
            response.TotalRecords = totalRecords;

            return response;
        }
        public static PagedResponse<List<T>> CreateBookPagedResponse<T>(
                                                                        List<T> pagedData,
                                                                        BookPaginationFilter validFilter,
                                                                        int totalRecords,
                                                                        IUriService uriService,
                                                                        string route)
        {
            var response = new PagedResponse<List<T>>(pagedData, validFilter.PageNumber, validFilter.PageSize);
            var totalPages = ((double)totalRecords / (double)validFilter.PageSize);
            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

            // Build the query parameters for pagination and filtering
            var baseFilter = new BookPaginationFilter(
                validFilter.PageNumber,
                validFilter.PageSize,
                validFilter.SortBy,
                validFilter.SortOrder,
                validFilter.SearchTerm,
                validFilter.FilterBy,
                validFilter.FilterValue,
                validFilter.CategoryId,
                validFilter.PublisherId,
                validFilter.ContributorRevenueShareMin
            );

            response.NextPage =
                validFilter.PageNumber >= 1 && validFilter.PageNumber < roundedTotalPages
                ? uriService.GetPageUri(baseFilter, route)
                : null;

            response.PreviousPage =
                validFilter.PageNumber - 1 >= 1 && validFilter.PageNumber <= roundedTotalPages
                ? uriService.GetPageUri(baseFilter, route)
                : null;

            response.CurrentPage = uriService.GetPageUri(baseFilter, route);
            response.FirstPage = uriService.GetPageUri(new BookPaginationFilter(1, validFilter.PageSize), route);
            response.LastPage = uriService.GetPageUri(new BookPaginationFilter(roundedTotalPages, validFilter.PageSize), route);
            response.TotalPages = roundedTotalPages;
            response.TotalRecords = totalRecords;

            return response;
        }

    }

}