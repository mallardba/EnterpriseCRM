# Enterprise CRM - Database Design

## 🗄️ Database Overview

The Enterprise CRM system uses SQL Server with a well-designed relational database that follows normalization principles and supports comprehensive customer relationship management functionality.

## 📊 Entity Relationship Diagram

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│    Users    │    │  Customers  │    │   Contacts  │
│             │    │             │    │             │
│ - Id        │    │ - Id        │◄───┤ - Id        │
│ - FirstName │    │ - Company   │    │ - CustomerId│
│ - LastName  │    │ - Email     │    │ - FirstName │
│ - Email     │    │ - Type      │    │ - LastName  │
│ - Username  │    │ - Status    │    │ - Email     │
│ - Role      │    │ - Industry  │    │ - JobTitle  │
│ - Status    │    │ - CreatedAt │    │ - Role      │
└─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │
       │                   │                   │
       │                   ▼                   │
       │            ┌─────────────┐            │
       │            │    Leads    │            │
       │            │             │            │
       │            │ - Id        │            │
       │            │ - Company   │            │
       │            │ - Email     │            │
       │            │ - Source    │            │
       │            │ - Status    │            │
       │            │ - Priority  │            │
       │            │ - AssignedTo│            │
       │            │ - CustomerId│            │
       │            └─────────────┘            │
       │                   │                   │
       │                   ▼                   │
       │            ┌─────────────┐            │
       │            │Opportunities│            │
       │            │             │            │
       │            │ - Id        │            │
       │            │ - CustomerId│            │
       │            │ - Name      │            │
       │            │ - Stage     │            │
       │            │ - Amount    │            │
       │            │ - Probability│           │
       │            │ - AssignedTo│            │
       │            └─────────────┘            │
       │                   │                   │
       │                   ▼                   │
       │            ┌─────────────┐            │
       │            │    Tasks    │            │
       │            │             │            │
       │            │ - Id        │            │
       │            │ - Title     │            │
       │            │ - Type      │            │
       │            │ - Priority  │            │
       │            │ - Status    │            │
       │            │ - DueDate   │            │
       │            │ - AssignedTo│            │
       │            │ - CustomerId│            │
       │            │ - LeadId    │            │
       │            │ - OppId     │            │
       │            └─────────────┘            │
       │                   │                   │
       └───────────────────┼───────────────────┘
                           │
                           ▼
                   ┌─────────────┐
                   │   Reports   │
                   │             │
                   │ - Metrics   │
                   │ - Analytics │
                   │ - Charts    │
                   └─────────────┘
```

## 🏗️ Core Entities

### **1. BaseEntity**
**Purpose:** Common properties for all entities

**Properties:**
- `Id` (int) - Primary key
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime?) - Last update timestamp
- `CreatedBy` (string) - User who created the record
- `UpdatedBy` (string?) - User who last updated the record
- `IsDeleted` (bool) - Soft delete flag

**Benefits:**
- **Audit Trail:** Track who created/modified records
- **Soft Delete:** Preserve data integrity
- **Consistency:** Common properties across all entities

### **2. Customer**
**Purpose:** Represents companies or individual customers

**Key Properties:**
- `CompanyName` (string) - Company name
- `FirstName/LastName` (string?) - Individual customer names
- `Email` (string) - Primary email address
- `Phone` (string?) - Contact phone number
- `Address` (string?) - Physical address
- `Industry` (string?) - Business industry
- `Type` (CustomerType) - Individual or Company
- `Status` (CustomerStatus) - Active, Inactive, Suspended

**Relationships:**
- **One-to-Many:** Contacts, Leads, Opportunities, Tasks

### **3. Contact**
**Purpose:** Individual contacts within a customer organization

**Key Properties:**
- `CustomerId` (int) - Foreign key to Customer
- `FirstName/LastName` (string) - Contact names
- `Email` (string) - Contact email
- `JobTitle` (string?) - Position in company
- `Department` (string?) - Department
- `Role` (ContactRole) - Contact role type
- `IsPrimary` (bool) - Primary contact flag

**Relationships:**
- **Many-to-One:** Customer

### **4. Lead**
**Purpose:** Potential customers in the sales pipeline

**Key Properties:**
- `CompanyName` (string) - Company name
- `FirstName/LastName` (string?) - Contact names
- `Email` (string) - Contact email
- `Source` (LeadSource) - How lead was acquired
- `Status` (LeadStatus) - Current lead status
- `Priority` (LeadPriority) - Lead priority level
- `EstimatedValue` (decimal) - Estimated deal value
- `ExpectedCloseDate` (DateTime?) - Expected close date
- `AssignedToUserId` (int?) - Assigned salesperson

**Relationships:**
- **Many-to-One:** User (AssignedToUser)
- **Many-to-One:** Customer (optional)
- **One-to-Many:** Tasks

### **5. Opportunity**
**Purpose:** Sales opportunities with customers

**Key Properties:**
- `CustomerId` (int) - Foreign key to Customer
- `Name` (string) - Opportunity name
- `Stage` (OpportunityStage) - Sales stage
- `Amount` (decimal) - Deal amount
- `Probability` (decimal) - Win probability percentage
- `ExpectedCloseDate` (DateTime?) - Expected close date
- `ActualCloseDate` (DateTime?) - Actual close date
- `Status` (OpportunityStatus) - Open, Won, Lost, Cancelled
- `Product` (string?) - Product/service
- `AssignedToUserId` (int?) - Assigned salesperson

**Relationships:**
- **Many-to-One:** Customer
- **Many-to-One:** User (AssignedToUser)
- **One-to-Many:** Tasks

### **6. Task**
**Purpose:** Activities and tasks for users

**Key Properties:**
- `Title` (string) - Task title
- `Description` (string?) - Task description
- `Type` (TaskType) - Task type (Call, Email, Meeting, etc.)
- `Priority` (TaskPriority) - Task priority
- `Status` (TaskStatus) - Current status
- `DueDate` (DateTime?) - Due date
- `CompletedDate` (DateTime?) - Completion date
- `AssignedToUserId` (int) - Assigned user
- `CustomerId` (int?) - Related customer
- `LeadId` (int?) - Related lead
- `OpportunityId` (int?) - Related opportunity

**Relationships:**
- **Many-to-One:** User (AssignedToUser)
- **Many-to-One:** Customer (optional)
- **Many-to-One:** Lead (optional)
- **Many-to-One:** Opportunity (optional)

### **7. User**
**Purpose:** System users and employees

**Key Properties:**
- `FirstName/LastName` (string) - User names
- `Email` (string) - User email
- `Username` (string) - Login username
- `PasswordHash` (string) - Hashed password
- `Role` (UserRole) - User role (Admin, Manager, User, ReadOnly)
- `Status` (UserStatus) - Account status
- `LastLoginDate` (DateTime?) - Last login timestamp
- `JobTitle` (string?) - Job position
- `Department` (string?) - Department

**Relationships:**
- **One-to-Many:** AssignedTasks, AssignedLeads, AssignedOpportunities

## 📈 Enumerations

### **Customer Management**
- `CustomerType`: Individual, Company
- `CustomerStatus`: Active, Inactive, Suspended
- `ContactRole`: General, DecisionMaker, Influencer, User, Technical

### **Sales Pipeline**
- `LeadSource`: Website, Referral, ColdCall, Email, SocialMedia, TradeShow, Advertisement, Other
- `LeadStatus`: New, Contacted, Qualified, Proposal, Negotiation, ClosedWon, ClosedLost
- `LeadPriority`: Low, Medium, High, Critical
- `OpportunityStage`: Prospecting, Qualification, Proposal, Negotiation, ClosedWon, ClosedLost
- `OpportunityStatus`: Open, Won, Lost, Cancelled

### **Task Management**
- `TaskType`: General, Call, Email, Meeting, FollowUp, Proposal, Demo, Other
- `TaskPriority`: Low, Medium, High, Critical
- `TaskStatus`: Pending, InProgress, Completed, Cancelled, OnHold

### **User Management**
- `UserRole`: Admin, Manager, User, ReadOnly
- `UserStatus`: Active, Inactive, Suspended

## 🔧 Database Features

### **1. Soft Delete Pattern**
All entities implement soft delete using `IsDeleted` flag:
- **Benefits:** Data integrity, audit trail, recovery capability
- **Implementation:** Filter `IsDeleted = false` in queries

### **2. Audit Fields**
Every entity tracks:
- **Creation:** `CreatedAt`, `CreatedBy`
- **Modification:** `UpdatedAt`, `UpdatedBy`
- **Benefits:** Compliance, debugging, user tracking

### **3. Indexing Strategy**
**Performance Indexes:**
- Email fields for fast lookups
- Status fields for filtering
- Foreign keys for joins
- Date fields for reporting

**Index Examples:**
```sql
CREATE NONCLUSTERED INDEX IX_Customers_Email ON Customers(Email);
CREATE NONCLUSTERED INDEX IX_Customers_Status ON Customers(Status);
CREATE NONCLUSTERED INDEX IX_Tasks_DueDate ON Tasks(DueDate);
```

### **4. Stored Procedures**
**Business Logic Procedures:**
- `sp_GetCustomerMetrics` - Customer analytics
- `sp_CalculateSalesForecast` - Sales forecasting
- `sp_GenerateMonthlyReport` - Monthly reporting
- `sp_GetTaskAnalytics` - Task analytics
- `sp_GetUserPerformance` - User performance metrics

## 📊 Data Relationships

### **Customer-Centric Design**
- **Customers** are the central entity
- **Contacts** belong to customers
- **Leads** can convert to customers
- **Opportunities** are with customers
- **Tasks** can be related to customers, leads, or opportunities

### **User Assignment**
- **Users** can be assigned to leads, opportunities, and tasks
- **Performance tracking** through assigned records
- **Role-based access** controls

### **Sales Pipeline Flow**
```
Lead → Customer → Opportunity → Task
  ↓        ↓         ↓         ↓
User   Contact   User    User
```

## 🚀 Performance Considerations

### **Query Optimization**
- **Indexed fields** for common queries
- **Foreign key constraints** for data integrity
- **Stored procedures** for complex operations
- **Pagination** for large datasets

### **Scalability Features**
- **Normalized design** reduces redundancy
- **Soft delete** preserves data integrity
- **Audit trail** supports compliance
- **Flexible relationships** support growth

## 🔒 Security Features

### **Data Protection**
- **Password hashing** for user authentication
- **Role-based access** controls
- **Audit logging** for compliance
- **Soft delete** for data recovery

### **Access Control**
- **User roles** determine permissions
- **Status fields** control record visibility
- **Foreign key constraints** maintain integrity

## 📈 Reporting Capabilities

### **Built-in Analytics**
- **Customer metrics** and trends
- **Sales forecasting** and pipeline analysis
- **Task analytics** and performance tracking
- **User performance** metrics

### **Custom Reports**
- **Monthly reports** with date filtering
- **Performance dashboards** with real-time data
- **Export capabilities** for external analysis
- **Chart data** for visualization

This database design provides a solid foundation for enterprise CRM functionality with proper normalization, performance optimization, and scalability considerations.
