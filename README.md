# GLMS - Global Lecturer Management System

## 📋 Application Description

The **Global Lecturer Management System (GLMS)** is a comprehensive ASP.NET Core MVC web application designed to manage lecturer contracts, client relationships, and service requests for educational institutions operating globally. The system provides robust contract lifecycle management, currency conversion for international payments (USD to ZAR), secure document handling, and comprehensive workflow validation.

### Key Features

- **Client Management**: Maintain detailed records of educational clients across different regions
- **Contract Lifecycle Management**: Create, track, and manage contracts with multiple status states (Draft, Active, Expired, OnHold)
- **Service Request Processing**: Handle service requests tied to active contracts with automatic currency conversion
- **Real-time Currency Conversion**: Integrated with OpenExchangeRates API for live USD to ZAR conversion
- **Secure File Handling**: UUID-based file naming for contract agreements with strict PDF validation
- **Advanced Filtering & Search**: Filter contracts by client, status, and date ranges (yy/MM/dd format)
- **Business Rule Enforcement**: Automated validation preventing service requests on expired or on-hold contracts
- **Design Pattern Implementation**: 
  - Factory Method for contract creation
  - Observer pattern for status change notifications
  - Strategy pattern for currency conversion
- **Comprehensive Unit Testing**: xUnit test suite with Moq for dependency mocking
- **CI/CD Pipeline**: GitHub Actions workflow for automated build and test execution

---

## 🖥️ System Requirements

### Development Environment
- **Operating System**: Windows 10/11, macOS, or Linux
- **IDE**: Visual Studio 2022 (v17.8+) or Visual Studio Code with C# extension
- **.NET SDK**: .NET 9.0 or higher
- **Database**: SQL Server 2019+ or SQL Server LocalDB
- **Node.js** (optional, for front-end tooling): 18.x or higher

### Runtime Requirements
- .NET 9.0 Runtime
- SQL Server connection (LocalDB for development)
- Internet connection (for OpenExchangeRates API)

---

## 🚀 Setup and Run Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/msa1105/PROG7311-POE.git
cd PROG7311-POE
```

### 2. Configure Database Connection

The application uses SQL Server LocalDB by default. The connection string is located in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GLMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**For custom SQL Server instances**, update the connection string accordingly.

### 3. Configure OpenExchangeRates API Key

Update `appsettings.json` with your API key:

```json
{
  "ExternalServices": {
    "OpenExchangeRates": {
      "BaseUrl": "https://openexchangerates.org/api/",
      "AppId": "YOUR_API_KEY_HERE"
    }
  }
}
```

Get a free API key at: [https://openexchangerates.org/](https://openexchangerates.org/)

### 4. Restore NuGet Packages

```bash
dotnet restore
```

### 5. Apply Database Migrations

Run the following command from the project root directory:

```bash
dotnet ef database update
```

**Alternative (using Package Manager Console in Visual Studio):**

```powershell
Update-Database
```

This will create the database schema and apply all migrations.

### 6. Seed Initial Data

The application automatically seeds the database with **10+ mock records** (clients, contracts, and service requests) on first run. Simply start the application and the seeder will execute automatically.

### 7. Run the Application

#### Using Visual Studio:
1. Open `PROG7311-POE.sln`
2. Press `F5` or click the **Start** button

#### Using CLI:
```bash
dotnet run --project PROG7311-POE.csproj
```

The application will launch at `https://localhost:5001` (or the port configured in `launchSettings.json`).

### 8. Run Unit Tests

Execute the xUnit test suite:

```bash
dotnet test GLMS.Tests/GLMS.Tests.csproj
```

**[INSERT SCREENSHOT HERE: Unit Test Results showing all tests passing]**

---

## 📸 Application Screenshots

### Home Dashboard
**[INSERT SCREENSHOT HERE: Main landing page]**

### Client Management
**[INSERT SCREENSHOT HERE: Client list view with search/filter options]**

### Contract Management
**[INSERT SCREENSHOT HERE: Contract list with status indicators and filters]**

### Contract Creation Form
**[INSERT SCREENSHOT HERE: Contract form with file upload and date pickers]**

### Service Request Management
**[INSERT SCREENSHOT HERE: Service request list with USD/ZAR conversion]**

### Service Request Creation
**[INSERT SCREENSHOT HERE: Service request form showing currency conversion]**

---

## 🗂️ Project Structure

```
PROG7311-POE/
├── Controllers/           # MVC Controllers (Clients, Contracts, ServiceRequests)
├── Data/                  # EF Core DbContext and Database Seeder
├── Models/                # Domain models and ViewModels
│   ├── Client.cs
│   ├── Contract.cs
│   ├── ServiceRequest.cs
│   └── ViewModels/
├── Services/              # Business logic services (Currency, FileValidation, Workflow)
├── Views/                 # Razor views
├── wwwroot/              # Static files and uploaded documents
│   └── uploads/contracts/ # UUID-named PDF contract files
├── GLMS.Tests/           # xUnit test project
├── appsettings.json      # Configuration file
├── Program.cs            # Application entry point
└── Database_Schema.sql   # Generated migration script
```

---

## 🧪 Testing Strategy

The application includes comprehensive unit tests covering:

1. **Currency Calculation Tests** - Validates USD to ZAR conversion logic with various amounts including edge case (zero)
2. **File Validation Tests** - Ensures only PDF files are accepted and dangerous file types (.exe) are blocked
3. **Contract Workflow Tests** - Verifies business rules preventing service requests on expired/on-hold contracts

**Test Framework**: xUnit  
**Mocking Framework**: Moq  
**Code Coverage**: Target 80%+

**[INSERT SCREENSHOT HERE: Test Explorer showing all 10+ tests passing]**

---

## 🔐 Security Features

- **File Upload Security**: UUID-based naming prevents file overwriting; strict extension and MIME type validation
- **SQL Injection Protection**: Entity Framework Core parameterized queries
- **CSRF Protection**: Anti-forgery tokens on all POST requests
- **Input Validation**: Server-side and client-side validation using Data Annotations

---

## 🛠️ Design Patterns Implemented

### 1. Factory Method Pattern
Used for contract creation with different status types (Draft, Active, etc.)

### 2. Observer Pattern
Implements status change notifications for contract lifecycle events

### 3. Strategy Pattern
Encapsulates currency conversion logic, allowing for multiple API providers

---

## [DATABASE] Database Schema

The application uses Entity Framework Core Code-First approach with the following entities:

- **Clients**: Educational institutions with regional information
- **Contracts**: Agreements between clients and the system with lifecycle management
- **ServiceRequests**: Individual service requests tied to contracts with cost tracking

**Foreign Key Relationships**:
- `Contracts.ClientId` → `Clients.Id` (Restrict delete)
- `ServiceRequests.ContractId` → `Contracts.Id` (Cascade delete)

See `Database_Schema.sql` for the complete SQL migration script.

---

## 🌐 API Integration

### OpenExchangeRates API
- **Endpoint**: `https://openexchangerates.org/api/latest.json`
- **Purpose**: Real-time USD to ZAR currency conversion
- **Error Handling**: Graceful fallback with user-friendly error messages
- **Retry Logic**: SQL Server transient error handling with exponential backoff (max 5 retries)

---

## 📝 Code Attribution

Complex logic and pattern implementations were assisted by:

```
OpenAI. 2025. ChatGPT (Version GPT-4). [Large language model]
Available at: https://chat.openai.com/
[Accessed: 15 January 2025]
```

---

## 🐛 Known Issues & Limitations

- OpenExchangeRates free tier has rate limits (1000 requests/month)
- File uploads limited to 10MB (configurable in `appsettings.json`)
- LocalDB requires Windows operating system (use full SQL Server on Linux/Mac)

---

## 🚧 Future Enhancements

- [ ] Implement role-based authentication (Admin, Lecturer, Viewer)
- [ ] Add email notifications for contract expiration
- [ ] Export contracts and service requests to Excel/PDF reports
- [ ] Implement contract renewal workflow
- [ ] Add dashboard with analytics and charts
- [ ] Support for multiple currencies beyond USD/ZAR

---

## 📞 Support & Contact

- **GitHub Issues**: [https://github.com/msa1105/PROG7311-POE/issues](https://github.com/msa1105/PROG7311-POE/issues)
- **Repository**: [https://github.com/msa1105/PROG7311-POE](https://github.com/msa1105/PROG7311-POE)

---

## 📄 License

This project is submitted as part of PROG7311 coursework. All rights reserved.

---

## 🎓 Academic Integrity Statement

This application was developed as part of the PROG7311 module assessment. All code is original or properly attributed. External libraries and frameworks are used in accordance with their respective licenses.

**Lecturer**: [Lecturer Name]  
**Institution**: [Institution Name]  
**Submission Date**: [Date]
