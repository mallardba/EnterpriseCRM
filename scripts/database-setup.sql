-- Enterprise CRM Database Scripts
-- This file contains SQL Server scripts for database setup and stored procedures

USE EnterpriseCRM;
GO

-- =============================================
-- DROP EXISTING TABLES (in dependency order)
-- =============================================

-- Drop tables in reverse dependency order to avoid foreign key constraint errors
IF EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
    DROP TABLE OrderItems;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
    DROP TABLE Orders;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
    DROP TABLE Products;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Activities' AND xtype='U')
    DROP TABLE Activities;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Opportunities' AND xtype='U')
    DROP TABLE Opportunities;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Leads' AND xtype='U')
    DROP TABLE Leads;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
    DROP TABLE Users;
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
    DROP TABLE Customers;
GO

-- =============================================
-- TABLE CREATION
-- =============================================

-- Create Customers table
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(200),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(50),
    PostalCode NVARCHAR(20),
    Country NVARCHAR(100),
    Industry NVARCHAR(100),
    Type INT NOT NULL DEFAULT 0, -- 0=Individual, 1=Company
    Status INT NOT NULL DEFAULT 0, -- 0=Active, 1=Inactive, 2=Suspended
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2
);
GO

-- Create Opportunities table
CREATE TABLE Opportunities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Amount DECIMAL(18,2) NOT NULL,
    Probability INT NOT NULL DEFAULT 0,
    Stage INT NOT NULL DEFAULT 0, -- 0=Prospecting, 1=Qualification, 2=Proposal, 3=Negotiation, 4=Closed Won, 5=Closed Lost
    Status INT NOT NULL DEFAULT 0, -- 0=Open, 1=Closed
    ExpectedCloseDate DATETIME2,
    ActualCloseDate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);
GO

-- Create Leads table
CREATE TABLE Leads (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    CompanyName NVARCHAR(200),
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    Source NVARCHAR(100), -- Website, Referral, Cold Call, etc.
    Status INT NOT NULL DEFAULT 0, -- 0=New, 1=Contacted, 2=Qualified, 3=Converted, 4=Lost
    Score INT DEFAULT 0, -- Lead scoring
    Notes NVARCHAR(MAX),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2
);
GO

-- Create Activities table
CREATE TABLE Activities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT,
    LeadId INT,
    OpportunityId INT,
    Type INT NOT NULL, -- 0=Call, 1=Email, 2=Meeting, 3=Task, 4=Note
    Subject NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    ScheduledDate DATETIME2,
    CompletedDate DATETIME2,
    Status INT NOT NULL DEFAULT 0, -- 0=Scheduled, 1=Completed, 2=Cancelled
    Priority INT NOT NULL DEFAULT 0, -- 0=Low, 1=Medium, 2=High
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (LeadId) REFERENCES Leads(Id),
    FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id)
);
GO

-- Create Products table
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    SKU NVARCHAR(100),
    Price DECIMAL(18,2) NOT NULL,
    Cost DECIMAL(18,2),
    Category NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2
);
GO

-- Create Orders table
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderNumber NVARCHAR(50) NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Processing, 2=Shipped, 3=Delivered, 4=Cancelled
    TotalAmount DECIMAL(18,2) NOT NULL,
    Notes NVARCHAR(MAX),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);
GO

-- Create OrderItems table
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- Create Users table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    JobTitle NVARCHAR(100),
    Department NVARCHAR(100),
    Role INT NOT NULL DEFAULT 0, -- 0=User, 1=Manager, 2=Admin
    Status INT NOT NULL DEFAULT 0, -- 0=Active, 1=Inactive, 2=Suspended
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100),
    UpdatedAt DATETIME2
);
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

-- Create stored procedure for customer metrics
CREATE OR ALTER PROCEDURE sp_GetCustomerMetrics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) as TotalCustomers,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as ActiveCustomers,
        COUNT(CASE WHEN Status = 1 THEN 1 END) as InactiveCustomers,
        COUNT(CASE WHEN Status = 2 THEN 1 END) as SuspendedCustomers,
        COUNT(CASE WHEN Type = 0 THEN 1 END) as IndividualCustomers,
        COUNT(CASE WHEN Type = 1 THEN 1 END) as CompanyCustomers,
        COUNT(CASE WHEN CreatedAt >= DATEADD(month, -1, GETDATE()) THEN 1 END) as NewCustomersThisMonth
    FROM Customers
    WHERE IsDeleted = 0;
END
GO

-- Create stored procedure for sales forecast
CREATE OR ALTER PROCEDURE sp_CalculateSalesForecast
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SUM(Amount) as TotalPipelineValue,
        SUM(Amount * Probability / 100) as ForecastedRevenue,
        COUNT(*) as TotalOpportunities,
        COUNT(CASE WHEN Stage = 0 THEN 1 END) as ProspectingCount,
        COUNT(CASE WHEN Stage = 1 THEN 1 END) as QualificationCount,
        COUNT(CASE WHEN Stage = 2 THEN 1 END) as ProposalCount,
        COUNT(CASE WHEN Stage = 3 THEN 1 END) as NegotiationCount,
        COUNT(CASE WHEN Stage = 4 THEN 1 END) as ClosedWonCount,
        COUNT(CASE WHEN Stage = 5 THEN 1 END) as ClosedLostCount
    FROM Opportunities
    WHERE IsDeleted = 0 AND Status = 0;
END
GO

-- Create stored procedure for monthly report
CREATE OR ALTER PROCEDURE sp_GenerateMonthlyReport
    @Year INT,
    @Month INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StartDate DATETIME = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @EndDate DATETIME = EOMONTH(@StartDate);
    
    -- Customer metrics
    SELECT 
        'CustomerMetrics' as ReportType,
        COUNT(*) as TotalCustomers,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as ActiveCustomers,
        COUNT(CASE WHEN CreatedAt >= @StartDate AND CreatedAt <= @EndDate THEN 1 END) as NewCustomers
    FROM Customers
    WHERE IsDeleted = 0
    
    UNION ALL
    
    -- Lead metrics
    SELECT 
        'LeadMetrics' as ReportType,
        COUNT(*) as TotalLeads,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as NewLeads,
        COUNT(CASE WHEN Status = 6 THEN 1 END) as ClosedWonLeads
    FROM Leads
    WHERE IsDeleted = 0 AND CreatedAt >= @StartDate AND CreatedAt <= @EndDate
    
    UNION ALL
    
    -- Opportunity metrics
    SELECT 
        'OpportunityMetrics' as ReportType,
        COUNT(*) as TotalOpportunities,
        SUM(Amount) as TotalValue,
        SUM(Amount * Probability / 100) as ForecastedValue
    FROM Opportunities
    WHERE IsDeleted = 0 AND CreatedAt >= @StartDate AND CreatedAt <= @EndDate;
END
GO

-- -- Create stored procedure for task analytics
-- CREATE OR ALTER PROCEDURE sp_GetTaskAnalytics
-- AS
-- BEGIN
--     SET NOCOUNT ON;
    
--     SELECT 
--         COUNT(*) as TotalTasks,
--         COUNT(CASE WHEN Status = 0 THEN 1 END) as PendingTasks,
--         COUNT(CASE WHEN Status = 1 THEN 1 END) as InProgressTasks,
--         COUNT(CASE WHEN Status = 2 THEN 1 END) as CompletedTasks,
--         COUNT(CASE WHEN Status = 3 THEN 1 END) as CancelledTasks,
--         COUNT(CASE WHEN Status = 4 THEN 1 END) as OnHoldTasks,
--         COUNT(CASE WHEN DueDate < GETDATE() AND Status != 2 THEN 1 END) as OverdueTasks,
--         COUNT(CASE WHEN DueDate = CAST(GETDATE() AS DATE) AND Status != 2 THEN 1 END) as DueTodayTasks
--     FROM Tasks
--     WHERE IsDeleted = 0;
-- END
-- GO

-- -- Create stored procedure for user performance
-- CREATE OR ALTER PROCEDURE sp_GetUserPerformance
--     @UserId INT = NULL
-- AS
-- BEGIN
--     SET NOCOUNT ON;
    
--     SELECT 
--         u.Id,
--         u.FirstName + ' ' + u.LastName as FullName,
--         u.Email,
--         u.JobTitle,
--         u.Department,
--         COUNT(DISTINCT t.Id) as TotalTasks,
--         COUNT(DISTINCT CASE WHEN t.Status = 2 THEN t.Id END) as CompletedTasks,
--         COUNT(DISTINCT CASE WHEN t.DueDate < GETDATE() AND t.Status != 2 THEN t.Id END) as OverdueTasks,
--         COUNT(DISTINCT l.Id) as AssignedLeads,
--         COUNT(DISTINCT o.Id) as AssignedOpportunities,
--         ISNULL(SUM(o.Amount), 0) as TotalOpportunityValue
--     FROM Users u
--     LEFT JOIN Tasks t ON u.Id = t.AssignedToUserId AND t.IsDeleted = 0
--     LEFT JOIN Leads l ON u.Id = l.AssignedToUserId AND l.IsDeleted = 0
--     LEFT JOIN Opportunities o ON u.Id = o.AssignedToUserId AND o.IsDeleted = 0
--     WHERE u.IsDeleted = 0
--     AND (@UserId IS NULL OR u.Id = @UserId)
--     GROUP BY u.Id, u.FirstName, u.LastName, u.Email, u.JobTitle, u.Department
--     ORDER BY TotalTasks DESC;
-- END
-- GO

-- =============================================
-- INDEXES
-- =============================================

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX IX_Customers_Email ON Customers(Email);
CREATE NONCLUSTERED INDEX IX_Customers_Status ON Customers(Status);
CREATE NONCLUSTERED INDEX IX_Customers_Type ON Customers(Type);
CREATE NONCLUSTERED INDEX IX_Customers_CreatedAt ON Customers(CreatedAt);

-- CREATE NONCLUSTERED INDEX IX_Contacts_CustomerId ON Contacts(CustomerId);
-- CREATE NONCLUSTERED INDEX IX_Contacts_Email ON Contacts(Email);
-- CREATE NONCLUSTERED INDEX IX_Contacts_Role ON Contacts(Role);

CREATE NONCLUSTERED INDEX IX_Leads_Status ON Leads(Status);
CREATE NONCLUSTERED INDEX IX_Leads_Source ON Leads(Source);
-- CREATE NONCLUSTERED INDEX IX_Leads_AssignedToUserId ON Leads(AssignedToUserId);
CREATE NONCLUSTERED INDEX IX_Leads_CreatedAt ON Leads(CreatedAt);

CREATE NONCLUSTERED INDEX IX_Opportunities_CustomerId ON Opportunities(CustomerId);
CREATE NONCLUSTERED INDEX IX_Opportunities_Stage ON Opportunities(Stage);
CREATE NONCLUSTERED INDEX IX_Opportunities_Status ON Opportunities(Status);
CREATE NONCLUSTERED INDEX IX_Opportunities_ExpectedCloseDate ON Opportunities(ExpectedCloseDate);

CREATE NONCLUSTERED INDEX IX_Activities_CustomerId ON Activities(CustomerId);
CREATE NONCLUSTERED INDEX IX_Activities_Type ON Activities(Type);
CREATE NONCLUSTERED INDEX IX_Activities_Status ON Activities(Status);
CREATE NONCLUSTERED INDEX IX_Activities_ScheduledDate ON Activities(ScheduledDate);

CREATE NONCLUSTERED INDEX IX_Products_Name ON Products(Name);
CREATE NONCLUSTERED INDEX IX_Products_Category ON Products(Category);
CREATE NONCLUSTERED INDEX IX_Products_IsActive ON Products(IsActive);

CREATE NONCLUSTERED INDEX IX_Orders_CustomerId ON Orders(CustomerId);
CREATE NONCLUSTERED INDEX IX_Orders_Status ON Orders(Status);
CREATE NONCLUSTERED INDEX IX_Orders_OrderDate ON Orders(OrderDate);

CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE NONCLUSTERED INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);

CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Users_Username ON Users(Username);
CREATE NONCLUSTERED INDEX IX_Users_Role ON Users(Role);
CREATE NONCLUSTERED INDEX IX_Users_Status ON Users(Status);

-- =============================================
-- SAMPLE DATA
-- =============================================

-- Insert sample data for testing
INSERT INTO Customers (CompanyName, FirstName, LastName, Email, Phone, Address, City, State, PostalCode, Country, Industry, Type, Status, CreatedBy, CreatedAt)
VALUES 
('Acme Corporation', 'John', 'Smith', 'john.smith@acme.com', '555-0101', '123 Main St', 'New York', 'NY', '10001', 'USA', 'Technology', 1, 0, 'admin', GETDATE()),
('Tech Solutions Inc', 'Sarah', 'Johnson', 'sarah.johnson@techsolutions.com', '555-0102', '456 Oak Ave', 'San Francisco', 'CA', '94102', 'USA', 'Software', 1, 0, 'admin', GETDATE()),
('Global Industries', 'Mike', 'Wilson', 'mike.wilson@global.com', '555-0103', '789 Pine St', 'Chicago', 'IL', '60601', 'USA', 'Manufacturing', 1, 0, 'admin', GETDATE()),
('Innovation Labs', 'Emily', 'Davis', 'emily.davis@innovationlabs.com', '555-0104', '321 Tech Blvd', 'Austin', 'TX', '73301', 'USA', 'Software', 1, 0, 'admin', GETDATE()),
('Data Dynamics', 'Robert', 'Brown', 'robert.brown@datadynamics.com', '555-0105', '654 Data Dr', 'Seattle', 'WA', '98101', 'USA', 'Technology', 1, 0, 'admin', GETDATE()),
('Cloud Systems', 'Lisa', 'Garcia', 'lisa.garcia@cloudsystems.com', '555-0106', '987 Cloud Way', 'Denver', 'CO', '80201', 'USA', 'Technology', 1, 0, 'admin', GETDATE()),
('Digital Marketing Pro', 'David', 'Martinez', 'david.martinez@digitalmarketingpro.com', '555-0107', '147 Marketing Ave', 'Miami', 'FL', '33101', 'USA', 'Marketing', 1, 0, 'admin', GETDATE()),
('Financial Services Group', 'Jennifer', 'Anderson', 'jennifer.anderson@financialservices.com', '555-0108', '258 Finance St', 'Boston', 'MA', '02101', 'USA', 'Finance', 1, 0, 'admin', GETDATE()),
('Healthcare Solutions', 'Michael', 'Taylor', 'michael.taylor@healthcaresolutions.com', '555-0109', '369 Health Blvd', 'Phoenix', 'AZ', '85001', 'USA', 'Healthcare', 1, 0, 'admin', GETDATE()),
('Retail Excellence', 'Amanda', 'Thomas', 'amanda.thomas@retailexcellence.com', '555-0110', '741 Retail Rd', 'Las Vegas', 'NV', '89101', 'USA', 'Retail', 1, 0, 'admin', GETDATE());

INSERT INTO Leads (CompanyName, FirstName, LastName, Email, Phone, Source, Status, Score, Notes, CreatedBy, CreatedAt)
VALUES 
('StartupXYZ', 'Alice', 'Brown', 'alice.brown@startupxyz.com', '555-0201', 'Website', 0, 85, 'High-value prospect from website', 'admin', GETDATE()),
('Innovation Labs', 'Bob', 'Davis', 'bob.davis@innovationlabs.com', '555-0202', 'Referral', 1, 92, 'Referred by existing customer', 'admin', GETDATE()),
('TechStart Inc', 'Carol', 'Wilson', 'carol.wilson@techstart.com', '555-0203', 'Cold Call', 0, 78, 'Interested in CRM solution', 'admin', GETDATE()),
('DataCorp', 'Daniel', 'Miller', 'daniel.miller@datacorp.com', '555-0204', 'Trade Show', 2, 88, 'Met at TechExpo 2024', 'admin', GETDATE()),
('CloudFirst', 'Eva', 'Garcia', 'eva.garcia@cloudfirst.com', '555-0205', 'Website', 1, 95, 'Downloaded whitepaper', 'admin', GETDATE()),
('Mobile Solutions', 'Frank', 'Rodriguez', 'frank.rodriguez@mobilesolutions.com', '555-0206', 'Referral', 0, 82, 'Referred by Tech Solutions Inc', 'admin', GETDATE()),
('AI Innovations', 'Grace', 'Lee', 'grace.lee@aiinnovations.com', '555-0207', 'LinkedIn', 1, 90, 'Connected on LinkedIn', 'admin', GETDATE()),
('Blockchain Tech', 'Henry', 'White', 'henry.white@blockchaintech.com', '555-0208', 'Website', 0, 75, 'Requested demo', 'admin', GETDATE());

INSERT INTO Opportunities (CustomerId, Title, Description, Amount, Probability, Stage, Status, ExpectedCloseDate, CreatedBy, CreatedAt)
VALUES 
(1, 'Enterprise Software License', 'Annual license for enterprise CRM solution', 100000.00, 75, 2, 0, DATEADD(month, 1, GETDATE()), 'admin', GETDATE()),
(2, 'Custom Development Project', 'Custom web application development', 150000.00, 60, 3, 0, DATEADD(month, 2, GETDATE()), 'admin', GETDATE()),
(3, 'Manufacturing ERP System', 'Complete ERP implementation for manufacturing', 250000.00, 80, 4, 0, DATEADD(month, 3, GETDATE()), 'admin', GETDATE()),
(4, 'Cloud Migration Services', 'Migration to cloud infrastructure', 75000.00, 70, 2, 0, DATEADD(month, 1, GETDATE()), 'admin', GETDATE()),
(5, 'Data Analytics Platform', 'Advanced analytics and reporting solution', 120000.00, 65, 3, 0, DATEADD(month, 2, GETDATE()), 'admin', GETDATE()),
(6, 'Security Audit and Implementation', 'Comprehensive security assessment', 50000.00, 85, 1, 0, DATEADD(month, 1, GETDATE()), 'admin', GETDATE()),
(7, 'Digital Marketing Automation', 'Marketing automation platform setup', 35000.00, 90, 4, 0, DATEADD(month, 1, GETDATE()), 'admin', GETDATE()),
(8, 'Financial Reporting System', 'Custom financial reporting and compliance', 180000.00, 55, 2, 0, DATEADD(month, 4, GETDATE()), 'admin', GETDATE()),
(9, 'Healthcare Data Management', 'HIPAA-compliant data management system', 200000.00, 70, 3, 0, DATEADD(month, 3, GETDATE()), 'admin', GETDATE()),
(10, 'E-commerce Platform Upgrade', 'Modern e-commerce platform implementation', 95000.00, 75, 2, 0, DATEADD(month, 2, GETDATE()), 'admin', GETDATE());

INSERT INTO Activities (CustomerId, Type, Subject, Description, ScheduledDate, Status, Priority, CreatedBy, CreatedAt)
VALUES 
(1, 0, 'Follow up with Acme Corp', 'Call to discuss contract renewal', DATEADD(day, 3, GETDATE()), 0, 2, 'admin', GETDATE()),
(2, 2, 'Prepare proposal for Tech Solutions', 'Create detailed proposal for software upgrade', DATEADD(day, 5, GETDATE()), 1, 1, 'admin', GETDATE()),
(3, 2, 'Demo presentation for Global Industries', 'Prepare and deliver product demo', DATEADD(day, 7, GETDATE()), 0, 1, 'admin', GETDATE()),
(4, 1, 'Send pricing information to Innovation Labs', 'Email with detailed pricing breakdown', DATEADD(day, 1, GETDATE()), 1, 2, 'admin', GETDATE()),
(5, 0, 'Technical discussion with Data Dynamics', 'Call to discuss technical requirements', DATEADD(day, 4, GETDATE()), 0, 1, 'admin', GETDATE()),
(6, 2, 'Security presentation for Cloud Systems', 'Present security features and compliance', DATEADD(day, 6, GETDATE()), 0, 2, 'admin', GETDATE()),
(7, 1, 'Follow up email to Digital Marketing Pro', 'Send additional case studies', DATEADD(day, 2, GETDATE()), 1, 1, 'admin', GETDATE()),
(8, 0, 'Contract negotiation call with Financial Services', 'Discuss contract terms and pricing', DATEADD(day, 8, GETDATE()), 0, 2, 'admin', GETDATE()),
(9, 2, 'HIPAA compliance demo for Healthcare Solutions', 'Demonstrate HIPAA-compliant features', DATEADD(day, 9, GETDATE()), 0, 2, 'admin', GETDATE()),
(10, 1, 'Proposal review meeting with Retail Excellence', 'Review and discuss proposal details', DATEADD(day, 10, GETDATE()), 0, 1, 'admin', GETDATE()),
(1, 3, 'Update customer records', 'Update Acme Corp contact information', DATEADD(day, 1, GETDATE()), 1, 0, 'admin', GETDATE()),
(2, 4, 'Note: Tech Solutions interested in mobile app', 'Customer mentioned interest in mobile solution', DATEADD(day, 2, GETDATE()), 1, 1, 'admin', GETDATE()),
(3, 0, 'Follow up call with Global Industries', 'Check on demo feedback and next steps', DATEADD(day, 11, GETDATE()), 0, 1, 'admin', GETDATE()),
(4, 2, 'Technical architecture discussion', 'Deep dive into technical requirements', DATEADD(day, 12, GETDATE()), 0, 2, 'admin', GETDATE()),
(5, 1, 'Send technical documentation', 'Email technical specifications and requirements', DATEADD(day, 3, GETDATE()), 1, 1, 'admin', GETDATE());

INSERT INTO Products (Name, Description, SKU, Price, Cost, Category, IsActive, CreatedBy, CreatedAt)
VALUES 
('Enterprise CRM License', 'Full-featured CRM solution for enterprise customers', 'CRM-ENT-001', 10000.00, 2000.00, 'Software', 1, 'admin', GETDATE()),
('Custom Development Services', 'Custom software development and implementation', 'DEV-CUST-001', 150.00, 75.00, 'Services', 1, 'admin', GETDATE()),
('Cloud Migration Package', 'Complete cloud migration and setup services', 'CLOUD-MIG-001', 25000.00, 5000.00, 'Services', 1, 'admin', GETDATE()),
('Data Analytics Platform', 'Advanced analytics and reporting solution', 'DATA-ANAL-001', 15000.00, 3000.00, 'Software', 1, 'admin', GETDATE()),
('Security Audit Service', 'Comprehensive security assessment and implementation', 'SEC-AUD-001', 5000.00, 1000.00, 'Services', 1, 'admin', GETDATE()),
('Mobile App Development', 'Custom mobile application development', 'MOB-DEV-001', 25000.00, 5000.00, 'Services', 1, 'admin', GETDATE()),
('Training and Support', 'User training and ongoing support services', 'TRN-SUP-001', 2000.00, 400.00, 'Services', 1, 'admin', GETDATE()),
('API Integration Service', 'Third-party API integration and setup', 'API-INT-001', 8000.00, 1600.00, 'Services', 1, 'admin', GETDATE()),
('Backup and Recovery', 'Data backup and disaster recovery solution', 'BCK-REC-001', 3000.00, 600.00, 'Software', 1, 'admin', GETDATE()),
('Performance Optimization', 'System performance tuning and optimization', 'PERF-OPT-001', 12000.00, 2400.00, 'Services', 1, 'admin', GETDATE());

INSERT INTO Orders (CustomerId, OrderNumber, OrderDate, Status, TotalAmount, Notes, CreatedBy, CreatedAt)
VALUES 
(1, 'ORD-2024-001', GETDATE(), 3, 100000.00, 'Annual CRM license renewal', 'admin', GETDATE()),
(2, 'ORD-2024-002', DATEADD(day, -30, GETDATE()), 2, 150000.00, 'Custom development project', 'admin', GETDATE()),
(3, 'ORD-2024-003', DATEADD(day, -15, GETDATE()), 1, 250000.00, 'Manufacturing ERP implementation', 'admin', GETDATE()),
(4, 'ORD-2024-004', DATEADD(day, -7, GETDATE()), 0, 75000.00, 'Cloud migration services', 'admin', GETDATE()),
(5, 'ORD-2024-005', DATEADD(day, -45, GETDATE()), 3, 120000.00, 'Data analytics platform', 'admin', GETDATE()),
(6, 'ORD-2024-006', DATEADD(day, -20, GETDATE()), 2, 50000.00, 'Security audit and implementation', 'admin', GETDATE()),
(7, 'ORD-2024-007', DATEADD(day, -10, GETDATE()), 1, 35000.00, 'Digital marketing automation', 'admin', GETDATE()),
(8, 'ORD-2024-008', DATEADD(day, -5, GETDATE()), 0, 180000.00, 'Financial reporting system', 'admin', GETDATE());

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice, CreatedBy, CreatedAt)
VALUES 
(1, 1, 10, 10000.00, 100000.00, 'admin', GETDATE()),
(2, 2, 1000, 150.00, 150000.00, 'admin', GETDATE()),
(3, 2, 1667, 150.00, 250000.00, 'admin', GETDATE()),
(4, 3, 3, 25000.00, 75000.00, 'admin', GETDATE()),
(5, 4, 8, 15000.00, 120000.00, 'admin', GETDATE()),
(6, 5, 10, 5000.00, 50000.00, 'admin', GETDATE()),
(7, 6, 1, 25000.00, 25000.00, 'admin', GETDATE()),
(7, 7, 5, 2000.00, 10000.00, 'admin', GETDATE()),
(8, 8, 22, 8000.00, 176000.00, 'admin', GETDATE()),
(8, 9, 1, 3000.00, 3000.00, 'admin', GETDATE()),
(8, 10, 1, 1000.00, 1000.00, 'admin', GETDATE());

INSERT INTO Users (Username, FirstName, LastName, Email, PasswordHash, JobTitle, Department, Role, Status, CreatedBy, CreatedAt)
VALUES 
('admin', 'System', 'Administrator', 'admin@enterprisecrm.com', 'hashedpassword123', 'System Administrator', 'IT', 2, 0, 'system', GETDATE()),
('john.doe', 'John', 'Doe', 'john.doe@enterprisecrm.com', 'hashedpassword123', 'Sales Manager', 'Sales', 1, 0, 'admin', GETDATE()),
('jane.smith', 'Jane', 'Smith', 'jane.smith@enterprisecrm.com', 'hashedpassword123', 'Sales Representative', 'Sales', 0, 0, 'admin', GETDATE()),
('mike.johnson', 'Mike', 'Johnson', 'mike.johnson@enterprisecrm.com', 'hashedpassword123', 'Account Manager', 'Sales', 0, 0, 'admin', GETDATE()),
('sarah.wilson', 'Sarah', 'Wilson', 'sarah.wilson@enterprisecrm.com', 'hashedpassword123', 'Technical Consultant', 'Professional Services', 0, 0, 'admin', GETDATE()),
('david.brown', 'David', 'Brown', 'david.brown@enterprisecrm.com', 'hashedpassword123', 'Support Manager', 'Support', 1, 0, 'admin', GETDATE()),
('lisa.garcia', 'Lisa', 'Garcia', 'lisa.garcia@enterprisecrm.com', 'hashedpassword123', 'Marketing Manager', 'Marketing', 1, 0, 'admin', GETDATE()),
('robert.martinez', 'Robert', 'Martinez', 'robert.martinez@enterprisecrm.com', 'hashedpassword123', 'Developer', 'Engineering', 0, 0, 'admin', GETDATE());
