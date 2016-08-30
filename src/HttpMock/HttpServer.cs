using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Nowin;

namespace HttpMock
{
    public class HttpServer : IHttpServer
    {
        private readonly RequestProcessor _requestProcessor;
        private readonly RequestWasCalled _requestWasCalled;
        private readonly RequestWasNotCalled _requestWasNotCalled;
        private readonly Uri _uri;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AutoResetEvent _finishEvent = new AutoResetEvent(false);

        private Thread _thread;
        private readonly RequestHandlerFactory _requestHandlerFactory;

        public HttpServer(Uri uri)
        {
            _uri = uri;
            _requestProcessor = new RequestProcessor(new EndpointMatchingRule(), new RequestHandlerList());
            _requestWasCalled = new RequestWasCalled(_requestProcessor);
            _requestWasNotCalled = new RequestWasNotCalled(_requestProcessor);
            _requestHandlerFactory = new RequestHandlerFactory(_requestProcessor);
        }

        public void Start()
        {
            _thread = new Thread(StartListening);
            _thread.Start();
            if (!IsAvailable())
            {
                throw new InvalidOperationException("Kayak server not listening yet.");
            }
        }

        public bool IsAvailable()
        {
            const int timesToWait = 5;
            var attempts = 0;
            using (var tcpClient = new TcpClient())
            {
                while (attempts < timesToWait)
                {
                    try
                    {
                        tcpClient.Connect(_uri.Host, _uri.Port);
                        return tcpClient.Connected;
                    }
                    catch (SocketException)
                    {
                    }
                    attempts++;
                }
                return false;
            }
        }

        public void Dispose()
        {
            _finishEvent.Set();
        }

        public RequestHandler Stub(Func<RequestHandlerFactory, RequestHandler> func)
        {
            return func.Invoke(_requestHandlerFactory);
        }

        public RequestHandler AssertWasCalled(Func<RequestWasCalled, RequestHandler> func)
        {
            return func.Invoke(_requestWasCalled);
        }

        public RequestHandler AssertWasNotCalled(Func<RequestWasNotCalled, RequestHandler> func)
        {
            return func.Invoke(_requestWasNotCalled);
        }

        public IHttpServer WithNewContext()
        {
            _requestProcessor.ClearHandlers();
            return this;
        }

        public IHttpServer WithNewContext(string baseUri)
        {
            WithNewContext();
            return this;
        }

        public string WhatDoIHave()
        {
            return _requestProcessor.WhatDoIHave();
        }

        private void StartListening()
        {
            try
            {
                var server = ServerBuilder
                    .New().SetPort(_uri.Port)
                    .SetOwinApp(context =>
                    {
                        var requestHeader = HttpRequestHead.LoadFromOwinContext(context);
                        _requestProcessor.OnRequest(requestHeader, null, null);

                        var responseText = "Hello World via OWIN";
                        var responseBytes = Encoding.UTF8.GetBytes(responseText);

                        // OWIN Environment Keys: http://owin.org/spec/spec/owin-1.0.0.html
                        var responseStream = (Stream) context["owin.ResponseBody"];
                        var responseHeaders = (IDictionary<string, string[]>) context["owin.ResponseHeaders"];

                        responseHeaders["Content-Length"] = new string[]
                            {responseBytes.Length.ToString(CultureInfo.InvariantCulture)};
                        responseHeaders["Content-Type"] = new string[] {"text/plain"};



                        return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    })
                    .Build();
                server.Start();
                _finishEvent.WaitOne();
                server.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Error when trying to StartListening", ex);
            }
        }
    }
}