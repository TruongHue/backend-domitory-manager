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

                var today = DateTime.UtcNow;
                var filter = Builders<RegisterRoomModels>.Filter.And(
                    Builders<RegisterRoomModels>.Filter.Eq(r => r.Status, OperatingStatusEnum.active),
                    Builders<RegisterRoomModels>.Filter.Lt(r => r.EndDate, today)
                );

                var update = Builders<RegisterRoomModels>.Update.Set(r => r.Status, OperatingStatusEnum.inactive);

                await registerRoomCollection.UpdateManyAsync(filter, update);
                Console.WriteLine("Đã cập nhật trạng thái đăng ký phòng hết hạn.");
            }
        }
    }
}
