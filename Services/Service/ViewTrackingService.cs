using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Viewtrackings;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ViewTrackingService : IViewTrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ViewTrackingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ViewTracking>> CreateViewTrackingAsync(Guid recapId, Guid? userId, DeviceType deviceType)

        {
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<ViewTracking>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Recap.",
                    Errors = new[] { "Recap không tồn tại." }
                };
            }

            if (userId == null)
            {
                recap.ViewsCount = (recap.ViewsCount ?? 0) + 1;
                _unitOfWork.RecapRepository.Update(recap);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<ViewTracking>
                {
                    Succeeded = true,
                    Message = "View count increased for anonymous user."
                };
            }



            var viewTracking = new CreateViewTracking
            {
                RecapId = recapId,
                UserId = userId.Value,
                isPremium = recap.isPremium,
                DeviceType = deviceType,
                ViewValue = 0,
                PublisherValueShare = 0,
                ContributorValueShare = 0,
                PlatformValueShare = 0
            };

            if (!recap.isPremium)
            {
                recap.ViewsCount = (recap.ViewsCount ?? 0) + 1;
                _unitOfWork.RecapRepository.Update(recap);
                var newView = _mapper.Map<ViewTracking>(viewTracking);
                await _unitOfWork.ViewTrackingRepository.AddAsync(newView);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<ViewTracking>
                {
                    Succeeded = true,
                    Message = "Tạo view cho recap không premium thành công.",
                    Data = newView
                };
            }

            // Lấy danh sách subscription hợp lệ cho user
            var subscriptions = await _unitOfWork.SubscriptionRepository.GetValidSubscriptionsByUserIdAsync(userId.Value);
            var subscription = subscriptions.FirstOrDefault();

            if (subscription == null)
            {
                return new ApiResponse<ViewTracking>
                {
                    Succeeded = false,
                    Message = "Lượt premium view đã hết hoặc Subscription đã hết hạn. User cần mua thêm Subscription để xem Premium Recap.",
                    Errors = new[] { "No valid subscription found for the premium view." }
                };
            }
            // Kiểm tra xem người dùng đã xem recap này trong cùng một subscription chưa
            var existingView = await _unitOfWork.ViewTrackingRepository
                .GetByUserIdRecapIdAndSubscriptionIdAsync(userId.Value, recapId, subscription.Id);

            if (existingView != null)
            {
                recap.ViewsCount = (recap.ViewsCount ?? 0) + 1;
                _unitOfWork.RecapRepository.Update(recap);
                var newView = _mapper.Map<ViewTracking>(viewTracking);
                await _unitOfWork.ViewTrackingRepository.AddAsync(newView);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<ViewTracking>
                {
                    Succeeded = true,
                    Message = "Người dùng đã xem recap này trong gói subscription hiện tại, view được thêm mà không tính tiền.",
                    Data = newView
                };
            }


            // Tìm hợp đồng liên kết với Book trong Recap
            var book = await _unitOfWork.BookRepository.GetByIdAsync(recap.BookId);
            var contract = await _unitOfWork.ContractRepository.GetValidContractByBookIdAsync(book.Id);
            if (contract == null)
            {
                return new ApiResponse<ViewTracking>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy hợp đồng hợp lệ cho cuốn sách này.",
                    Errors = new[] { "No valid contract found for the book." }
                };
            }

            // Lấy RevenueSharePercentage từ hợp đồng
            var publisherValueSharePercentage = (contract.RevenueSharePercentage ?? 0m) / 100;

            // Tính toán Publisher Value Share
            var viewValue = subscription.Price / subscription.ExpectedViewsCount;
            var publisherValueShare = viewValue * publisherValueSharePercentage;

            // Cập nhật giá trị cho ViewTracking
            var revenueSharePercentage = (await _unitOfWork.SystemSettingRepository.GetRevenueSharePercentageAsync()) / 100;
            var contributorValueShare = (viewValue - publisherValueShare) * (revenueSharePercentage ?? 0m);
            var platformValueShare = viewValue - publisherValueShare - contributorValueShare;

            viewTracking.ViewValue = viewValue;
            viewTracking.PublisherValueShare = publisherValueShare;
            viewTracking.ContributorValueShare = contributorValueShare;
            viewTracking.PlatformValueShare = platformValueShare;
            viewTracking.SubscriptionId = subscription.Id;

            recap.ViewsCount = (recap.ViewsCount ?? 0) + 1;
            _unitOfWork.RecapRepository.Update(recap);
            subscription.ActualViewsCount += 1;
            if (subscription.ActualViewsCount == subscription.ExpectedViewsCount)
            {
                // Đánh dấu gói hiện tại là hết hạn
                subscription.Status = SubStatus.Expired;

                // Lấy gói tiếp theo
                var nextSubscription = await _unitOfWork.SubscriptionRepository
                    .GetNextSubscriptionByUserIdAsync(subscription.UserId);

                if (nextSubscription != null)
                {
                    // Xác định ngày bắt đầu mới (ngày hôm nay)
                    var newStartDate = DateOnly.FromDateTime(DateTime.Now);
                    var duration = nextSubscription.EndDate.Value.DayNumber - nextSubscription.StartDate.Value.DayNumber;

                        // Cập nhật ngày bắt đầu và ngày kết thúc của gói tiếp theo
                        nextSubscription.StartDate = newStartDate;
                        nextSubscription.EndDate = newStartDate.AddDays(duration);

                    // Đánh dấu gói tiếp theo là Active
                    nextSubscription.Status = SubStatus.Active;

                    // Cập nhật thông tin trong database
                    _unitOfWork.SubscriptionRepository.Update(nextSubscription);
                }
            }

            _unitOfWork.SubscriptionRepository.Update(subscription);

            var newViews = _mapper.Map<ViewTracking>(viewTracking);
            await _unitOfWork.ViewTrackingRepository.AddAsync(newViews);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<ViewTracking>
            {
                Succeeded = true,
                Message = "Tạo view thành công cho Premium recap.",
                Data = newViews
            };
        }


        public async Task<decimal> CalculateContributorValueShareAsync(decimal viewValue, decimal publisherValueShare)
        {
            var revenueSharePercentage = await _unitOfWork.SystemSettingRepository.GetRevenueSharePercentageAsync();

            if (!revenueSharePercentage.HasValue)
            {
                throw new Exception("RevenueSharePercentage setting not found.");
            }

            var contributorValueShare = (viewValue - publisherValueShare) * revenueSharePercentage.Value;
            return contributorValueShare;
        }

        // Lấy dữ liệu theo ID
        public async Task<ApiResponse<ViewTracking>> GetViewTrackingById(Guid trackingId)
        {
            var tracking = await _unitOfWork.ViewTrackingRepository.GetByIdAsync(trackingId);

            if (tracking == null)
            {
                return new ApiResponse<ViewTracking>
                {
                    Succeeded = false,
                    Message = "Tracking not found.",
                    Errors = new[] { "Tracking does not exist." }
                };
            }

            return new ApiResponse<ViewTracking>
            {
                Succeeded = true,
                Message = "Tracking retrieved successfully.",
                Data = tracking
            };
        }
        public async Task<ApiResponse<ViewTracking>> UpdateDurationViewtracking(Guid trackingId, int duration)
        {
            var tracking = await _unitOfWork.ViewTrackingRepository.GetByIdAsync(trackingId);

            if (tracking == null)
            {
                return new ApiResponse<ViewTracking>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy lượt xem này.",
                    Errors = new[] { "Tracking does not exist." }
                };
            }
            tracking.Durations = duration;
            _unitOfWork.ViewTrackingRepository.Update(tracking);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<ViewTracking>
            {
                Succeeded = true,
                Message = "Cập nhật thời lượng xem thành công.",
                Data = tracking
            };
        }

        // Lấy tất cả view tracking
        public async Task<ApiResponse<List<ViewTracking>>> GetAllViewTrackings()
        {
            var viewTrackings = await _unitOfWork.ViewTrackingRepository.GetAllAsync();

            if (viewTrackings == null || !viewTrackings.Any())
            {
                return new ApiResponse<List<ViewTracking>>
                {
                    Succeeded = false,
                    Message = "No view trackings found.",
                    Errors = new[] { "No data available." }
                };
            }

            return new ApiResponse<List<ViewTracking>>
            {
                Succeeded = true,
                Message = "View trackings retrieved successfully.",
                Data = viewTrackings
            };
        }
        public async Task<ApiResponse<PaginationResponse<ViewTrackingDTO>>> GetViewTrackingByUserId(Guid userId, int pageNumber, int pageSize)
        {
            // Lấy tất cả các ViewTracking của người dùng
            var totalCount = await _unitOfWork.ViewTrackingRepository.GetCountByUserIdAsync(userId);

            if (totalCount == 0)
            {
                return new ApiResponse<PaginationResponse<ViewTrackingDTO>>
                {
                    Succeeded = true,
                    Message = "Không tìm thấy view tracking nào của người dùng này.",
                    Data = new PaginationResponse<ViewTrackingDTO>(new List<ViewTrackingDTO>(), totalCount, pageNumber, pageSize)
                };
            }

            // Áp dụng phân trang cho ViewTracking
            var skip = (pageNumber - 1) * pageSize;
            var pagedViewTrackings = await _unitOfWork.ViewTrackingRepository.GetPagedViewTrackingsWithDetailsAsync(userId, skip, pageSize);

            // Tạo danh sách ViewTrackingDTO
            var viewTrackingDTOs = pagedViewTrackings.Select(vt => new ViewTrackingDTO
            {
                RecapId = vt.Recap.Id,
                RecapName = vt.Recap.Name,
                isPublished = vt.Recap.isPublished,
                isPremium = vt.Recap.isPremium,
                LikesCount = vt.Recap.LikesCount,
                ViewsCount = vt.Recap.ViewsCount,
                ContributorName = vt.Recap.Contributor.FullName,
                ContributorImage = vt.Recap.Contributor.ImageUrl,
                Book = new BookDTO
                {
                    bookId = vt.Recap.BookId,
                    Title = vt.Recap.Book.Title,
                    OriginalTitle = vt.Recap.Book.OriginalTitle,
                    CoverImage = vt.Recap.Book.CoverImage,
                    Authors = vt.Recap.Book.Authors.Select(a => a.Name).ToList()
                },
                Durations = vt.Durations,
                DeviceType = vt.DeviceType,
                CreatedAt = vt.CreatedAt
            }).ToList();

            // Tạo đối tượng phân trang cho ViewTrackingDTO
            var paginationResponse = new PaginationResponse<ViewTrackingDTO>(viewTrackingDTOs, totalCount, pageNumber, pageSize);

            return new ApiResponse<PaginationResponse<ViewTrackingDTO>>
            {
                Succeeded = true,
                Message = "View trackings retrieved successfully.",
                Data = paginationResponse
            };
        }





    }
}
