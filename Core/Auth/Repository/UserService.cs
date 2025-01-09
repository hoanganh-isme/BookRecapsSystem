using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessObject.Data;
using BusinessObject.Models;
using Core.Helpers;
using Core.Infrastructure.FileStorage;
using Core.Infrastructure.Notifications;
using Core.Infrastructure.SpeedSMS;
using Core.Models;
using Core.Models.Personal;
using Core.Models.UserModels;
using Core.Services;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using static BusinessObject.Enums.BasicNotification;
using Core.Enums;
using Core.Auth.Services;
using Services.Service.Helper;
using Microsoft.Extensions.Options;
using Services;

namespace Core.Repository
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMailService _mailService;
        private readonly IFileStorageService _fileStorage;
        private readonly ISpeedSMSService _speedSMSService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly GoogleCloudService _googleCloudService;

        public UserService(AppDbContext context,
                           UserManager<User> userManager,
                           RoleManager<IdentityRole<Guid>> roleManager,
                           IMailService mailService,
                           IFileStorageService fileStorage,
                           ISpeedSMSService speedSMSService,
                           INotificationService notificationService,
                           ILogger<UserService> logger,
                           IMapper mapper,
                           IConfiguration config,
                           IOptions<GoogleSettings> googleSettings,
                           ICurrentUserService currentUser,
                            IConfiguration configuration)
        {
            _context = context;
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _mailService = mailService;
            _fileStorage = fileStorage;
            _speedSMSService = speedSMSService;
            _notificationService = notificationService;
            _logger = logger;
            _mapper = mapper;
            _currentUser = currentUser;
            _googleCloudService = new GoogleCloudService(configuration);
        }

        //All Users
        public async Task<List<UserDetailsDto>> GetUsers()
        {
            var users = _userManager.Users
                .AsNoTracking()
                .Where(u => u.EmailConfirmed)
                .AsQueryable();
            var adminRoleId = _roleManager.Roles.FirstOrDefault(r => r.Name == "SuperAdmin")?.Id;
            if (adminRoleId != null)
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                users = users.Where(u => !adminUsers.Contains(u));
            }
            var usersDto = _mapper.Map<List<UserDetailsDto>>(users);
            foreach (var user in usersDto)
            {
                var User = await _userManager.FindByIdAsync(user.Id.ToString());
                var userRoles = await _userManager.GetRolesAsync(User);
                user.RoleType = DetermineRoleType(userRoles);
                user.IsAccountLocked = await _userManager.IsLockedOutAsync(await _userManager.FindByIdAsync(user.Id.ToString()));
            }
            return usersDto;
        }

        private Roles DetermineRoleType(IList<string> userRoles)
        {
            if (userRoles.Contains("SuperAdmin")) return Roles.SuperAdmin;
            if (userRoles.Contains("Staff")) return Roles.Staff;
            if (userRoles.Contains("Contributor")) return Roles.Contributor;
            if (userRoles.Contains("Publisher")) return Roles.Publisher;
            if (userRoles.Contains("Customer")) return Roles.Customer;
            return Roles.Guest;
        }

        //GetUserByID
        public async Task<ResponseManager> GetUserbyId(Guid id)
        {
            var userById = await _userManager.FindByIdAsync(id.ToString());
            return new ResponseManager
            {
                IsSuccess = true,
                Message = userById,
            };
        }
        public async Task<UserDetailsDto> GetProfileByUserId(Guid userId)
        {
            var user = await _userManager.Users
                            .AsNoTracking()
                            .Include(u => u.Subscriptions.OrderByDescending(s => s.EndDate))
                            .Where(u => u.Id == userId)
                            .FirstOrDefaultAsync();

            _ = user ?? throw new Exception("User not found");

            return _mapper.Map<UserDetailsDto>(user);
        }

        public async Task<Response<User>> GetUsersbyId(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return new Response<User>
                {
                    Succeeded = false,
                    Message = "User not found",
                };
            }
            return new Response<User>
            {
                Succeeded = true,
                Data = user,
                Message = "User found",
            };
        }

        public async Task<ResponseManager> CreateUser(RegisterUser model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Data provided is NULL");
            }

            // Kiểm tra mật khẩu và xác nhận mật khẩu có khớp không
            if (model.Password != model.ConfirmPassword)
            {
                return new ResponseManager
                {
                    Message = "Confirm password doesn't match the password",
                    IsSuccess = false,
                };
            }

            // Kiểm tra xem người dùng đã tồn tại chưa
            var userFound = await _userManager.FindByEmailAsync(model.Email);
            if (userFound != null)
            {
                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "User already exists!",
                };
            }

            // Tạo UserName
            var userName = RemoveUnicode(model.FullName.Replace(" ", ""));
            var normalizedUserName = userName.Normalize();

            // Kiểm tra xem username đã tồn tại chưa
            var existingUser = await _userManager.FindByNameAsync(normalizedUserName);
            if (existingUser != null)
            {
                // Nếu đã tồn tại, thêm hậu tố ngẫu nhiên vào username
                var uniqueUserName = normalizedUserName + Guid.NewGuid().ToString("N").Substring(0, 6); // Thêm 6 ký tự ngẫu nhiên
                userName = uniqueUserName; // Cập nhật lại UserName mới
            }

            var identityUser = new User
            {
                Email = model.Email,
                UserName = userName,
                FullName = model.FullName,
                NormalizedUserName = userName.Normalize(),
                NormalizedEmail = model.Email.Normalize(),
                PhoneNumber = model.PhoneNumber,
            };

            try
            {
                var result = await _userManager.CreateAsync(identityUser, model.Password);

                // Thêm vai trò cho người dùng mới
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(identityUser, "Customer");

                    // Gửi thông báo chào mừng
                    var notification = new BasicNotification
                    {
                        Title = "Welcome to Book Recaps",
                        Label = LabelType.Information,
                        Message = "This is the first page of the Book Recaps application. You can navigate through the application using the menu on the left. Enjoy your experience!"
                    };
                    await _notificationService.SendNotificationToUser(identityUser.Id.ToString(), notification, null, CancellationToken.None);

                    return new ResponseManager
                    {
                        IsSuccess = true,
                        Message = "User created successfully!",
                    };
                }

                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "Failed to create user.",
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi trong trường hợp có lỗi ngoài ý muốn
                return new ResponseManager()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        //Update User
        public async Task<Response<UserDetailsDto>> UpdateUser(Guid id, UpdateUserRequest user)
        {
            if (user != null)
            {
                var findUser = await _userManager.FindByIdAsync(id.ToString());
                if (findUser != null)
                {

                    try
                    {
                        findUser.FullName = user.FullName;
                        findUser.Gender = user.Gender;
                        findUser.Address = user.Address;
                        findUser.BankAccount = user.BankAccount;
                        findUser.BirthDate = user.BirthDate;
                        findUser.UpdatedAt = DateTime.Now;
                        await _userManager.UpdateAsync(findUser);
                        var updatedUserResponse = new User
                        {
                            Id = id,
                            UserName = findUser.UserName,
                            FullName = findUser.FullName,
                            Gender = findUser.Gender,
                            Address = findUser.Address,
                            BankAccount = findUser.BankAccount,
                            isContributor = findUser.isContributor,
                            Earning = findUser.Earning,
                            isOnboarded = findUser.isOnboarded,
                            ImageUrl = findUser.ImageUrl,
                            Subscriptions = findUser.Subscriptions,
                            EmailConfirmed = findUser.EmailConfirmed,
                            BirthDate = findUser.BirthDate,
                            NormalizedUserName = findUser.Email,
                            Email = findUser.Email,
                            NormalizedEmail = findUser.Email,
                            PhoneNumber = findUser.PhoneNumber,
                            UpdatedAt = findUser.UpdatedAt,
                        };
                        return new Response<UserDetailsDto>
                        {
                            Succeeded = true,
                            Message = "User updated successfully!",
                            Data = _mapper.Map<UserDetailsDto>(updatedUserResponse),
                        };

                    }
                    catch (Exception ex)
                    {
                        return new Response<UserDetailsDto>
                        {
                            Succeeded = false,
                            Message = ex.Message,
                            Data = null,
                        };
                    }
                }
                return new Response<UserDetailsDto>
                {
                    Succeeded = false,
                    Message = "User not found!",
                    Data = null,
                };
            }
            return new Response<UserDetailsDto>
            {
                Succeeded = false,
                Message = "updating property should not null!",
                Data = null,
            };
        }
        public async Task<Response<UserDetailsDto>> ChangeisOnboard(Guid id, ChangeisOnboard user)
        {
            if (user != null)
            {
                var findUser = await _userManager.FindByIdAsync(id.ToString());
                if (findUser != null)
                {

                    try
                    {
                        findUser.isOnboarded = true;
                        await _userManager.UpdateAsync(findUser);
                        var updatedUserResponse = new User
                        {
                            Id = id,
                            UserName = findUser.UserName,
                            FullName = findUser.FullName,
                            Gender = findUser.Gender,
                            Address = findUser.Address,
                            BirthDate = findUser.BirthDate,
                            NormalizedUserName = findUser.Email,
                            Email = findUser.Email,
                            NormalizedEmail = findUser.Email,
                            isOnboarded = true,
                            PhoneNumber = findUser.PhoneNumber,
                        };
                        return new Response<UserDetailsDto>
                        {
                            Succeeded = true,
                            Message = "Cập nhật thông tin thành công!",
                            Data = _mapper.Map<UserDetailsDto>(updatedUserResponse),
                        };

                    }
                    catch (Exception ex)
                    {
                        return new Response<UserDetailsDto>
                        {
                            Succeeded = false,
                            Message = ex.Message,
                            Data = null,
                        };
                    }
                }
                return new Response<UserDetailsDto>
                {
                    Succeeded = false,
                    Message = "User not found!",
                    Data = null,
                };
            }
            return new Response<UserDetailsDto>
            {
                Succeeded = false,
                Message = "updating property should not null!",
                Data = null,
            };
        }

        //Delete User
        public async Task<Response<UserDetailsDto>> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                try
                {
                    await _userManager.DeleteAsync(user);
                    var userTokens = await _context.Tokens.Where(t => t.UserId == id).ToListAsync();
                    _context.Tokens.RemoveRange(userTokens);
                    var currentUser = _currentUser.GetCurrentUserId();
                    await _context.SaveChangesAsync(Guid.Parse(currentUser));
                    return new Response<UserDetailsDto>
                    {
                        Succeeded = true,
                        Message = "User deleted successfully!",
                        Data = _mapper.Map<UserDetailsDto>(user),
                    };
                }
                catch (Exception ex)
                {
                    return new Response<UserDetailsDto>
                    {
                        Succeeded = false,
                        Message = ex.Message,
                        Data = null,
                    };
                }
            }
            return new Response<UserDetailsDto>
            {
                Succeeded = false,
                Message = "User not found!",
                Data = null,
            };
        }

        //User Exist
        public bool IsExist(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // Additional Creating Password Hash
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null)
        {
            return await _userManager.FindByEmailAsync(email.Normalize()) is User user && user.Id == Guid.Parse(exceptId);
        }

        public async Task<bool> VerifyCurrentPassword(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new Exception("User not found!");

            return await _userManager.CheckPasswordAsync(user, password);
        }

        //public async Task UpdateAvatarAsync(UpdateAvatarRequest request, CancellationToken cancellationToken)
        //{
        //    var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        //    if (user == null)
        //    {
        //        _logger.LogError("User not found");
        //        throw new Exception("User not found");
        //    }

        //    string currentImage = user.ImageUrl ?? string.Empty;

        //    if (request.Image != null)
        //    {
        //        RemoveCurrentAvatar(currentImage);
        //        user.ImageUrl = await _fileStorage.SaveFileAsync(request.Image, cancellationToken);
        //        if (string.IsNullOrEmpty(user.ImageUrl))
        //        {
        //            _logger.LogError("Image upload failed");
        //            throw new Exception("Image upload failed");
        //        }
        //    }
        //    else if (request.DeleteCurrentImage)
        //    {
        //        RemoveCurrentAvatar(currentImage);
        //        user.ImageUrl = null;
        //    }

        //    var result = await _userManager.UpdateAsync(user);

        //    if (!result.Succeeded)
        //    {
        //        _logger.LogError("Update profile failed");
        //        throw new Exception("Update profile failed");
        //    }
        //}

        public async Task UpdateAvatarAsync(UpdateAvatarRequest request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }

            string currentImage = user.ImageUrl ?? string.Empty;

            if (request.Image != null)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(currentImage))
                {
                    Uri oldUri = new Uri(currentImage);
                    string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
                    await _googleCloudService.DeleteFileAsync(oldObjectName);
                }

                // Upload ảnh mới
                string avatarFileName = $"user_avatars/avatar_{Guid.NewGuid()}.jpg";
                user.ImageUrl = await _googleCloudService.UploadImageAsync(avatarFileName, request.Image.OpenReadStream(), request.Image.ContentType);

                if (string.IsNullOrEmpty(user.ImageUrl))
                {
                    _logger.LogError("Image upload failed");
                    throw new Exception("Image upload failed");
                }
            }
            else if (request.DeleteCurrentImage)
            {
                if (!string.IsNullOrEmpty(currentImage))
                {
                    Uri oldUri = new Uri(currentImage);
                    string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
                    await _googleCloudService.DeleteFileAsync(oldObjectName);
                }
                user.ImageUrl = null;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Update profile failed");
                throw new Exception("Update profile failed");
            }
        }

        private void RemoveCurrentAvatar(string currentImage)
        {
            if (string.IsNullOrEmpty(currentImage)) return;
            string root = Directory.GetCurrentDirectory();
            _fileStorage.Remove(Path.Combine(root, currentImage));
        }

        public async Task<UserDetailsDto> GetAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                            .AsNoTracking()
                            .Include(u => u.Subscriptions.OrderByDescending(s => s.EndDate))
                            .Where(u => u.Id == userId)
                            .FirstOrDefaultAsync(cancellationToken);

            _ = user ?? throw new Exception("User not found");

            return _mapper.Map<UserDetailsDto>(user);
        }
        


        public async Task ChangePasswordAsync(ChangePasswordRequest model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

            if (!result.Succeeded)
            {
                throw new Exception("Change password failed");
            }
        }

        public async Task<string> UpdateEmailAsync(UpdateEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            _ = user ?? throw new Exception("User not found");

            var result = await _userManager.SetEmailAsync(user, request.Email);

            if (!result.Succeeded)
            {
                throw new Exception("Email update failed");
            }

            string confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var emailToken = Uri.EscapeDataString(confirmEmailToken);
            var encodedEmailToken = Encoding.UTF8.GetBytes(emailToken);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

            var confirmUser = await _userManager.FindByEmailAsync(user.Email);

            string url = $"{_config["AppUrl"]}/auth/confirm-email?userId={confirmUser.Id}&token={validEmailToken}";

            var mailRecap = new MailRequest
            {
                ToEmail = user.Email,
                Subject = "Confirm to update your email address",
                Body = $"<h1>Welcome to our website</h1>" + $"<p>Hi {user.UserName} !, Please confirm your email by <a href='{url}'>Clicking here</a></p><br><strong>Email Confirmation token for ID '" + confirmUser.Id + "' : <code>" + validEmailToken + "</code></strong>",
            };

            await _mailService.SendEmailAsync(mailRecap);

            return confirmEmailToken;
        }

        public async Task<string> ResendEmailCodeConfirm(string userId, string origin)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new Exception("User not found");

            if (user.Email == null || user.EmailConfirmed)
            {
                throw new Exception("An error occurred while resending Email confirmation.");
            }

            string emailVerificationUri = await GetEmailVerificationUriAsync(user, origin);
            RegisterUserEmailModel eMailModel = new RegisterUserEmailModel()
            {
                Email = user.Email,
                UserName = user.UserName,
                Url = emailVerificationUri
            };
            var mailRecap = new MailRequest
            {
                ToEmail = user.Email,
                Subject = "Resend Email Confirmation",
                Body = "<h1>Welcome to our website</h1>" + $"<p>Hi {user.UserName} !, Please confirm your email by <a href='{emailVerificationUri}'>Clicking here</a></p>",
            };

            await _mailService.SendEmailAsync(mailRecap);
            return $"Please check {user.Email} to verify your account!";
        }

        private async Task<string> GetEmailVerificationUriAsync(User user, string origin)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            const string route = "auth/confirm-email";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", user.Id.ToString());
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "token", code);
            return verificationUri;
        }

        public async Task UpdatePhoneNumberAsync(UpdatePhoneNumberRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId!);

            _ = user ?? throw new Exception("User not found");

            var result = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);

            string code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.PhoneNumber);
            _speedSMSService.sendSMS(new string[] { request.PhoneNumber }, $"Your verification code is: {code}", SpeedSMSType.TYPE_CSKH);

            if (!result.Succeeded)
            {
                throw new Exception("Phone number update failed");
            }
        }

        public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null)
        {
            return await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber) is User user && user.Id != Guid.Parse(exceptId);
        }

        public async Task<string> ResendPhoneNumberCodeConfirm(string userId)
        {

            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new Exception("An error occurred while resending Mobile Phone confirmation code.");
            if (string.IsNullOrEmpty(user.PhoneNumber) || user.PhoneNumberConfirmed) throw new Exception("An error occurred while resending Mobile Phone confirmation code.");

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);

            _speedSMSService.sendSMS(new string[] { user.PhoneNumber }, $"Your verification code is: {code}", SpeedSMSType.TYPE_CSKH);

            return "Send code successfully!";
        }

        public async Task<string> ConfirmPhoneNumberAsync(string userId, string code)
        {

            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new Exception("An error occurred while confirming Mobile Phone.");
            if (string.IsNullOrEmpty(user.PhoneNumber)) throw new Exception("An error occurred while confirming Mobile Phone.");

            var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, code);

            return result.Succeeded
                ? user.PhoneNumberConfirmed
                    ? string.Format("Account Confirmed for Phone Number {0}. You can now use the /api/tokens endpoint to generate JWT.", user.PhoneNumber)
                    : string.Format("Account Confirmed for Phone Number {0}. You should confirm your E-mail before using the /api/tokens endpoint to generate JWT.", user.PhoneNumber)
                : throw new Exception(string.Format("An error occurred while confirming {0}", user.PhoneNumber));
        }

        // RemoveUnicode
        public static string RemoveUnicode(string input)
        {
            var regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            var temp = input.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').ToLower();
        }
    }
}