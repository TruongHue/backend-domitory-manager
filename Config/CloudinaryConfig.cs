using CloudinaryDotNet;

public class CloudinaryConfig
{
    private static Cloudinary _cloudinary;

    public static Cloudinary GetCloudinaryInstance()
    {
        if (_cloudinary == null)
        {
            var account = new Account(
                "your-cloud-name",  // Thay bằng Cloud Name của bạn
                "your-api-key",     // Thay bằng API Key của bạn
                "your-api-secret"   // Thay bằng API Secret của bạn
            );
            _cloudinary = new Cloudinary(account);
        }
        return _cloudinary;
    }
}
