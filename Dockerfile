# Dùng image .NET Core chính thức
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

# Copy tất cả file vào container
COPY . .

# Khôi phục dependencies
RUN dotnet restore

# Build project
RUN dotnet build -c Release -o out

# Chạy ứng dụng
CMD ["dotnet", "YourApp.dll"]