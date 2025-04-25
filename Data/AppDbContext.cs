using Microsoft.EntityFrameworkCore;
using API_dormitory.Models.Users;
using API_dormitory.Models.Rooms;
using API_dormitory.Models.Bills;
using API_dormitory.Models.registerRoom;
using API_dormitory.Models.Registrations;

namespace API_dormitory.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AccountModels> Accounts { get; set; }
        public DbSet<InfoStudentModels> InfoStudents { get; set; }
        public DbSet<BuildingModels> Buildings { get; set; }
        public DbSet<InfoRoomModels> InfoRoom { get; set; }
        public DbSet<ElectricityBillModels> ElectricityBill { get; set; }
        public DbSet<WaterBillModels> WaterBill { get; set; }
        public DbSet<RoomBillModels> RoomBill { get; set; }
        public DbSet<RegisterRoomModels> RegisterRoom { get; set; }
        public DbSet<RegistrationPeriodModels>  RegistrationPeriods {  get; set; }
        public DbSet<PriceWaterAndElectricity> PriceWaterAndElectricities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ 1-1 giữa InfoUser và Account
        /*    modelBuilder.Entity<InfoStudentModels>()
                .HasOne(u => u.Account)
                .WithOne(a => a.InfoStudent)
                .HasForeignKey<InfoStudentModels>(u => u.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
*/
            // Đảm bảo Email là Unique
            modelBuilder.Entity<InfoStudentModels>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Quan hệ 1-N: Một tòa nhà có nhiều phòng
            modelBuilder.Entity<InfoRoomModels>()
                .HasOne(r => r.Building)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.IdBuilding)
                .OnDelete(DeleteBehavior.Cascade);

           
         /*   // Quan hệ 1-N: Một user có nhiều feedback
            modelBuilder.Entity<FeedbackModels>()
                .HasOne(f => f.InfoStudent)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.IdStudent)
                .OnDelete(DeleteBehavior.NoAction);

            // Quan hệ 1-N: Một user có thể đăng ký nhiều phòng
            modelBuilder.Entity<RegisterRoomModels>()
                .HasOne(rr => rr.InfoStudent)
                .WithMany(u => u.RegisterRooms)
                .HasForeignKey(rr => rr.idStudent)
                .OnDelete(DeleteBehavior.Cascade);*/

           

            base.OnModelCreating(modelBuilder);
        }
    }
}
