FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "Src/API/API.csproj"
RUN dotnet publish "Src/API/API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]