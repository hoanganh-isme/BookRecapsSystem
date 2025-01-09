using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using BusinessObject.Models;
using Net.payOS;
using Net.payOS.Types;
using Net.payOS.Utils;
using Net.payOS.Errors;
using Newtonsoft.Json.Linq;

namespace Services.Service.Helper
{
    public class PayOSService
    {
        private readonly PayOS _payOSClient;
        private readonly string _checksumKey;

        public PayOSService(IOptions<PayOSOptions> payOSOptions)
        {
            // Lấy các giá trị từ options
            var options = payOSOptions.Value;
            _payOSClient = new PayOS(options.ClientId, options.ApiKey, options.ChecksumKey);
            _checksumKey = options.ChecksumKey;
        }

        public async Task<PayResponse> CreateTransactionAsync(BusinessObject.Models.Transaction transaction, SubscriptionPackage subscriptionPackage)
        {
            try
            {
                var items = new List<ItemData>
                {
                    new ItemData(subscriptionPackage.Name, 1, (int)subscriptionPackage.Price) 
                };

                var requestUrl = "https://bookrecaps.net";
                //var requestUrl = "http://localhost:5173";

                // Thiết lập các tham số thanh toán
                var paymentData = new PaymentData(
                    transaction.OrderCode,
                    (int)subscriptionPackage.Price,
                    "Thanh toán gói đăng ký",
                    items,
                    $"{requestUrl}/result",
                    $"{requestUrl}/result"

                );
                var createPaymentResult = await _payOSClient.createPaymentLink(paymentData);

                if (createPaymentResult != null && !string.IsNullOrEmpty(createPaymentResult.checkoutUrl))
                {
                    return new PayResponse { IsSuccess = true, CheckoutUrl = createPaymentResult.checkoutUrl };
                }
                else
                {
                    return new PayResponse { IsSuccess = false, ErrorMessage = "Failed to create payment link." };
                }
            }
            catch (Exception ex)
            {
                return new PayResponse { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
        // Hủy giao dịch
        public async Task<PayResponse> CancelTransactionAsync(int orderId)
        {
            try
            {
                // Gọi phương thức cancelPaymentLink từ thư viện PayOS
                PaymentLinkInformation cancelResult = await _payOSClient.cancelPaymentLink(orderId);

                // Kiểm tra kết quả trả về
                if (cancelResult != null)
                {
                    return new PayResponse
                    {
                        IsSuccess = true,
                        Message = "Order canceled successfully."
                    };
                }
                else
                {
                    return new PayResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to cancel order."
                    };
                }
            }
            catch (PayOSError ex)
            {
                // Xử lý lỗi từ PayOS nếu có
                return new PayResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error from PayOS: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                return new PayResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }


        // Lấy thông tin giao dịch
        public async Task<PayResponse> GetTransactionInfoAsync(int orderId)
        {
            try
            {
                var transactionInfo = await _payOSClient.getPaymentLinkInformation(orderId);

                if (transactionInfo != null)
                {
                    return new PayResponse { IsSuccess = true, Data = transactionInfo };
                }
                else
                {
                    return new PayResponse { IsSuccess = false, ErrorMessage = "Transaction not found." };
                }
            }
            catch (Exception ex)
            {
                return new PayResponse { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
        // Xác nhận webhook URL
        public async Task<PayResponse> ConfirmWebhookAsync(string webhookUrl)
        {
            try
            {
                // Gọi phương thức confirmWebhook từ thư viện PayOS để xác nhận webhook URL
                string confirmedWebhookUrl = await _payOSClient.confirmWebhook(webhookUrl);

                if (!string.IsNullOrEmpty(confirmedWebhookUrl))
                {
                    return new PayResponse
                    {
                        IsSuccess = true,
                        Message = "Webhook URL confirmed successfully",
                        Data = confirmedWebhookUrl
                    };
                }
                else
                {
                    return new PayResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to confirm webhook URL"
                    };
                }
            }
            catch (PayOSError ex)
            {
                return new PayResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error from PayOS: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new PayResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        public WebhookData verifyPaymentWebhookData(WebhookType webhookBody)
        {
            WebhookData data = webhookBody.data;
            string signature = webhookBody.signature;
            if (data == null)
            {
                throw new Exception("No data.");
            }

            if (signature == null)
            {
                throw new Exception("No signature.");
            }

            if (SignatureControl.CreateSignatureFromObj(JObject.FromObject(data), _checksumKey) != signature)
            {
                throw new Exception("The data is unreliable because the signature of the response does not match the signature of the data");
            }

            return data;
        }
    }

    public class PayResponse
    {
        public bool IsSuccess { get; set; }
        public string CheckoutUrl { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }  
        public object Data { get; set; }
    }
}
