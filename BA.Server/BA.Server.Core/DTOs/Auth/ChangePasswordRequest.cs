using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Auth
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "目前密碼不能為空")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "新密碼不能為空")]
        [MinLength(6, ErrorMessage = "新密碼至少需要6個字元")]
        [MaxLength(50, ErrorMessage = "新密碼不能超過50個字元")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "確認密碼不能為空")]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不符")]
        public required string ConfirmPassword { get; set; }
    }
}