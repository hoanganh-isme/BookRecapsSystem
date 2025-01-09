using BusinessObject.Models;
using BusinessObject.ViewModels.PlayLists;
using Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IPlayListService
    {
        Task<ApiResponse<PlayList>> CreatePlayList(CreatePlayList request);
        Task<ApiResponse<List<MyPlayListDTO>>> GetPlayListsByUserId(Guid userId);
        Task<ApiResponse<bool>> DeletePlayList(Guid userId, Guid playListId);
        Task<ApiResponse<bool>> SoftDeletePlayList(Guid userId, Guid playListId);
        Task<ApiResponse<PlayList>> GetPlayListById(Guid playListItemId);
        Task<ApiResponse<PlayListItem>> UpdatePlayList(Guid userId, Guid playListId, UpdatePlayList request);
        Task<ApiResponse<List<PlayList>>> GetAllPlayLists(Guid playListId);

    }
}
