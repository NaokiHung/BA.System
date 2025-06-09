using Microsoft.AspNetCore.Mvc;
using BA.Server.Core.DTOs.Auth;
using BA.Server.Core.Interfaces;

namespace BA.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.LoginAsync(request);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入 API 發生錯誤");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 使用者註冊
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.RegisterAsync(request);
                
                if (response.Success)
                {
                    return CreatedAtAction(nameof(Login), response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "註冊 API 發生錯誤");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 檢查使用者名稱是否可用
        /// </summary>
        [HttpGet("check-username/{username}")]
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
    }
}