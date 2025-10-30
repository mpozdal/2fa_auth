using MassTransit;
using Shared.Events;
using Microsoft.AspNetCore.Identity;
using AuthService.Domain.Entities;

namespace AuthService.Application.Consumers
{
    public class User2FAStatusConsumer(UserManager<ApplicationUser> userManager)
        : IConsumer<User2FAEnabledEvent>, IConsumer<User2FADisabledEvent>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task Consume(ConsumeContext<User2FAEnabledEvent> context)
        {
            var userId = context.Message.UserId;

            var user = await _userManager.FindByIdAsync(userId);

            if (user is not null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
            }
        }

        public async Task Consume(ConsumeContext<User2FADisabledEvent> context)
        {
            var userId = context.Message.UserId;

            var user = await _userManager.FindByIdAsync(userId);

            if (user is not null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
            }
        }
    }
}

