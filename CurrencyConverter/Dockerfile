# =========================
# STAGE 1: Build the project
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0.8 AS build
WORKDIR /src

# Copy only the .csproj file first for layer caching
COPY ["CurrencyConverter/CurrencyConverter.csproj", "CurrencyConverter/"]
RUN dotnet restore "CurrencyConverter/CurrencyConverter.csproj"

# Copy the entire source code
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0.8 AS final
WORKDIR /app

# Copy published output from the previous stage
COPY --from=publish /app/publish .

# Install optional dependencies for System.Drawing (if used)
RUN apt-get update && apt-get install -y libgdiplus && apt-get clean && rm -rf /var/lib/apt/lists/*

# Expose port for Railway
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Start the application
ENTRYPOINT ["dotnet", "CurrencyConverter.dll"]
