using BusinessObject.Data;
using BusinessObject.Models;
using Core.Auth.Services;
using Core.Enums;
using Core.Models.UserModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Interface;

namespace Core.Auth.Controllers
{
    [Route("api")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IPublisherService _publisherService;

        public RolesController(RoleManager<IdentityRole<Guid>> roleManager, 
            UserManager<User> userManager, 
            AppDbContext context, 
            ICurrentUserService currentUser,
            IPublisherService publisherService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _currentUser = currentUser;
            _publisherService = publisherService;
        }

        // /api/Roles
        [HttpGet("roles")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(new UserResponseManager
            {
                IsSuccess = true,
                Message = roles,
            });
        }

        // /api/Roles/{RoleName}
        [HttpPost("roles")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> AddRole(string roleName)
        {
            if (roleName != null)
            {
                var newRole = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName.Trim()));
            }
            return Ok(new UserResponseManager
            {
                IsSuccess = true,
                Message = "Role '" + roleName + "' has been added to Role Manager!",
            });
        }

        // /api/Roles/{RoleName}
        [HttpDelete("roles")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> RemoveRole(string roleName)
        {
            if (roleName != null)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                var removeRole = await _roleManager.DeleteAsync(role);
            }
            return Ok(new UserResponseManager
            {
                IsSuccess = true,
                Message = "Role '" + roleName + "' has been Removed from Role Manager!",
            });
        }

        // /api/userRoles/{id}
        [HttpGet("roles/userId")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUserRolebyId(Guid userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId.ToString());
            if (existingUser != null)
            {
                var roles = await _userManager.GetRolesAsync(existingUser);
                return Ok(roles);
            };
            return BadRequest("User not found!");

        }

        // /api/AddUserRole/{RoleName}
        [HttpPost("add-user-role/role-name")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> AddUserRole(Guid userId, string userRole)
        {
            var existingUser = await _userManager.FindByIdAsync(userId.ToString());
            if (userRole != null)
            {
                if (await _roleManager.RoleExistsAsync(userRole))
                {
                    await _userManager.AddToRoleAsync(existingUser, userRole);
                    var addedRoles = await _userManager.GetRolesAsync(existingUser);
                    return Ok(addedRoles);
                };
                return BadRequest("Role does not exits!");
            }
            return NotFound("Please fill the required fields ! ");
        }

        // /api/RemoveUserRole/{RoleName}
        [HttpDelete("remove-user-role/user-role")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> RemoveUserRole(Guid userId, string userRole)
        {
            var existingUser = await _userManager.FindByIdAsync(userId.ToString());
            if (userRole != null)
            {
                if (await _roleManager.RoleExistsAsync(userRole))
                {
                    await _userManager.RemoveFromRoleAsync(existingUser, userRole);
                    var addedRoles = await _userManager.GetRolesAsync(existingUser);
                    return Ok(addedRoles);
                };
                return BadRequest("Role does not exits!");
            }
            return NotFound("Please fill the required fields ! ");
        }

        // /api/UpdateUserRole/{RoleType}
        [HttpPut("update-user-role/{roleType}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, Roles roleType)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(userId.ToString());
                if (roleType != null)
                {
                    var roles = await _userManager.GetRolesAsync(existingUser);
                    if (roles.Contains(Roles.Publisher.ToString()))
                    {
                        return BadRequest("User already has Publisher role. Cannot update.");
                    }
                    await _userManager.RemoveFromRolesAsync(existingUser, roles.ToArray());
                    await _userManager.AddToRoleAsync(existingUser, roleType.ToString());                  
                    var addedRoles = await _userManager.GetRolesAsync(existingUser);
                    if (roleType == Roles.Publisher)
                    {
                    var response = await _publisherService.CreatePubliser(userId);
                        if (!response.Succeeded)
                        {
                            return BadRequest(response);
                        }
                    return Ok(response);
                    }
                    return Ok(addedRoles);
                }
                return NotFound("Please fill the required fields ! ");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPut("self-update-role")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Customer")]
        public async Task<IActionResult> SelfUpdateRole()
        {
            try
            {
                // Lấy ID của người dùng hiện tại từ JWT
                var userId = _currentUser.GetCurrentUserId();

                // Tìm kiếm người dùng
                var existingUser = await _userManager.FindByIdAsync(userId.ToString());
                existingUser.isContributor = true;
                if (existingUser == null)
                {
                    return NotFound("User not found!");
                }

                // Kiểm tra vai trò hiện tại của người dùng
                var roles = await _userManager.GetRolesAsync(existingUser);

                // Nếu người dùng đã có vai trò Contributor, không cần cập nhật
                if (roles.Contains(Roles.Contributor.ToString()))
                {
                    return BadRequest("Bạn đã là Contributor.");
                }

                // Xóa tất cả các vai trò hiện tại (nếu bạn muốn cho phép người dùng chỉ có vai trò Contributor)
                await _userManager.RemoveFromRolesAsync(existingUser, roles.ToArray());

                // Thêm vai trò Contributor
                await _userManager.AddToRoleAsync(existingUser, Roles.Contributor.ToString());

                // Lấy lại vai trò của người dùng sau khi cập nhật
                var addedRoles = await _userManager.GetRolesAsync(existingUser);

                return Ok(new UserResponseManager
                {
                    IsSuccess = true,
                    Message = "Bạn đã trở thành Contributor!",
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        // /api/LockUserAccount/{id}
        [HttpPut("lock-user-account")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> LockUserAccount(Guid userId)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(userId.ToString());
                if (existingUser != null)
                {
                    existingUser.LockoutEnabled = true;
                    existingUser.LockoutEnd = DateTime.Now.AddYears(100);
                    await _userManager.UpdateAsync(existingUser);
                    var userTokens = await _context.Tokens.Where(t => t.UserId == userId).ToListAsync();
                    _context.Tokens.RemoveRange(userTokens);
                    var currentUser = _currentUser.GetCurrentUserId();
                    await _context.SaveChangesAsync(Guid.Parse(currentUser));
                    return Ok("User Account has been locked!");
                }
                return BadRequest("User not found!");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // /api/UnlockUserAccount/{id}
        [HttpPut("unlock-user-account")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperAdmin")]
        public async Task<IActionResult> UnlockUserAccount(Guid userId)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(userId.ToString());
                if (existingUser != null)
                {
                    existingUser.LockoutEnabled = false;
                    existingUser.LockoutEnd = null;
                    await _userManager.UpdateAsync(existingUser);
                    return Ok("User Account has been unlocked!");
                }
                return BadRequest("User not found!");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}