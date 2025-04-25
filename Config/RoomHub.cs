using Microsoft.AspNetCore.SignalR;

namespace API_dormitory.Config
{
    public class RoomHub : Hub
    {
        // Hàm này sẽ gửi thông báo đến tất cả người dùng về số lượng phòng đã thay đổi
        public async Task UpdateRoomCount(int newCount)
        {
            await Clients.All.SendAsync("ReceiveRoomCount", newCount);
        }
    }

}
