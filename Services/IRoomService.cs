using API_dormitory.Models.DTO.RegisterRoom;

namespace API_dormitory.Services
{
    public interface IRoomService
    {
        Task<bool> RegisterUserAsync(RegisterRoomDTO model); // Đăng ký người dùng
        Task<int> GetUpdatedRoomCountAsync(); // Lấy số lượng phòng đã cập nhật
    }

}
