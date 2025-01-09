using AutoMapper;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interface;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public TransactionController(ITransactionService transactionService, IMapper mapper, ICurrentUserService currentUser)
        {
            _transactionService = transactionService;
            _mapper = mapper;
            _currentUser = currentUser;
        }
        [HttpPost("create-transaction/{subscriptionPackageId}")]
        public async Task<IActionResult> CreateTransaction(Guid subscriptionPackageId)
        {
            var currentUserId = _currentUser.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not authenticated.");
            }
            if (!Guid.TryParse(currentUserId, out Guid userId))
            {
                return BadRequest("Invalid contributor ID.");
            }

            // Gọi CreateTransactionAsync từ TransactionService để tạo giao dịch và lấy link thanh toán
            var response = await _transactionService.CreateTransactionAsync(userId, subscriptionPackageId);

            // Kiểm tra xem giao dịch có tạo thành công không
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            // Trả về chỉ checkout URL nếu thành công
            return Ok(new { Success = true,
                CheckoutUrl = response.Data,
                Message = "Tạo đơn hàng thành công"
            });
        }

    }
}
