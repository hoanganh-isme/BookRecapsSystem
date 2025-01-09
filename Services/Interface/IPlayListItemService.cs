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
    public interface IPlayListItemService
    {
        Task<ApiResponse<PlayListItem>> AddRecapToPlayList(CreatePlayListItem request);
        Task<ApiResponse<bool>> DeletePlayListItem(Guid playListItemId);
        Task<ApiResponse<PlayListItem>> GetPlayListItemById(Guid playListItemId);
        Task<ApiResponse<PlayListItem>> UpdatePlayListItem(Guid playListItemId, UpdatePlayListItem request);
        Task<ApiResponse<List<PlayListItem>>> GetAllPlayListItems(Guid playListId);
    }
}
