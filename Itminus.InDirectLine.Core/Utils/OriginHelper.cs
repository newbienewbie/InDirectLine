
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
            var path=b.Uri.ToString();
            return path.Substring(0,path.Length-1);
        }

    }
}