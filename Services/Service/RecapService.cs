using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Recaps;
using BusinessObject.ViewModels.KeyIdea;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using BusinessObject.ViewModels.Contents;
using Google.Api;
using Services.Responses;

namespace Services.Service
{
    public class RecapService : IRecapService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApiSettings _apiSettings;
        public RecapService(IUnitOfWork unitOfWork, IMapper mapper,
            IOptions<ApiSettings> apiSettingsOptions)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _apiSettings = apiSettingsOptions.Value;
        }
        public async Task<ApiResponse<Recap>> CreateRecap(CreateRecapRequest request)
        {
            // Kiểm tra nếu Recap đã tồn tại cho cuốn sách và contributor này
            var existingRecap = await _unitOfWork.RecapRepository
                .QueryWithIncludes(c => c.RecapVersions) // Đảm bảo RecapVersions được tải về
                .FirstOrDefaultAsync(c => c.BookId == request.BookId && c.UserId == request.ContributorId);

            if (existingRecap != null)
            {
                return new ApiResponse<Recap>
                {
                    Succeeded = false,
                    Message = "Bạn đã tạo recap cho quyển sách này.",
                    Data = existingRecap
                };
            }
            else
            {
                // Nếu Recap không tồn tại, tạo mới Recap và RecapVersion
                var recap = _mapper.Map<Recap>(request);
                recap.UserId = request.ContributorId;
                recap.BookId = request.BookId;
                recap.isPublished = false; // Giá trị mặc định
                recap.isPremium = false;   // Giá trị mặc định

                var newRecapVersion = new RecapVersion
                {
                    VersionName = request.Name,
                    VersionNumber = 1.0M, // Phiên bản đầu tiên
                    Status = RecapStatus.Draft,
                    RecapId = recap.Id
                };

                // Thêm RecapVersion vào Recap
                recap.RecapVersions = new List<RecapVersion> { newRecapVersion };

                // Thêm Recap vào cơ sở dữ liệu
                await _unitOfWork.RecapRepository.AddAsync(recap);
                await _unitOfWork.SaveChangesAsync();

                // Cập nhật CurrentVersion cho Recap
                recap.CurrentVersionId = newRecapVersion.Id;
                recap.CurrentVersion = newRecapVersion;

                // Cập nhật Recap trong cơ sở dữ liệu
                _unitOfWork.RecapRepository.Update(recap);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<Recap>
                {
                    Succeeded = true,
                    Message = "Tạo Recap thành công.",
                    Data = recap
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteRecap(Guid recapId)
        {
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = $"Recap with ID {recapId} not found.",
                    Errors = new[] { "Recap not found." },
                    Data = false
                };
            }

            _unitOfWork.RecapRepository.Delete(recap);
            var result = await _unitOfWork.SaveChangesAsync();

            if (!result)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Xóa Recap thất bại.",
                    Errors = new[] { "Failed to delete recap." },
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa Recap thành công.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteRecap(Guid recapId)
        {
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = $"Recap with ID {recapId} not found.",
                    Errors = new[] { "Recap not found." },
                    Data = false
                };
            }

            _unitOfWork.RecapRepository.SoftDelete(recap);
            var result = await _unitOfWork.SaveChangesAsync();

            if (!result)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Xóa Recap thất bại.",
                    Errors = new[] { "Failed to soft delete recap." },
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa Recap thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<List<Recap>>> GetAllRecap()
        {
            var recaps = await _unitOfWork.RecapRepository.QueryWithIncludes(m => m.Book, m => m.Contributor).Include(m => m.RecapVersions).ToListAsync();

            if (recaps == null || recaps.Count == 0)
            {
                return new ApiResponse<List<Recap>>
                {
                    Succeeded = false,
                    Message = "No recap found.",
                    Errors = new[] { "No recap found." },
                    Data = new List<Recap>()
                };
            }

            return new ApiResponse<List<Recap>>(recaps);
        }
        public async Task<ApiResponse<List<Recap>>> GetAllRecapByContributorId(Guid userId, string? published)
        {
            // Truy vấn tất cả Recap có UserId tương ứng và bao gồm thông tin của Book
            var query = _unitOfWork.RecapRepository
                .QueryWithIncludes(r => r.Book)
                .Where(rv => rv.UserId == userId);

            // Áp dụng bộ lọc theo giá trị của published
            if (!string.IsNullOrEmpty(published))
            {
                if (published.Equals("private", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(rv => !rv.isPublished);
                }
                else if (published.Equals("public", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(rv => rv.isPublished);
                }
            }

            // Thực hiện truy vấn và lấy danh sách kết quả
            var recaps = await query.ToListAsync();

            if (recaps == null || recaps.Count == 0)
            {
                return new ApiResponse<List<Recap>>
                {
                    Succeeded = false,
                    Message = "No recaps found for this contributor.",
                    Errors = new[] { "No recaps found." },
                    Data = new List<Recap>()
                };
            }

            return new ApiResponse<List<Recap>>(recaps);
        }


        public async Task<RecapResponse<Recap, object>> GetRecapById(Guid recapId, Guid? userId = null)
        {
            // Lấy Recap với các liên kết cần thiết
            var recap = await _unitOfWork.RecapRepository
                .QueryWithIncludes(
                    r => r.RecapVersions,
                    r => r.Book.Authors,
                    r => r.Book.Categories,
                    r => r.Contributor
                )
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == recapId);

            if (recap == null)
            {
                return new RecapResponse<Recap, object>
                {
                    Succeeded = false,
                    Message = $"Recap with ID {recapId} not found.",
                    Errors = new[] { "Recap not found." },
                    Data = null,
                    Data2 = null
                };
            }

            List<PlayListItem> playListItems = new();
            bool isLiked = false;

            // Nếu userId được truyền, lấy PlayListItems và check Like
            if (userId.HasValue)
            {
                // Lấy PlayListItems của User liên quan đến Recap
                playListItems = await _unitOfWork.PlayListRepository.GetPlayListItemsByRecapIdAndUserId(recapId, userId.Value);

                // Kiểm tra isLiked
                isLiked = await _unitOfWork.LikeRepository.IsRecapLikedByUser(userId.Value, recapId);
            }

            // Tạo đối tượng chứa thông tin bổ sung
            var additionalData = new
            {
                PlayListItems = playListItems,
                isLiked = isLiked
            };

            // Trả về ApiResponse chứa Recap và thông tin bổ sung
            return new RecapResponse<Recap, object>(recap, additionalData);
        }





        public async Task<ApiResponse<string>> CallPostApiAsync(Guid? recapVersionId, string url)
        {
            using (var httpClient = new HttpClient())
            {
                // Tạo một mảng chứa recapId
                var postData = new[]
                {
                    recapVersionId.ToString()
                };

                // Serialize mảng thành JSON
                var jsonContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");

                // Gửi POST request với body là JSON (dạng mảng)
                var response = await httpClient.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string>
                    {
                        Succeeded = true,
                        Message = "POST request successful.",
                        Data = null
                    };
                }
                else
                {
                    throw new Exception($"Error calling POST API: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }






        public async Task<ApiResponse<string>> CallDeleteApiAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string>
                    {
                        Succeeded = true,
                        Message = "DELETE request successful.",
                        Data = null
                    };
                }
                else
                {
                    throw new Exception($"Error calling DELETE API: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }


        public async Task<ApiResponse<Recap>> UpdateRecap(UpdateRecapForPublished request)
        {
            // Tìm recap theo RecapId
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(request.RecapId);

            if (recap == null)
            {
                return new ApiResponse<Recap>
                {
                    Succeeded = false,
                    Message = $"Recap with ID {request.RecapId} not found.",
                    Errors = new[] { "Recap not found." },
                    Data = null
                };
            }

            // Kiểm tra xem recap có phiên bản nào được Approved không
            var approvedVersion = recap.RecapVersions?.FirstOrDefault(v => v.Status == RecapStatus.Approved);

            if (approvedVersion == null)
            {
                return new ApiResponse<Recap>
                {
                    Succeeded = false,
                    Message = "Không thể public bài viết không có phiên bản được chấp nhận.",
                    Errors = new[] { "No approved version found." },
                    Data = null
                };
            }

            // Nếu có phiên bản được Approved, cập nhật isPublished và isPremium
            recap.isPublished = request.isPublished;
            recap.isPremium = request.isPremium;

            _unitOfWork.RecapRepository.Update(recap);
            var result = await _unitOfWork.SaveChangesAsync();

            if (!result)
            {
                return new ApiResponse<Recap>
                {
                    Succeeded = false,
                    Message = "Failed to update recap.",
                    Errors = new[] { "Failed to update recap." },
                    Data = null
                };
            }
            return new ApiResponse<Recap>
            {
                Succeeded = true,
                Message = "Cập nhật Recap thành công.",
                Data = recap
            };
        }

        public async Task<ApiResponse<Recap>> ChooseVersionForRecap(Guid recapId, ChooseVersion version)
        {
            // Tìm recap theo RecapId
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<Recap>
                {
                    Succeeded = false,
                    Message = $"Recap with ID {recapId} not found.",
                    Errors = new[] { "Recap not found." },
                    Data = null
                };
            }
            recap.CurrentVersionId = version.CurrentVersionId;
            if (recap.CurrentVersion.IsDeleted == true)
            {
                return new ApiResponse<Recap>
                {
                    Succeeded = false,
                    Message = "Version này đã bị xóa.",
                    Errors = new[] { "Failed to update recap version." },
                    Data = null
                };
            }
            _unitOfWork.RecapRepository.Update(recap);
            var result = await _unitOfWork.SaveChangesAsync();

            if (!result)
            {
                return new ApiResponse<Recap>
                {
                    Succeeded = false,
                    Message = "Failed to update recap version.",
                    Errors = new[] { "Failed to update recap version." },
                    Data = null
                };
            }

            return new ApiResponse<Recap>
            {
                Succeeded = true,
                Message = "Cập nhật trạng thái thành công.",
                Data = recap
            };
        }
    }
}
