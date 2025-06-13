using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Auth
{
    public class UpdateUserProfileRequest
    {
        [Required(ErrorMessage = "顯示名稱不能為空")]
        [MinLength(2, ErrorMessage = "顯示名稱至少需要2個字元")]
        [MaxLength(50, ErrorMessage = "顯示名稱不能超過50個字元")]
        public required string DisplayName { get; set; }

        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        [MaxLength(100, ErrorMessage = "電子郵件長度不能超過100個字元")]
        public string? Email { get; set; }
    }
}