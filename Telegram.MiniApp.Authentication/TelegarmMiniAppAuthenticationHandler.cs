using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Telegram.MiniApp.Authentication;

public class TelegarmMiniAppAuthenticationHandler(
    IOptionsMonitor<TelegarmMiniAppAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<TelegarmMiniAppAuthenticationOptions>(options, logger, encoder)
{
    private static byte[]? _secretKey = null;

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TelegarmMiniAppAuthenticationConstants.Header, out var rawInitData))
            return AuthenticateResult.Fail("Header not found.");

        var searchParams = QueryHelpers.ParseQuery(rawInitData);

        if (searchParams.Count == 0)
            return AuthenticateResult.Fail("Missing parameters.");

        var authDate = DateTime.MinValue;
        var hash = string.Empty;
        var pairs = new List<string>(searchParams.Count - 1);

        foreach (var param in searchParams)
        {
            if (param.Key == TelegarmMiniAppAuthenticationConstants.HashKey)
            {
                hash = param.Value.ToString();
                continue;
            }

            if (param.Key == TelegarmMiniAppAuthenticationConstants.AuthDateKey)
            {
                if (!int.TryParse(param.Value.ToString(), out var authDateNum))
                {
                    return AuthenticateResult.Fail("\"auth_date\" should present an integer.");
                }

                authDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(authDateNum);
            }

            pairs.Add($"{param.Key}={param.Value}");
        }

        if (string.IsNullOrEmpty(hash))
        {
            return AuthenticateResult.Fail("\"hash\" is empty or not found.");
        }

        if (authDate == DateTime.MinValue)
        {
            return AuthenticateResult.Fail("\"auth_date\" is empty or not found.");
        }

        if (authDate.Add(Options.ExpirationDuration) < DateTime.UtcNow)
        {
            return AuthenticateResult.Fail("Init data expired");
        }

        pairs.Sort();

        if (_secretKey == null)
        {
            var data = Encoding.UTF8.GetBytes(Options.BotToken);
            var key = Encoding.UTF8.GetBytes(TelegarmMiniAppAuthenticationConstants.SecretKeySalt);
            _secretKey ??= HMACSHA256.HashData(key, data);
        }

        var computedHash = BitConverter.ToString(HMACSHA256.HashData(_secretKey, Encoding.UTF8.GetBytes(string.Join("\n", pairs)))).Replace("-", "").ToLower();

        var identity = new ClaimsIdentity(Enumerable.Empty<Claim>(), Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        if (computedHash != hash)
        {
            return await Task.FromResult(AuthenticateResult.Fail("Hashes not equal."));
        }
        else
        {
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
