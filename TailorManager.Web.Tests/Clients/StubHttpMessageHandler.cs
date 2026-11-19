using System.Net;
using System.Text;

namespace TailorManager.Web.Tests;

// Answers each path with the JSON the test gives it, and remembers the requests.
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, (HttpStatusCode Status, string Json)> _responses = new();
    public List<HttpRequestMessage> Requests { get; } = [];
    public List<string?> Bodies { get; } = [];

    // An HttpClient that sends to this stub instead of the API: new CustomersClient(_handler.CreateHttpClient(), tokens).
    public HttpClient CreateHttpClient() => new(this) { BaseAddress = new Uri("http://api.test/") };

    public StubHttpMessageHandler Respond(string path, string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses[path] = (status, json);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        Bodies.Add(request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken));
        if (!_responses.TryGetValue(request.RequestUri!.AbsolutePath, out var response))
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        return new HttpResponseMessage(response.Status) { Content = new StringContent(response.Json, Encoding.UTF8, "application/json") };
    }
}
