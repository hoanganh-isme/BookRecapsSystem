using BusinessObject.ViewModels.PlayLists;
using Core.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace Core.Controllers
{
    [ApiController]
    [Route("api/playlists")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PlayListController : ControllerBase
    {
        private readonly IPlayListService _playListService;
        private readonly IPlayListItemService _playListItemService;
        private readonly ICurrentUserService _currentUserService;

        public PlayListController(IPlayListService playListService, 
            ICurrentUserService currentUserService,
            IPlayListItemService playListItemService)
        {
            _playListService = playListService;
            _currentUserService = currentUserService;
            _playListItemService = playListItemService;
        }

        // API tạo PlayList mới
        [HttpPost("createPlaylist")]
        public async Task<IActionResult> CreatePlayList([FromBody] CreatePlayList createPlayListViewModel)
        {
            // Lấy ID của người dùng hiện tại
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid parsedUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            // Gán ID người dùng vào ViewModel
            createPlayListViewModel.UserId = parsedUserId;

            // Gọi service để tạo PlayList
            var response = await _playListService.CreatePlayList(createPlayListViewModel);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // API thêm Recap vào một PlayList cụ thể
        [HttpPost("{playListId}/add-recap/{recapId}")]
        public async Task<IActionResult> AddRecapToPlayList([FromRoute] Guid playListId, [FromRoute] Guid recapId)
        {
            // Tạo ViewModel cho PlayListItem
            var createPlayListItem = new CreatePlayListItem
            {
                PlayListId = playListId,
                RecapId = recapId,
            };

            // Gọi service để thêm Recap vào PlayList
            var response = await _playListItemService.AddRecapToPlayList(createPlayListItem);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // API lấy tất cả PlayLists của người dùng hiện tại
        [HttpGet("my-playlists")]
        public async Task<IActionResult> GetMyPlayLists()
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid parsedUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            var response = await _playListService.GetPlayListsByUserId(parsedUserId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpPut("updateplaylist/{playListId}")]
        public async Task<IActionResult> UpdatePlayList(Guid playListId, [FromBody] UpdatePlayList request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid parsedUserId))
            {
                return BadRequest("User is not authenticated.");
            }

            var response = await _playListService.UpdatePlayList(parsedUserId, playListId, request);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpPut("updateplaylistitem/{playListItemId}")]
        public async Task<IActionResult> UpdatePlayListItem(Guid playListItemId, [FromBody] UpdatePlayListItem request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _playListItemService.UpdatePlayListItem(playListItemId, request);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("deleteplaylistitem/{playListItemId}")]
        public async Task<IActionResult> DeletePlayListItem(Guid playListItemId)
        {
            var response = await _playListItemService.DeletePlayListItem(playListItemId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("getplaylistitembyid/{playListItemId}")]
        public async Task<IActionResult> GetPlayListItemById(Guid playListItemId)
        {
            var response = await _playListItemService.GetPlayListItemById(playListItemId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }


        [HttpGet("getallplaylistitem/{playListId}")]
        public async Task<IActionResult> GetAllPlayListItems(Guid playListId)
        {
            var response = await _playListItemService.GetAllPlayListItems(playListId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        [HttpDelete("deleteplaylist/{playListId}")]
        public async Task<IActionResult> DeletePlayList(Guid playListId)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid parsedUserId))
            {
                return BadRequest("User is not authenticated.");
            }
            var response = await _playListService.DeletePlayList(parsedUserId, playListId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpDelete("softdeleteplaylist/{playListId}")]
        public async Task<IActionResult> SoftDeletePlayList(Guid playListId)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid parsedUserId))
            {
                return BadRequest("User is not authenticated.");
            }
            var response = await _playListService.SoftDeletePlayList(parsedUserId, playListId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("getallplaylistbyuser/{userId}")]
        public async Task<IActionResult> GetAllPlayLists(Guid userId)
        {
            var response = await _playListService.GetAllPlayLists(userId);
            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("getplaylistbyid/{playListId}")]
        public async Task<IActionResult> GetPlayListById(Guid playListId)
        {
            var response = await _playListService.GetPlayListById(playListId);
            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        

    }
}
