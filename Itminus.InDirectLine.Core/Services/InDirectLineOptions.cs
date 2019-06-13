namespace Itminus.InDirectLine.Core.Services{


    public class InDirectLineSettings{
        public string ServiceUrl{get;set;}  = "http://127.0.0.1:3000";
        public string BotEndpoint {get;set;} = "http://127.0.0.1:3978/api/messages";
        public int TokenExpiresIn {get;set;} = 36000;
        public int StreamUrlMustBeConnectedIn{get;set;} =60;

        public AttachmentUrls Attachments  {get;set;}
    }

    public class AttachmentUrls
    {
        public string BaseDirectoryForUploading {get;set;}
        public string BaseUrlForDownloading {get;set;}
    }
}