# =========================
# STAGE 1: Build the project
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the .csproj first for caching
COPY ["CurrencyConverter/CurrencyConverter.csproj", "CurrencyConverter/"]
RUN dotnet restore "CurrencyConverter/CurrencyConverter.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/CurrencyConverter"

# Build the project in Release mode
RUN dotnet build "CurrencyConverter.csproj" -c Release -o /app/build


# =========================
# STAGE 2: Publish the project
# =========================
FROM build AS publish
RUN dotnet publish "CurrencyConverter.csproj" -c Release -o /app/publish /p:UseAppHost=false


# =========================
# STAGE 3: Runtime image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=publish /app/publish .

# Expose port for Railway (default: 8080)
EXPOSE 8080

# Optional: install dependencies for System.Drawing (used by some libraries)
RUN apt-get update && apt-get install -y libgdiplus && apt-get clean

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Start the application
ENTRYPOINT ["dotnet", "CurrencyConverter.dll"]
