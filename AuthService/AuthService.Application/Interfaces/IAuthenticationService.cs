namespace AuthService.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task Login(string email, string password);
    }
}
