using BusinessObject.Models;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.PublisherPayouts.PublisherPayoutDTO;

namespace Services.Interface
{
    public interface IPublisherPayoutService
    {
        Task<List<PublisherDto>> GetAllPublishersWithPayoutsAsync();
        Task<ApiResponse<PublisherPayoutDto>> CreatePublisherPayoutAsync(Guid publisherId, Stream ImageStream, string ImageContentType, string description, DateTime toDate);
        Task<ApiResponse<PublisherPayout>> GetPayoutById(Guid id);
        Task<ApiResponse<List<PublisherHistoryDto>>> GetPayoutByPublisherId(Guid publisherId);
    }
}
