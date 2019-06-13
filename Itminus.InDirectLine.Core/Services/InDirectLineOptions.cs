namespace Itminus.InDirectLine.Core.Services{

    /// <summary>
    /// 
    /// </summary>

    public class InDirectLineSettings{

        /// <summary>
        /// This is the serive URL exposed to Bot as well as Clients.
        /// Note the ServiceUrl is different from the `--urls` listened by InDirectLine server. The `--urls` might be used internally (let's say it sits behind a reverse proxy).
        /// </summary>
        /// <value></value>
        public string ServiceUrl{get;set;}  = "http://127.0.0.1:3000";
        /// <summary>
        /// the message endpoint provided by Bot 
        /// </summary>
        /// <value></value>
        public string BotEndpoint {get;set;} = "http://127.0.0.1:3978/api/messages";

        /// <summary>
        /// count in seconds
        /// </summary>
        /// <value></value>
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