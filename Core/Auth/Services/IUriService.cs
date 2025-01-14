using Core.Models;

namespace Core.Auth.Services
{
    public interface IUriService
    {
        public Uri GetPageUri(PaginationFilter filter, string route);
        Uri GetPageUri(BookPaginationFilter filter, string route);

    }
}