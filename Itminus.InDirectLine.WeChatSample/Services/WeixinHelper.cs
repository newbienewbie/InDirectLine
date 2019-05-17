using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Itminus.InDirectLine.WeChatBotSample.Services
{
    public class WeixinHelper
    {
        private readonly WeiXinOptions _opts;

        public WeixinHelper(IOptions<WeiXinOptions> opts)
        {
            this._opts = opts.Value;
        }

        public bool IsMessageFromWeiXin(string signature, string nonce, string timestamp)
        {
            var wxToken = this._opts.Token;
            string[] tempArr = { nonce, timestamp, wxToken };
            Array.Sort(tempArr);
            string tempStr = string.Join("", tempArr);
            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(tempStr));
            string calculatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return calculatedSignature == signature;
        }

    }
}