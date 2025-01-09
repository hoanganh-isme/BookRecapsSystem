using BusinessObject.Models;
using BusinessObject.ViewModels.Categories;
using BusinessObject.ViewModels.Contents;
using BusinessObject.ViewModels.Recaps;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IRecapService
    {
        Task<ApiResponse<Recap>> CreateRecap(CreateRecapRequest recap);
        Task<ApiResponse<Recap>> UpdateRecap(UpdateRecapForPublished request);
        Task<ApiResponse<bool>> DeleteRecap(Guid recapId);
        Task<ApiResponse<bool>> SoftDeleteRecap(Guid recapId);
        Task<ApiResponse<List<Recap>>> GetAllRecap();
        Task<ApiResponse<List<Recap>>> GetAllRecapByContributorId(Guid userId, string published);
        Task<RecapResponse<Recap, object>> GetRecapById(Guid recapId, Guid? userId = null);
        Task<ApiResponse<Recap>> ChooseVersionForRecap(Guid recapId, ChooseVersion version);
        
    }
}
