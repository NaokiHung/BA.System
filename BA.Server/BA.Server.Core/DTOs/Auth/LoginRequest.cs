using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "帳號不能為空")]
        [MaxLength(50, ErrorMessage = "帳號長度不能超過50個字元")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "密碼不能為空")]
        [MinLength(6, ErrorMessage = "密碼長度至少需要6個字元")]
        public required string Password { get; set; }
    }
}