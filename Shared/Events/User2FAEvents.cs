namespace Shared.Events
{
    public record User2FAEnabledEvent
    {
        public string UserId { get; init; } = string.Empty;
    }

    public record User2FADisabledEvent
    {
        public string UserId { get; init; } = string.Empty;
    }
}
