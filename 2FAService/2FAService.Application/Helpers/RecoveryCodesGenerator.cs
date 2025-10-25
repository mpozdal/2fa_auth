using System.Security.Cryptography;

namespace TwoFactorService.Application.Helpers
{
    public static class RecoverCodesGenerator
    {
        public static List<string> Generate(int count)
        {
            var codes = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                codes.Add(string.Join("",
                    GenerateRandomString(4),
                    GenerateRandomString(4)));
            }

            return codes;
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)])]);
        }
    }
}
