using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BusinessObject.Models;
using Core.Auth.Services;
using Core.Models;
using Core.Models.AuthModel;
using Core.Models.AuthModels;
using Core.Models.UserModels;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Core.Controllers
{
    [ApiController]
    [Route("api")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly IConfiguration _config;
        private readonly IUserService _user;
        private readonly ITokenService _token;

        public AuthController(IAuthService AuthService, IMapper mapper, IMailService mailService, IConfiguration configuration, IUserService userService, ITokenService tokenService)
        {
            _auth = AuthService;
            _mapper = mapper;
            _mailService = mailService;
            _config = configuration;
            _user = userService;
            _token = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUser model)
        {
            if (ModelState.IsValid)
            {
                var result = await _auth.RegisterUser(model, GetOriginFromRequest());

                if (result.IsSuccess)
                    return Ok(result); // Status Code: 200 

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid"); // Status code: 400
        }

        [HttpPost("tokens")]
        public async Task<IActionResult> Authenticate([FromBody] AuthUser model)
        {
            if (ModelState.IsValid)
            {
                string deviceId = GetDeviceId(Request);
                bool isMobile = IsMobile(Request);
                string ipAddress = GetIpAddress();

                var result = await _auth.LoginUser(model, deviceId, isMobile, ipAddress);

                if (result.IsSuccess)
                {
                    return Ok(new ResponseManager
                    {
                        IsSuccess = true,
                        Message = new
                        {
                            recap = "??ng nh?p th�nh c�ng",
                            token = result.Message,
                            expires = DateTime.Now.AddHours(24),
                        },
                    });
                }
                return BadRequest(result);
            }
            return BadRequest("Some properties are not valid");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var token = accessToken.Split(" ")[1];
            var result = await _auth.LogoutUser(token);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel model)
        {
            var deviceId = GetDeviceId(Request);
            bool isMobile = IsMobile(Request);
            string ipAddress = GetIpAddress();

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenValidateParam = new TokenValidationParameters
            {
                SaveSigninToken = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = false, // we don't care about the token's expiration
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ClockSkew = TimeSpan.Zero,
            };

            try
            {

                // Check authen valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken,
                    tokenValidateParam, out var validatedToken);

                // Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return BadRequest(new ResponseManager
                        {
                            IsSuccess = false,
                            Message = "Invalid Token",
                        });
                    }

                    // Check accessToken is expired?
                    var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                    var expiryDate = ConvertUnixTimeStampToDateTime(utcExpiryDate);

                    if (expiryDate > DateTime.UtcNow)
                    {
                        return BadRequest(new ResponseManager
                        {
                            IsSuccess = false,
                            Message = "This token hasn't expired yet",
                        });
                    }

                    // Check refreshToken exists in DB
                    var refreshToken = await _token.GetRefreshToken(model.RefreshToken);
                    if (refreshToken == null)
                    {
                        return BadRequest(new ResponseManager
                        {
                            IsSuccess = false,
                            Message = "This refresh token does not exist",
                        });
                    }

                    // Check refreshToken is used/revoked?
                    if (refreshToken.IsUsed)
                    {
                        return BadRequest(new ResponseManager
                        {
                            IsSuccess = false,
                            Message = "This refresh token has been used",
                        });
                    }

                    if (refreshToken.IsRevoked)
                    {
                        return BadRequest(new ResponseManager
                        {
                            IsSuccess = false,
                            Message = "This refresh token has been revoked",
                        });
                    }

                    // Check AccessToken Id == JwtId in RefreshToken
                    var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                    if (refreshToken.JwtId != jti)
                    {
                        return BadRequest(new ResponseManager
                        {
                            IsSuccess = false,
                            Message = "This refresh token does not match this JWT",
                        });
                    }

                    // Update token is used
                    refreshToken.IsUsed = true;
                    refreshToken.IsRevoked = true;

                    // Update token in DB
                    var user = await _user.GetUserbyId(refreshToken.UserId);
                    var token = await _token.GenerateToken(user.Message as User, deviceId, isMobile, ipAddress);

                    return Ok(new ResponseManager
                    {
                        IsSuccess = true,
                        Message = new
                        {
                            recap = "Token Refreshed Successfully",
                            token = token,
                            expires = DateTime.Now.AddHours(24),
                        },
                    });
                }

                return BadRequest(new ResponseManager
                {
                    IsSuccess = false,
                    Message = "Invalid Token",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseManager
                {
                    IsSuccess = false,
                    Message = ex.Message,
                });
            }
        }

        private DateTime ConvertUnixTimeStampToDateTime(long utcExpiryDate)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(utcExpiryDate).ToUniversalTime();
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId.ToString()) || string.IsNullOrWhiteSpace(token))
                return NotFound();

            var result = await _auth.ConfirmEmail(userId, token);

            if (result.IsSuccess)
            {
                return Content("X�c nh?n Email th�nh c�ng!");
            }

            return BadRequest(result);
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(string email, [FromQuery] string captchaToken)
        {
            if (string.IsNullOrEmpty(email))
                return NotFound();

            var result = await _auth.ForgetPassword(email, captchaToken, GetOriginFromRequest());

            if (result.IsSuccess)
                return Ok(result); // 200

            return BadRequest(result); // 400
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _auth.ResetPassword(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid");
        }

        [NonAction]
        public string GetDeviceId(HttpRequest request)
        {
            var device = request.Headers["User-Agent"].ToString();
            var ip = request.HttpContext.Connection.RemoteIpAddress.ToString();
            // su dung Sha256Hash de ma hoa device va ip
            var sha256 = SHA256.Create();
            var deviceHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(device));
            var ipHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(ip));
            var deviceId = BitConverter.ToString(deviceHash).Replace("-", "") + BitConverter.ToString(ipHash).Replace("-", "");
            return deviceId;
        }

        [NonAction]
        public bool IsMobile(HttpRequest request)
        {
            string userAgent = request.Headers["User-Agent"];
            if (userAgent.Contains("Mobile") || userAgent.Contains("iPhone") || userAgent.Contains("iPad") || userAgent.Contains("Android"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [NonAction]
        private string GetOriginFromRequest()
        {
            if (Request.Headers.TryGetValue("x-from-host", out var values))
            {
                return $"{Request.Scheme}://{values.First()}";
            }

            return $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
        }

        [NonAction]
        private string? GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"]
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
    }
}