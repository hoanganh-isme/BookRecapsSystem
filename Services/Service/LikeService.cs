using BusinessObject.Models;
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
    public class LikeService : ILikeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LikeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Thêm Like và tăng LikesCount
        public async Task<ApiResponse<bool>> AddLikeAsync(Guid recapId, Guid userId)
        {
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Recap not found.",
                    Data = false
                };
            }

            // Kiểm tra nếu người dùng đã like recap này
            var existingLike = await _unitOfWork.LikeRepository.FindLikeByUserAndRecapAsync(userId, recapId);
            if (existingLike != null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Bạn đã thích recap này rồi.",
                    Data = false
                };
            }

            // Thêm like mới
            var like = new Like
            {
                RecapId = recapId,
                UserId = userId,
                LikeAt = DateTime.UtcNow
            };

            await _unitOfWork.LikeRepository.AddAsync(like);

            // Tăng LikesCount
            recap.LikesCount = (recap.LikesCount ?? 0) + 1;
            _unitOfWork.RecapRepository.Update(recap);

            var result = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = result,
                Message = result ? "Recap liked successfully." : "Failed to like recap.",
                Data = result
            };
        }


        // Xóa Like và giảm LikesCount
        public async Task<ApiResponse<bool>> RemoveLikeAsync(Guid recapId, Guid userId)
        {
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Recap not found.",
                    Data = false
                };
            }
            var like = await _unitOfWork.LikeRepository
                .FindLikeByUserAndRecapAsync(userId, recapId);

            if (like == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Like not found.",
                    Data = false
                };
            }

            // Xóa like
            _unitOfWork.LikeRepository.Delete(like);

            // Giảm LikesCount
            recap.LikesCount = (recap.LikesCount > 0) ? recap.LikesCount - 1 : 0;
            _unitOfWork.RecapRepository.Update(recap);

            var result = await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Succeeded = result,
                Message = result ? "Like removed successfully." : "Failed to remove like.",
                Data = result
            };
        }
        public async Task<ApiResponse<int>> GetLikesCountByRecapIdAsync(Guid recapId)
        {
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<int>
                {
                    Succeeded = false,
                    Message = "Recap not found.",
                    Data = 0
                };
            }

            return new ApiResponse<int>
            {
                Succeeded = true,
                Message = "Successfully retrieved like count.",
                Data = recap.LikesCount ?? 0
            };
        }

    }

}
