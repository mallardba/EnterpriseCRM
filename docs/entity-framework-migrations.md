# Enterprise CRM - Entity Framework Migrations Guide

This document explains the Entity Framework Core migration process, build dependencies, and why specific commands must be run from the Infrastructure project directory.

## 🏗️ Understanding the Build Process

### **What Happens During `dotnet build`**

When you run `dotnet build`, the .NET CLI performs several critical steps:

#### **1. Project Discovery and Dependency Resolution**
- **Scans the solution file** (`EnterpriseCRM.sln`) to identify all projects and their relationships
- **Reads project files** (`.csproj`) to understand dependencies, target frameworks, and package references
- **Builds dependency graph** to determine the correct build order

#### **2. Package Restoration**
- **Downloads NuGet packages** from configured sources (nuget.org, private feeds)
- **Resolves version conflicts** using the dependency resolution algorithm
- **Caches packages** locally for faster subsequent builds
- **Generates project assets** (`project.assets.json`) containing resolved package information

#### **3. Compilation Process**
- **Compiles source code** from C# to Intermediate Language (IL)
- **Resolves type references** across project boundaries
- **Validates syntax and semantics** according to C# language rules
- **Generates assemblies** (`.dll` files) in `bin/Debug/net8.0/` or `bin/Release/net8.0/`

#### **4. Cross-Project References**
- **Validates project references** ensure all dependencies are available
- **Checks assembly compatibility** between referenced projects
- **Ensures proper dependency order** (Core → Application → Infrastructure → WebAPI)

### **Why Builds Can Fail Even When Individual Projects Compile**

#### **Scenario: EF Migrations Build Failure**
```
dotnet ef migrations add InitialCreate --startup-project ../EnterpriseCRM.WebAPI
Build started...
Build failed. Use dotnet build to see the errors.
```

**Root Cause:** EF migrations build the **startup project** (WebAPI), not just the Infrastructure project.

#### **The Build Chain:**
1. **Infrastructure project** compiles successfully ✅
2. **WebAPI project** (startup project) fails to compile ❌
3. **EF tools** cannot proceed without a working startup project

#### **Common Failure Points:**
- **Missing service registrations** in `Program.cs`
- **Unresolved type references** between projects
- **Configuration issues** in `appsettings.json`
- **Package version conflicts** across projects

## 🔧 Entity Framework Migration Commands

### **Command 1: `dotnet ef migrations add InitialCreate --startup-project ../EnterpriseCRM.WebAPI`**

#### **What This Command Does:**

##### **1. Project Analysis**
- **Identifies the DbContext** (`ApplicationDbContext`) in the Infrastructure project
- **Scans entity classes** for table mappings and relationships
- **Analyzes configuration** in `OnModelCreating` method

##### **2. Model Comparison**
- **Compares current model** with existing database schema
- **Identifies differences** between code-first entities and database tables
- **Generates migration script** to synchronize the database

##### **3. Migration File Creation**
Creates three files in `Migrations/` folder:
```
20241201120000_InitialCreate.cs          # Migration class
20241201120000_InitialCreate.Designer.cs # Model snapshot
20241201120000_InitialCreate.sql         # SQL script (optional)
```

##### **4. Migration Class Structure**
```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Creates tables, indexes, constraints
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CompanyName = table.Column<string>(type: "nvarchar(200)", nullable: true),
                // ... other columns
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drops tables, indexes, constraints
        migrationBuilder.DropTable(name: "Customers");
    }
}
```

#### **Why `--startup-project` Parameter is Required:**

##### **1. Configuration Access**
- **Reads connection strings** from `appsettings.json` in WebAPI project
- **Accesses environment-specific settings** (Development, Production)
- **Resolves dependency injection** for DbContext configuration

##### **2. Service Resolution**
- **Registers services** defined in `Program.cs`
- **Configures DbContext options** (connection string, provider, etc.)
- **Resolves any custom configurations** or interceptors

##### **3. Assembly Loading**
- **Loads all referenced assemblies** including Infrastructure project
- **Resolves entity types** and their relationships
- **Accesses custom configurations** and conventions

### **Command 2: `dotnet ef database update --startup-project ../EnterpriseCRM.WebAPI`**

#### **What This Command Does:**

##### **1. Migration History Check**
- **Queries `__EFMigrationsHistory` table** to see applied migrations
- **Identifies pending migrations** not yet applied to database
- **Determines migration order** based on timestamps

##### **2. Database Schema Update**
- **Executes `Up()` methods** of pending migrations
- **Creates tables, indexes, constraints** as defined in migration files
- **Updates migration history** table with applied migration records

##### **3. Transaction Management**
- **Wraps each migration** in a database transaction
- **Rolls back on failure** to maintain database consistency
- **Commits successful migrations** atomically

##### **4. Schema Validation**
- **Verifies applied changes** match migration definitions
- **Checks constraint creation** and index building
- **Validates foreign key relationships**

## 📁 Why Commands Must Run from Infrastructure Directory

### **Directory Structure Context:**
```
src/
├── EnterpriseCRM.Core/           # Domain entities
├── EnterpriseCRM.Application/    # Business logic
├── EnterpriseCRM.Infrastructure/ # DbContext and migrations
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   └── Migrations/              # Generated migration files
└── EnterpriseCRM.WebAPI/       # Startup project
    ├── Program.cs
    └── appsettings.json
```

### **Technical Reasons:**

#### **1. DbContext Location**
- **DbContext is defined** in Infrastructure project (`ApplicationDbContext.cs`)
- **EF tools scan** the current directory for DbContext classes
- **Migration files** are created in the same project as DbContext

#### **2. Project Reference Resolution**
- **Infrastructure project** references Core and Application projects
- **EF tools need access** to all entity classes and configurations
- **Migration generation** requires complete model information

#### **3. File System Organization**
- **Migration files** are created in `Migrations/` folder within Infrastructure project
- **Designer files** maintain model snapshots for the specific project
- **Version control** keeps migrations with the data access layer

#### **4. Build Context**
- **EF tools build** the Infrastructure project to access DbContext
- **Startup project** provides configuration and connection strings
- **Cross-project references** are resolved through the solution structure

## 🔄 Complete Migration Workflow

### **Step-by-Step Process:**

#### **1. Initial Setup**
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Navigate to Infrastructure project
cd src/EnterpriseCRM.Infrastructure
```

#### **2. Create Initial Migration**
```bash
# Generate migration from current model
dotnet ef migrations add InitialCreate --startup-project ../EnterpriseCRM.WebAPI
```

**What happens:**
- Scans `ApplicationDbContext` for all `DbSet<T>` properties
- Analyzes entity configurations in `OnModelCreating`
- Generates SQL to create tables, indexes, constraints
- Creates migration files in `Migrations/` folder

#### **3. Apply Migration to Database**
```bash
# Update database schema
dotnet ef database update --startup-project ../EnterpriseCRM.WebAPI
```

**What happens:**
- Connects to database using connection string from WebAPI
- Checks `__EFMigrationsHistory` table for applied migrations
- Executes pending migrations in chronological order
- Updates database schema to match current model

#### **4. Verify Migration**
```bash
# Check migration status
dotnet ef migrations list --startup-project ../EnterpriseCRM.WebAPI

# View database schema
dotnet ef dbcontext info --startup-project ../EnterpriseCRM.WebAPI
```

## 🚨 Common Issues and Solutions

### **Issue 1: Build Failures During Migration**
```
Build started...
Build failed. Use dotnet build to see the errors.
```

**Solution:**
1. **Build WebAPI project first:**
   ```bash
   cd src/EnterpriseCRM.WebAPI
   dotnet build
   ```

2. **Fix compilation errors** in WebAPI project
3. **Return to Infrastructure** and retry migration

### **Issue 2: Connection String Problems**
```
A connection string named 'DefaultConnection' could not be found.
```

**Solution:**
1. **Verify connection string** in `appsettings.json`
2. **Check environment** (Development vs Production)
3. **Ensure database exists** and is accessible

### **Issue 3: DbContext Not Found**
```
No DbContext was found in assembly 'EnterpriseCRM.Infrastructure'.
```

**Solution:**
1. **Run from correct directory** (Infrastructure project)
2. **Verify DbContext class** inherits from `DbContext`
3. **Check project references** are correct

### **Issue 4: Migration Conflicts**
```
The migration 'InitialCreate' already exists.
```

**Solution:**
1. **Remove existing migration:**
   ```bash
   dotnet ef migrations remove --startup-project ../EnterpriseCRM.WebAPI
   ```

2. **Recreate with new name:**
   ```bash
   dotnet ef migrations add InitialCreateV2 --startup-project ../EnterpriseCRM.WebAPI
   ```

## 📊 Migration File Anatomy

### **Migration Class (`InitialCreate.cs`)**
```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Forward migration - applies changes
        migrationBuilder.CreateTable(/* table definition */);
        migrationBuilder.CreateIndex(/* index definition */);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse migration - undoes changes
        migrationBuilder.DropTable(/* table name */);
        migrationBuilder.DropIndex(/* index name */);
    }
}
```

### **Designer File (`InitialCreate.Designer.cs`)**
```csharp
[DbContext(typeof(ApplicationDbContext))]
partial class InitialCreateModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        // Complete model snapshot at this point in time
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            // ... all entity configurations
        });
    }
}
```

## 🎯 Best Practices

### **1. Migration Naming**
- **Use descriptive names:** `AddCustomerEmailIndex`, `CreateOrdersTable`
- **Include version numbers:** `InitialCreate`, `AddFeaturesV2`
- **Avoid generic names:** `Migration1`, `Update1`

### **2. Migration Management**
- **Never edit existing migrations** after they've been applied
- **Create new migrations** for additional changes
- **Test migrations** in development before production

### **3. Database Strategy**
- **Use migrations for development** and testing
- **Consider database projects** for production deployments
- **Backup databases** before applying migrations

### **4. Team Collaboration**
- **Commit migration files** to version control
- **Coordinate migration timing** across team members
- **Use consistent naming conventions**

## 🔍 Troubleshooting Commands

### **Diagnostic Commands:**
```bash
# Check EF Core tools version
dotnet ef --version

# List all migrations
dotnet ef migrations list --startup-project ../EnterpriseCRM.WebAPI

# Check database connection
dotnet ef dbcontext info --startup-project ../EnterpriseCRM.WebAPI

# Generate SQL script without applying
dotnet ef migrations script --startup-project ../EnterpriseCRM.WebAPI

# Remove last migration
dotnet ef migrations remove --startup-project ../EnterpriseCRM.WebAPI
```

### **Debugging Build Issues:**
```bash
# Build with verbose output
dotnet build --verbosity detailed

# Clean and rebuild
dotnet clean
dotnet build

# Restore packages
dotnet restore
```

This comprehensive guide should help you understand the Entity Framework migration process and why specific commands must be run from the Infrastructure project directory.
