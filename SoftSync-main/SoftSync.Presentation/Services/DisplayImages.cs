namespace SoftSync.Presentation.Services;

/// <summary>Small display assets; stored URLs and original uploads stay intact.</summary>
public static class DisplayImages
{
    public static string? Url(string? original) => original switch
    {
        "/images/mentor-nguyen-hoang-van-anh.png" => "/images/optimized/mentor-nguyen-hoang-van-anh.webp",
        "/images/mentor-nguyen-thuy-to-tram.png" => "/images/optimized/mentor-nguyen-thuy-to-tram.webp",
        "/images/mentor-nguyen-xuan.png" => "/images/optimized/mentor-nguyen-xuan.webp",
        "/images/syncy-robot.png" => "/images/optimized/syncy-robot.webp",
        "/images/logo.png" => "/images/optimized/logo.webp",
        _ => original
    };
}
