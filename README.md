# NaverCafeClient

네이버 카페 크롤링을 위한 닷넷 라이브러리입니다.

## 기능

- 카페 기본 정보 (멤버수, 카페종류 등)
- 모바일 카페 대문
- 현재 로그인한 유저의 카페 내 정보 (**로그인 기능은 포함되어 있지 않음**)
- 게시판 목록
- 전체 글 목록
- 특정 게시판 내 글 목록
- 전체 공지글 목록
- 게시판 공지글 목록
- 인기글 목록

## 설치

[NuGet 패키지 NaverCafeClient](https://www.nuget.org/packages/NaverCafeClient)

## 예제

```csharp
using NaverCafeClient;

var httpClient = new HttpClient();
var cafe = new NaverCafe(10050146, "joonggonara", httpClient); // 중고나라

// 카페 대문
var gateInfo = await cafe.GetCafeGateInfo();

// 멤버 정보
var memberInfo = await cafe.GetMemberInfo();

// 게시판 목록
var sideMenuList = await cafe.GetSideMenuList();

// 전체글
var articleList = await cafe.GetArticleList(new NaverCafeArticleListQuery());

// 전체게시판 공지
var noticeList = await cafe.GetNoticeList();

// 인기글
var popularArticleList = await cafe.GetWeeklyPopularArticleList();

// '중고나라 무료나눔' 게시판 글
var articleListFromBoard = await cafe.GetArticleListFromBoard("96", new NaverCafeArticleListQuery());
foreach (var a in articleListFromBoard.ArticleList)
    Console.WriteLine($"{a.Type}, {a.Item.Subject}, {a.Item.GetDesktopUrl(cafe.ClubUrl)}");

// '중고나라 무료나눔' 게시판 공지
var noticeListFromBoard = await cafe.GetNoticeListFromBoard("96");
foreach (var n in noticeListFromBoard.ArticleList)
    Console.WriteLine($"{n.Type}, {n.Item.Subject}, {n.Item.GetMobileUrl()}");

```

