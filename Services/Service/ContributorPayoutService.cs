using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.ViewModels.ContributorPayouts.ContributorPayoutDTO;

namespace Services.Service
{
    public class ContributorPayoutService : IContributorPayoutService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;
        public ContributorPayoutService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
             IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }
        public async Task<List<ContributorDto>> GetAllContributorWithPayoutsAsync()
        {
            // Lấy danh sách tất cả người dùng đã xác thực email
            var users = _userManager.Users
                .AsNoTracking()
                .Where(u => u.EmailConfirmed)
                .AsQueryable();

            // Lọc chỉ những người thuộc role "Contributor"
            var contributorUsers = await _userManager.GetUsersInRoleAsync("Contributor");
            users = users.Where(u => contributorUsers.Select(cu => cu.Id).Contains(u.Id));

            // Lấy thông tin payout mới nhất của mỗi contributor
            var contributorPayouts = await _unitOfWork.ContributorPayoutRepository.GetLatestPayoutsForAllContributorsAsync();

            // Map danh sách contributors từ users
            var usersDto = _mapper.Map<List<ContributorDto>>(users);

            // Kết hợp thông tin payouts
            foreach (var userDto in usersDto)
            {
                var payoutInfo = contributorPayouts.FirstOrDefault(p => p.UserId == userDto.contributorId);

                if (payoutInfo != null)
                {
                    // Gán thông tin payout mới nhất
                    userDto.payoutId = payoutInfo.Id;
                    userDto.contributorId = payoutInfo.UserId;
                    userDto.Fromdate = payoutInfo.FromDate;
                    userDto.Description = payoutInfo.Description;
                    userDto.Todate = payoutInfo.ToDate;
                    userDto.TotalEarnings = payoutInfo.Amount;
                    userDto.Status = payoutInfo.Status.ToString();
                }
                else
                {
                    // Gán giá trị mặc định nếu không có payout
                    userDto.Fromdate = null;
                    userDto.Todate = null;
                    userDto.TotalEarnings = 0;
                    userDto.Status = "Không có dữ liệu";
                }
            }

            return usersDto;
        }


        public async Task<ApiResponse<ContributorPayoutDto>> CreateContributorPayoutAsync(Guid contributorId, string description, DateTime toDate)
        {
            if (toDate > DateTime.UtcNow)
            {
                return new ApiResponse<ContributorPayoutDto>
                {
                    Succeeded = false,
                    Message = "Ngày quyết toán không lớn hơn ngày hiện tại.",
                    Data = null
                };
            }
            // Lấy danh sách Recap của contributor
            var recaps = await _unitOfWork.RecapRepository.GetRecapsByContributorIdAsync(contributorId);
            if (recaps == null || !recaps.Any())
            {
                return new ApiResponse<ContributorPayoutDto>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy recap của người đóng góp này.",
                    Data = null
                };
            }
            var recapIds = recaps.Select(r => r.Id).ToList();
            var user = _userManager.FindByIdAsync(contributorId.ToString());
            // Xác định FromDate và ToDate
            var lastPayout = await _unitOfWork.ContributorPayoutRepository
                        .GetLastPayoutByContributorIdAsync(contributorId);


            var fromDate = lastPayout?.ToDate ?? DateTime.MinValue;

            // Lấy ViewTracking trong khoảng thời gian
            var viewTrackings = await _unitOfWork.ViewTrackingRepository
                .GetViewTrackingsByRecapIdsAndDateRangeAsync(recapIds, fromDate, toDate);


            // Tính earnings cho từng recap
            var recapEarnings = new List<RecapEarning>();
            foreach (var recap in recaps)
            {
                var recapViewTrackings = viewTrackings.Where(vt => vt.RecapId == recap.Id);
                var recapEarningAmount = recapViewTrackings
                    .Where(vt => vt.ContributorValueShare.HasValue)
                    .Sum(vt => vt.ContributorValueShare.Value);
                recapEarnings.Add(new RecapEarning
                {
                    RecapId = recap.Id,
                    EarningAmount = recapEarningAmount,
                    FromDate = fromDate,
                    ToDate = toDate
                });

            }

            // Tính tổng earnings cho tất cả Recaps
            var totalEarnings = recapEarnings.Sum(re => re.EarningAmount);

            // Tạo ContributorPayout
            var payout = new ContributorPayout
            {
                Description = description,
                Amount = totalEarnings,
                Status = PayoutStatus.Done,
                FromDate = fromDate,
                ToDate = toDate,
                UserId = contributorId,
                RecapEarnings = recapEarnings
            };

            // Lưu vào cơ sở dữ liệu
            user.Result.Earning = payout.Amount;
            await _unitOfWork.ContributorPayoutRepository.AddAsync(payout);
            await _unitOfWork.SaveChangesAsync();

            // Tạo kết quả trả về
            var result = new ContributorPayoutDto
            {
                PayoutId = payout.Id,
                TotalAmount = payout.Amount,
                FromDate = payout.FromDate,
                ToDate = payout.ToDate,
                RecapEarnings = recapEarnings.Select(re => new RecapEarningDto
                {
                    RecapId = re.RecapId,
                    RecapName = recaps.FirstOrDefault(r => r.Id == re.RecapId)?.Name,
                    ViewCount = recaps.FirstOrDefault(r => r.Id == re.RecapId)?.ViewsCount,
                    TotalEarnings = re.EarningAmount
                }).ToList()
            };

            return new ApiResponse<ContributorPayoutDto>
            {
                Succeeded = true,
                Message = "Payout created successfully.",
                Data = result
            };
        }
        public async Task<ApiResponse<ContributorPayout>> GetPayoutById(Guid id)
        {
            // Truy vấn thông tin ContributorPayout bao gồm RecapEarnings và Recap
            var payout = await _unitOfWork.ContributorPayoutRepository.GetPayoutWithDetailsByIdAsync(id);


            if (payout == null)
            {
                return new ApiResponse<ContributorPayout>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy quyết toán.",
                    Errors = new[] { "Invalid PayoutId." }
                };
            }

            // Ánh xạ RecapEarnings sang RecapEarningsDto
            var recapDetails = payout.RecapEarnings.Select(re => new RecapEarningsDto
            {
                RecapId = re.RecapId,
                RecapName = re.Recap?.Name ?? "Unknown Recap",
                ViewCount = re.Recap?.ViewsCount,
                RecapEarnings = re.EarningAmount
            }).ToList();

            // Ánh xạ dữ liệu ContributorPayout sang ContributorEarningsDto
            //var contributorEarningsDto = new ContributorPayout
            //{
            //    ContributorName = payout.Contributor.FullName,
            //    ContributorId = payout.UserId,
            //    Username = payout.Contributor.UserName,
            //    Email = payout.Contributor.Email,
            //    TotalEarnings = payout.Amount,
            //    Fromdate = payout.FromDate,
            //    Todate = payout.ToDate,
            //    RecapDetails = recapDetails
            //};

            return new ApiResponse<ContributorPayout>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu quyết toán thành công.",
                Data = payout
            };
        }

        public async Task<ApiResponse<List<ContributorDto>>> GetPayoutByContributorId(Guid contributorId)
        {
            var payout = await _unitOfWork.ContributorPayoutRepository.GetListPayoutByContributorId(contributorId);

            if (payout == null)
            {
                return new ApiResponse<List<ContributorDto>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy quyết toán.",
                    Errors = new[] { "Invalid PayoutId." }
                };
            }
            var contributordtos = payout
               .OrderByDescending(a => a.CreatedAt)
               .Select(a => new ContributorDto
               {
                   payoutId = a.Id,
                   contributorId = a.UserId,
                   ContributorName = a.Contributor.FullName,
                   Description = a.Description,
                   Fromdate = a.FromDate,
                   Todate = a.ToDate,
                   TotalEarnings = a.Amount,
                   Status = a.Status.ToString(),
                   CreateAt = a.CreatedAt
               }).ToList();

            return new ApiResponse<List<ContributorDto>>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu quyết toán thành công.",
                Data = contributordtos
            };
        }
    }
}
