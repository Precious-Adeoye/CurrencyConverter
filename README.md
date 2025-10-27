# CurrencyConverter

# Country Currency & Exchange API

A robust RESTful API built with C# ASP.NET 8.0 that fetches country data and currency exchange rates, computes estimated GDP, and provides comprehensive CRUD operations.

## 🚀 Features

- ✅ Fetch country data from REST Countries API v3
- ✅ Get exchange rates from Open Exchange Rates API  
- ✅ Compute estimated GDP with random multipliers
- ✅ Full CRUD operations with filtering and sorting
- ✅ Automatic retry mechanism for external APIs
- ✅ Summary image generation with top GDP countries
- ✅ Comprehensive error handling and logging
- ✅ Health checks and API monitoring
- ✅ Swagger documentation
- ✅ Database migrations with Entity Framework Core

## 🛠️ Technologies

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- SQL Server
- HttpClientFactory with retry policies
- System.Drawing for image generation
- Swagger/OpenAPI documentation
- Newtonsoft.Json for serialization

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full version)
- Git

## ⚡ Quick Start

1. **Clone and setup**
   ```bash
   git clone <your-repo-url>
   cd CountryCurrencyApi
