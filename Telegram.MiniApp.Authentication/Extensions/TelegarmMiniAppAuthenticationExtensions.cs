using Microsoft.AspNetCore.Authentication;

namespace Telegram.MiniApp.Authentication.Extensions;

public static class TelegarmMiniAppAuthenticationExtensions
{
    public static AuthenticationBuilder AddTelegarmMiniAppAuthentication(this AuthenticationBuilder authenticationBuilder, string botToken, TimeSpan? expirationDuration = null)
    {
        authenticationBuilder
            .AddScheme
            <TelegarmMiniAppAuthenticationOptions,
            TelegarmMiniAppAuthenticationHandler>
            (TelegarmMiniAppAuthenticationConstants.SchemeName,
            options =>
        {
            options.BotToken = botToken ?? throw new ArgumentNullException(nameof(botToken), $"Bot token value is null.");
            if (expirationDuration.HasValue)
            {
                options.ExpirationDuration = expirationDuration.Value;
            }
        });

        return authenticationBuilder;
    }
}
