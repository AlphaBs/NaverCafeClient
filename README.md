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


## Native AOT 검증

- 기존 대상 프레임워크를 유지하고 `net8.0` 자산을 추가했습니다. 이 자산에는 AOT·트리밍 분석을 활성화합니다.
- 내장 요청·응답 모델은 System.Text.Json 소스 생성 메타데이터를 사용합니다.
- 실제 계정이나 외부 API 없이 mock HTTP 응답과 요청 본문으로 검증합니다. 실제 로그인 성공 여부를 검증하는 테스트는 아닙니다.
- .NET 10 SDK와 대상 플랫폼의 Native AOT 도구 체인이 필요합니다. 아래 예시는 Windows x64입니다.

```powershell
dotnet build src/NaverCafeClient.csproj -c Release
dotnet run --project test/NativeAotSmoke -c Release
dotnet publish test/NativeAotSmoke -c Release -r win-x64 -o artifacts/native-smoke
./artifacts/native-smoke/NativeAotSmoke.exe
```
