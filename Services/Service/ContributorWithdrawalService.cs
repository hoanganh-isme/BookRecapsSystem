using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Withdrawal;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Net.payOS;
using Repository;
using Services.Interface;
using Services.Responses;
using Services.Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ContributorWithdrawalService : IContributorWithdrawalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly GoogleCloudService _googleCloudService;
        private readonly IMapper _mapper;
        public ContributorWithdrawalService(IUnitOfWork unitOfWork,
             UserManager<User> userManager, IOptions<GoogleSettings> googleSettings,
             IConfiguration configuration,
             IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _googleCloudService = new GoogleCloudService(configuration);
            _mapper = mapper;
        }

        public async Task<List<ContributorWithdrawalDTO>> GetAllContributorWithDrawalsAsync()
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
            var contributorWithdrawals = await _unitOfWork.ContributorWithdrawalRepository.GetLatestDrawalForAllContributorsAsync();

            // Map danh sách contributors từ users
            var usersDto = _mapper.Map<List<ContributorWithdrawalDTO>>(users);

            // Kết hợp thông tin payouts
            foreach (var userDto in usersDto)
            {
                var drawalInfo = contributorWithdrawals.FirstOrDefault(p => p.ContributorId == userDto.contributorId);

                if (drawalInfo != null)
                {
                    // Gán thông tin payout mới nhất
                    userDto.Description = drawalInfo.Description;
                    userDto.CreateAt = drawalInfo.CreatedAt;
                    userDto.TotalEarnings = drawalInfo.Amount;
                    userDto.Status = drawalInfo.Status.ToString();
                }
                else
                {
                    // Gán giá trị mặc định nếu không có payout
                    userDto.CreateAt = null;
                    userDto.TotalEarnings = 0;
                    userDto.Status = "Không có dữ liệu";
                }
            }

            return usersDto;
        }
        public async Task<List<ContributorWithdrawalDTO>> GetAllDrawalsAsync()
        {
            // Lấy tất cả các bản ghi ContributorWithdrawals từ repository
            var contributorWithdrawals = await _unitOfWork.ContributorWithdrawalRepository
                .QueryWithIncludes(x => x.Contributor)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            // Map danh sách ContributorWithdrawals thành ContributorWithdrawalDTO
            var withdrawalDtos = contributorWithdrawals.OrderByDescending(a => a.CreatedAt).Select(drawal => new ContributorWithdrawalDTO
            {
                drawalId = drawal.Id,
                contributorId = drawal.ContributorId,
                ContributorName = drawal.Contributor.FullName,
                Description = drawal.Description,
                TotalEarnings = drawal.Amount,
                CreateAt = drawal.CreatedAt,
                Status = drawal.Status.ToString() // Chuyển enum thành string
            }).ToList();

            return withdrawalDtos;
        }


        public async Task<ApiResponse<ContributorWithdrawal>> CreateDrawal(Guid contributorId, decimal amount)
        {
            // Tìm Contributor bằng UserManager
            var contributor = await _userManager.FindByIdAsync(contributorId.ToString());
            if (contributor == null)
            {
                return new ApiResponse<ContributorWithdrawal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Contributor.",
                    Errors = new[] { "Contributor not found." }
                };
            }
            // Kiểm tra số tiền
            if (amount <= 0)
            {
                return new ApiResponse<ContributorWithdrawal>
                {
                    Succeeded = false,
                    Message = "Số tiền phải lớn hơn 50000.",
                    Errors = new[] { "Invalid amount." }
                };
            }
            // Kiểm tra số dư (giả sử có hàm kiểm tra số dư)
            var balance = contributor.Earning;
            if (balance < amount)
            {
                return new ApiResponse<ContributorWithdrawal>
                {
                    Succeeded = false,
                    Message = "Số dư không đủ để thực hiện rút tiền.",
                    Errors = new[] { "Insufficient balance." }
                };
            }

            // Tạo yêu cầu rút tiền
            var withdrawal = new ContributorWithdrawal
            {
                ContributorId = contributorId,
                Amount = amount,
                Status = WithdrawalStatus.Pending
            };

            // Lưu vào cơ sở dữ liệu
            await _unitOfWork.ContributorWithdrawalRepository.AddAsync(withdrawal);
            await _unitOfWork.SaveChangesAsync();

            // Trả về kết quả
            return new ApiResponse<ContributorWithdrawal>
            {
                Succeeded = true,
                Message = "Yêu cầu rút tiền được tạo thành công.",
                Data = withdrawal
            };
        }

        public async Task<ApiResponse<ContributorWithdrawal>> AcceptWithdrawal(Guid withdrawalId, Stream ImageStream, string ImageContentType, ProcessWithdrawal processWithdrawal)
        {
            //var withdrawal = await _unitOfWork.ContributorWithdrawalRepository.GetDrawalByContributorIdAsync(contributorId);
            var withdrawal = await _unitOfWork.ContributorWithdrawalRepository.QueryWithIncludes(x => x.Contributor)
                .FirstOrDefaultAsync(x => x.Id == withdrawalId);
            
            if (withdrawal == null)
            {
                return new ApiResponse<ContributorWithdrawal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy yêu cầu rút tiền.",
                    Errors = new[] { "Withdrawal not found." }
                };
            }
            if (withdrawal.Status != WithdrawalStatus.Pending)
            {
                return new ApiResponse<ContributorWithdrawal>
                {
                    Succeeded = false,
                    Message = "Yêu cầu rút tiền không ở trạng thái chờ xử lý.",
                    Errors = new[] { "Withdrawal request is not in pending state." }
                };
            }
            var contributor = _userManager.FindByIdAsync(withdrawal.ContributorId.ToString());
            withdrawal.Description = processWithdrawal.Description ?? withdrawal.Description;            
            withdrawal.Status = processWithdrawal.Status;
            if (processWithdrawal.Status == WithdrawalStatus.Accepted)
            {
                if(contributor.Result.Earning < withdrawal.Amount)
                {
                    return new ApiResponse<ContributorWithdrawal>
                    {
                        Succeeded = false,
                        Message = "Tài khoản của người dùng không đủ.",
                        Errors = new[] { "Earning not enough." }
                    };
                }
                string imagepayout = "";
                if (ImageStream != null && !string.IsNullOrEmpty(ImageContentType))
                {
                    // Tạo thư mục động cho ảnh bìa dựa theo BookId
                    string contributorwithdrawalFolderName = $"contributor_withdrawal/";
                    string coverImageFileName = $"{contributorwithdrawalFolderName}contributorwithdrawal_{Guid.NewGuid()}.jpg";

                    var coverImageUrl = await _googleCloudService.UploadImageAsync(coverImageFileName, ImageStream, ImageContentType);
                    imagepayout = coverImageUrl;
                }
                withdrawal.ImageUrl = imagepayout;
                contributor.Result.Earning = contributor.Result.Earning - withdrawal.Amount;
            }            
            _unitOfWork.ContributorWithdrawalRepository.Update(withdrawal);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<ContributorWithdrawal>
            {
                Succeeded = true,
                Message = "Yêu cầu rút tiền đã được cập nhật thành công.",
                Data = withdrawal
            };
        }
        public async Task<RecapResponse<ContributorWithdrawal, object>> GetWithdrawalById(Guid withdrawalId)
        {
            var contributorwithdrawal = await _unitOfWork.ContributorWithdrawalRepository
                .QueryWithIncludes(x => x.Contributor)
                .FirstOrDefaultAsync(x => x.Id == withdrawalId);
            if(contributorwithdrawal == null)
            {
                return new RecapResponse<ContributorWithdrawal, object>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy yêu cầu rút tiền.",
                    Errors = new[] { "Withdrawal not found." }
                };
            }
            var user = await _userManager.FindByIdAsync(contributorwithdrawal.ContributorId.ToString());
            var earning = user.Earning;
            var bankaccount = user.BankAccount;
            var additionalData = new
            {
                BankAccount = bankaccount,
                Earning = earning
            };
            return new RecapResponse<ContributorWithdrawal, object>
            {
                Succeeded = true,
                Message = "Tìm kiếm thành công.",
                Data = contributorwithdrawal,
                Data2 = additionalData
            };
        }

        public async Task<ApiResponse<HistoryWithDrawal>> GetDrawalByContributorId(Guid contributorId)
        {
            // Lấy danh sách rút tiền của contributor
            var drawal = await _unitOfWork.ContributorWithdrawalRepository.GetListDrawalByContributorId(contributorId);

            // Kiểm tra contributor có tồn tại không
            var user = await _userManager.FindByIdAsync(contributorId.ToString());
            if (user == null)
            {
                return new ApiResponse<HistoryWithDrawal>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy contributor.",
                    Errors = new[] { "Contributor not found." }
                };
            }
            // Tính tổng số tiền đã rút
            var earning = user.Earning; // Earning hiện tại của contributor
            var withDrawal = drawal
                .Where(d => d.Status == WithdrawalStatus.Accepted) // Chỉ lấy các giao dịch Accepted
                .Sum(d => d.Amount ?? 0);
            var countwithdrawal = drawal.Count();
            var bankaccount = user.BankAccount ?? null;

            // Chuyển đổi danh sách ContributorWithdrawal thành DTO
            var contributorDtos = drawal.Select(d => new ContributorWithdrawalDTO
            {
                drawalId = d.Id,
                contributorId = d.ContributorId,
                ContributorName = user.FullName, // Lấy tên contributor từ user
                Description = d.Description,
                ImageURL = d.ImageUrl,
                TotalEarnings = d.Amount,
                CreateAt = d.CreatedAt,
                Status = d.Status.ToString() // Chuyển enum WithdrawalStatus thành string
            }).ToList();

            // Tạo đối tượng kết quả
            var historyWithDrawal = new HistoryWithDrawal
            {
                TotalEarning = (decimal)earning,
                Withdrawal = withDrawal,
                NumberOfWithdrawals = countwithdrawal,
                BankAccount = bankaccount,
                Contributors = contributorDtos
            };

            return new ApiResponse<HistoryWithDrawal>
            {
                Succeeded = true,
                Message = "Lấy dữ liệu rút tiền thành công.",
                Data = historyWithDrawal
            };
        }


    }
}
