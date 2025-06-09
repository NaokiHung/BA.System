using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "帳號不能為空")]
        [MaxLength(50, ErrorMessage = "帳號長度不能超過50個字元")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "密碼不能為空")]
        [MinLength(6, ErrorMessage = "密碼長度至少需要6個字元")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "確認密碼不能為空")]
        [Compare("Password", ErrorMessage = "密碼與確認密碼不符")]
        public required string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        public string? Email { get; set; }

        [MaxLength(100, ErrorMessage = "顯示名稱不能超過100個字元")]
        public string? DisplayName { get; set; }
    }
}