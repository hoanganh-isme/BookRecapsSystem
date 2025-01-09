using BusinessObject.ViewModels.Dashboards;
using BusinessObject.ViewModels.PublisherPayouts;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.PublisherPayouts.PublisherPayoutDTO;

namespace Services.Interface
{
    public interface IBookEarningService
    {
        Task<ApiResponse<PublisherEarningsDto>> GetPublisherEarningsAsync(Guid publisherId, DateTime toDate);
        Task<ApiResponse<BookDetailDto>> GetBookDetailAsync(Guid bookId, DateTime fromDate, DateTime toDate);
        Task<ApiResponse<BookAdminDetailDto>> GetBookDetailForAdminAsync(Guid bookId, DateTime fromDate, DateTime toDate);
    }
}
