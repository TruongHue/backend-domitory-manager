# Sử dụng SDK .NET để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy file csproj vào container và chạy restore
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ source code vào container
COPY . . 
RUN dotnet publish -c Release -o /app/publish

# Sử dụng runtime .NET để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "backend-domitory-manager.dll"]
