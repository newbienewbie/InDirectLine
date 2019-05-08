namespace Itminus.InDirectLine.Services{


    public class InDirectLineOptions{
        public string ServiceUrl{get;set;}  = "http://127.0.0.1:3000";
        public string BotEndpoint {get;set;} = "http://127.0.0.1:3978/api/messages";
        public int TokenExpiresIn {get;set;} = 36000;
        public int StreamUrlMustBeConnectedIn{get;set;} =60;
    }
}