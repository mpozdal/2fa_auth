using System.ComponentModel.DataAnnotations;

namespace TwoFactorService.Domain.Entities
{
    public class UserTwoFactorSetting
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Required]
        public string EncryptedSecretKey { get; set; } = "";

        public bool IsEnabled { get; set; }

        public ICollection<UserRecoveryCode> RecoveryCodes { get; set; } = [];
    }
}
