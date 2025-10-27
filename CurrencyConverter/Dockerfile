# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:8.0.303 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["CurrencyConverter/CurrencyConverter.csproj", "CurrencyConverter/"]
RUN dotnet restore "CurrencyConverter/CurrencyConverter.csproj"

# Copy everything and publish
COPY . .
WORKDIR "/src/CurrencyConverter"
RUN dotnet publish "CurrencyConverter.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0.3 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Tell ASP.NET Core to listen on the Railway port
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080

ENTRYPOINT ["dotnet", "CurrencyConverter.dll"]
