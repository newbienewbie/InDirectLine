# InDirectLine

Azure rocks. But sometime we need **Host our own `DirectLine` & [Bot Framework](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0) without Azure**.
For example, test [webchat](https://github.com/Microsoft/BotFramework-WebChat) with no internet. :)

## How it works

Actually, the `DirectLine` is a **bridge** that connects your bot and your client. This project (`InDirectLine`) is a custom implementation of my own written in [ASP.NET Core](https://github.com/aspnet/AspNetCore).

For more details, see [Direct Line API 3.0](https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-direct-line-3-0-concepts?view=azure-bot-service-4.0)

## Features & ToDo

* [x] REST API for normal messages
* [x] WebSocket support
* [x] Attachment support
    * [X] Basic support : allow uploading
    * [x] allow download
* [x] Security
    * [x] Token Generate & Refresh API
    * [x] Any user can only access his own conversation data
* [ ] Persistene Layer & InMemory storage & clean up resources automatically

## How to Use

Typically, `InDirectLine` will be used as an standlone server ([Itminus.InDirectLine.Web](https://github.com/newbienewbie/InDirectLine/tree/master/Itminus.InDirectLine.Web)). In this way, the `InDirectLine` & your `Bot` are two different processes. You could create your `Bot` in `C#`/`Node.js`/`Python`/`Java` languages as you like.

Or if you're using `C#` and only want to test webchat within a single one website, you could add a reference to `Itminus.InDirectLine.Core` and make the `InDdirectLine` & your `Bot` share the same port. See [Itminus.InDirectLine.IntegrationBotSample](https://github.com/newbienewbie/InDirectLine/tree/master/Itminus.InDirectLine.IntegrationBotSample).


In order to use `Directline` with `WebChat`, we need create a `directLine` instance by `WebChat.createDirectLine()` firstly:

```html
<div id="webchat" role="main"></div>
<script src="https://cdn.botframework.com/botframework-webchat/latest/webchat-minimal.js"></script>
<script>
    fetch('http://localhost:3000/v3/directline/tokens/generate', { method: 'POST' })
        .then(res => res.json())
        .then(res => {
            var directLine = window.WebChat.createDirectLine({
                domain: "http://localhost:3000/v3/directline",
                token: res.token,
            });
            window.WebChat.renderWebChat({
                directLine: directLine,
                userID: 'YOUR_USER_ID',
                username: 'Web Chat User',
                locale: 'en-US',
            }, document.getElementById('webchat'));
        });
</script>
```
Note the `domain` has no trailing slash `/`. 

The `WebChat` will use `WebSocket` by default. If you want a `REST` way, set the `webSocket=false`:

```javascript
    var directLine = window.WebChat.createDirectLine({
        domain: "http://localhost:3000/v3/directline",
        token: 'YOUR_DIRECT_LINE_TOKEN',
        webSocket:false, 
    });
```

### `InDirectLine` Configuration

The `InDirectLine` reads the `appsettings.json` file by default, which means it will listen on `http://localhost:3000` and assumes that the `http://127.0.0.1:3978/api/messages` is the bot message endpoint.

You could create a `appsettings.Development.json` or a `appsettings.Production.json` and configure the options as you like. 

Also you could pass the settings by [command line arguments](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2#arguments) or by [environment variables](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2#environment-variables-configuration-provider). 

For more details, see [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/).
