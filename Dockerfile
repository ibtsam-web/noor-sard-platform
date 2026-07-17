FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NoorSardPlatform.csproj", "./"]
RUN dotnet restore "NoorSardPlatform.csproj"

COPY . .
RUN dotnet publish "NoorSardPlatform.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "NoorSardPlatform.dll"]