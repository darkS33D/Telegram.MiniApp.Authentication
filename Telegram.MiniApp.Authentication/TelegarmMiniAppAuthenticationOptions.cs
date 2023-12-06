using Microsoft.AspNetCore.Authentication;

namespace Telegram.MiniApp.Authentication;

public class TelegarmMiniAppAuthenticationOptions : AuthenticationSchemeOptions
{
    public string BotToken { get; set; } = string.Empty;
    public TimeSpan ExpirationDuration { get; set; } = TimeSpan.FromDays(1);
}
