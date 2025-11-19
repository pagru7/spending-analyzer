FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore project dependencies first to leverage Docker layer caching.
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining source and publish the app.
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

# Bind Kestrel to port 8080 inside the container.
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SpendingAnalyzer.dll"]