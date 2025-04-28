using API_dormitory.Data;
using API_dormitory.Models.common;
using API_dormitory.Models.Post;
using API_dormitory.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<PostModel> _postCollection;
        private readonly EmailService _emailService;
        private readonly IMongoCollection<AccountModels> _accounts;
        private readonly IMongoCollection<InfoStudentModels> _infoStudents;


        public PostController(IConfiguration configuration, MongoDbContext context, EmailService emailService)
        {
            _configuration = configuration;
            _postCollection = context.GetCollection<PostModel>("Posts");
            _accounts = context.GetCollection<AccountModels>("Accounts");
            _infoStudents = context.GetCollection<InfoStudentModels>("InfoStudents");
            _emailService = emailService;
        }

        // Lấy tất cả bài đăng
        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _postCollection
                                .Find(_ => true)
                                .SortByDescending(post => post.PostDate)  // Sắp xếp theo PostDate giảm dần
                                .ToListAsync();

            return Ok(posts);
        }


        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest(new { message = "ID không hợp lệ." });
            }

            var post = await _postCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound(new { message = "Bài đăng không tồn tại." });
            }
            return Ok(post);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostModel newPost)
        {
            if (newPost == null)
                return BadRequest(new { message = "Dữ liệu bài đăng không hợp lệ." });

            newPost.PostDate = DateTime.UtcNow;
            await _postCollection.InsertOneAsync(newPost);

            // 🔍 Tìm các tài khoản đang hoạt động và có role là "student"
            var activeStudents = await _accounts
                .Find(acc => acc.Status == OperatingStatusEnum.active && acc.Roles == RoleTypeStatusEnum.Student)
                .ToListAsync();

            foreach (var studentAccount in activeStudents)
            {
                var infoStudent = await _infoStudents
                    .Find(info => info.AccountId == studentAccount.AccountId)
                    .FirstOrDefaultAsync();

                if (infoStudent != null && !string.IsNullOrEmpty(infoStudent.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            infoStudent.Email,
                            studentAccount.UserName ?? "Bạn",
                            "Có bài viết mới",
                            $"<p>Xin chào <strong>{studentAccount.UserName}</strong>,</p><p>Bài viết mới: <b>{newPost.Title}</b></p><p>{newPost.Content}</p>"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Lỗi gửi email tới {infoStudent.Email}: {ex.Message}");
                    }
                }
            }

            return CreatedAtAction(nameof(GetPostById), new { id = newPost.Id.ToString() }, newPost);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] PostModel updatedPost)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest(new { message = "ID không hợp lệ." });
            }

            var existingPost = await _postCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (existingPost == null)
            {
                return NotFound(new { message = "Bài đăng không tồn tại." });
            }

            updatedPost.Id = existingPost.Id; // Đảm bảo ID không bị thay đổi
            updatedPost.PostDate = existingPost.PostDate; // Giữ nguyên ngày đăng

            var updateDefinition = Builders<PostModel>.Update
                .Set(p => p.Title, updatedPost.Title)
                .Set(p => p.Content, updatedPost.Content);

            await _postCollection.UpdateOneAsync(p => p.Id == id, updateDefinition);
            return Ok(updatedPost);
        }


        // Xóa bài đăng
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            var result = await _postCollection.DeleteOneAsync(p => p.Id == id);

            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Bài đăng không tồn tại." });
            }

            return Ok(new { message = "Bài đăng đã bị xóa." });
        }
    }
}
