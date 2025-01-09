using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Contents;
using BusinessObject.ViewModels.Recaps;
using Microsoft.AspNetCore.Http;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IRecapVersionService
    {
        Task<ApiResponse<RecapVersion>> CreatePrepareRecapVersion(CreatePrepareVersion version);
        Task<ApiResponse<TranscriptStatus>> GenerateRecapVersionAsync(UpdateRecapVersion request);
        Task<ApiResponse<RecapVersion>> CreateRecapVersionAsync(CreateRecapVersion request);
        public Task<ApiResponse<TranscriptStatus>> UploadAudioByContributorAsync(Guid recapVersionId, IFormFile audioFile);
        Task<ApiResponse<RecapVersion>> GetVersionById(Guid versionId);
        Task<ApiResponse<List<RecapVersion>>> GetAllRecapVersions(Guid userId);
        Task<ApiResponse<RecapVersion>> ChangeVersionStatus(ChangeVersionStatus request);
        Task<ApiResponse<bool>> DeleteRecapVersion(Guid versionId);
        Task<ApiResponse<bool>> SoftDeleteRecapVersion(Guid versionId);
        Task<ApiResponse<List<RecapVersion>>> GetVersionApprovedByRecapId(Guid recapId);
        Task<ApiResponse<List<ListRecapNotDraft>>> GetListRecapVersionNotDraft();
    }
}
