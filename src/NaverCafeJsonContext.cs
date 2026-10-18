using System.Text.Json.Serialization;

namespace NaverCafeClient;

// 응답 모델을 빌드 시 등록하여 Native AOT에서도 리플렉션 없이 역직렬화한다.
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(NaverCafeGateInfo))]
[JsonSerializable(typeof(NaverCafeMemberInfo))]
[JsonSerializable(typeof(NaverCafeArticleList))]
[JsonSerializable(typeof(NaverCafeSideMenuList))]
[JsonSerializable(typeof(NaverCafeError))]
internal partial class NaverCafeJsonContext : JsonSerializerContext
{
}
