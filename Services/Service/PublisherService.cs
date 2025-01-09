using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.Publisher;
using Microsoft.AspNetCore.Identity;
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
    public class PublisherService : IPublisherService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public PublisherService(IMapper mapper, IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<ApiResponse<Publisher>> CreatePubliser(Guid userId)
        {
            var existPublisher = await _unitOfWork.PublisherRepository.GetPublisherByUserIdAsync(userId);
            var user = _userManager.FindByIdAsync(userId.ToString());
            if (existPublisher == null)
            {
                var publisher = new CreatePublisher()
                {
                    UserId = userId,
                    ContactInfo = user.Result.Email
                    
                };
                var newPublisher = _mapper.Map<Publisher>(publisher);
                await _unitOfWork.PublisherRepository.AddAsync(newPublisher);
                await _unitOfWork.SaveChangesAsync();
                return new ApiResponse<Publisher>
                {
                    Succeeded = true,
                    Message = "Tạo publisher thành công.",
                    Data = newPublisher
                };
            }
            else{
                return new ApiResponse<Publisher>
                {
                    Succeeded = false,
                    Message = "Tài khoản này đã được tạo Publisher.",
                    Data = null
                };
            }
        }
        public async Task<ApiResponse<Publisher>> GetPublisherByUserId(Guid userId)
        {
            var publisher = await _unitOfWork.PublisherRepository.GetPublisherByUserIdAsync(userId);
            if (publisher == null)
            {
                return new ApiResponse<Publisher>
                {
                    Succeeded = false,
                    Message = "Publisher not found for the given user ID."
                };
            }

            return new ApiResponse<Publisher>
            {
                Succeeded = true,
                Data = publisher,
                Message = "Publisher retrieved successfully."
            };
        }

        public async Task<ApiResponse<Publisher>> GetPublisherById(Guid publisherId)
        {
            var publisher = await _unitOfWork.PublisherRepository.GetByIdAsync(publisherId);
            if (publisher == null)
            {
                return new ApiResponse<Publisher>
                {
                    Succeeded = false,
                    Message = "Publisher not found."
                };
            }

            return new ApiResponse<Publisher>
            {
                Succeeded = true,
                Data = publisher,
                Message = "Publisher retrieved successfully."
            };
        }

        public async Task<ApiResponse<List<Publisher>>> GetAllPublisher()
        {
            var publishers = await _unitOfWork.PublisherRepository.GetAllAsync();
            if (publishers == null || !publishers.Any())
            {
                return new ApiResponse<List<Publisher>>
                {
                    Succeeded = false,
                    Message = "No publishers found."
                };
            }

            return new ApiResponse<List<Publisher>>
            {
                Succeeded = true,
                Data = publishers,
                Message = "Publishers retrieved successfully."
            };
        }

        public async Task<ApiResponse<Publisher>> UpdatePublisherInfo(Guid publisherId, UpdatePublisher publisherUpdate)
        {
            var existingPublisher = await _unitOfWork.PublisherRepository.GetByIdAsync(publisherId);
            if (existingPublisher == null)
            {
                return new ApiResponse<Publisher>
                {
                    Succeeded = false,
                    Message = "Publisher not found."
                };
            }

            _mapper.Map(publisherUpdate, existingPublisher);
            _unitOfWork.PublisherRepository.Update(existingPublisher);
            var saveResult = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<Publisher>
            {
                Succeeded = saveResult,
                Data = existingPublisher,
                Message = saveResult ? "Publisher information updated successfully." : "Failed to update publisher information."
            };
        }
    }
}
