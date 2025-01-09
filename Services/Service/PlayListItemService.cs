using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.PlayLists;
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
    public class PlayListItemService : IPlayListItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public PlayListItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        // Thêm Recap vào một PlayList cụ thể
        public async Task<ApiResponse<PlayListItem>> AddRecapToPlayList(CreatePlayListItem request)
        {
            // Kiểm tra nếu Playlist tồn tại
            var playList = await _unitOfWork.PlayListRepository.GetByIdAsync(request.PlayListId);
            if (playList == null)
            {
                return new ApiResponse<PlayListItem>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Playlist.",
                    Errors = new[] { "Invalid PlayList ID." }
                };
            }

            // Kiểm tra Recap có tồn tại hay không
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(request.RecapId);
            if (recap == null)
            {
                return new ApiResponse<PlayListItem>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy recap.",
                    Errors = new[] { "Invalid Recap ID." }
                };
            }

            // Kiểm tra xem RecapId đã tồn tại trong PlayList chưa
            var isRecapExists = await _unitOfWork.PlayListItemRepository
                .AnyAsync(item => item.PlayListId == request.PlayListId && item.RecapId == request.RecapId);

            if (isRecapExists)
            {
                return new ApiResponse<PlayListItem>
                {
                    Succeeded = false,
                    Message = "Recap này đã tồn tại.",
                    Errors = new[] { "Recap is already added to this PlayList." }
                };
            }

            // Lấy tất cả PlayListItems hiện có
            var existingItems = await _unitOfWork.PlayListItemRepository
                .GetAllAsync(item => item.PlayListId == request.PlayListId);

            // Đặt OrderPlayList cho PlayListItem mới
            // Nếu không có mục nào hiện có, đặt thứ tự là 1
            request.OrderPlayList = existingItems.Any() ? existingItems.Max(item => item.OrderPlayList) + 1 : 1;
           

            // Sử dụng AutoMapper để map ViewModel sang Entity
            var playListItem = _mapper.Map<PlayListItem>(request);    

            // Thêm PlayListItem mới vào PlayList
            await _unitOfWork.PlayListItemRepository.AddAsync(playListItem);

            // Cập nhật thứ tự cho tất cả các PlayListItems hiện có
            foreach (var item in existingItems)
            {
                item.OrderPlayList = item.OrderPlayList >= request.OrderPlayList ? item.OrderPlayList + 1 : item.OrderPlayList;
                _unitOfWork.PlayListItemRepository.Update(item);
            }

            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<PlayListItem>
            {
                Succeeded = true,
                Message = "Thêm Recap vào Playlist thành công.",
                Data = playListItem
            };
        }
        public async Task<ApiResponse<bool>> DeletePlayListItem(Guid playListItemId)
        {
            // Kiểm tra nếu PlayListItem tồn tại
            var playListItem = await _unitOfWork.PlayListItemRepository.GetByIdAsync(playListItemId);
            if (playListItem == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "PlayListItem not found.",
                    Errors = new[] { "Invalid PlayListItem ID." }
                };
            }

            _unitOfWork.PlayListItemRepository.Delete(playListItem);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>(true);
        }
        public async Task<ApiResponse<PlayListItem>> GetPlayListItemById(Guid playListItemId)
        {
            var playListItem = await _unitOfWork.PlayListItemRepository.GetByIdAsync(playListItemId);
            if (playListItem == null)
            {
                return new ApiResponse<PlayListItem>
                {
                    Succeeded = false,
                    Message = "PlayListItem not found.",
                    Errors = new[] { "Invalid PlayListItem ID." }
                };
            }

            return new ApiResponse<PlayListItem>(playListItem);
        }
        public async Task<ApiResponse<PlayListItem>> UpdatePlayListItem(Guid playListItemId, UpdatePlayListItem request)
        {
            // Kiểm tra nếu PlayListItem tồn tại
            var playListItem = await _unitOfWork.PlayListItemRepository.GetByIdAsync(playListItemId);
            if (playListItem == null)
            {
                return new ApiResponse<PlayListItem>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy PlaylistItem.",
                    Errors = new[] { "Invalid PlayListItem ID." }
                };
            }
            var existingItems = await _unitOfWork.PlayListItemRepository
                .GetAllAsync(item => item.PlayListId == request.PlayListId);
            if (request.OrderPlayList != 0)
            {
                if (existingItems.Any(k => k.OrderPlayList == request.OrderPlayList))
                {
                    return new ApiResponse<PlayListItem>
                    {
                        Succeeded = false,
                        Message = "Thứ tự đã có trong danh sách phát.",
                        Errors = new[] { "Duplicate order in the same RecapVersion." },
                        Data = null
                    };
                }
                playListItem.OrderPlayList = request.OrderPlayList;
            }
            // Cập nhật các thuộc tính của PlayListItem
            playListItem.RecapId = request.RecapId;
            playListItem.OrderPlayList = request.OrderPlayList;
            playListItem.PlayListId = request.PlayListId;

            _unitOfWork.PlayListItemRepository.Update(playListItem);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<PlayListItem>
            {
                Succeeded = true,
                Message = "Cập nhật Playlist thành công.",
                Data = playListItem
            };
        }
        public async Task<ApiResponse<List<PlayListItem>>> GetAllPlayListItems(Guid playListId)
        {
            // Kiểm tra nếu Playlist tồn tại
            var playList = await _unitOfWork.PlayListRepository.GetByIdAsync(playListId);
            if (playList == null)
            {
                return new ApiResponse<List<PlayListItem>>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Playlist.",
                    Errors = new[] { "Invalid PlayList ID." }
                };
            }

            // Lấy tất cả PlayListItems thuộc về PlayList
            var playListItems = await _unitOfWork.PlayListItemRepository.GetAllAsync(pi => pi.PlayListId == playListId);

            return new ApiResponse<List<PlayListItem>>(playListItems);
        }


    }
}
