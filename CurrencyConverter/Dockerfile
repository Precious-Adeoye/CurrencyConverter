# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:8.0.100 AS build
WORKDIR /src

# Copy project files and restore
COPY ["CurrencyConverter/CurrencyConverter.csproj", "CurrencyConverter/"]
RUN dotnet restore "CurrencyConverter/CurrencyConverter.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/CurrencyConverter"

# Publish the app
RUN dotnet publish "CurrencyConverter.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0.100 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway assigns a PORT environment variable
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080

ENTRYPOINT ["dotnet", "CurrencyConverter.dll"]
