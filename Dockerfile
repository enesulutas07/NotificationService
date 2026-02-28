# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Notification.Mail.Consumer/Notification.Mail.Consumer.csproj", "Notification.Mail.Consumer/"]
COPY ["Notification.Mail.Service/Notification.Mail.Service.csproj", "Notification.Mail.Service/"]
COPY ["Notification.Data/Notification.Data.csproj", "Notification.Data/"]
COPY ["Notification.Entity/Notification.Entity.csproj", "Notification.Entity/"]
COPY ["ECommerce.Shared/ECommerce.Shared.csproj", "ECommerce.Shared/"]

RUN dotnet restore "Notification.Mail.Consumer/Notification.Mail.Consumer.csproj"

COPY . .

WORKDIR "/src/Notification.Mail.Consumer"
RUN dotnet build "Notification.Mail.Consumer.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Notification.Mail.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Notification.Mail.Consumer.dll"]
