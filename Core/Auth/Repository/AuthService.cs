using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BusinessObject.Models;
using Core.Auth.Services;
using Core.Helpers;
using Core.Infrastructure.reCAPTCHAv3;
using Core.Models;
using Core.Models.AuthModels;
using Core.Models.UserModels;
using Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Errors.Model;
using Services;

namespace Core.Repository
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private UserManager<User> _userManager;
        private RoleManager<IdentityRole<Guid>> _roleManager;
        private IMailService _mailService;
        private readonly IUserService _useService;
        private readonly ITokenService _tokenService;
        private readonly IReCAPTCHAv3Service _reCAPTCHAv3Service;
        private readonly ApiSettings _apiSettings;

        public AuthService(IConfiguration config,
                            UserManager<User> userManager,
                            IMailService mailService,
                            IUserService userService,
                            ITokenService tokenService,
                            IReCAPTCHAv3Service reCAPTCHAv3Service,
                            RoleManager<IdentityRole<Guid>> roleManager,
                            IOptions<ApiSettings> apiSettingsOptions)
        {
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _mailService = mailService;
            _tokenService = tokenService;
            _useService = userService;
            _reCAPTCHAv3Service = reCAPTCHAv3Service;
            _apiSettings = apiSettingsOptions.Value;
        }

        ////Register User
        public async Task<ResponseManager> RegisterUser(RegisterUser model, string origin)
        {
            //try
            //{
            //    var res = await _reCAPTCHAv3Service.Verify(model.captchaToken);
            //    if (!res.success)
            //    {
            //        throw new UnauthorizedException("reCAPTCHA verification failed");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new UnauthorizedException(ex.Message);
            //}

            //Regex for Password
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";
            if (!Regex.IsMatch(model.Password, pattern))
            {
                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "Mật khẩu phải chứa ít nhất một chữ hoa, một chữ thường, một chữ số, một ký tự đặc biệt và tối thiểu 8 ký tự.",
                };
            }

            // Password and Confirm Password Check
            if (model.Password != model.ConfirmPassword)
            {
                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "Mật khẩu xác nhận không khớp!",
                };
            }

            var identityUser = new User
            {
                Email = model.Email,
                UserName = RemoveUnicode(model.FullName.Replace(" ", "")),
                FullName = model.FullName,
            };

            //user creation
            var createdUser = await _useService.CreateUser(model);

            //Mail Sending
            if (createdUser.IsSuccess != false)
            {
                // tìm User trong cơ sở dữ liệu sau khi gọi CreateUser.
                identityUser = await _userManager.FindByEmailAsync(model.Email);
                string confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

                var emailToken = Uri.EscapeDataString(confirmEmailToken);
                var encodedEmailToken = Encoding.UTF8.GetBytes(emailToken);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                var confirmUser = await _userManager.FindByEmailAsync(identityUser.Email);

                // string url = $"{_config["AppUrl"]}/auth/confirm-email?userId={confirmUser.Id}&token={validEmailToken}";
                string url = await GetEmailVerificationUriAsync(confirmUser, _apiSettings.ClientUrl, "confirm-email");
                url = QueryHelpers.AddQueryString(url, "userId", confirmUser.Id.ToString());
                url = QueryHelpers.AddQueryString(url, "token", validEmailToken);
                url = QueryHelpers.AddQueryString(url, "email", identityUser.Email);

                var mailRecap = new MailRequest
                {
                    ToEmail = identityUser.Email,
                    Subject = "Confirm your email",
                    Body = $"<h1>Welcome to our website</h1>" + $"<p>Hi {identityUser.FullName} !, Please confirm your email by <a href='{url}'>Clicking here</a></p><br>",
                };

                await _mailService.SendEmailAsync(mailRecap);

                return new ResponseManager
                {
                    IsSuccess = true,
                    Message = url,
                };
            }

            return new ResponseManager
            {
                IsSuccess = false,
                Message = "Email này đã được đăng ký, hãy thử lại!",
            };

        }
        //Register User
        //public async Task<ResponseManager> RegisterUser(RegisterUser model, string origin)
        //{
        //    //try
        //    //{
        //    //    var res = await _reCAPTCHAv3Service.Verify(model.captchaToken);
        //    //    if (!res.success)
        //    //    {
        //    //        throw new UnauthorizedException("reCAPTCHA verification failed");
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    throw new UnauthorizedException(ex.Message);
        //    //}

        //    //Regex for Password
        //    string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";
        //    if (!Regex.IsMatch(model.Password, pattern))
        //    {
        //        return new ResponseManager
        //        {
        //            IsSuccess = false,
        //            Message = "Password must contain at least one uppercase, one lowercase, one digit, one special character and minimum 8 characters",
        //        };
        //    }

        //    // Password and Confirm Password Check
        //    if (model.Password != model.ConfirmPassword)
        //    {
        //        return new ResponseManager
        //        {
        //            IsSuccess = false,
        //            Message = "Password doesn't match its confirmation",
        //        };
        //    }

        //    var identityUser = new User
        //    {
        //        Email = model.Email,
        //        UserName = RemoveUnicode(model.FullName.Replace(" ", "")),
        //        FullName = model.FullName,
        //    };

        //    //user creation
        //    var createdUser = await _useService.CreateUser(model);

        //    //Mail Sending
        //    if (createdUser.IsSuccess != false)
        //    {
        //        // tìm User trong cơ sở dữ liệu sau khi gọi CreateUser.
        //        identityUser = await _userManager.FindByEmailAsync(model.Email);
        //        string confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

        //        var emailToken = Uri.EscapeDataString(confirmEmailToken);
        //        var encodedEmailToken = Encoding.UTF8.GetBytes(emailToken);
        //        var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

        //        var confirmUser = await _userManager.FindByEmailAsync(identityUser.Email);

        //        // string url = $"{_config["AppUrl"]}/auth/confirm-email?userId={confirmUser.Id}&token={validEmailToken}";
        //        string url = await GetEmailVerificationUriAsync(confirmUser, origin, "auth/confirm-email");
        //        url = QueryHelpers.AddQueryString(url, "userId", confirmUser.Id.ToString());
        //        url = QueryHelpers.AddQueryString(url, "token", validEmailToken);

        //        var mailContent = new MailRequest
        //        {
        //            ToEmail = identityUser.Email,
        //            Subject = "Confirm your email",
        //            Body = $"<h1>Welcome to our website</h1>" + $"<p>Hi {identityUser.FullName} !, Please confirm your email by <a href='{url}'>Clicking here</a></p><br><strong>Email Confirmation token for ID '" + confirmUser.Id + "' : <code>" + validEmailToken + "</code></strong>",
        //        };

        //        await _mailService.SendEmailAsync(mailContent);

        //        return new ResponseManager
        //        {
        //            IsSuccess = true,
        //            Message = "User created successfully! Please confirm the your Email!",
        //        };
        //    }

        //    return new ResponseManager
        //    {
        //        IsSuccess = false,
        //        Message = "User Email Already Registered, Try to login again!",
        //    };

        //}

        //Login User
        public async Task<ResponseManager> LoginUser(AuthUser model, string deviceId, bool isMobile, string ipAddress)
        {
            //try
            //{
            //    var res = await _reCAPTCHAv3Service.Verify(model.captchaToken);
            //    if (!res.success)
            //    {
            //        throw new UnauthorizedException("reCAPTCHA verification failed");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new UnauthorizedException(ex.Message);
            //}

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return new ResponseManager
                {
                    Message = "Không tìm thấy người dùng với Email này! ",
                    IsSuccess = false,
                };
            }
            else
            {
                // Account Confirmation Check
                if (!user.EmailConfirmed)
                {
                    return new ResponseManager
                    {
                        Message = "Email chưa xác thực! Hãy xác thực email!",
                        IsSuccess = false,
                    };
                }

                var result = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!result)
                {
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        return new ResponseManager
                        {
                            Message = "Người dùng đã bị khóa",
                            IsSuccess = false,
                        };
                    }
                    else
                    {
                        // Increase the failed login attempt count
                        await _userManager.AccessFailedAsync(user);
                        return new ResponseManager
                        {
                            Message = "Bạn còn " + (5 - await _userManager.GetAccessFailedCountAsync(user)) + " lần thử trước khi bị khóa",
                            IsSuccess = false,
                        };
                    }
                }

                // Check is locked out or not
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return new ResponseManager
                    {
                        Message = "Người dùng đã bị khóa",
                        IsSuccess = false,
                    };
                }

                var userRole = new List<string>(await _userManager.GetRolesAsync(user));
                //Generate Token JWT
                var Token = await _tokenService.GenerateToken(user, deviceId, isMobile, ipAddress);

                return new ResponseManager
                {
                    Message = Token,
                    IsSuccess = true,
                };
            }

        }

        // Logout User
        public async Task<ResponseManager> LogoutUser(string accessToken)
        {
            var tokenEntity = await _tokenService.GetToken(accessToken);
            if (tokenEntity == null)
            {
                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "Token not found",
                };
            }

            var result = await _tokenService.RevokeToken(new List<Token> { tokenEntity });
            if (result.IsSuccess)
            {
                return new ResponseManager
                {
                    IsSuccess = true,
                    Message = "Token revoked successfully",
                };
            }

            return new ResponseManager
            {
                IsSuccess = false,
                Message = "Token not revoked",
            };
        }

        // ConfirmEmail
        public async Task<ResponseManager> ConfirmEmail(Guid userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new ResponseManager
                    {
                        IsSuccess = false,
                        Message = "User not found",
                    };
                }

                var decodedToken = WebEncoders.Base64UrlDecode(token);
                // Convert back to string
                var normalToken = Encoding.UTF8.GetString(decodedToken);
                // Unescape to get the original email token
                normalToken = Uri.UnescapeDataString(normalToken);

                // check if the token is valid
                var result = await _userManager.ConfirmEmailAsync(user, normalToken);

                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    return new ResponseManager
                    {
                        Message = "Xác thực Email thành công!",
                        IsSuccess = true,
                    };
                }

                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "Email did not confirm",
                    //Errors = result.Errors.ToArray()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //Forget Password
        public async Task<ResponseManager> ForgetPassword(string email, string captchaToken, string origin)
        {
            //try
            //{
            //    var res = await _reCAPTCHAv3Service.Verify(captchaToken);
            //    if (!res.success)
            //    {
            //        throw new UnauthorizedException("reCAPTCHA verification failed");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new UnauthorizedException(ex.Message);
            //}

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);
            string pass = "Tester@123";
            // string url = $"{_config["AppUrl"]}/auth/reset-password?Email={email}&Token={validToken}&NewPassword={pass}&ConfirmPassword={pass}";
            string url = await GetEmailVerificationUriAsync(user, _apiSettings.ClientUrl, "reset-password");
            url = QueryHelpers.AddQueryString(url, "Email", email);
            url = QueryHelpers.AddQueryString(url, "Token", validToken);
            url = QueryHelpers.AddQueryString(url, "NewPassword", pass);
            url = QueryHelpers.AddQueryString(url, "ConfirmPassword", pass);

            var mailRecap = new MailRequest
            {
                ToEmail = email,
                Subject = "Reset Password",
                Body = "<h1>Follow the instructions to reset your password</h1>" +
                $"<p>To reset your password, <br><br> 1. Copy the Link :  <a href='{url}'>{url}</a><br><br> 2. Navigate to API Testing Tools(Postman)<br><br> 3. Set the Method to 'POST' <br><br> 4. Make a Request <br><br> or Use SWAGGER <br><br> <strong>Reset Token : {validToken}</strong></p>",
            };

            await _mailService.SendEmailAsync(mailRecap);

            return new ResponseManager
            {
                IsSuccess = true,
                Message = validToken,
            };
        }

        //Reset Password
        public async Task<ResponseManager> ResetPassword(ResetPasswordModel model)
        {
            //try
            //{
            //    var res = await _reCAPTCHAv3Service.Verify(model.captchaToken);
            //    if (!res.success)
            //    {
            //        throw new UnauthorizedException("reCAPTCHA verification failed");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new UnauthorizedException(ex.Message);
            //}

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };

            if (model.NewPassword != model.ConfirmPassword)
                return new ResponseManager
                {
                    IsSuccess = false,
                    Message = "Password doesn't match its confirmation",
                };

            var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ResetPasswordAsync(user, normalToken, model.NewPassword);

            if (result.Succeeded)
                return new ResponseManager
                {
                    Message = "Password has been reset successfully!",
                    IsSuccess = true,
                };

            return new ResponseManager
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }

        private async Task<string> GetEmailVerificationUriAsync(User user, string origin, string route)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            return endpointUri.ToString();
        }

        // RemoveUnicode
        public static string RemoveUnicode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }
    }
}