using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NaverCafeClient;

public class NaverCafe
{
    private readonly HttpClient _httpClient;

    public NaverCafe(
        int cafeId, 
        string clubUrl, 
        HttpClient httpClient)
    {
        CafeId = cafeId;
        ClubUrl = clubUrl;
        _httpClient = httpClient;
    }

    public int CafeId { get; }
    public string ClubUrl { get; }

    public async Task<NaverCafeGateInfo> GetCafeGateInfo()
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/CafeGateInfo.json?cluburl={ClubUrl}"),
        }, NaverCafeJsonContext.Default.NaverCafeGateInfo);
    }

    public async Task<NaverCafeMemberInfo> GetMemberInfo()
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/CafeMemberInfo.json?cafeId={CafeId}")
        }, NaverCafeJsonContext.Default.NaverCafeMemberInfo);
    }

    public async Task<NaverCafeArticleList> GetArticleList(NaverCafeArticleListQuery query)
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/ArticleListV2dot1.json?search.clubid={CafeId}&{query.BuildQueryString()}&ad=true&adUnit=MW_CAFE_ARTICLE_LIST_RS")
        }, NaverCafeJsonContext.Default.NaverCafeArticleList);
    }

    public async Task<NaverCafeArticleList> GetArticleListFromBoard(string menuId, NaverCafeArticleListQuery query)
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/ArticleListV2dot1.json?search.clubid={CafeId}&search.menuid={menuId}&{query.BuildQueryString()}&ad=true&adUnit=MW_CAFE_ARTICLE_LIST_RS")
        }, NaverCafeJsonContext.Default.NaverCafeArticleList);
    }

    public async Task<NaverCafeArticleList> GetNoticeList()
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/NoticeListV3.json?cafeId={CafeId}&ad=true&mobileWeb=true&adUnit=MW_CAFE_BOARD")
        }, NaverCafeJsonContext.Default.NaverCafeArticleList);
    }

    public async Task<NaverCafeArticleList> GetNoticeListFromBoard(string menuId)
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/NoticeListV3.json?cafeId={CafeId}&menuId={menuId}&ad=true&mobileWeb=true&adUnit=MW_CAFE_BOARD")
        }, NaverCafeJsonContext.Default.NaverCafeArticleList);
    }

    public async Task<NaverCafeArticleList> GetWeeklyPopularArticleList()
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/WeeklyPopularArticleListV3.json?cafeId={CafeId}&ad=true&adUnit=MW_CAFE_BOARD&mobileWeb=true")
        }, NaverCafeJsonContext.Default.NaverCafeArticleList);
    }

    public async Task<NaverCafeSideMenuList> GetSideMenuList()
    {
        return await request(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/SideMenuList?cafeId={CafeId}")
        }, NaverCafeJsonContext.Default.NaverCafeSideMenuList);
    }

    public string GetMainPage(bool isMobile = false)
    {
        if (isMobile)
            return $"https://m.cafe.naver.com/{ClubUrl}";
        else
            return $"https://cafe.naver.com/{ClubUrl}";
    }

    private async Task<T> request<T>(HttpRequestMessage req, JsonTypeInfo<T> typeInfo)
    {
        using var request = req;
        using var res = await _httpClient.SendAsync(request);
        using var stream = await res.Content.ReadAsStreamAsync();
        return await parseResponseStream(stream, typeInfo);
    }

    private async Task<T> parseResponseStream<T>(Stream stream, JsonTypeInfo<T> typeInfo)
    {
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var message = jsonDoc.RootElement.GetProperty("message");
        var status = message.GetProperty("status");

        if (message.TryGetProperty("error", out var error))
        {
            var errorObj = error.Deserialize(NaverCafeJsonContext.Default.NaverCafeError);
            if (!string.IsNullOrEmpty(errorObj?.Message) || !string.IsNullOrEmpty(errorObj?.Code))
                throw new NaverCafeApiException(errorObj?.Code, errorObj?.Message);
        }

        if (message.TryGetProperty("result", out var result))
        {
            return result.Deserialize(typeInfo) ?? throw new NaverCafeApiException();
        }

        throw new NaverCafeApiException();
    }
}