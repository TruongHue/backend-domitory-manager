# Sử dụng SDK .NET để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy toàn bộ source code vào container và chạy restore
COPY . . 
RUN dotnet publish -c Release -o /app/publish
RUN dotnet list package

# Sử dụng runtime .NET để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API_dormitory.dll"]  # Đổi tên file DLL đúng với tên project của bạn
