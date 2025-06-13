using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BA.Server.Core.DTOs.Auth;
using BA.Server.Core.Interfaces;

namespace BA.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            IAuthService authService,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 取得使用者個人資料
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("找不到使用者");
                }

                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    displayName = user.DisplayName,
                    email = user.Email,
                    createdDate = user.CreatedDate,
                    lastLoginDate = user.LastLoginDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者資料 API 發生錯誤");
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
            }
        }

        /// <summary>
        /// 更新使用者個人資料
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<object>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("找不到使用者");
                }

                // 檢查電子郵件是否已被其他使用者使用
                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    var emailExists = await _authService.EmailExistsAsync(request.Email);
                    if (emailExists)
                    {
                        return BadRequest(new { success = false, message = "此電子郵件已被使用" });
                    }
                }

                // 更新使用者資料
                user.DisplayName = request.DisplayName?.Trim() ?? user.DisplayName;
                user.Email = request.Email?.Trim();

                await _userRepository.UpdateAsync(user);

                return Ok(new
                {
                    success = true,
                    message = "個人資料更新成功",
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        displayName = user.DisplayName,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新個人資料 API 發生錯誤");
                return StatusCode(500, new { success = false, message = "系統暫時無法處理請求" });
            }
        }

        /// <summary>
        /// 變更密碼
        /// </summary>
        [HttpPut("change-password")]
        public async Task<ActionResult<object>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("找不到使用者");
                }

                // 驗證當前密碼
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { success = false, message = "目前密碼不正確" });
                }

                // 檢查新密碼和確認密碼是否相符
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new { success = false, message = "新密碼與確認密碼不符" });
                }

                // 檢查新密碼是否與當前密碼相同
                if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                {
                    return BadRequest(new { success = false, message = "新密碼不能與目前密碼相同" });
                }

                // 更新密碼
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("使用者 {UserId} 成功變更密碼", userId);

                return Ok(new
                {
                    success = true,
                    message = "密碼變更成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "變更密碼 API 發生錯誤");
                return StatusCode(500, new { success = false, message = "系統暫時無法處理請求" });
            }
        }

        /// <summary>
        /// 取得帳戶統計資料
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetAccountStatistics()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("找不到使用者");
                }

                // 這裡可以擴展統計功能，目前先返回基本資料
                return Ok(new
                {
                    registrationDate = user.CreatedDate,
                    lastLoginDate = user.LastLoginDate,
                    totalExpenseRecords = 0, // 可以透過ExpenseService取得
                    totalBudgets = 0, // 可以透過ExpenseService取得
                    totalCashExpenses = 0,
                    totalCreditCardExpenses = 0,
                    averageMonthlyExpense = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得帳戶統計 API 發生錯誤");
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
            }
        }

        /// <summary>
        /// 檢查使用者名稱是否可用
        /// </summary>
        [HttpGet("check-username/{username}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CheckUsername(string username)
        {
            try
            {
                var exists = await _authService.UserExistsAsync(username);
                return Ok(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查使用者名稱 API 發生錯誤");
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
            }
        }

        /// <summary>
        /// 檢查電子郵件是否可用
        /// </summary>
        [HttpGet("check-email/{email}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CheckEmail(string email)
        {
            try
            {
                var exists = await _authService.EmailExistsAsync(email);
                return Ok(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查電子郵件 API 發生錯誤");
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
            }
        }

        #region Private Methods

        /// <summary>
        /// 從 JWT Token 中取得目前使用者的 ID
        /// </summary>
        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        #endregion
    }
}