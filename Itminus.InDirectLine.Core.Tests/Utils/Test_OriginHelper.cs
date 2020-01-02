using System;
using Xunit;

namespace Itminus.InDirectLine.Core.Tests
{
    public class TestOriginHelper
    {
        [Theory]
        [InlineData("http://localhost:5001/","http://localhost:5001")]
        [InlineData("http://localhost:5001/abcd/efg","http://localhost:5001")]
        [InlineData("https://localhost:5001/abcd/efg","https://localhost:5001")]
        [InlineData("https://localhost/abcd/efg","https://localhost")]
        [InlineData("https://www.itminus.com/efg","https://www.itminus.com")]
        public void TestGetOrigin(string url, string origin)
        {
            var result = Utils.UtilsEx.GetOrigin(url);
            // "origin uri should has no trailing /"
            Assert.NotEqual("/" , result.Substring(result.Length -1)); 
            Assert.Equal( origin, result, ignoreCase:true);
        }

        [Theory]
        [InlineData("http://localhost:5001/","ws://localhost:5001")]
        [InlineData("http://localhost:5001/abcd/efg","ws://localhost:5001")]
        [InlineData("https://localhost:5001/abcd/efg","wss://localhost:5001")]
        [InlineData("https://localhost/abcd/efg","wss://localhost")]
        [InlineData("https://www.itminus.com/efg","wss://www.itminus.com")]
        public void TestGetWebSocketOrigin(string url, string wsOrigin )
        {
            var result = Utils.UtilsEx.GetWebSocketOrigin(url);
            // "origin uri should has no trailing /"
            Assert.NotEqual("/" , result.Substring(result.Length -1)); 
            Assert.Equal( wsOrigin, result, ignoreCase:true);
        }
    }
}
