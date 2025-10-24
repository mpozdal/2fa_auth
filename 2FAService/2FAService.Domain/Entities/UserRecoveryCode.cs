using System.ComponentModel.DataAnnotations;

namespace TwoFactorService.Domain.Entities
{
    public class UserRecoveryCode
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string HashedCode { get; set; } = "";

        public bool IsUsed { get; set; }

        public Guid UserTwoFactorSettingId { get; set; }
        public UserTwoFactorSetting UserTwoFactorSetting { get; set; } = new UserTwoFactorSetting();
    }
}
