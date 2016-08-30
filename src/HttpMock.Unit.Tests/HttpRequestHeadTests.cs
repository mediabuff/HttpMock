using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Should;

namespace HttpMock.Unit.Tests
{
    public class HttpRequestHeadTests
    {
        [Test]
        public void ShouldCorrectlyLoadHttpRequestHeadFromOwinContext()
        {
            var context = new Dictionary<string, object>
            {
                ["owin.RequestMethod"] = "GET",
                ["owin.RequestScheme"] = "https",
                ["owin.RequestPathBase"] = "/app",
                ["owin.RequestPath"] = "/test",
                ["owin.RequestQueryString"] = "a=1&b=2",
                ["owin.RequestHeaders"] = new Dictionary<string, string[]>
                {
                    ["Host"] = new []{"example.com"},
                    ["Single"] = new[] {"val"},
                    ["Multiple"] = new[] {"val1", "val2"},
                },
                ["owin.RequestBody"] = new MemoryStream(Encoding.UTF8.GetBytes("Test body"))
            };

            var httpRequestHead = HttpRequestHead.LoadFromOwinContext(context);

            httpRequestHead.Method.ShouldEqual("GET");
            httpRequestHead.Uri.ShouldEqual("https://example.com/app/test?a=1&b=2");
            httpRequestHead.Path.ShouldEqual("/app/test");
            httpRequestHead.QueryString.ShouldEqual("a=1&b=2");
            httpRequestHead.HasBody.ShouldBeTrue();
            httpRequestHead.Headers["Host"].ShouldEqual("example.com");
            httpRequestHead.Headers["Single"].ShouldEqual("val");
            httpRequestHead.Headers["Multiple"].ShouldEqual("val1,val2");
            httpRequestHead.ToString().ShouldEqual("GET https://example.com/app/test?a=1&b=2\r\n" +
            "Host: example.com\r\n" +
            "Single: val\r\n" +
            "Multiple: val1,val2\r\n\r\n");
        }
    }
}