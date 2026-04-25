# Database Migration Instructions

## Prerequisites

1. **SQL Server** must be running and accessible
2. **.NET 10 CLI** installed on your system
3. **Entity Framework Core Tools** (should be included with the project)

## Configuration

### Update Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AdegaroyalDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**For Docker SQL Server**:
```json
"DefaultConnection": "Server=localhost,1433;Database=AdegaroyalDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;"
```

**For Azure SQL Database**:
```json
"DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=AdegaroyalDb;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## Step-by-Step Migration Process

### Step 1: Navigate to Project Directory

```bash
cd d:\Dev\AdegaRoyal\api
```

### Step 2: Create Initial Migration

This will generate all tables for the new Adega Royal entities (Category, Product, Order, OrderItem):

```bash
dotnet ef migrations add InitialAdegaroyalMigration
```

**What this does**:
- Analyzes your DbContext and existing entities
- Generates SQL migration files in `Migrations/` folder
- Creates both `{timestamp}_InitialAdegaroyalMigration.cs` and `.Designer.cs` files

### Step 3: Review Migration (Optional)

Open the generated migration file to verify it matches your expectations:

```
Migrations/20260423120000_InitialAdegaroyalMigration.cs
```

Expected tables to be created:
- `Categories`
- `Products`
- `Orders`
- `OrderItems`

### Step 4: Apply Migration to Database

```bash
dotnet ef database update
```

**This will**:
- Connect to your SQL Server instance
- Create the database if it doesn't exist
- Execute all pending migrations
- Create all tables with proper indexes and constraints

### Step 5: Verify Migration Success

#### Using SQL Server Management Studio:

```sql
-- Check if tables were created
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';

-- Verify Categories table structure
EXEC sp_help 'dbo.Categories';

-- View all indexes
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Products');
```

#### Using .NET CLI:

```bash
dotnet ef migrations list
```

You should see:
```
20250420015036_InitialMigration
20260423120000_InitialAdegaroyalMigration
```

## Database Schema Overview

### Categories Table
```sql
CREATE TABLE [Categories] (
    [Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](500) NULL
)
```

### Products Table
```sql
CREATE TABLE [Products] (
    [Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
    [Name] [nvarchar](200) NOT NULL,
    [Description] [nvarchar](1000) NULL,
    [Price] [numeric](18, 2) NOT NULL,
    [StockQuantity] [int] NOT NULL,
    [ImageUrl] [nvarchar](500) NULL,
    [CategoryId] [uniqueidentifier] NOT NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id]) ON DELETE CASCADE
)
```

### Orders Table
```sql
CREATE TABLE [Orders] (
    [Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
    [UserId] [nvarchar](100) NOT NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
    [TotalAmount] [numeric](18, 2) NOT NULL,
    [Status] [nvarchar](50) NOT NULL,
    [Notes] [nvarchar](500) NULL
)
```

### OrderItems Table
```sql
CREATE TABLE [OrderItems] (
    [Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
    [OrderId] [uniqueidentifier] NOT NULL,
    [ProductId] [uniqueidentifier] NOT NULL,
    [Quantity] [int] NOT NULL,
    [UnitPrice] [numeric](18, 2) NOT NULL,
    [CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE RESTRICT
)
```

## Common Migration Tasks

### Rolling Back to Previous State

**Remove the last migration** (before applying to database):
```bash
dotnet ef migrations remove
```

**Revert database to a specific migration**:
```bash
dotnet ef database update 20250420015036_InitialMigration
```

### Updating Existing Migrations

If you need to modify the domain model:

1. Update the entity in `Entities/`
2. Create a new migration:
   ```bash
   dotnet ef migrations add AddNewPropertyToProduct
   ```
3. Apply the changes:
   ```bash
   dotall ef database update
   ```

### Dropping and Recreating Database

**Warning**: This deletes all data!

```bash
dotnet ef database drop --force
dotnet ef database update
```

## Troubleshooting

### Issue: "Cannot create database - permission denied"

**Solution**: 
- Ensure SQL Server is running
- Check user has database creation permissions
- Verify connection string is correct

### Issue: "Cannot find migrations"

**Solution**:
```bash
# Reinstall EF Core tools
dotnet tool update --global dotnet-ef

# Or install if not present
dotnet tool install --global dotnet-ef
```

### Issue: "A migration with the same name already exists"

**Solution**:
```bash
# Remove the conflicting migration
dotnet ef migrations remove

# Create with a different timestamp
dotnet ef migrations add UniqueNameForMigration
```

### Issue: "The DbContext is incompatible"

**Solution**:
```bash
# Rebuild the project
dotnet clean
dotnet build

# Then run migrations again
dotnet ef database update
```

## Production Deployment

### Before Deploying to Production

1. **Backup your database** - Always!
   ```sql
   BACKUP DATABASE [AdegaroyalDb] TO DISK = 'C:\Backups\AdegaroyalDb.bak'
   ```

2. **Test migrations in staging** environment first

3. **Generate migration script** (no automatic execution):
   ```bash
   dotnet ef migrations script -o migration.sql
   ```

4. **Review the SQL script** before execution

### Deploying Migrations

**Automated (development)**:
```bash
dotnet ef database update --configuration Release
```

**Manual (production)**:
1. Generate SQL script: `dotnet ef migrations script -o prod_migration.sql`
2. Execute script in SQL Server Management Studio
3. Verify schema changes
4. Deploy application

## Seed Data (Optional)

To add initial categories after migration, add to `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category(Guid.NewGuid(), "Red Wines", "Premium red wine selection"),
            new Category(Guid.NewGuid(), "White Wines", "Crisp and refreshing white wines"),
            new Category(Guid.NewGuid(), "Beers", "Craft and imported beers"),
            new Category(Guid.NewGuid(), "Spirits", "Whiskey, rum, vodka and more")
        );
        
        await context.SaveChangesAsync();
    }
}
```

## Monitoring

### Check Migration History

```sql
-- SQL Server
SELECT * FROM [__EFMigrationsHistory] ORDER BY [MigrationId] DESC;
```

### View Active Queries

```sql
SELECT * FROM sys.dm_exec_sessions WHERE database_id = DB_ID('AdegaroyalDb');
```

## Useful Commands Reference

```bash
# List all migrations
dotnet ef migrations list

# Show migration details
dotnet ef migrations show 20260423120000_InitialAdegaroyalMigration

# Generate SQL script without executing
dotnet ef migrations script

# Get detailed migration info
dotnet ef dbcontext info

# Scaffold from existing database
dotnet ef dbcontext scaffold "connection-string" Microsoft.EntityFrameworkCore.SqlServer
```

---

**Migration created on**: 2026-04-23
**Migration applies to**: Adega Royal API
**Includes**: Categories, Products, Orders, OrderItems tables
