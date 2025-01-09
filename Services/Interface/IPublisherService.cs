using BusinessObject.Models;
using BusinessObject.ViewModels.Publisher;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IPublisherService
    {
        public Task<ApiResponse<Publisher>> CreatePubliser(Guid userId);
        public Task<ApiResponse<Publisher>> GetPublisherByUserId(Guid userId);
        public Task<ApiResponse<Publisher>> GetPublisherById(Guid publisherId);
        public Task<ApiResponse<List<Publisher>>> GetAllPublisher();
        public Task<ApiResponse<Publisher>> UpdatePublisherInfo(Guid publisherId, UpdatePublisher publisher);
        
    }
}
