using BusinessObject.Models;
using Net.payOS.Types;
using Services.Responses;
using Services.Service.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ITransactionService
    {
        Task<ApiResponse<bool>> CompleteTransactionAsync(long orderCode, WebhookType webhookRequest);
        Task<ApiResponse<string>> CreateTransactionAsync(Guid userId, Guid subscriptionPackageId);
    }
}
