namespace TwoFactorService.Application.Contracts
{
    public record ServiceResult<T> : ServiceResult
    {
        public T? Value { get; }

        internal ServiceResult(T value)
            : base(true, null)
        {
            Value = value;
        }

        internal ServiceResult(string? errorCode = null)
            : base(false, errorCode)
        {
            Value = default;
        }

        public static ServiceResult<T> Success(T value)
            => new(value);

        public new static ServiceResult<T> Fail(string? errorCode = null)
            => new( errorCode);
    }
}
