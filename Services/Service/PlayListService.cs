using AutoMapper;
using BusinessObject.Models;
using BusinessObject.ViewModels.PlayLists;
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
    public class PlayListService : IPlayListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlayListService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Thêm mới PlayList
        public async Task<ApiResponse<PlayList>> CreatePlayList(CreatePlayList request)
        {
            var playList = _mapper.Map<PlayList>(request);

            await _unitOfWork.PlayListRepository.AddAsync(playList);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<PlayList>
            {
                Succeeded = true,
                Message = "Tạo Playlist thành công.",
                Data = playList
            };
        }


        public async Task<ApiResponse<List<MyPlayListDTO>>> GetPlayListsByUserId(Guid userId)
        {
            // Truy vấn PlayList và các PlayListItems kèm thông tin Recap, Book, Contributor
            var playLists = await _unitOfWork.PlayListRepository.GetPlayListsWithDetailsByUserId(userId);

            // Kiểm tra nếu không có PlayLists
            if (playLists == null || !playLists.Any())
            {
                return new ApiResponse<List<MyPlayListDTO>>
                {
                    Succeeded = false,
                    Message = "No PlayLists found for the user.",
                    Errors = new[] { "User has no PlayLists." }
                };
            }

            var myPlayListDtos = playLists
                        .Select(pl => new MyPlayListDTO
                        {
                            PlayListId = pl.Id,
                            PlayListName = pl.PlayListName,
                            PlayListItems = pl.PlayListItems.Select(pli => new MyPlayListItemDTO
                            {
                                PlaylistItemId = pli.Id,
                                RecapId = pli.RecapId,
                                OrderPlayList = pli.OrderPlayList,
                                RecapName = pli.Recap?.Name,
                                isPremium = pli.Recap?.isPremium,
                                LikesCount = pli.Recap?.LikesCount,
                                ViewsCount = pli.Recap?.ViewsCount,
                                ContributorName = pli.Recap?.Contributor?.FullName,
                                ContributorImage = pli.Recap?.Contributor?.ImageUrl,
                                BookName = pli.Recap?.Book?.Title,
                                BookImage = pli.Recap?.Book?.CoverImage,
                                Authors = pli.Recap?.Book?.Authors.Select(a => new AuthorPlayListDTO
                                    {
                                    AuthorId = a.Id,
                                    AuthorName = a.Name,
                                    AuthorDescription = a.Description,
                                    AuthorImage = a.Image
                                }).ToList()
                            }).ToList()
                        })
                        .ToList();
            return new ApiResponse<List<MyPlayListDTO>>
            {
                Succeeded = true,
                Message = "PlayLists retrieved successfully.",
                Data = myPlayListDtos
            };
        }


        public async Task<ApiResponse<bool>> DeletePlayList(Guid userId, Guid playListId)
        {
            // Lấy playlist theo playlistId và userId
            var playList = await _unitOfWork.PlayListRepository
                .FirstOrDefaultAsync(pl => pl.Id == playListId && pl.UserId == userId);

            // Kiểm tra nếu không tìm thấy playlist hoặc không thuộc về user đó
            if (playList == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy Playlist hoặc bạn không có quyền để xóa nó.",
                    Errors = new[] { "Invalid PlayList ID or User does not own this PlayList." }
                };
            }
            var playListItems = await _unitOfWork.PlayListItemRepository.GetAllPlaylistItemByPlaylistId(playListId);
            foreach (var playListItem in playListItems)
            {
                _unitOfWork.PlayListItemRepository.Delete(playListItem);
            }
            // Xóa playlist
            _unitOfWork.PlayListRepository.Delete(playList);
            await _unitOfWork.SaveChangesAsync();

            // Trả về phản hồi
            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa playlist thành công."
            };
        }
        public async Task<ApiResponse<bool>> SoftDeletePlayList(Guid userId, Guid playListId)
        {
            // Lấy playlist theo playlistId và userId
            var playList = await _unitOfWork.PlayListRepository
                .FirstOrDefaultAsync(pl => pl.Id == playListId && pl.UserId == userId);

            // Kiểm tra nếu không tìm thấy playlist hoặc không thuộc về user đó
            if (playList == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "PlayList not found or you do not have permission to delete it.",
                    Errors = new[] { "Invalid PlayList ID or User does not own this PlayList." }
                };
            }

            // Xóa playlist
            _unitOfWork.PlayListRepository.SoftDelete(playList);
            await _unitOfWork.SaveChangesAsync();

            // Trả về phản hồi
            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa playlist thành công."
            };
        }


        public async Task<ApiResponse<List<PlayList>>> GetAllPlayLists(Guid userId)
        {
            // Lấy tất cả PlayLists của User
            var playLists = await _unitOfWork.PlayListRepository.GetAllAsync(p => p.UserId == userId);
            if (!playLists.Any())
            {
                return new ApiResponse<List<PlayList>>
                {
                    Succeeded = false,
                    Message = "No PlayLists found for the user.",
                    Errors = new[] { "User has no PlayLists." }
                };
            }

            return new ApiResponse<List<PlayList>>(playLists.ToList());
        }

        public async Task<ApiResponse<PlayList>> GetPlayListById(Guid playListId)
        {
            var playList = await _unitOfWork.PlayListRepository.GetByIdAsync(playListId);
            if (playList == null)
            {
                return new ApiResponse<PlayList>
                {
                    Succeeded = false,
                    Message = "PlayList not found.",
                    Errors = new[] { "Invalid PlayList ID." }
                };
            }

            return new ApiResponse<PlayList>(playList);
        }

        public async Task<ApiResponse<PlayListItem>> UpdatePlayList(Guid userId, Guid playListId, UpdatePlayList request)
        {
            // Lấy playlist theo playlistId và userId
            var playList = await _unitOfWork.PlayListRepository
                .QueryWithIncludes(pl => pl.PlayListItems)
                .FirstOrDefaultAsync(pl => pl.Id == playListId && pl.UserId == userId);

            // Kiểm tra nếu không tìm thấy playlist hoặc không thuộc về user đó
            if (playList == null)
            {
                return new ApiResponse<PlayListItem>
                {
                    Succeeded = false,
                    Message = "PlayList not found or you do not have permission to update it.",
                    Errors = new[] { "Invalid PlayList ID or User does not own this PlayList." }
                };
            }

            // Cập nhật các thuộc tính của PlayList
            playList.PlayListName = request.PlayListName;

            // Lưu thay đổi
            _unitOfWork.PlayListRepository.Update(playList);
            await _unitOfWork.SaveChangesAsync();

            // Trả về phản hồi
            return new ApiResponse<PlayListItem>(_mapper.Map<PlayListItem>(playList));
        }

    }

}
