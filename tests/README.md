# HTTP resource regression test

Run `dotnet run --project tests/NaverCafeClient.Regression.csproj` with the .NET 8 SDK or newer. The fake handler checks disposal after success, API error, malformed JSON, and transport failure, then reuses the caller-owned HttpClient. No Naver endpoint or customer data is used.
