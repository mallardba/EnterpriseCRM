# Entity Framework Core Migrations - Complete Guide

## 🎯 Overview

Migrations are EF Core's way of managing database schema changes. When you change your entities (like adding the `Product` entity), migrations generate SQL scripts to update the database.

---

## 📚 Table of Contents

1. [What Are Migrations?](#-what-are-migrations)
2. [Why Migrations Are Necessary](#-why-migrations-are-necessary)
3. [The Migration Commands Explained](#-the-migration-commands-explained)
4. [Complete Migration Workflow](#-complete-migration-workflow)
5. [Common Scenarios](#-common-scenarios)
6. [Troubleshooting](#-troubleshooting)
7. [Best Practices](#-best-practices)

---

## 🔍 What Are Migrations?

### **Simple Explanation**

Migrations are **version control for your database schema**. Just like Git tracks code changes, migrations track database changes.

### **How It Works**

1. You change your C# entity (e.g., add `Product` class)
2. EF Core detects the change
3. You create a migration (generates SQL script)
4. You apply the migration (updates database)

---

## ❓ Why Migrations Are Necessary

### **The Problem**

When you add a new entity like `Product` to your C# code:

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // ... other properties
}
```

**Your SQL Server database doesn't automatically know about this!**

The `Products` table doesn't exist yet in the database. EF Core needs to:
1. Create the table
2. Add columns
3. Add constraints
4. Set up relationships

### **Migrations Solve This**

Migrations generate SQL scripts that:
- Create the `Products` table
- Add all columns (`Id`, `Name`, `Price`, etc.)
- Add constraints (NOT NULL, MaxLength, etc.)
- Create indexes
- Set up relationships (foreign keys)

---

## 🛠️ The Migration Commands Explained

### **Command 1: Create Migration**

```bash
dotnet ef migrations add AddProductEntity --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**What It Does:**
- Compares your current entity models with the last migration
- Generates a new migration file containing SQL changes
- **Does NOT modify the database yet**

**Breaking Down the Command:**

| Part | Explanation |
|------|-------------|
| `dotnet ef migrations add` | EF Core command to create a new migration |
| `AddProductEntity` | Name of the migration (describes what it does) |
| `--project src/EnterpriseCRM.Infrastructure` | Where your DbContext is located |
| `--startup-project src/EnterpriseCRM.WebAPI` | Project to use for finding connection string |

**What Gets Generated:**

```
src/EnterpriseCRM.Infrastructure/Migrations/
├── 20240115120000_AddProductEntity.cs    ← New migration file
└── ApplicationDbContextModelSnapshot.cs   ← Updated snapshot
```

**Migration File Contents:**

```csharp
public partial class AddProductEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Price = table.Column<decimal>(nullable: false),
                IsActive = table.Column<bool>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                // ... more columns
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Products");
    }
}
```

---

### **Command 2: Apply Migration**

```bash
dotnet ef database update --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**What It Does:**
- Reads all migration files in order
- Compares with database's migration history
- Executes only new migrations
- **Actually modifies the database**

**Breaking Down the Command:**

| Part | Explanation |
|------|-------------|
| `dotnet ef database update` | EF Core command to apply migrations |
| `--project src/EnterpriseCRM.Infrastructure` | Where migrations are located |
| `--startup-project src/EnterpriseCRM.WebAPI` | Project containing connection string |

**What Happens:**

1. EF Core reads `__EFMigrationsHistory` table
2. Finds which migrations have been applied
3. Executes pending migrations (in order)
4. Runs the `Up()` method from each migration
5. Records migration names in `__EFMigrationsHistory`

**SQL Executed:**

```sql
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240115120000_AddProductEntity')
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
    
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240115120000_AddProductEntity', N'8.0.0');
END
```

---

## 🔄 Complete Migration Workflow

### **Step-by-Step Process**

```
1. Add/Modify Entity
   ↓
2. Run: dotnet ef migrations add MigrationName
   ↓
3. Review generated migration file
   ↓
4. Run: dotnet ef database update
   ↓
5. Database schema is now in sync
```

### **Detailed Example: Adding Product Entity**

#### **Step 1: Add Product Entity**

```csharp
// src/EnterpriseCRM.Core/Entities.cs
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // ... other properties
}
```

---

#### **Step 2: Create Migration**

```bash
dotnet ef migrations add AddProductEntity --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**Output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'dotnet ef migrations remove'
```

**Generated Files:**
```
src/EnterpriseCRM.Infrastructure/Migrations/
├── 20240115120000_AddProductEntity.cs
└── ApplicationDbContextModelSnapshot.cs (updated)
```

---

#### **Step 3: Review Migration File**

Open `20240115120000_AddProductEntity.cs` and verify:

```csharp
public partial class AddProductEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Description = table.Column<string>(maxLength: 1000, nullable: true),
                SKU = table.Column<string>(maxLength: 100, nullable: true),
                Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                Category = table.Column<string>(maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedBy = table.Column<string>(maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                UpdatedAt = table.Column<DateTime>(nullable: true),
                OrderItems = table.Constraint("PK_Products", x => x.Id);
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Products");
    }
}
```

**Key Points:**
- `Up()`: What to do when applying migration (CREATE TABLE)
- `Down()`: How to undo migration (DROP TABLE)
- All columns match your entity properties
- Constraints are applied (max length, NOT NULL, etc.)

---

#### **Step 4: Apply Migration**

```bash
dotnet ef database update --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**Output:**
```
Build started...
Build succeeded.
Applying migration '20240115120000_AddProductEntity'.
Done.
```

**What Happened:**
1. EF Core connected to SQL Server
2. Checked `__EFMigrationsHistory` table
3. Found new migration `AddProductEntity`
4. Executed SQL to create `Products` table
5. Recorded migration in `__EFMigrationsHistory`

---

#### **Step 5: Verify**

Connect to database and verify table was created:

```sql
SELECT * FROM Products;
SELECT * FROM __EFMigrationsHistory WHERE MigrationId LIKE '%Product%';
```

**Result:**
- `Products` table exists ✅
- Table has all columns ✅
- Migration recorded ✅

---

## 💡 Common Scenarios

### **Scenario 1: Adding a New Property to Existing Entity**

**Problem**: You want to add `StockQuantity` to `Product`

#### **Steps:**

1. **Modify Entity**
```csharp
public class Product : BaseEntity
{
    // ... existing properties
    public int StockQuantity { get; set; }  // NEW
}
```

2. **Create Migration**
```bash
dotnet ef migrations add AddStockQuantityToProduct --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

3. **Generated Migration**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<int>(
        name: "StockQuantity",
        table: "Products",
        nullable: false,
        defaultValue: 0);
}
```

4. **Apply Migration**
```bash
dotnet ef database update --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**Result**: `StockQuantity` column added to `Products` table ✅

---

### **Scenario 2: Removing a Property**

**Problem**: You want to remove `SKU` from `Product`

#### **Steps:**

1. **Remove from Entity**
```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // public string? SKU { get; set; }  ← REMOVED
}
```

2. **Create Migration**
```bash
dotnet ef migrations add RemoveSKUFromProduct --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

3. **Generated Migration**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "SKU",
        table: "Products");
}
```

4. **Apply Migration**
```bash
dotnet ef database update
```

**Result**: `SKU` column removed from `Products` table ✅

---

### **Scenario 3: Undoing a Migration (Rollback)**

**Problem**: You want to undo the last migration

#### **Solution:**

```bash
dotnet ef database update PreviousMigrationName --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**Example:**
```bash
# Current migrations
# 1. AddProductEntity
# 2. AddStockQuantityToProduct  ← Current

# Rollback to AddProductEntity
dotnet ef database update AddProductEntity
```

**Result**: `AddStockQuantityToProduct` migration is rolled back ✅

---

### **Scenario 4: Removing a Migration (Before Applying)**

**Problem**: You created a migration but haven't applied it yet, and you want to remove it

#### **Solution:**

```bash
dotnet ef migrations remove --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

**Result**: Last migration file is deleted ✅

---

## 🔧 Troubleshooting

### **Problem 1: "Unable to create an object of type 'ApplicationDbContext'"**

**Solution:**
```bash
# Ensure startup project is specified
dotnet ef migrations add AddProductEntity --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

---

### **Problem 2: "Migration already exists"**

**Solution:**
```bash
# Remove old migration first
dotnet ef migrations remove --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI

# Then create new one
dotnet ef migrations add NewMigrationName --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

---

### **Problem 3: "Database update fails"**

**Solution:**

Check database connection:
```bash
# List pending migrations
dotnet ef migrations list --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI
```

Check connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EnterpriseCRM;Trusted_Connection=True;"
  }
}
```

---

### **Problem 4: "Model snapshot is out of sync"**

**Solution:**

Regenerate snapshot:
```bash
dotnet ef migrations add RefreshSnapshot --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.WebAPI --force
```

---

## ✅ Best Practices

### **1. One Migration Per Feature**

```bash
# Good
dotnet ef migrations add AddProductEntity
dotnet ef migrations add AddOrderEntity

# Bad (combining multiple changes)
dotnet ef migrations add AddProductsAndOrdersAndCustomers
```

---

### **2. Descriptive Migration Names**

```bash
# Good
dotnet ef migrations add AddEmailColumnToUserTable

# Bad
dotnet ef migrations add change1
dotnet ef migrations add stuff
```

---

### **3. Review Before Applying**

Always review generated migration files before running `database update`:

```bash
# Create migration
dotnet ef migrations add AddProductEntity

# Review file
# src/EnterpriseCRM.Infrastructure/Migrations/20240115120000_AddProductEntity.cs

# Apply only if correct
dotnet ef database update
```

---

### **4. Test on Local Database First**

```bash
# Create migration
dotnet ef migrations add AddProductEntity

# Apply to local database
dotnet ef database update

# Test application

# If successful, deploy to production
```

---

### **5. Never Modify Applied Migrations**

**❌ Don't:**
```csharp
// Editing a migration that's already been applied
public partial class AddProductEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // DON'T modify this if it's already applied to database!
        migrationBuilder.CreateTable(/* ... */);
    }
}
```

**✅ Do:**
Create a new migration instead:
```bash
dotnet ef migrations add RemoveWrongColumn
```

---

### **6. Version Control Migrations**

**Always commit migration files to Git:**

```bash
git add src/EnterpriseCRM.Infrastructure/Migrations/
git commit -m "Add Product entity migration"
```

**Why**: Other developers need the same migrations to keep databases in sync.

---

## 📊 Migration Lifecycle

### **Development Workflow**

```
1. Change C# Entity
   ↓
2. Create Migration (generates SQL file)
   ↓
3. Review SQL in migration file
   ↓
4. Apply Migration (updates database)
   ↓
5. Test Application
   ↓
6. Commit to Git
   ↓
7. Deploy to Production
   ↓
8. Apply migrations in production
```

---

## 🎯 Key Takeaways

1. **Migrations sync database with code** - When you change entities, migrations keep DB in sync
2. **Two commands**: `add` creates migration, `update` applies it
3. **Always review before applying** - Check generated SQL
4. **Version control migrations** - Commit migration files to Git
5. **One migration per change** - Keeps history clean and rollback-safe

---

## 🔗 Related Concepts

- **Entity Framework Core**: ORM that manages database
- **DbContext**: Represents database in code
- **Model Snapshot**: EF Core's representation of current schema
- **Migration History**: Table tracking applied migrations (`__EFMigrationsHistory`)
- **Up/Down Methods**: How to apply/undo migrations

---

## 📚 Further Reading

- **Official Docs**: `https://learn.microsoft.com/ef/core/managing-schemas/migrations/`
- **Getting Started**: `https://learn.microsoft.com/ef/core/get-started/overview/first-app`
- **Migrations Overview**: `https://learn.microsoft.com/ef/core/managing-schemas/migrations/`

---

**Summary**: Migrations are EF Core's way of keeping your database schema in sync with your C# entities. When you add `Product` entity, you must run migrations to create the `Products` table in the database. Use `dotnet ef migrations add` to create migrations, and `dotnet ef database update` to apply them.

