
### WebChat

Run CLI to start the demo :

```bash
dotnet run --urls=http://127.0.0.1:5000
```

Browse `http://127.0.0.1:5000`

Note: the default `appsettings.json` indicates that the port should be `5000`. If you need to listen on another port, please do change the configuration (by environment varibles/cli/appsettings file). 


![Demo](https://github.com/newbienewbie/InDirectLine/blob/master/Itminus.InDirectLine.Samples/Itminus.InDirectLine.IntegrationBotSample/webchat-demo.png?raw=true)

### WeChat MP

To integrate with WeChat MP (微信公众号), don't forget to add a `Weixin` section for `appsettings.json`:

```json
"Weixin":{
	"AppId":"",
	"AppSecret":"",
	"Token":"",
	"EncodingAESKey ":"",
}
```