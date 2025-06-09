using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BA.Server.Core.DTOs.Expense;
using BA.Server.Core.Interfaces;

namespace BA.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 所有的記帳功能都需要登入
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IExpenseService expenseService, ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService;
            _logger = logger;
        }

        /// <summary>
        /// 取得當月預算資訊
        /// </summary>
        [HttpGet("budget/current")]
        public async Task<ActionResult<MonthlyBudgetResponse>> GetCurrentMonthBudget()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var response = await _expenseService.GetCurrentMonthBudgetAsync(userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得當月預算 API 發生錯誤");
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
            }
        }

        /// <summary>
        /// 新增現金支出
        /// </summary>
        [HttpPost("cash")]
        public async Task<ActionResult<ExpenseResponse>> AddCashExpense([FromBody] AddCashExpenseRequest request)
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

                var response = await _expenseService.AddCashExpenseAsync(userId, request);
                
                if (response.Success)
                {
                    return CreatedAtAction(nameof(GetCurrentMonthBudget), response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增現金支出 API 發生錯誤");
                return StatusCode(500, new ExpenseResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 設定月預算
        /// </summary>
        [HttpPost("budget")]
        public async Task<ActionResult<ExpenseResponse>> SetMonthlyBudget([FromBody] SetBudgetRequest request)
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

                var response = await _expenseService.SetMonthlyBudgetAsync(
                    userId, request.Amount, request.Year, request.Month);
                
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
                _logger.LogError(ex, "設定月預算 API 發生錯誤");
                return StatusCode(500, new ExpenseResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 取得支出歷史記錄
        /// </summary>
        [HttpGet("history/{year}/{month}")]
        public async Task<ActionResult<IEnumerable<object>>> GetExpenseHistory(int year, int month)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var response = await _expenseService.GetExpenseHistoryAsync(userId, year, month);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出歷史 API 發生錯誤");
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
            }
        }

        #region Private Methods

        /// <summary>
        /// 從 JWT Token 中取得目前使用者的 ID
        /// 為什麼這樣做？確保安全性，避免使用者偽造身份
        /// </summary>
        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        #endregion
    }
}