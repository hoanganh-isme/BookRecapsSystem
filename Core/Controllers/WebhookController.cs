using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Services.Service.Webhook;
using Net.payOS.Types;
using Net.payOS;
using Services.Service.Helper;
using Core.Models;
using Azure;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    [AllowAnonymous]
    public class WebhookController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly PayOSService _payOSService;

        public WebhookController(ITransactionService transactionService, PayOSService payOSService)
        {
            _transactionService = transactionService;
            _payOSService = payOSService;
        }

        [HttpPost("handle-complete-transaction")]
        public async Task<IActionResult> CompleteTransaction(WebhookType body)
        {         
                WebhookData data = _payOSService.verifyPaymentWebhookData(body);
                var orderCode = body.data.orderCode;
                var response = await _transactionService.CompleteTransactionAsync(orderCode, body);
                return Ok(new
                {
                    success = true,
                });  
        }

    }

}
