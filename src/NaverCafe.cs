using System.Text.Json;

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
        return await request<NaverCafeGateInfo>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/CafeGateInfo.json?cluburl={ClubUrl}"),
        });
    }

    public async Task<NaverCafeMemberInfo> GetMemberInfo()
    {
        return await request<NaverCafeMemberInfo>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/CafeMemberInfo.json?cafeId={CafeId}")
        });
    }

    public async Task<NaverCafeArticleList> GetArticleList(NaverCafeArticleListQuery query)
    {
        return await request<NaverCafeArticleList>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/ArticleListV2dot1.json?search.clubid={CafeId}&{query.BuildQueryString()}&ad=true&adUnit=MW_CAFE_ARTICLE_LIST_RS")
        });
    }

    public async Task<NaverCafeArticleList> GetArticleListFromBoard(string menuId, NaverCafeArticleListQuery query)
    {
        return await request<NaverCafeArticleList>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/ArticleListV2dot1.json?search.clubid={CafeId}&search.menuid={menuId}&{query.BuildQueryString()}&ad=true&adUnit=MW_CAFE_ARTICLE_LIST_RS")
        });
    }

    public async Task<NaverCafeArticleList> GetNoticeList()
    {
        return await request<NaverCafeArticleList>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/NoticeListV3.json?cafeId={CafeId}&ad=true&mobileWeb=true&adUnit=MW_CAFE_BOARD")
        });
    }

    public async Task<NaverCafeArticleList> GetNoticeListFromBoard(string menuId)
    {
        return await request<NaverCafeArticleList>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/NoticeListV3.json?cafeId={CafeId}&menuId={menuId}&ad=true&mobileWeb=true&adUnit=MW_CAFE_BOARD")
        });
    }

    public async Task<NaverCafeArticleList> GetWeeklyPopularArticleList()
    {
        return await request<NaverCafeArticleList>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/WeeklyPopularArticleListV3.json?cafeId={CafeId}&ad=true&adUnit=MW_CAFE_BOARD&mobileWeb=true")
        });
    }

    public async Task<NaverCafeSideMenuList> GetSideMenuList()
    {
        return await request<NaverCafeSideMenuList>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://apis.naver.com/cafe-web/cafe2/SideMenuList?cafeId={CafeId}")
        });
    }

    public string GetMainPage(bool isMobile = false)
    {
        if (isMobile)
            return $"https://m.cafe.naver.com/{ClubUrl}";
        else
            return $"https://cafe.naver.com/{ClubUrl}";
    }

    private async Task<T> request<T>(HttpRequestMessage req)
    {
        using (req)
        using (var res = await _httpClient.SendAsync(req))
        using (var stream = await res.Content.ReadAsStreamAsync())
        {
            return await parseResponseStream<T>(stream);
        }
    }

    private async Task<T> parseResponseStream<T>(Stream stream)
    {
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var message = jsonDoc.RootElement.GetProperty("message");
        var status = message.GetProperty("status");

        if (message.TryGetProperty("error", out var error))
        {
            var errorObj = error.Deserialize<NaverCafeError>();
            if (!string.IsNullOrEmpty(errorObj?.Message) || !string.IsNullOrEmpty(errorObj?.Code))
            throw new NaverCafeApiException(errorObj?.Code, errorObj?.Message);
        }

        if (message.TryGetProperty("result", out var result))
        {
            return result.Deserialize<T>() ?? throw new NaverCafeApiException();
        }

        throw new NaverCafeApiException();
    }
}