namespace AuthService.Application.Contracts
{
    public static class AuthErrorCodes
    {
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string Invalid2FA = "INVALID_2FA";
        public const string UserExists = "USER_EXISTS";
        public const string ErrorCreatingUser = "ERROR_CREATING_USER";
    }
}
