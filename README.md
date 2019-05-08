# InDirectLine

Azure rocks. But sometime we need **Host our own `DirectLine` & [Bot Framework](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0) without Azure**.
For example, test [webchat](https://github.com/Microsoft/BotFramework-WebChat) with no internet. :)

## How it works

Actually, the DirectLine is a bridge that connects your bot and your client. This project (`InDirectLine`) is a custom implementation of my own.

For more details, see [Direct Line API 3.0](https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-direct-line-3-0-concepts?view=azure-bot-service-4.0)

## Features & ToDo

* [x] REST API for normal messages
* [x] WebSocket support
* [ ] Attachment support
    * [X] Basic support : allow uploading
    * [ ] allow download
* [ ] Persistene Layer & InMemory storage & clean up resources automatically
* [ ] Security
    * [x] Any user can only access his own conversation data
    * [ ] Token Generate & Refresh API
    * [ ] Support secret & token at the same time

## How to Use

Create a `directLine` by `WebChat.createDirectLine()` :

```html
<div id="webchat" role="main"></div>
<script src="https://cdn.botframework.com/botframework-webchat/latest/webchat-minimal.js"></script>
<script>
    var directLine = window.WebChat.createDirectLine({
        domain: "http://localhost:3000/v3/directline",
        token: 'YOUR_DIRECT_LINE_TOKEN',
    });
    window.WebChat.renderWebChat({
        directLine: directLine,
        userID: 'YOUR_USER_ID',
        username: 'Web Chat User',
        locale: 'en-US',
        botAvatarInitials: 'Bot',
        userAvatarInitials: 'Me'
    }, document.getElementById('webchat'));
</script>
```
This will use websocket by default. If you want a `REST` way, set the `webSocket=false`:

```javascript
    var directLine = window.WebChat.createDirectLine({
        domain: "http://localhost:3000/v3/directline",
        token: 'YOUR_DIRECT_LINE_TOKEN',
        webSocket:false, 
    });
```