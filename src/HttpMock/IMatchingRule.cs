using Kayak.Http;

namespace HttpMock
{
    public interface IMatchingRule
	{
		bool IsEndpointMatch(IRequestHandler requestHandler, Kayak.Http.HttpRequestHead request);
	}
}