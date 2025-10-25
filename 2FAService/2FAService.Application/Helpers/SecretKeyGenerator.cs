using System.Security.Cryptography;

namespace TwoFactorService.Application.Helpers
{
    public static class SecretKeyGenerator
    {
        public static string Generate()
        {
            byte[] secretKeyBytes = RandomNumberGenerator.GetBytes(20);
            string secretKeyString = Convert.ToBase64String(secretKeyBytes);
            secretKeyString = secretKeyString.TrimEnd('=');

            return secretKeyString;
        }
    }
}
