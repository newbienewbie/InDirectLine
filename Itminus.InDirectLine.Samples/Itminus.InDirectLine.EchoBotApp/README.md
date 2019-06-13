

### How to Test

1. Start this Bot App on  `http://localhost:5000` by `dotnet run --urls=http://localhost:5000`
2. `InDirectLine` Server: We will start the `InDirectLine` make it listen on `http://localhost:3000`
    * Changes the `appsettings.json` of `InDirectLine`, make sure:
        * the `BotEndpoint` is  `http://127.0.0.1:5000/api/messages`
        * the `ServiceUrl` is `http://127.0.0.1:3000`
        * change the `JWT` configuration if need.
    * Start the Standlone `InDirectLine` by setting : `dotnet run --urls=http://localhost:3000`
3. Browse the `http://localhost:5000`, there will be a simple webchat initialized for you.

