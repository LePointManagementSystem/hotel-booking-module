FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelBookingPlatform.sln", "."]
COPY ["HotelBookingPlatform.API/HotelBookingPlatform.API.csproj", "HotelBookingPlatform.API/"]
COPY ["HotelBookingPlatform.Application/HotelBookingPlatform.Application.csproj", "HotelBookingPlatform.Application/"]
COPY ["HotelBookingPlatform.Infrastructure/HotelBookingPlatform.Infrastructure.csproj", "HotelBookingPlatform.Infrastructure/"]
COPY ["HotelBookingPlatform.Domain/HotelBookingPlatform.Domain.csproj", "HotelBookingPlatform.Domain/"]

RUN dotnet restore "HotelBookingPlatform.API/HotelBookingPlatform.API.csproj"

COPY . .
WORKDIR "/src/HotelBookingPlatform.API"
RUN dotnet publish "HotelBookingPlatform.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends openssl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS="http://+:8080;https://+:8443"
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=8443

COPY --from=build /app/publish .
COPY scripts/generate-self-signed-cert.sh /usr/local/bin/generate-self-signed-cert.sh
COPY scripts/docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh

RUN chmod +x /usr/local/bin/generate-self-signed-cert.sh /usr/local/bin/docker-entrypoint.sh

EXPOSE 8080
EXPOSE 8443

ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
