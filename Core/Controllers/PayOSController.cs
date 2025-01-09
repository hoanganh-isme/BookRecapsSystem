using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Service.Helper;
using Services.Service.Webhook;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/payos")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class PayOSController : ControllerBase
    {
        private readonly PayOSService _payOSService;

        public PayOSController(PayOSService payOSService)
        {
            _payOSService = payOSService;
        }

        // Lấy thông tin giao dịch từ PayOS
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderInfo(int orderId)
        {
            var paymentLinkInfo = await _payOSService.GetTransactionInfoAsync(orderId);
            if (paymentLinkInfo == null)
            {
                return NotFound(new { Success = false, Message = "Order not found" });
            }
            return Ok(new { Success = true, Data = paymentLinkInfo });
        }

        // Hủy giao dịch
        [HttpPut("canceltransaction/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var cancelResult = await _payOSService.CancelTransactionAsync(orderId);
            if (!cancelResult.IsSuccess)
            {
                return BadRequest(new { Success = false, Message = "Failed to cancel order", Errors = new[] { cancelResult.ErrorMessage } });
            }
            return Ok(new { Success = true, Message = "Order canceled successfully" });
        }
        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhook body)
        {
            try
            {
                // Gọi phương thức xác nhận webhook URL từ PayOSService
                await _payOSService.ConfirmWebhookAsync(body.webhook_url);
                return Ok(new { Success = true, Message = "Webhook URL confirmed successfully" });
            }
            catch (System.Exception exception)
            {
                // Log lỗi và trả về phản hồi thất bại nếu có lỗi
                Console.WriteLine(exception);
                return BadRequest(new { Success = false, Message = "Failed to confirm webhook URL", Error = exception.Message });
            }
        }

    }

}
