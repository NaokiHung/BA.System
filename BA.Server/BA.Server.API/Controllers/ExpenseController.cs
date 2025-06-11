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
        /// 新增信用卡支出
        /// 為什麼要分開處理現金和信用卡支出？
        /// 1. 業務邏輯不同：現金支出立即影響預算餘額，信用卡不會
        /// 2. 記帳方式不同：信用卡需要標記付款方式
        /// 3. 後續報表統計需要區分不同的支出類型
        /// </summary>
        [HttpPost("credit-card")]
        public async Task<ActionResult<ExpenseResponse>> AddCreditCardExpense([FromBody] AddCreditCardExpenseRequest request)
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

                var response = await _expenseService.AddCreditCardExpenseAsync(userId, request);

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
                _logger.LogError(ex, "新增信用卡支出 API 發生錯誤");
                return StatusCode(500, new ExpenseResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 更新支出記錄
        /// 為什麼需要更新功能？
        /// 1. 使用者可能輸入錯誤需要修正
        /// 2. 支出類別或描述可能需要調整
        /// 3. 提升系統的實用性和使用者體驗
        /// </summary>
        [HttpPut("{expenseId}")]
        public async Task<ActionResult<ExpenseResponse>> UpdateExpense(int expenseId, [FromBody] UpdateExpenseRequest request)
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

                var response = await _expenseService.UpdateExpenseAsync(userId, expenseId, request);

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
                _logger.LogError(ex, "更新支出記錄 API 發生錯誤，ExpenseId: {ExpenseId}", expenseId);
                return StatusCode(500, new ExpenseResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 刪除支出記錄
        /// 為什麼需要刪除功能？
        /// 1. 使用者可能重複記錄或記錄錯誤
        /// 2. 刪除記錄需要同步更新預算餘額
        /// 3. 符合 CRUD 完整性要求
        /// </summary>
        [HttpDelete("{expenseId}")]
        public async Task<ActionResult<ExpenseResponse>> DeleteExpense(int expenseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var response = await _expenseService.DeleteExpenseAsync(userId, expenseId);

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
                _logger.LogError(ex, "刪除支出記錄 API 發生錯誤，ExpenseId: {ExpenseId}", expenseId);
                return StatusCode(500, new ExpenseResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 取得單一支出記錄詳情
        /// 為什麼需要這個 API？
        /// 1. 支援編輯功能時預載入原始資料
        /// 2. 提供支出記錄的詳細檢視
        /// 3. 符合 RESTful API 設計原則
        /// </summary>
        [HttpGet("{expenseId}")]
        public async Task<ActionResult<ExpenseDetailResponse>> GetExpenseDetail(int expenseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var response = await _expenseService.GetExpenseDetailAsync(userId, expenseId);

                if (response != null)
                {
                    return Ok(response);
                }
                else
                {
                    return NotFound(new { message = "找不到指定的支出記錄" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出記錄詳情 API 發生錯誤，ExpenseId: {ExpenseId}", expenseId);
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
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
        public async Task<ActionResult<IEnumerable<ExpenseHistory>>> GetExpenseHistory(int year, int month)
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
        
        /// <summary>
        /// 更新現金支出記錄
        /// </summary>
        [HttpPut("cash/{expenseId}")]
        public async Task<ActionResult<ExpenseResponse>> UpdateCashExpense(int expenseId, [FromBody] UpdateExpenseRequest request)
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

                var response = await _expenseService.UpdateExpenseAsync(userId, expenseId, request);
                
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
                _logger.LogError(ex, "更新支出記錄 API 發生錯誤，ExpenseId: {ExpenseId}", expenseId);
                return StatusCode(500, new ExpenseResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 刪除現金支出記錄
        /// </summary>
        [HttpDelete("cash/{expenseId}")]
        public async Task<ActionResult<ExpenseResponse>> DeleteCashExpense(int expenseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var response = await _expenseService.DeleteExpenseAsync(userId, expenseId);
                
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
                _logger.LogError(ex, "刪除支出記錄 API 發生錯誤，ExpenseId: {ExpenseId}", expenseId);
                return StatusCode(500, new ExpenseResponse
                {
                    Success = false,
                    Message = "系統暫時無法處理請求"
                });
            }
        }

        /// <summary>
        /// 取得支出記錄詳情
        /// </summary>
        [HttpGet("cash/{expenseId}")]
        public async Task<ActionResult<CashExpenseDetailResponse>> GetCashExpenseDetail(int expenseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("無效的使用者身份");
                }

                var response = await _expenseService.GetExpenseDetailAsync(userId, expenseId);
                
                if (response != null)
                {
                    return Ok(response);
                }
                else
                {
                    return NotFound(new { message = "找不到指定的支出記錄" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出記錄詳情 API 發生錯誤，ExpenseId: {ExpenseId}", expenseId);
                return StatusCode(500, new { message = "系統暫時無法處理請求" });
            }
        }
    }
}