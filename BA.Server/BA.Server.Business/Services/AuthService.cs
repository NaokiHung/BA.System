using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BA.Server.Core.DTOs.Auth;
using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;

namespace BA.Server.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IBaseRepository<User> userRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // 步驟1：找尋使用者
                var users = await _userRepository.FindAsync(u => u.Username == request.Username);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("登入失敗：使用者不存在 - {Username}", request.Username);
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "帳號或密碼錯誤"
                    };
                }

                // 步驟2：驗證密碼
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("登入失敗：密碼錯誤 - {Username}", request.Username);
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "帳號或密碼錯誤"
                    };
                }

                // 步驟3：檢查帳號狀態
                if (!user.IsActive)
                {
                    _logger.LogWarning("登入失敗：帳號已停用 - {Username}", request.Username);
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "帳號已停用"
                    };
                }

                // 步驟4：產生 JWT Token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24); // Token 24小時後過期

                // 步驟5：更新最後登入時間
                user.LastLoginDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("使用者登入成功 - {Username}", request.Username);

                return new LoginResponse
                {
                    Success = true,
                    Message = "登入成功",
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入過程中發生錯誤 - {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "登入失敗，請稍後再試"
                };
            }
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // 步驟1：檢查使用者是否已存在
                if (await UserExistsAsync(request.Username))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "此帳號已存在"
                    };
                }

                // 步驟2：檢查信箱是否已被使用
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var existingUsers = await _userRepository.FindAsync(u => u.Email == request.Email);
                    if (existingUsers.Any())
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = "此信箱已被使用"
                        };
                    }
                }

                // 步驟3：建立新使用者
                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = HashPassword(request.Password),
                    Email = request.Email,
                    DisplayName = request.DisplayName ?? request.Username,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                // 步驟4：儲存到資料庫
                await _userRepository.AddAsync(user);

                _logger.LogInformation("新使用者註冊成功 - {Username}", request.Username);

                return new LoginResponse
                {
                    Success = true,
                    Message = "註冊成功",
                    UserId = user.Id,
                    Username = user.Username
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "註冊過程中發生錯誤 - {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "註冊失敗，請稍後再試"
                };
            }
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            try
            {
                var users = await _userRepository.FindAsync(u => u.Username == username);
                return users.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查使用者是否存在時發生錯誤 - {Username}", username);
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                var users = await _userRepository.FindAsync(u => u.Email == email);
                return users.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查電子郵件是否存在時發生錯誤 - {Email}", email);
                return false;
            }
        }

       public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = _configuration["JwtSettings:SecretKey"];
                
                // 加上 null 檢查
                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogError("JWT SecretKey 設定為空");
                    return false;
                }
                
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                await Task.Run(() => // 加上 await 避免警告
                    tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken));
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Private Methods

        /// <summary>
        /// 產生 JWT Token
        /// 為什麼使用 JWT？無狀態、可攜帶使用者資訊、適合分散式系統
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            
            // 加上 null 檢查
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey 不能為空");
            }
            
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("DisplayName", user.DisplayName ?? user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 雜湊密碼
        /// 使用 BCrypt 演算法，安全性高，自動加鹽
        /// </summary>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// 驗證密碼
        /// 與儲存的雜湊值比較
        /// </summary>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        #endregion
    }
}