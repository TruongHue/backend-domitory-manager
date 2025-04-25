using API_dormitory.Data;
using API_dormitory.Models.Feedback;
using API_dormitory.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace API_dormitory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<FeedbackModels> _feedbacks;
        private readonly IMongoCollection<AccountModels> _accounts;

        public FeedbackController(IConfiguration configuration, MongoDbContext context)
        {
            _configuration = configuration;
            _feedbacks = context.GetCollection<FeedbackModels>("Feedbacks");
            _accounts = context.GetCollection<AccountModels>("Accounts");
        }

        // Lấy danh sách tất cả phản hồi (admin hoặc dashboard)
        [HttpGet]
        public async Task<ActionResult<List<object>>> GetAll()
        {
            // Lấy tất cả phản hồi từ MongoDB
            var feedbackList = await _feedbacks.Find(_ => true)
                                               .SortByDescending(f => f.CreatedAt)
                                               .ToListAsync();

            // Lấy tất cả tài khoản từ MongoDB
            var accounts = await _accounts.Find(_ => true).ToListAsync();

            // Kết hợp thông tin phản hồi với thông tin tài khoản
            var result = feedbackList.Select(feedback =>
            {
                // Tìm tài khoản liên quan đến phản hồi (dựa vào AccountId trong phản hồi)
                var account = accounts.FirstOrDefault(acc => acc.AccountId.ToString() == feedback.AccountId.ToString());

                return account != null ? new
                {
                    // Thông tin phản hồi
                    Feedback = new
                    {
                        feedback.Id,
                        feedback.Title,
                        feedback.Sender,
                        feedback.Content,
                        feedback.Response,
                        feedback.CreatedAt,
                        feedback.ResponseAt
                    },

                    // Thông tin tài khoản (Account)
                    Account = new
                    {
                        AccountId = account.AccountId.ToString(),
                        account.UserName,
                        account.UserCode,
                        account.NumberPhone,
                        account.Roles,
                        account.Status
                    }
                } : null;
            })
            .Where(result => result != null) // Loại bỏ phản hồi không có tài khoản hợp lệ
            .ToList();

            // Nếu không có phản hồi hoặc tài khoản hợp lệ, trả về lỗi
            if (!result.Any())
            {
                return NotFound(new { message = "Không có phản hồi nào với tài khoản hợp lệ!" });
            }

            // Trả về danh sách kết hợp phản hồi và tài khoản
            return Ok(result);
        }


        // Gửi phản hồi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedbackModels feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback.Title) ||
                string.IsNullOrWhiteSpace(feedback.Content) ||
                string.IsNullOrWhiteSpace(feedback.AccountId))
            {
                return BadRequest("Vui lòng nhập đầy đủ thông tin.");
            }

            // Tìm thông tin người gửi
            var account = await _accounts.Find(a => a.AccountId.ToString() == feedback.AccountId).FirstOrDefaultAsync();
            if (account == null)
                return NotFound("Không tìm thấy tài khoản gửi phản hồi.");

            feedback.Sender = account.UserName; // Lấy tên từ tài khoản
            feedback.CreatedAt = DateTime.UtcNow;
            feedback.Response = string.Empty;
            feedback.ResponseAt = null; // Không có phản hồi lúc mới gửi

            await _feedbacks.InsertOneAsync(feedback);
            return Ok(new { message = "Gửi thành công!" });
        }

        // Admin phản hồi lại
        [HttpPut("{id}/response")]
        public async Task<IActionResult> UpdateResponse(string id, [FromBody] string response)
        {
            var update = Builders<FeedbackModels>.Update
                .Set(f => f.Response, response)
                .Set(f => f.ResponseAt, DateTime.UtcNow); // Lưu thời gian phản hồi

            var result = await _feedbacks.UpdateOneAsync(
                f => f.Id == id,
                update
            );

            if (result.MatchedCount == 0)
                return NotFound();

            return Ok(new { message = "Phản hồi đã được cập nhật." });
        }

        // Lấy phản hồi theo AccountId
        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<List<object>>> GetByAccountId(string accountId)
        {
            // Tìm các phản hồi theo AccountId
            var feedbackList = await _feedbacks.Find(f => f.AccountId == accountId)
                                               .SortByDescending(f => f.CreatedAt)
                                               .ToListAsync();

            if (feedbackList == null || feedbackList.Count == 0)
            {
                return NotFound(new { message = "Không có phản hồi nào của tài khoản này!" });
            }

            // Tìm tài khoản tương ứng
            var account = await _accounts.Find(a => a.AccountId.ToString() == accountId).FirstOrDefaultAsync();
            if (account == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản!" });
            }

            // Trả về danh sách phản hồi kèm thông tin tài khoản
            var result = feedbackList.Select(feedback => new
            {
                // Thông tin phản hồi
                Feedback = new
                {
                    feedback.Id,
                    feedback.Title,
                    feedback.Sender,
                    feedback.Content,
                    feedback.Response,
                    feedback.CreatedAt,
                    feedback.ResponseAt
                },

                // Thông tin tài khoản (Account)
                Account = new
                {
                    AccountId = account.AccountId.ToString(),
                    account.UserName,
                    account.UserCode,
                    account.NumberPhone,
                    account.Roles,
                    account.Status
                }
            }).ToList();

            return Ok(result);
        }

    }
}
