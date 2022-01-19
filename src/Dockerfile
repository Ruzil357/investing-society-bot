FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PsnHackathonBot/PsnHackathonBot.csproj", "PsnHackathonBot/"]
RUN dotnet restore "PsnHackathonBot/PsnHackathonBot.csproj"
COPY . .
WORKDIR "/src/PsnHackathonBot"
RUN dotnet build "PsnHackathonBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PsnHackathonBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PsnHackathonBot.dll"]
