
using System;

namespace Itminus.InDirectLine.Core.Utils
{
    public static class UtilsEx
    {
        public static string GetOrigin(string url)
        {
            var endpoint = new UriBuilder(url);
            var b=new UriBuilder();
            b.Scheme= endpoint.Scheme;
            b.Host = endpoint.Host;
            b.Port = endpoint.Port;
            return b.Uri.ToString().TrimEnd('/');
        }

        public static string GetWebSocketOrigin(string url)
        {
            var endpoint = new UriBuilder(url);
            var b=new UriBuilder();
            switch( endpoint.Scheme.ToLower()){
                case "http":
                    b.Scheme = "ws";
                    break;
                case "https":
                    b.Scheme = "wss";
                    break;
                default:
                    b.Scheme = "wss";
                    break;
            }
            b.Host = endpoint.Host;
            b.Port = endpoint.Port;
            return b.Uri.ToString().TrimEnd('/');
        }

    }
}