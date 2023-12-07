# Telegram.MiniApp.Authentication

## How to use

1. From your frontend app add raw init data to header Authentication with every request. [Telegram documentation](https://core.telegram.org/bots/webapps#validating-data-received-via-the-mini-app)

Add to your ASP.NET Core application this line:

```
builder.Services.AddAuthentication().AddTelegarmMiniAppAuthentication("<BotToken>");
```

Replace <BotToken> placeholder with your actual bot token that you can get from [BotFather](https://t.me/BotFather).

Also you can add your own TimeSpan for expiration duration. (Example for 10 seconds)

```
builder.Services.AddAuthentication().AddTelegarmMiniAppAuthentication("<BotToken>", TimeSpan.FromSeconds(10));
```

Enjoy!✨
