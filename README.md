# Inventory Management System

A robust inventory management system built with ASP.NET Core 8.0, designed to help businesses track their inventory, manage stock movements, and receive alerts for low stock items.

## Features

- **User Authentication & Authorization**
  - Secure login and registration system
  - Role-based access control
  - Access denied handling

- **Inventory Management**
  - Create, read, update, and delete inventory items
  - Track stock levels
  - View item details
  - Monitor low stock items

- **Stock Movement Tracking**
  - Record stock ins and outs
  - Track movement history
  - Associate movements with users

- **Alert System**
  - Automatic low stock alerts
  - Alert notifications
  - Alert management interface

- **Dashboard**
  - Overview of inventory status
  - Real-time stock level monitoring
  - Alert counter integration

## Technology Stack

- ASP.NET Core 8.0
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity
- Bootstrap (for UI)

## Prerequisites

- .NET 8.0 SDK
- SQL Server (Local or Remote)
- Visual Studio 2022 or VS Code

## Setup Instructions

1. Clone the repository
```bash
git clone [repository-url]
```

2. Navigate to the project directory
```bash
cd Inventory_Management
```

3. Restore NuGet packages
```bash
dotnet restore
```

4. Update the connection string in `appsettings.json`

5. Apply database migrations
```bash
dotnet ef database update
```

6. Run the application
```bash
dotnet run
```

7. Default Admin Credentials
```
Email: admin@inventory.com
Password: Admin123!
```

Note: It's highly recommended to change these credentials after your first login.

## Project Structure

- `Controllers/`: Contains all MVC controllers
  - `AccountController.cs`: Handles user authentication
  - `AlertController.cs`: Manages alert system
  - `InventoryController.cs`: Manages inventory items
  - `StockMovementController.cs`: Handles stock movements

- `Models/`: Contains data models
  - `Alert.cs`: Alert system model
  - `Item.cs`: Inventory item model
  - `StockMovement.cs`: Stock movement model
  - View models for various features

- `Views/`: Contains all Razor views organized by controller
  - Separate folders for Account, Alert, Inventory, and StockMovement views

- `Migrations/`: Contains database migrations

## Contributing

Please read the contributing guidelines before submitting pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.