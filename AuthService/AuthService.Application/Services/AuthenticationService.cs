using AuthService.Application.Interfaces;

namespace AuthService.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public Task Login(string email, string password)
        {
            throw new NotImplementedException();
        }
    }

}
