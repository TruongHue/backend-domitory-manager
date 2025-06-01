using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using API_dormitory.Models.registerRoom;
using API_dormitory.Models.common;

namespace API_dormitory.Services
{
    public class UpdateRegisterStatusService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdateRegisterStatusService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateRegisterStatus();

                    // Giờ hiện tại theo giờ Việt Nam (UTC+7)
                    var nowUtc = DateTime.UtcNow;
                    var nowVietnam = nowUtc.AddHours(7);

                    // Tính thời điểm 00:00 ngày hôm sau (giờ Việt Nam)
                    var nextMidnightVietnam = nowVietnam.Date.AddDays(1);

                    // Tính khoảng thời gian cần delay
                    var delayVietnam = nextMidnightVietnam - nowVietnam;

                    Console.WriteLine($"🕛 Đợi đến {nextMidnightVietnam} giờ Việt Nam để chạy lại (sau {delayVietnam.TotalMinutes:F0} phút)");

                    await Task.Delay(delayVietnam, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Bỏ qua nếu task bị huỷ khi shutdown app
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi trong UpdateRegisterStatusService: {ex.Message}");
                    // Có thể log thêm nếu muốn
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // chờ 1 phút rồi thử lại
                }
            }
        }


        private async Task UpdateRegisterStatus()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var registerRoomCollection = scope.ServiceProvider.GetRequiredService<IMongoCollection<RegisterRoomModels>>();

                await DeactivateExpiredRegistrations(registerRoomCollection);
                await DeactivateUnpaidLateRegistrations(registerRoomCollection);
            }
        }

        private async Task DeactivateExpiredRegistrations(IMongoCollection<RegisterRoomModels> collection)
        {
            var today = DateTime.UtcNow;
            var filter = Builders<RegisterRoomModels>.Filter.And(
                Builders<RegisterRoomModels>.Filter.Eq(r => r.Status, OperatingStatusEnum.active),
                Builders<RegisterRoomModels>.Filter.Lt(r => r.EndDate, today)
            );

            var update = Builders<RegisterRoomModels>.Update.Set(r => r.Status, OperatingStatusEnum.inactive);
            var result = await collection.UpdateManyAsync(filter, update);

            Console.WriteLine($"✅ Cập nhật {result.ModifiedCount} đơn đăng ký hết hạn thành inactive.");
        }

        private async Task DeactivateUnpaidLateRegistrations(IMongoCollection<RegisterRoomModels> collection)
        {
            var today = DateTime.UtcNow;
            var filter = Builders<RegisterRoomModels>.Filter.And(
                Builders<RegisterRoomModels>.Filter.Eq(r => r.Status, OperatingStatusEnum.wait),
                Builders<RegisterRoomModels>.Filter.Eq(r => r.PaymentStatus, PaymentStatusEnum.unpaid),
                Builders<RegisterRoomModels>.Filter.Lt(r => r.StartDate, today)
            );

            var update = Builders<RegisterRoomModels>.Update.Set(r => r.Status, OperatingStatusEnum.inactive);
            var result = await collection.UpdateManyAsync(filter, update);

            Console.WriteLine($"⚠️ Cập nhật {result.ModifiedCount} đơn đăng ký chưa thanh toán quá hạn thành inactive.");
        }

    }
}
