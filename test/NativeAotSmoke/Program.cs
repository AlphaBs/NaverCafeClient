using System.Net;
using System.Text.Json;
using NaverCafeClient;

var handler = new FixtureHandler();
using var http = new HttpClient(handler);
var cafe = new NaverCafe(42, "fixture", http);
var checks = 0;

handler.Result = "{\"mobileCafeName\":\"테스트\",\"memberCount\":123}";
var gate = await cafe.GetCafeGateInfo();
Check(gate.MemberCount == 123 && gate.MobileCafeName == "테스트", "gate");
handler.Result = "{}";
await cafe.GetMemberInfo();
Check(handler.LastPath.Contains("CafeMemberInfo"), "member");

handler.Result = "{\"cafeId\":42,\"articleList\":[{\"type\":\"ARTICLE\",\"item\":{\"articleId\":7,\"subject\":\"공지\"}}],\"blockMemberList\":[{\"unknown\":true}]}";
Func<Task<NaverCafeArticleList>>[] calls =
{
    () => cafe.GetArticleList(new NaverCafeArticleListQuery()),
    () => cafe.GetArticleListFromBoard("3", new NaverCafeArticleListQuery()),
    () => cafe.GetNoticeList(),
    () => cafe.GetNoticeListFromBoard("3"),
    () => cafe.GetWeeklyPopularArticleList()
};
foreach (var call in calls)
{
    var articles = await call();
    Check(articles.CafeId == 42 && articles.ArticleList[0].Item.ArticleId == 7, "articles");
    Check(articles.BlockMemberList?[0] is JsonElement, "unknown object metadata");
}
handler.Result = "{\"menus\":[{\"menuId\":3,\"menuName\":\"게시판\"}]}";
var menus = await cafe.GetSideMenuList();
Check(menus.Menus[0].MenuId == 3, "side menu");

foreach (var error in new[] { "{\"code\":\"E1\",\"msg\":\"denied\"}", "{\"code\":\"E2\"}" })
{
    handler.Error = error;
    try
    {
        await cafe.GetCafeGateInfo();
        throw new Exception("Expected API error");
    }
    catch (NaverCafeApiException ex)
    {
        Check(ex.Code is "E1" or "E2", "error metadata");
    }
}
handler.Error = null;
foreach (var result in new[] { "null", "MISSING" })
{
    handler.Result = result;
    try
    {
        await cafe.GetCafeGateInfo();
        throw new Exception("Expected empty result error");
    }
    catch (NaverCafeApiException)
    {
        Check(true, "missing/null result");
    }
}
handler.Result = "[]";
try
{
    await cafe.GetCafeGateInfo();
    throw new Exception("Expected malformed model error");
}
catch (JsonException)
{
    Check(true, "malformed model");
}
Check(cafe.GetMainPage(true) == "https://m.cafe.naver.com/fixture", "mobile URL");
Console.WriteLine($"Passed {checks} NaverCafe protocol checks. JSON reflection: {JsonSerializer.IsReflectionEnabledByDefault}");

void Check(bool condition, string name)
{
    if (!condition)
        throw new Exception(name);

    checks++;
}

sealed class FixtureHandler : HttpMessageHandler
{
    public string Result { get; set; } = "{}";
    public string? Error { get; set; }
    public string LastPath { get; private set; } = "";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        LastPath = request.RequestUri!.PathAndQuery;
        var body = "{\"message\":{\"status\":\"200\"";
        if (Error != null)
            body += ",\"error\":" + Error;
        else if (Result != "MISSING")
            body += ",\"result\":" + Result;

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body + "}}")
        });
    }
}
