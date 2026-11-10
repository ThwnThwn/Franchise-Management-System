using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Dtos.User.Request
{
    public class ChangePasswordReq
    {

        [Required]
        [MinLength(5)]
        public string OldPassword { get; set; } = null!;

        [Required]
        [MinLength(5)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [MinLength(5)]
        public string ConfirmNewPassword { get; set; } = null!;

        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public bool ForgetAll { get; set; } = false;
    }
}
