using System.Net;

namespace FigmaMcpServer.Api.Clients;

public sealed class FigmaApiException : Exception
{
    public FigmaApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
