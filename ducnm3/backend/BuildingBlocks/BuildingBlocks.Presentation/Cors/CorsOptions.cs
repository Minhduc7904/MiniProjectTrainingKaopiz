namespace BuildingBlocks.Presentation.Cors;

/// <summary>Options bind từ <c>Cors:AllowedOrigins</c>; được <c>AddLmsCors</c> chuẩn hóa và validate.</summary>
public sealed class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = [];
}
