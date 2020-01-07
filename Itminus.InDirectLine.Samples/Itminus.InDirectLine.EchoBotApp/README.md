

### How to Test

1. `InDirectLine` Server: We will start the `InDirectLine` and make it listen on `http://localhost:3000`
    * Changes the `appsettings.json` of `InDirectLine` :
        * make sure the `BotEndpoint` is  `http://127.0.0.1:5000/api/messages`,  this is the Bot Server messsages endpoint
        * make sure the `ServiceUrl` is `http://127.0.0.1:3000`, this is the Channel Service URL, namely the InDirectLine
        * change the `JWT` configuration if need.
    * Start the Standlone `InDirectLine` by setting : `dotnet run --urls=http://localhost:3000`
2. `Bot` Server: Let's start this Bot App on  `http://localhost:5000` by `dotnet run --urls=http://localhost:5000`
3. open a browser with the URL of `http://localhost:5000`, and there will be a simple webchat initialized for you.

