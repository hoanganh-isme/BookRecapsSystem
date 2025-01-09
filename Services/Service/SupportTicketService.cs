using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Appeals;
using BusinessObject.ViewModels.SupportTicket;
using Microsoft.EntityFrameworkCore;
using Repository;
using Services.Interface;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupportTicketService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<SupportTicket>> CreateSupportTicket(CreateSupportTicketRequest supportTicketRequest)
        {
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(supportTicketRequest.RecapId);
            if(recap == null)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Recap not available.",
                    Errors = new[] { "Database error while saving support ticket." }
                };
            }
            // Map dữ liệu từ CreateSupportTicketRequest sang đối tượng SupportTicket
            var newTicket = _mapper.Map<SupportTicket>(supportTicketRequest);
            newTicket.Status = SupportStatus.Pending;
            // Lưu SupportTicket vào cơ sở dữ liệu
            await _unitOfWork.SupportTicketRepository.AddAsync(newTicket);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Failed to create support ticket.",
                    Errors = new[] { "Database error while saving support ticket." }
                };
            }

            return new ApiResponse<SupportTicket>
            {
                Succeeded = true,
                Message = "Tạo hỗ trợ thành công.",
                Data = newTicket
            };
        }

        public async Task<ApiResponse<SupportTicket>> UpdateSupportTicket(UpdateSupportTicketRequest supportTicketRequest, Guid userId, Guid id)
        {
            var existingTicket = await _unitOfWork.SupportTicketRepository.GetByIdAsync(id);
            if (existingTicket == null)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Support ticket not found.",
                    Errors = new[] { "The specified support ticket does not exist." }
                };
            }

            // Kiểm tra xem người dùng có phải là người tạo SupportTicket không
            if (existingTicket.UserId != userId)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới được cập nhật."
                };
            }

            // Cập nhật thông tin SupportTicket
            _mapper.Map(supportTicketRequest, existingTicket);
            _unitOfWork.SupportTicketRepository.Update(existingTicket);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<SupportTicket>
            {
                Succeeded = true,
                Message = "Support ticket updated successfully.",
                Data = existingTicket
            };
        }

        public async Task<ApiResponse<SupportTicket>> ResponseTicket(ResponseSupportTicket response, Guid id)
        {
            var ticket = await _unitOfWork.SupportTicketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "SupportTicket not found.",
                    Errors = new[] { "Invalid AppealId." }
                };
            }

            if (ticket.Status != SupportStatus.Pending)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Chỉ trả lời được hỗ trợ đang chờ.",
                    Errors = new[] { "Staff can only respond to 'pending' supportticket assigned to them." }
                };
            }

            // Update response and status
            _mapper.Map(response, ticket);
            ticket.Status = SupportStatus.Closed;

            _unitOfWork.SupportTicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<SupportTicket>
            {
                Succeeded = true,
                Message = "Supportticket responded and resolved.",
                Data = ticket
            };
        }
        public async Task<ApiResponse<SupportTicket>> ChangeTicketStatus(UpdateStatusTicket ticket, Guid userId, Guid id)
        {
            // Tìm kiếm appeal dựa trên ID
            var existingticket = await _unitOfWork.SupportTicketRepository.GetByIdAsync(id);

            if (existingticket == null)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Ticket not found.",
                    Errors = new[] { "Invalid ticketId." }
                };
            }

            // Kiểm tra nếu người dùng hiện tại là contributor của appeal
            if (existingticket.UserId != userId)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Bạn không có quyền cập nhật.",
                    Errors = new[] { "Access denied." }
                };
            }

            // Kiểm tra trạng thái hiện tại của appeal, chỉ cho phép cập nhật nếu trạng thái là Open
            if (existingticket.Status != SupportStatus.Open)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Chỉ hỗ trợ đang mở mới được cập nhật.",
                    Errors = new[] { "Cannot modify ticket status." }
                };
            }

            // Cập nhật trạng thái appeal
            ticket.Status = SupportStatus.Pending;
            existingticket.Status = ticket.Status;
            _unitOfWork.SupportTicketRepository.Update(existingticket);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (!saveResult)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Failed to update appeal status.",
                    Errors = new[] { "Database error while saving." }
                };
            }

            return new ApiResponse<SupportTicket>
            {
                Succeeded = true,
                Message = "Cập nhật trạng thái hỗ trợ thành công.",
                Data = existingticket
            };
        }
        public async Task<ApiResponse<bool>> DeleteSupportTicket(Guid supportTicketId, Guid userId)
        {
            var existingTicket = await _unitOfWork.SupportTicketRepository.GetByIdAsync(supportTicketId);
            if (existingTicket == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Support ticket not found.",
                    Errors = new[] { "The specified support ticket does not exist." },
                    Data = false
                };
            }

            if (existingTicket.UserId != userId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới xóa được hỗ trợ này.",
                    Data = false
                };
            }

            _unitOfWork.SupportTicketRepository.Delete(existingTicket);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa hỗ trợ thành công.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteSupportTicket(Guid supportTicketId, Guid userId)
        {
            var existingTicket = await _unitOfWork.SupportTicketRepository.GetByIdAsync(supportTicketId);
            if (existingTicket == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Support ticket not found.",
                    Errors = new[] { "The specified support ticket does not exist." },
                    Data = false
                };
            }

            if (existingTicket.UserId != userId)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Chỉ người tạo mới xóa được hỗ trợ này.",
                    Data = false
                };
            }

            _unitOfWork.SupportTicketRepository.SoftDelete(existingTicket);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa hỗ trợ thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<SupportTicket>> GetSupportTicketById(Guid supportTicketId)
        {
            var ticket = await _unitOfWork.SupportTicketRepository.GetByIdAsync(supportTicketId);
            if (ticket == null)
            {
                return new ApiResponse<SupportTicket>
                {
                    Succeeded = false,
                    Message = "Support ticket not found.",
                    Errors = new[] { "The specified support ticket does not exist." }
                };
            }

            return new ApiResponse<SupportTicket>
            {
                Succeeded = true,
                Message = "Support ticket retrieved successfully.",
                Data = ticket
            };
        }

        public async Task<ApiResponse<List<SupportTicket>>> GetSupportTicketByUserId(Guid userId)
        {
            var tickets = await _unitOfWork.SupportTicketRepository.GetAllAsync(t => t.UserId == userId);
            if (!tickets.Any())
            {
                return new ApiResponse<List<SupportTicket>>
                {
                    Succeeded = false,
                    Message = "No support tickets found for this user.",
                    Data = new List<SupportTicket>()
                };
            }

            return new ApiResponse<List<SupportTicket>>
            {
                Succeeded = true,
                Message = "Support tickets retrieved successfully.",
                Data = tickets.ToList()
            };
        }

        public async Task<ApiResponse<List<SupportTicketResponse>>> GetAllSupportTicketsWithCurrentVersionAndReview()
        {
            // Query SupportTicket và include Recap, CurrentVersion và Review
            var supportTickets = await _unitOfWork.SupportTicketRepository.QueryWithIncludes()
                .Include(st => st.Recaps)
                    .ThenInclude(recap => recap.CurrentVersion)
                        .ThenInclude(currentVersion => currentVersion.Review)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();

            // Nếu không có SupportTicket nào
            if (!supportTickets.Any())
            {
                return new ApiResponse<List<SupportTicketResponse>>
                {
                    Succeeded = false,
                    Message = "No support tickets found.",
                    Data = new List<SupportTicketResponse>()
                };
            }

            // Map dữ liệu thành response
            var response = supportTickets.Select(st => new SupportTicketResponse
            {
                SupportTicketId = st.Id,
                Category = st.Category,
                Description = st.Description,
                Status = st.Status,
                Response = st.Response,
                UserId = st.UserId,
                RecapId = st.Recaps.Id,
                RecapName = st.Recaps.Name,
                CreatedAt = st.CreatedAt,
                UpdatedAt = st.UpdatedAt,
                CurrentVersion = st.Recaps.CurrentVersion != null ? new RecapVersionResponse
                {
                    VersionId = st.Recaps.CurrentVersion.Id,
                    VersionName = st.Recaps.CurrentVersion.VersionName,
                    Status = st.Recaps.CurrentVersion.Status,
                    Review = st.Recaps.CurrentVersion.Review != null ? new ReviewResponse
                    {
                        ReviewId = st.Recaps.CurrentVersion.Review.Id,
                        Comments = st.Recaps.CurrentVersion.Review.Comments,
                        StaffId = st.Recaps.CurrentVersion.Review.StaffId
                    } : null
                } : null
            }).ToList();

            // Trả về response
            return new ApiResponse<List<SupportTicketResponse>>
            {
                Succeeded = true,
                Message = "Support tickets retrieved successfully.",
                Data = response
            };
        }

    }

}
