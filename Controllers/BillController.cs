using API_dormitory.Data;
using API_dormitory.Models.Bills;
using API_dormitory.Models.common;
using API_dormitory.Models.DTO;
using API_dormitory.Models.DTO.Bill.BillElectricity;
using API_dormitory.Models.DTO.Bill.BillRoom;
using API_dormitory.Models.DTO.Bill.BillWater;
using API_dormitory.Models.DTO.Building;
using API_dormitory.Models.DTO.Room;
using API_dormitory.Models.registerRoom;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API_dormitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : ControllerBase
    {

        private readonly IMongoCollection<RoomBillModels> _roomBillCollection;
        private readonly IMongoCollection<RegisterRoomModels> _registerRoomCollection;
        private readonly IMongoCollection<InfoRoomModels> _roomsCollection;
        private readonly IMongoCollection<ElectricityBillModels> _electricityBillCollection;
        private readonly IMongoCollection<WaterBillModels> _waterBillCollection;
        private readonly IMongoCollection<PriceWaterAndElectricity> _priceCollection;
        private readonly EmailService _emailService;
        private readonly IMongoCollection<InfoStudentModels> _infoStudents;

        public BillController(MongoDbContext database, EmailService emailService)
        {
            _roomBillCollection = database.GetCollection<RoomBillModels>("RoomBills");
            _electricityBillCollection = database.GetCollection<ElectricityBillModels>("ElectricityBills");
            _waterBillCollection = database.GetCollection<WaterBillModels>("WaterBills");
            _roomsCollection = database.GetCollection<InfoRoomModels>("Rooms");
            _priceCollection = database.GetCollection<PriceWaterAndElectricity>("PriceWaterAndElectricity");
            _infoStudents = database.GetCollection<InfoStudentModels>("InfoStudents");
            _registerRoomCollection = database.GetCollection<RegisterRoomModels>("RegisterRoom");
            _emailService = emailService;
        }


        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("Room")]
        public async Task<IActionResult> GetAllBillRooms()
        {
            var bills = await _roomBillCollection
                .Find(_ => true) // Lấy tất cả hóa đơn phòng
                .Project(rb => new RoomBillDTO
                {
                    IdRoomBill = rb.IdRoomBill.ToString(),
                    IdRoom = rb.IdRoom.ToString(),
                    PriceYear = rb.PriceYear,
                    DailyPrice = rb.DailyPrice,
                    DateOfRecord = rb.DateOfRecord,
                    Status = rb.Status,
                })
                .ToListAsync();

            if (!bills.Any())
            {
                return NoContent();
            }

            return Ok(bills);
        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("Room/{idRoom}")]
        public async Task<IActionResult> GetBillByRoomId(string idRoom)
        {
            var bill = await _roomBillCollection
                .Find(rb => rb.IdRoom.ToString() == idRoom) // Lọc theo IdRoom
                .Project(rb => new RoomBillDTO
                {
                    IdRoomBill = rb.IdRoomBill.ToString(),
                    IdRoom = rb.IdRoom.ToString(),
                    PriceYear = rb.PriceYear,
                    DailyPrice = rb.DailyPrice,
                    DateOfRecord = rb.DateOfRecord,
                    Status = rb.Status
                })
                .ToListAsync(); // Chuyển đổi thành danh sách bất đồng bộ

            if (bill == null || bill.Count == 0)
            {
                return NoContent(); // Trả về 204 nếu không tìm thấy hóa đơn nào
            }

            return Ok(bill); // Trả về danh sách hóa đơn của phòng
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("Room")]
        public async Task<IActionResult> CreateRoomBill([FromBody] AddBillRoomDTOs roomBillDto)
        {
            if (roomBillDto == null)
            {
                return BadRequest("Dữ liệu hóa đơn phòng không hợp lệ.");
            }


            // Kiểm tra phòng có tồn tại không
            var room = await _roomsCollection.Find(r => r.IdRoom.ToString() == roomBillDto.IdRoom).FirstOrDefaultAsync();
            if (room == null)
            {
                return NotFound($"Không tìm thấy phòng có ID {roomBillDto.IdRoom}.");
            }

            // Tìm tất cả các hóa đơn Active của phòng đó và cập nhật thành Inactive
            var roomId = ObjectId.Parse(roomBillDto.IdRoom); // Chuyển string thành ObjectId
            var filter = Builders<RoomBillModels>.Filter.Eq(rb => rb.IdRoom, roomId) &
                         Builders<RoomBillModels>.Filter.Eq(rb => rb.Status, OperatingStatusEnum.active);

            var update = Builders<RoomBillModels>.Update.Set(rb => rb.Status, OperatingStatusEnum.inactive);
            await _roomBillCollection.UpdateManyAsync(filter, update);

            // Thêm hóa đơn mới với trạng thái Active
            var newRoomBill = new RoomBillModels
            {
                IdRoomBill = ObjectId.GenerateNewId(),
                IdRoom = ObjectId.Parse(roomBillDto.IdRoom),
                DailyPrice = roomBillDto.DailyPrice,
                PriceYear = roomBillDto.PriceYear,
                DateOfRecord = roomBillDto.DateOfRecord,
                Status = OperatingStatusEnum.active,
            };

            await _roomBillCollection.InsertOneAsync(newRoomBill);

            return Ok(new { Message = "Thêm hóa đơn phòng thành công!", NewBill = newRoomBill });
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("price/add-or-update-price")]
        public async Task<IActionResult> AddOrUpdatePrice([FromBody] PriceWaterAndElectricity request)
        {
            if (request == null)
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ.");
            }

            if (_priceCollection == null)
            {
                return StatusCode(500, "Lỗi kết nối cơ sở dữ liệu.");
            }

            // Tạo một bản ghi mới với thời gian hiện tại
            var newPrice = new PriceWaterAndElectricity
            {
                ElectricityPrice = request.ElectricityPrice,
                WaterPrice = request.WaterPrice,
                WaterLimit = request.WaterLimit,
                WaterPriceOverLimit = request.WaterPriceOverLimit,
                ActionDate = DateTime.UtcNow // Lưu thời gian thực hiện thao tác
            };

            try
            {
                await _priceCollection.InsertOneAsync(newPrice);
                return Ok(new { Message = "Thêm bảng giá điện nước thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi thêm giá điện nước: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,Student")]
        [HttpGet("price")]
        public async Task<IActionResult> GetAllPrices()
        {
            var prices = await _priceCollection.Find(_ => true)
                .Project(p => new
                {
                    Id = p.Id.ToString(), // Chuyển ObjectId thành string
                    ElectricityPrice = p.ElectricityPrice,
                    WaterPrice = p.WaterPrice,
                    WaterLimit = p.WaterLimit,
                    WaterPriceOverLimit = p.WaterPriceOverLimit,
                    ActionDate = p.ActionDate
                })
                .ToListAsync();

            return Ok(prices);
        }

     

       
        [Authorize(Roles = "Admin")]
        [HttpDelete("price/{id}")]
        public async Task<IActionResult> DeletePriceById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("ID không hợp lệ.");
            }

            var result = await _priceCollection.DeleteOneAsync(p => p.Id == objectId);

            if (result.DeletedCount == 0)
            {
                return NotFound("Không tìm thấy bản ghi giá điện nước để xóa.");
            }

            return Ok(new { Message = "Xóa giá điện nước thành công!", DeletedId = id });
        }

        [Authorize(Roles = "Admin,Student")]
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetRoomBillByRoomId(string roomId)
        {
            if (!ObjectId.TryParse(roomId, out ObjectId objectIdRoom))
            {
                return BadRequest("ID phòng không hợp lệ.");
            }

            var roomBill = await _roomBillCollection
                .Find(bill => bill.IdRoom == objectIdRoom)
                .FirstOrDefaultAsync();

            if (roomBill == null)
            {
                return NotFound($"Không tìm thấy hóa đơn điện nước cho phòng có ID: {roomId}");
            }

            // Chuyển đổi ID sang string trước khi trả về
            var response = new
            {
                IdRoomBill = roomBill.IdRoomBill.ToString(),
                IdRoom = roomBill.IdRoom.ToString(),
                DailyPrice = roomBill.DailyPrice,
                PriceYear = roomBill.PriceYear,
                DateOfRecord = roomBill.DateOfRecord,
                Status = roomBill.Status
            };

            return Ok(response);
        }

        [Authorize(Roles = "Admin,Student,Staff")]

        [HttpGet("all/electricity/{roomId}")]
        public async Task<List<ElectricityBillDto>> GetAllElectricityBillsByRoom(string roomId)
        {
            if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
            {
                throw new ArgumentException("ID phòng không hợp lệ");
            }

            var bills = await _electricityBillCollection.Find(bill => bill.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .ToListAsync();

            // Chuyển đổi sang DTO với ID là string
            var result = bills.Select(b => new ElectricityBillDto
            {
                Id = b.Id.ToString(),
                IdRoom = b.IdRoom.ToString(),
                StudentCode = b.StudentCode,
                StudentName = b.StudentName,
                BeforeIndex = b.BeforeIndex,
                AfterIndex = b.AfterIndex,
                Price = b.Price,
                Total = b.Total,
                DateOfRecord = b.DateOfRecord,
                Status = b.Status
            }).ToList();

            return result;
        }
        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("all/water/{roomId}")]
        public async Task<List<WaterBillDto>> GetAllWaterBillsByRoom(string roomId)
        {
            if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
            {
                throw new ArgumentException("ID phòng không hợp lệ");
            }

            var bills = await _waterBillCollection.Find(bill => bill.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .ToListAsync();

            // Chuyển đổi sang DTO với ID là string
            var result = bills.Select(b => new WaterBillDto
            {
                Id = b.Id.ToString(),
                IdRoom = b.IdRoom.ToString(),
                StudentCode = b.StudentCode,
                StudentName = b.StudentName,
                BeforeIndex = b.BeforeIndex,
                AfterIndex = b.AfterIndex,
                Price = b.Price,
                Total = b.Total,
                IndexLimit = b.IndexLimit,
                PriceLimit = b.PriceLimit,
                DateOfRecord = b.DateOfRecord,
                Status = b.Status
            }).ToList();

            return result;
        }
        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("all/bills/{roomId}")]
        public async Task<IActionResult> GetAllBillsByRoom(string roomId)
        {
            if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
            {
                return BadRequest(new { message = "ID phòng không hợp lệ" });
            }

            // Lấy tất cả hóa đơn điện
            var electricityBills = await _electricityBillCollection.Find(bill => bill.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .ToListAsync();

            var electricityBillDtos = electricityBills.Select(b => new ElectricityBillDto
            {
                Id = b.Id.ToString(),
                IdRoom = b.IdRoom.ToString(),
                StudentCode = b.StudentCode,
                StudentName = b.StudentName,
                BeforeIndex = b.BeforeIndex,
                AfterIndex = b.AfterIndex,
                Price = b.Price,
                Total = b.Total,
                DateOfRecord = b.DateOfRecord,
                Status = b.Status
            }).ToList();

            // Lấy tất cả hóa đơn nước
            var waterBills = await _waterBillCollection.Find(bill => bill.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .ToListAsync();

            var waterBillDtos = waterBills.Select(b => new WaterBillDto
            {
                Id = b.Id.ToString(),
                IdRoom = b.IdRoom.ToString(),
                StudentCode = b.StudentCode,
                StudentName = b.StudentName,
                BeforeIndex = b.BeforeIndex,
                AfterIndex = b.AfterIndex,
                Price = b.Price,
                Total = b.Total,
                IndexLimit = b.IndexLimit,
                PriceLimit = b.PriceLimit,
                DateOfRecord = b.DateOfRecord,
                Status = b.Status
            }).ToList();

            // Kết hợp các hóa đơn điện và nước
            var allBills = new
            {
                ElectricityBills = electricityBillDtos,
                WaterBills = waterBillDtos
            };

            return Ok(allBills);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("electric/{id}")]
        public async Task<IActionResult> DeleteElectricBill(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId billId))
                return BadRequest(new { message = "ID hóa đơn không hợp lệ" });

            var result = await _electricityBillCollection.DeleteOneAsync(b => b.Id == billId);
            if (result.DeletedCount == 0)
                return NotFound(new { message = "Không tìm thấy hóa đơn điện để xóa" });

            return Ok(new { message = "Đã xóa hóa đơn điện thành công" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("water/{id}")]
        public async Task<IActionResult> DeleteWaterBill(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId billId))
                return BadRequest(new { message = "ID hóa đơn không hợp lệ" });

            var result = await _waterBillCollection.DeleteOneAsync(b => b.Id == billId);
            if (result.DeletedCount == 0)
                return NotFound(new { message = "Không tìm thấy hóa đơn nước để xóa" });

            return Ok(new { message = "Đã xóa hóa đơn nước thành công" });
        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("latestElectricity/{roomId}")]
        public async Task<IActionResult> GetLatestElectricityBillByRoom(string roomId)
        {
            if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
            {
                return BadRequest("ID phòng không hợp lệ");
            }

            var latestBill = await _electricityBillCollection.Find(bill => bill.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .Limit(1)
                .FirstOrDefaultAsync();

            if (latestBill == null)
            {
                return NotFound("Không tìm thấy hóa đơn điện nào cho phòng này.");
            }

            var result = new ElectricityBillDto
            {
                Id = latestBill.Id.ToString(),
                IdRoom = latestBill.IdRoom.ToString(),
                StudentCode = latestBill.StudentCode,
                StudentName = latestBill.StudentName,
                BeforeIndex = latestBill.BeforeIndex,
                AfterIndex = latestBill.AfterIndex,
                Price = latestBill.Price,
                Total = latestBill.Total,
                DateOfRecord = latestBill.DateOfRecord,
                Status = latestBill.Status
            };

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("latestWater/{roomId}")]
        public async Task<IActionResult> GetLatestWaterBillByRoom(string roomId)
        {
            if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
            {
                return BadRequest("ID phòng không hợp lệ");
            }

            var latestBill = await _waterBillCollection.Find(bill => bill.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .Limit(1)
                .FirstOrDefaultAsync();

            if (latestBill == null)
            {
                return NotFound("Không tìm thấy hóa đơn điện nào cho phòng này.");
            }

            var result = new WaterBillDto
            {
                Id = latestBill.Id.ToString(),
                IdRoom = latestBill.IdRoom.ToString(),
                StudentCode = latestBill.StudentCode,
                StudentName = latestBill.StudentName,
                BeforeIndex = latestBill.BeforeIndex,
                AfterIndex = latestBill.AfterIndex,
                Price = latestBill.Price,
                Total = latestBill.Total,
                IndexLimit = latestBill.IndexLimit,
                PriceLimit = latestBill.PriceLimit,
                DateOfRecord = latestBill.DateOfRecord,
                Status = latestBill.Status
            };

            return Ok(result);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("add/electricity")]
        public async Task<IActionResult> AddElectricityBill([FromBody] AddBillElectricityDTO billDto)
        {
            if (billDto == null)
            {
                return BadRequest("Dữ liệu hóa đơn điện không hợp lệ.");
            }

            if (!ObjectId.TryParse(billDto.IdRoom.ToString(), out ObjectId roomObjectId))
            {
                return BadRequest("ID phòng không hợp lệ.");
            }

            // Lấy hóa đơn điện gần nhất của phòng
            var latestBill = await _electricityBillCollection
                .Find(b => b.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .FirstOrDefaultAsync();

            int beforeIndex = latestBill?.AfterIndex ?? 0; // Nếu không có hóa đơn trước đó, chỉ số trước là 0

            // Lấy giá điện mới nhất
            var priceRecord = await _priceCollection
                .Find(_ => true)
                .SortByDescending(p => p.ActionDate)
                .Limit(1)
                .FirstOrDefaultAsync();

            if (priceRecord == null)
            {
                return StatusCode(500, "Không tìm thấy bảng giá điện.");
            }

            // Kiểm tra chỉ số sau phải lớn hơn chỉ số trước
            if (billDto.AfterIndex <= beforeIndex)
            {
                return BadRequest("Chỉ số sau phải lớn hơn chỉ số trước của kỳ trước đó.");
            }

            decimal total = (billDto.AfterIndex - beforeIndex) * priceRecord.ElectricityPrice;

            // Tạo hóa đơn mới
            var newElectricityBill = new ElectricityBillModels
            {
                Id = ObjectId.GenerateNewId(),
                IdRoom = roomObjectId,
                BeforeIndex = beforeIndex,
                AfterIndex = billDto.AfterIndex,
                StudentCode = null,
                StudentName = null,
                Price = priceRecord.ElectricityPrice,
                Total = total,
                DateOfRecord = billDto.DateOfRecord,
                Status = PaymentStatusEnum.unpaid
            };

            await _electricityBillCollection.InsertOneAsync(newElectricityBill);

            return Ok(new { Message = "Thêm hóa đơn điện thành công!"});
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("add/water")]
        public async Task<IActionResult> AddWaterBill([FromBody] AddBillWaterDTO billDto)
        {
            if (billDto == null)
            {
                return BadRequest("Dữ liệu hóa đơn điện không hợp lệ.");
            }

            if (!ObjectId.TryParse(billDto.IdRoom.ToString(), out ObjectId roomObjectId))
            {
                return BadRequest("ID phòng không hợp lệ.");
            }

            // Lấy hóa đơn điện gần nhất của phòng
            var latestBill = await _waterBillCollection
                .Find(b => b.IdRoom == roomObjectId)
                .SortByDescending(b => b.DateOfRecord)
                .FirstOrDefaultAsync();

            int beforeIndex = latestBill?.AfterIndex ?? 0; // Nếu không có hóa đơn trước đó, chỉ số trước là 0

            // Lấy giá điện mới nhất
            var priceRecord = await _priceCollection
                .Find(_ => true)
                .SortByDescending(p => p.ActionDate)
                .Limit(1)
                .FirstOrDefaultAsync();

            if (priceRecord == null)
            {
                return StatusCode(500, "Không tìm thấy bảng giá điện.");
            }

            // Kiểm tra chỉ số sau phải lớn hơn chỉ số trước
            if (billDto.AfterIndex <= beforeIndex)
            {
                return BadRequest("Chỉ số sau phải lớn hơn chỉ số trước của kỳ trước đó.");
            }
            int usedAmount = billDto.AfterIndex - beforeIndex; // Số nước đã dùng trong tháng

            decimal total;
            if (usedAmount > priceRecord.WaterLimit)
            {
                // Nếu vượt giới hạn, tính phần trong giới hạn và phần dư
                int excessAmount = usedAmount - priceRecord.WaterLimit;
                total = (priceRecord.WaterLimit * priceRecord.ElectricityPrice) +
                        (excessAmount * priceRecord.WaterPriceOverLimit);
            }
            else
            {
                // Nếu không vượt giới hạn, tính toàn bộ với giá thông thường
                total = usedAmount * priceRecord.ElectricityPrice;
            }
            // Tạo hóa đơn mới
            var newWaterBill = new WaterBillModels
            {
                Id = ObjectId.GenerateNewId(),
                IdRoom = roomObjectId,
                BeforeIndex = beforeIndex,
                AfterIndex = billDto.AfterIndex,
                Price = priceRecord.ElectricityPrice,
                IndexLimit = priceRecord.WaterLimit,
                PriceLimit = priceRecord.WaterPriceOverLimit,
                Total = total,
                DateOfRecord = billDto.DateOfRecord,
                Status = PaymentStatusEnum.unpaid
            };

            await _waterBillCollection.InsertOneAsync(newWaterBill);

            return Ok(new { Message = "Thêm hóa đơn điện thành công!",newWaterBill });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("electricity/pay/{billId}")]
        public async Task<IActionResult> UpdateElectricityBillPayment(string billId, [FromBody] UpdateElectricityBillDTO updateDto)
        {
            var existingBill = await _electricityBillCollection.Find(b => b.Id.ToString() == billId).FirstOrDefaultAsync();
            if (existingBill == null)
            {
                return NotFound("Không tìm thấy hóa đơn với ID này.");
            }

            if (!ObjectId.TryParse(billId, out ObjectId billObjectId))
            {
                return BadRequest("ID hóa đơn không hợp lệ.");
            }

            var updateDefinition = Builders<ElectricityBillModels>.Update
                .Set(b => b.Status, PaymentStatusEnum.paid); // Cập nhật trạng thái đã thanh toán
            if (!string.IsNullOrEmpty(updateDto.StudentCode))
            {
                updateDefinition = updateDefinition.Set(b => b.StudentCode, updateDto.StudentCode);
            }

            if (!string.IsNullOrEmpty(updateDto.StudentName))
            {
                updateDefinition = updateDefinition.Set(b => b.StudentName, updateDto.StudentName);
            }

            var updateResult = await _electricityBillCollection.UpdateOneAsync(
                b => b.Id == billObjectId,
                updateDefinition
            );

            if (updateResult.MatchedCount == 0)
            {
                return NotFound("Không tìm thấy hóa đơn cần cập nhật.");
            }

            // Sau khi update hóa đơn thành công...
            if (updateResult.MatchedCount > 0 && existingBill.IdRoom != null)
            {
                var roomId = existingBill.IdRoom;

                // Lấy danh sách sinh viên trong phòng (status active hoặc wait)
                var filter = Builders<RegisterRoomModels>.Filter.And(
                    Builders<RegisterRoomModels>.Filter.Eq(r => r.IdRoom, roomId),
                    Builders<RegisterRoomModels>.Filter.In(r => r.Status, new[] { OperatingStatusEnum.active, OperatingStatusEnum.wait })
                );

                var registerRooms = await _registerRoomCollection.Find(filter).ToListAsync();

                foreach (var register in registerRooms)
                {
                    var student = await _infoStudents.Find(s => s.Id == register.IdStudent).FirstOrDefaultAsync();
                    if (student?.Email != null)
                    {
                        var name = updateDto.StudentName ?? "một thành viên trong phòng";
                        var code = updateDto.StudentCode ?? "";
                        var subject = "Thông báo thanh toán hóa đơn điện";
                        var body = $"<p>Xin chào,</p><p>Hóa đơn điện của phòng bạn đã được thanh toán bởi <strong>{name}</strong> ({code}).</p><p>Trân trọng.</p>";

                        try
                        {
                            await _emailService.SendEmailAsync(student.Email, student.Account?.UserName ?? "Bạn", subject, body);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("❌ Lỗi gửi email: " + ex.Message);
                        }
                    }
                }
            }

            return Ok(new { Message = "Cập nhật thông tin thanh toán thành công!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("water/pay/{billId}")]
        public async Task<IActionResult> UpdateWaterBillPayment(string billId, [FromBody] UpdateElectricityBillDTO updateDto)
        {
            var existingBill = await _waterBillCollection.Find(b => b.Id.ToString() == billId).FirstOrDefaultAsync();
            if (existingBill == null)
            {
                return NotFound("Không tìm thấy hóa đơn với ID này.");
            }

            if (!ObjectId.TryParse(billId, out ObjectId billObjectId))
            {
                return BadRequest("ID hóa đơn không hợp lệ.");
            }

            var updateDefinition = Builders<WaterBillModels>.Update
                .Set(b => b.Status, PaymentStatusEnum.paid); // Cập nhật trạng thái đã thanh toán
            if (!string.IsNullOrEmpty(updateDto.StudentCode))
            {
                updateDefinition = updateDefinition.Set(b => b.StudentCode, updateDto.StudentCode);
            }

            if (!string.IsNullOrEmpty(updateDto.StudentName))
            {
                updateDefinition = updateDefinition.Set(b => b.StudentName, updateDto.StudentName);
            }

            var updateResult = await _waterBillCollection.UpdateOneAsync(
                b => b.Id == billObjectId,
                updateDefinition
            );

            if (updateResult.MatchedCount == 0)
            {
                return NotFound("Không tìm thấy hóa đơn cần cập nhật.");
            }
            // Sau khi update hóa đơn thành công...
            if (updateResult.MatchedCount > 0 && existingBill.IdRoom != null)
            {
                var roomId = existingBill.IdRoom;

                // Lấy danh sách sinh viên trong phòng (status active hoặc wait)
                var filter = Builders<RegisterRoomModels>.Filter.And(
                    Builders<RegisterRoomModels>.Filter.Eq(r => r.IdRoom, roomId),
                    Builders<RegisterRoomModels>.Filter.In(r => r.Status, new[] { OperatingStatusEnum.active, OperatingStatusEnum.wait })
                );

                var registerRooms = await _registerRoomCollection.Find(filter).ToListAsync();

                foreach (var register in registerRooms)
                {
                    var student = await _infoStudents.Find(s => s.Id == register.IdStudent).FirstOrDefaultAsync();
                    if (student?.Email != null)
                    {
                        var name = updateDto.StudentName ?? "một thành viên trong phòng";
                        var code = updateDto.StudentCode ?? "";
                        var subject = "Thông báo thanh toán hóa đơn nước";
                        var body = $"<p>Xin chào,</p><p>Hóa đơn nước của phòng bạn đã được thanh toán bởi <strong>{name}</strong> ({code}).</p><p>Trân trọng.</p>";

                        try
                        {
                            await _emailService.SendEmailAsync(student.Email, student.Account?.UserName ?? "Bạn", subject, body);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("❌ Lỗi gửi email: " + ex.Message);
                        }
                    }
                }
            }
            return Ok(new { Message = "Cập nhật thông tin thanh toán thành công!" });
        }

        [Authorize(Roles = "Admin,Student,Staff")]
        [HttpGet("room/{roomId}/has-unpaid-bill")]
        public async Task<IActionResult> CheckHasUnpaidBill(string roomId)
        {
            if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
            {
                return BadRequest(new { message = "ID phòng không hợp lệ" });
            }

            // Kiểm tra hóa đơn điện chưa thanh toán
            var hasUnpaidElectricity = await _electricityBillCollection
                .Find(bill => bill.IdRoom == roomObjectId && bill.Status != PaymentStatusEnum.paid)
                .AnyAsync();

            // Kiểm tra hóa đơn nước chưa thanh toán
            var hasUnpaidWater = await _waterBillCollection
                .Find(bill => bill.IdRoom == roomObjectId && bill.Status != PaymentStatusEnum.paid)
                .AnyAsync();

            // Nếu có ít nhất một loại hóa đơn chưa thanh toán
            bool hasUnpaidBill = hasUnpaidElectricity || hasUnpaidWater;

            return Ok(new { hasUnpaidBill });
        }



    }

}