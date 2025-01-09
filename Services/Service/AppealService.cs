using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Appeals;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Service
{
    public class AppealService : IAppealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AppealService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<Appeal>> CreateAppeal(CreateAppealRequest appealRequest)
        {
            var review = await _unitOfWork.ReviewRepository
                .FirstOrDefaultAsync(
                    r => r.Id == appealRequest.ReviewId && r.StaffId != null,
                    include: query => query.Include(r => r.Appeals)
                );

            if (review == null)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Review hoặc không đúng Staff.",
                    Errors = new[] { "Invalid ReviewId." }
                };
            }

            // Check if the review already has an open or under review appeal
            if (review.Appeals.Any(a => a.AppealStatus == AppealStatus.Open || a.AppealStatus == AppealStatus.UnderReview))
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Đã có kháng cáo đang giải quyết của Review này",
                    Errors = new[] { "Multiple active appeals not allowed." }
                };
            }

            // Map and create a new appeal
            var newAppeal = _mapper.Map<Appeal>(appealRequest);
            newAppeal.ContributorId = appealRequest.ContributorId;
            newAppeal.AppealStatus = AppealStatus.UnderReview;

            await _unitOfWork.AppealRepository.AddAsync(newAppeal);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Tạo kháng cáo thất bại.",
                    Errors = new[] { "Database error while saving appeal." }
                };
            }

            return new ApiResponse<Appeal>
            {
                Succeeded = true,
                Message = "Tạo kháng cáo thành công.",
                Data = newAppeal
            };
        }

        public async Task<ApiResponse<Appeal>> UpdateAppeal(UpdateAppealContributor appealUpdate, Guid contributorId)
        {
            var appeal = await _unitOfWork.AppealRepository.GetByIdAsync(appealUpdate.Id);
            if (appeal == null)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Errors = new[] { "Invalid AppealId." }
                };
            }

            if (appeal.ContributorId != contributorId || appeal.AppealStatus != AppealStatus.UnderReview)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Chỉ Contributor tạo mới được chỉnh sửa hoặc kháng cáo này đã đóng.",
                    Errors = new[] { "Only the creator can update and only if status is 'Open'." }
                };
            }

            // Update appeal details
            _mapper.Map(appealUpdate, appeal);
            _unitOfWork.AppealRepository.Update(appeal);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Appeal>
            {
                Succeeded = true,
                Message = "Cập nhật kháng cáo thành công.",
                Data = appeal
            };
        }

        public async Task<ApiResponse<Appeal>> ResponseAppeal(UpdateAppealResponse appealResponse, Guid staffId, RecapStatus status)
        {
            var appeal = await _unitOfWork.AppealRepository.GetByIdAsync(appealResponse.Id);
            var review = await _unitOfWork.ReviewRepository.QueryWithIncludes(x => x.RecapVersion).FirstOrDefaultAsync(x => x.Id == appeal.ReviewId);
            if (appeal == null)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Errors = new[] { "Invalid AppealId." }
                };
            }

            if (appeal.AppealStatus != AppealStatus.UnderReview || review.StaffId != staffId)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Chỉ Staff tạo review mới được response kháng cáo hoặc kháng cáo không UnderReview.",
                    Errors = new[] { "Staff can only respond to 'UnderReview' appeals assigned to them." }
                };
            }

            // Update response and status
            review.RecapVersion.Status = status;
            appeal.Response = appealResponse.Response;
            appeal.StaffId = staffId;
            appeal.AppealStatus = AppealStatus.Resolved;

            _unitOfWork.AppealRepository.Update(appeal);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Appeal>
            {
                Succeeded = true,
                Message = "Đã response kháng cáo.",
                Data = appeal
            };
        }

        public async Task<ApiResponse<bool>> DeleteAppeal(Guid appealId, Guid contributorId)
        {
            var appeal = await _unitOfWork.AppealRepository.GetByIdAsync(appealId);
            if (appeal == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Errors = new[] { "Invalid AppealId." },
                    Data = false
                };
            }

            if (appeal.ContributorId != contributorId || appeal.AppealStatus != AppealStatus.UnderReview)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ Contributor tạo kháng cáo mới được xóa hoặc kháng cáo không Open.",
                    Data = false
                };
            }

            _unitOfWork.AppealRepository.Delete(appeal);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa báo cáo thành công.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteAppeal(Guid appealId, Guid contributorId)
        {
            var appeal = await _unitOfWork.AppealRepository.GetByIdAsync(appealId);
            if (appeal == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Errors = new[] { "Invalid AppealId." },
                    Data = false
                };
            }

            if (appeal.ContributorId != contributorId || appeal.AppealStatus != AppealStatus.UnderReview)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ Contributor tạo kháng cáo mới được xóa hoặc kháng cáo không Open.",
                    Data = false
                };
            }

            _unitOfWork.AppealRepository.SoftDelete(appeal);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Ẩn kháng cáo thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<Appeal>> GetAppealById(Guid appealId)
        {
            // Truy vấn Appeal và bao gồm thông tin Review
            var appeal = await _unitOfWork.AppealRepository
                .QueryWithIncludes(a => a.Review)
                .FirstOrDefaultAsync(a => a.Id == appealId);

            if (appeal == null)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Errors = new[] { "Invalid AppealId." }
                };
            }

            return new ApiResponse<Appeal>
            {
                Succeeded = true,
                Message = "Appeal retrieved successfully.",
                Data = appeal
            };
        }
        public async Task<ApiResponse<Appeal>> GetAppealByReviewId(Guid reviewId)
        {
            // Truy vấn Appeal và bao gồm thông tin Review
            var appeal = await _unitOfWork.AppealRepository
                .QueryWithIncludes(a => a.Review)
                .FirstOrDefaultAsync(a => a.ReviewId == reviewId);

            if (appeal == null)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Errors = new[] { "Invalid AppealId." }
                };
            }

            return new ApiResponse<Appeal>
            {
                Succeeded = true,
                Message = "Appeal retrieved successfully.",
                Data = appeal
            };
        }

        public async Task<ApiResponse<List<Appeal>>> GetAllAppeals()
        {
            // Truy vấn tất cả Appeal và bao gồm thông tin Review
            var appeals = await _unitOfWork.AppealRepository
                .QueryWithIncludes(a => a.Review, a => a.Staff, appeals => appeals.Contributor)
                .ToListAsync();

            if (!appeals.Any())
            {
                return new ApiResponse<List<Appeal>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Data = new List<Appeal>()
                };
            }

            return new ApiResponse<List<Appeal>>
            {
                Succeeded = true,
                Message = "Appeals retrieved successfully.",
                Data = appeals
            };
        }


        public async Task<ApiResponse<List<Appeal>>> GetAppealByStaffId(Guid staffId)
        {
            var appeals = await _unitOfWork.AppealRepository.GetAppealsByStaffAsync(staffId);

            if (!appeals.Any())
            {
                return new ApiResponse<List<Appeal>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo của Staff này.",
                    Data = new List<Appeal>()
                };
            }

            return new ApiResponse<List<Appeal>>
            {
                Succeeded = true,
                Message = "Appeals retrieved successfully.",
                Data = appeals
            };
        }

        public async Task<ApiResponse<List<Appeal>>> GetAppealByUserId(Guid userId)
        {
            var appeals = await _unitOfWork.AppealRepository
                .QueryWithIncludes(a => a.ContributorId == userId, a => a.Staff)
                .Include(a => a.Review).ToListAsync();

            if (!appeals.Any())
            {
                return new ApiResponse<List<Appeal>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo với người dùng này.",
                    Data = new List<Appeal>()
                };
            }

            return new ApiResponse<List<Appeal>>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = appeals
            };
        }

        public async Task<ApiResponse<List<Appeal>>> GetAllAppealsWithUnderReviewStatus()
        {
            var appeals = await _unitOfWork.AppealRepository
                        .QueryWithIncludes(a => a.Review) 
                        .Where(a => a.AppealStatus == AppealStatus.UnderReview) 
                        .ToListAsync();


            if (!appeals.Any())
            {
                return new ApiResponse<List<Appeal>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo UnderReview.",
                    Data = new List<Appeal>()
                };
            }

            return new ApiResponse<List<Appeal>>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu thành công.",
                Data = appeals
            };
        }


        public async Task<ApiResponse<Appeal>> ChangeAppealStatus(UpdateAppealStatus appeal, Guid userId)
        {
            // Tìm kiếm appeal dựa trên ID
            var existingAppeal = await _unitOfWork.AppealRepository.GetByIdAsync(appeal.Id);

            if (existingAppeal == null)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy kháng cáo.",
                    Errors = new[] { "Invalid AppealId." }
                };
            }

            // Kiểm tra nếu người dùng hiện tại là contributor của appeal
            if (existingAppeal.ContributorId != userId)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Chỉ Contributor tạo kháng cáo mới được cập nhật.",
                    Errors = new[] { "Access denied." }
                };
            }

            // Kiểm tra trạng thái hiện tại của appeal, chỉ cho phép cập nhật nếu trạng thái là Open
            if (existingAppeal.AppealStatus != AppealStatus.Open)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Chỉ kháng cáo với trạng thái Open mới được cập nhật.",
                    Errors = new[] { "Cannot modify appeal status." }
                };
            }

            // Cập nhật trạng thái appeal
            appeal.AppealStatus = AppealStatus.UnderReview;
            existingAppeal.AppealStatus = appeal.AppealStatus;
            _unitOfWork.AppealRepository.Update(existingAppeal);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<Appeal>
                {
                    Succeeded = false,
                    Message = "Cập nhật trạng thái kháng cáo thất bại.",
                    Errors = new[] { "Database error while saving." }
                };
            }

            return new ApiResponse<Appeal>
            {
                Succeeded = true,
                Message = "Cập nhật trạng thái kháng cáo thành công.",
                Data = existingAppeal
            };
        }
    }
}

