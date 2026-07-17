# OneSWebBooking

## Requirements

- .NET 10 SDK
- SQL Server
- Visual Studio 2026 (Recommended)

## Setup

Clone project

```bash
git clone https://github.com/TranKhai1122004/OneSWebBooking.git
cd OneSWebBooking
```

Restore packages

```bash
dotnet restore
```

## Database

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=<your_server>;Database=<your_database>;User Id=<your_user>;Password=<your_password>;TrustServerCertificate=True;"
}
```

If the project contains EF Core migrations, run:

```bash
dotnet ef database update
```

## Run

Using Visual Studio:

- Open `OneSWebBooking.slnx`
- Press **F5** or **Ctrl + F5**

Or using CLI:

```bash
dotnet run
```

## Default URLs

- https://localhost:7256
- http://localhost:5100