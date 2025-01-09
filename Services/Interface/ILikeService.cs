using BusinessObject.Models;
using BusinessObject.ViewModels.Likes;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ILikeService
    {
        Task<ApiResponse<int>> GetLikesCountByRecapIdAsync(Guid recapId);
        Task<ApiResponse<bool>> AddLikeAsync(Guid recapId, Guid userId);
        Task<ApiResponse<bool>> RemoveLikeAsync(Guid recapId, Guid userId);
    }
}
