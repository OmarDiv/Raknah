# Raknah (.NET 9 Web API)

Raknah is a comprehensive backend API for smart parking management and reservation systems, built with ASP.NET Core 9.

## Project Vision

**Raknah is designed to revolutionize the parking business by enabling smart, fully digital parking management. All user interactions—including booking a spot, payment processing, and managing parking sessions—can be performed seamlessly through a mobile app. The platform also integrates directly with hardware devices in smart garages, allowing for real-time control, automation, and monitoring of parking operations.**

---

## What Raknah Covers

Raknah includes full implementations for all advanced API topics and modules, similar to enterprise-grade systems:
- API Fundamentals & REST Principles
- CRUD Operations for key entities (parking lots, reservations, users, etc.)
- Model Binding, Mapping, and Validation
- Database Integration (Entity Framework Core)
- JWT Authentication & Role-based Authorization
- Application Options & Configuration
- Refresh Tokens
- Audit Logging & Structured Logging (Serilog)
- CORS Support
- Comprehensive Error & Exception Handling
- Problem Details Standardization
- Logging & Caching
- Registration & User Management
- Background Jobs (Hangfire)
- Account, Roles & Permissions Management
- Pagination, Filtering, and Sorting
- Health Checks
- Rate Limiting
- API Versioning
- Swagger & OpenAPI Documentation
- Code Review, Deployment, and Project Management

All modules are implemented using best practices for maintainability and extensibility.

---

## Tech Stack

- ASP.NET Core 9
- Entity Framework Core + SQL Server
- JWT Authentication
- Serilog, Hangfire, FluentValidation, Mapster
- Swagger/OpenAPI, CORS, Modular DI
- Hardware Integration (for smart garage control)

---

## Getting Started

1. **Clone the repository:**
    ```bash
    git clone https://github.com/OmarDiv/Raknah.git
    cd Raknah
    ```
2. **Configure Database:** Edit `appsettings.json` for your connection string.
3. **Restore dependencies:**  
    `dotnet restore`
4. **Run migrations:**  
    `dotnet ef database update`
5. **Run the API:**  
    `dotnet run`
6. **Access Swagger UI:**  
    [https://localhost:5001/swagger](https://localhost:5001/swagger)

---

## Project Structure (Example)

```
Raknah/
├── Controllers/
├── Models/
├── Persistence/
├── Services/
├── DTO/
├── Helpers/
├── Middlewares/
├── Program.cs
├── appsettings.json
├── Raknah.csproj
...
```

---

## Contributing

- Fork, branch, and submit descriptive PRs.
- Follow .NET standards and write tests.

---

## License

MIT

---

## Contact

- **WhatsApp:** 01013762770
- **Email:** omaar88mohamed@gmail.com
- **LinkedIn:** [Omar Mohamed](https://www.linkedin.com/in/omar-mohamed-713b53265)
