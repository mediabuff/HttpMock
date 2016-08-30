using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HttpMock
{
    public struct HttpRequestHead
    {
        public string Method;

        public string Uri;

        public string Path;

        public string QueryString;

        public IDictionary<string, string> Headers;

        public bool HasBody;

        // OWIN Spec: http://owin.org/spec/spec/owin-1.0.0.html
        public static HttpRequestHead LoadFromOwinContext(IDictionary<string, object> context)
        {
            var headers = (IDictionary<string, string[]>) context["owin.RequestHeaders"];
            var host = headers["Host"].First();
            var scheme = (string) context["owin.RequestScheme"];
            var body = (Stream) context["owin.RequestBody"];
            var result = new HttpRequestHead
            {
                Method = (string) context["owin.RequestMethod"],
                Path = (string) context["owin.RequestPathBase"] + (string) context["owin.RequestPath"],
                QueryString = (string) context["owin.RequestQueryString"],
                Uri = $"{scheme}://{host}",
                Headers = new Dictionary<string, string>(),
                HasBody = body.Length > 0
            };
            result.Uri += result.Path;
            if (!string.IsNullOrEmpty(result.QueryString))
            {
                result.Uri += "?" + result.QueryString;
            }
            foreach (var key in headers.Keys)
            {
                result.Headers[key] = headers[key].Length > 1 ?
                    string.Join(",", headers[key]) : headers[key][0];
            }

            return result;
        }

        public override string ToString()
        {
            return
                $"{Method} {Uri}\r\n{(Headers != null ? Headers.Aggregate("", (acc, kv) => acc += $"{kv.Key}: {kv.Value}\r\n") : "")}\r\n";
        }
    }
}