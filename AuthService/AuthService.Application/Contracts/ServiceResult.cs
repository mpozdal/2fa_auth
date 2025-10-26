namespace TwoFactorService.Application.Contracts
{
    public record ServiceResult
    {
        public bool IsSuccess { get; }
        public string? ErrorCode { get; }

        public ServiceResult(bool isSuccess, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
        }

        public static ServiceResult Success()
            => new(true, null);

        public static ServiceResult Fail(string? errorCode = null)
            => new(false, errorCode);
    }
}
