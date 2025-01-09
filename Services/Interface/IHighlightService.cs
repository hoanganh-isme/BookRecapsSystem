using BusinessObject.Models;
using BusinessObject.ViewModels.Highlight;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IHighlightService
    {
        Task<ApiResponse<Highlight>> CreateHighlight(CreateHighlightRequest highlight);
        Task<ApiResponse<Highlight>> UpdateHighlight(UpdateHighlightRequest highlight, Guid staffId);
        Task<ApiResponse<bool>> DeleteHighlight(Guid HighlightId, Guid userId);
        Task<ApiResponse<bool>> SoftDeleteHighlight(Guid HighlightId, Guid userId);
        Task<ApiResponse<Highlight>> GetHighlightById(Guid HighlightId);
        Task<ApiResponse<List<Highlight>>> GetHighlightByRecapId(Guid recapId, Guid userId);
        Task<ApiResponse<List<Highlight>>> GetAllHighlights();
    }
}
