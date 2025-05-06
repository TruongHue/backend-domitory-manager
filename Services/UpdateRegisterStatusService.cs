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
                await UpdateRegisterStatus();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Chạy mỗi 24 giờ
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
