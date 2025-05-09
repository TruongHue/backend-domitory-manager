using API_dormitory.Data;
using API_dormitory.Models.registerRoom;
using API_dormitory.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System.Text;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CloudinarySettings>(options =>
{
    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
});


builder.Services.AddScoped<CloudinaryService>();   // Đăng ký CloudinaryService
builder.Services.AddSingleton<MongoDbContext>(); // Đăng ký MongoDbContext
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    // Lấy chuỗi kết nối từ cấu hình
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["MongoDB:ConnectionString"];

    return new MongoClient(connectionString); // Truyền vào cho những class nào cần IMongoClient trực tiếp
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên thuộc tính
    });

builder.Services.AddScoped<RegisterRoomService>();  // Đăng ký RegisterRoomService

// Đăng ký IMongoCollection<RegisterRoomModels> dưới dạng Singleton
builder.Services.AddSingleton<IMongoCollection<RegisterRoomModels>>(sp =>
{
    var context = sp.GetRequiredService<MongoDbContext>(); // Đảm bảo MongoDbContext đã được đăng ký
    return context.GetCollection<RegisterRoomModels>("RegisterRooms"); // Đảm bảo "RegisterRooms" là tên đúng của collection trong MongoDB
});



// Kết nối cơ sở dữ liệu MySQL.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(5, 7, 31)) 
    ));

// Cấu hình JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "ThisIsAReallyStrongSecretKey!1234567890");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Không có thời gian trễ khi kiểm tra token
        };
    });

// Thêm Swagger và hỗ trợ JWT trong Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API Dormitory", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter Bearer token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Ignore circular reference issues
    c.CustomSchemaIds(type => type.ToString());
});

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()  // Cho phép mọi nguồn
                  .AllowAnyMethod()  // Cho phép tất cả các phương thức (GET, POST, PUT, DELETE,...)
                  .AllowAnyHeader(); // Cho phép tất cả các header
        });
});

builder.Services.AddSingleton<EmailService>();
builder.Services.AddHostedService<UpdateRegisterStatusService>();

// ✅ **Tạo `app` sau khi đã cấu hình tất cả các dịch vụ**
var app = builder.Build();

app.UseCors("AllowAll");

// Nếu là môi trường phát triển, bật Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseStaticFiles(); // Cho phép truy cập file trong wwwroot
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
var mongoDbContext = app.Services.GetRequiredService<MongoDbContext>();

try
{
    var collection = mongoDbContext.GetCollection<BsonDocument>("test_collection");
    var count = collection.CountDocuments(new BsonDocument());
    Console.WriteLine($"✅ Kết nối MongoDB thành công! Số tài liệu trong test_collection: {count}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Lỗi kết nối MongoDB: {ex.Message}");
}
app.Run();

