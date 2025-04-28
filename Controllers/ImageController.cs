using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly ImageUploadService _imageUploadService;

    public ImageController()
    {
        _imageUploadService = new ImageUploadService();
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Student,Staff")]
    public IActionResult UploadImage([FromForm] IFormFile file)
    {
        try
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("path-to-your-temp-folder", file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var imageUrl = _imageUploadService.UploadImage(filePath);

                return Ok(new { imageUrl });
            }
            return BadRequest("No file uploaded.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
