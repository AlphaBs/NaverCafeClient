using System.Net;
using System.Text.Json;
using NaverCafeClient;

using var handler = new TrackingHandler();
using var http = new HttpClient(handler);
var cafe = new NaverCafe(1, "offline", http);
var count = 0;
foreach (var scenario in new[] { "success", "api-error", "invalid-json" })
{
    handler.Scenario = scenario;
    try
    {
        await cafe.GetCafeGateInfo();
        if (scenario != "success") throw new Exception("Expected parsing failure");
    }
    catch (NaverCafeApiException) when (scenario == "api-error") { }
    catch (JsonException) when (scenario == "invalid-json") { }
    if (handler.LastRequestContent?.Disposed != true) throw new Exception("Request content not disposed: " + scenario);
    if (handler.LastContent?.Disposed != true) throw new Exception("Response content not disposed: " + scenario);
    count++;
}
handler.ThrowOnSend = true;
try { await cafe.GetCafeGateInfo(); throw new Exception("Expected transport failure"); }
catch (HttpRequestException) { }
if (handler.LastRequestContent?.Disposed != true) throw new Exception("Request not disposed after transport failure");
count++;
handler.ThrowOnSend = false;
// The injected HttpClient remains caller-owned and usable after every call.
handler.Scenario = "success";
await cafe.GetCafeGateInfo();
if (handler.Calls != 5) throw new Exception("Injected client was not reusable");
Console.WriteLine($"PASS: {count} disposal paths and reusable HttpClient (offline)");

sealed class TrackingContent : StringContent
{
    public bool Disposed { get; private set; }
    public TrackingContent(string body) : base(body) { }
    protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
}
sealed class TrackingHandler : HttpMessageHandler
{
    public string Scenario = "success";
    public int Calls;
    public TrackingContent? LastContent;
    public TrackingContent? LastRequestContent;
    public bool ThrowOnSend;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        // Give the request a disposable resource to observe ownership even for GET.
        LastRequestContent = new TrackingContent("request-fixture");
        request.Content = LastRequestContent;
        if (ThrowOnSend) throw new HttpRequestException("synthetic transport failure");
        var body = Scenario switch
        {
            "api-error" => "{\"message\":{\"status\":\"500\",\"error\":{\"code\":\"TEST\",\"msg\":\"offline\"}}}",
            "invalid-json" => "not-json",
            _ => "{\"message\":{\"status\":\"200\",\"result\":{}}}"
        };
        LastContent = new TrackingContent(body);
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = LastContent });
    }
}
