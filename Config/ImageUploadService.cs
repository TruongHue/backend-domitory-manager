using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;

public class ImageUploadService
{
    public string UploadImage(string filePath)
    {
        var cloudinary = CloudinaryConfig.GetCloudinaryInstance();

        // Đọc tệp ảnh từ đường dẫn
        using (var fs = new FileStream(filePath, FileMode.Open))
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(filePath, fs)
            };

            var uploadResult = cloudinary.Upload(uploadParams);

            // Trả về URL của ảnh đã upload
            return uploadResult?.SecureUrl?.ToString();
        }
    }
}
