using Kayak.Http;

namespace HttpMock
{
    public class ReceivedRequest
    {
        public Kayak.Http.HttpRequestHead RequestHead { get; private set; }
        public string Body { get; private set; }

        internal ReceivedRequest(Kayak.Http.HttpRequestHead head, string body)
        {
            RequestHead = head;
            Body = body;
        }
    }
}