FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SealHackathon.API/SealHackathon.API.csproj", "SealHackathon.API/"]
COPY ["SealHackathon.Application/SealHackathon.Application.csproj", "SealHackathon.Application/"]
COPY ["SealHackathon.Domain/SealHackathon.Domain.csproj", "SealHackathon.Domain/"]
COPY ["SealHackathon.Infrastructure/SealHackathon.Infrastructure.csproj", "SealHackathon.Infrastructure/"]
RUN dotnet restore "SealHackathon.API/SealHackathon.API.csproj"
COPY . .
WORKDIR "/src/SealHackathon.API"
RUN dotnet build "SealHackathon.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SealHackathon.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SealHackathon.API.dll"]
