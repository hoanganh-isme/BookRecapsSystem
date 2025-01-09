using BusinessObject.Models;
using BusinessObject.ViewModels.SupportTicket;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ISupportTicketService
    {
        Task<ApiResponse<SupportTicket>> CreateSupportTicket(CreateSupportTicketRequest supportTicket);
        Task<ApiResponse<SupportTicket>> UpdateSupportTicket(UpdateSupportTicketRequest supportTicket, Guid userId, Guid id);
        Task<ApiResponse<bool>> DeleteSupportTicket(Guid supportTicketId, Guid userId);
        Task<ApiResponse<bool>> SoftDeleteSupportTicket(Guid supportTicketId, Guid userId);
        Task<ApiResponse<SupportTicket>> GetSupportTicketById(Guid supportTicketId);
        Task<ApiResponse<List<SupportTicket>>> GetSupportTicketByUserId(Guid userId);
        Task<ApiResponse<List<SupportTicketResponse>>> GetAllSupportTicketsWithCurrentVersionAndReview();
        Task<ApiResponse<SupportTicket>> ResponseTicket(ResponseSupportTicket response, Guid id);
        Task<ApiResponse<SupportTicket>> ChangeTicketStatus(UpdateStatusTicket ticket, Guid userId, Guid id);
    }
}
